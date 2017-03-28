using BasicNopSync.Model;
using MercatorORM;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using BasicNopSync.Model.NopCommerce;
using System.Linq;
using BasicNopSync.Utils;
using System.Runtime.CompilerServices;
using BasicNopSync.OData;

namespace BasicNopSync.Syncers.NopToMercator
{

    public class ClientNToMSyncer : Syncer
    {

        public const string ENTITY = "Customer";
        public const string KEY_MERCATORID = "MercatorId";
        public const string KEY_VATNUMBER = "VatNumber";

        public ClientNToMSyncer() : base()
        {
            urlBuilder = new UrlBuilder(ENTITY);
        }
        
        public override bool Sync()
        {
            using (SqlConnection connection = new SqlConnection(dataSettings.DataConnectionString))
            {
                try
                {
                    connection.Open();

                    //Get clients (lastActivityDate comes with it)
                    string nopClients = WebService.Get(urlBuilder.FilterNe("Email",null).FilterEq("IsSystemAccount",false).FilterEq("Active",true).Expand("Addresses").BuildQuery());
                    JToken[] clis = ParserJSon.ParseResultToJTokenList(nopClients);

                    List<Customer> toSync = new List<Customer>();
                    
                    foreach (JToken c in clis)
                    {
						try
                        {
                        string mercatorIdResult = WebService.Get(new UrlBuilder("GenericAttribute").And().FilterEq("KeyGroup", ENTITY).FilterEq("Key", KEY_MERCATORID).FilterEq("EntityId", (int)c["Id"]).BuildQuery());
                        JToken[] mercatorIdToken = ParserJSon.ParseResultToJTokenList(mercatorIdResult);
                        
                        string mercatorId = mercatorIdToken.FirstOrDefault()?["Value"]?.ToString();

                        if ((DateTimeOffset)c["LastActivityDateUtc"] > DateTimeOffset.Now.AddHours(-12) || mercatorId == null)
                        {
                            string ordersByCliResult = WebService.Get(new UrlBuilder("Order")
                                                                    .FilterEq("CustomerId", c["Id"])
                                                                    .BuildQuery()); 

                            JToken[] ordersTokens = ParserJSon.ParseResultToJTokenList(ordersByCliResult);

                            if (ordersTokens.Length > 0)
                            {
                                var address = c["Addresses"].ToArray();

                                if (address.Length > 0)
                                {
                                    //Prend la première adresse
                                    Address a = getAddressFromToken(address[0]);

                                    Customer cli = new Customer
                                    {
                                        Id = (int)c["Id"],
                                        Email = c["Email"].ToString(),
                                        Username = c["Username"].ToString(),
                                        Name = a.FirstName + " " + a.LastName,
                                        CreatedOnUtc = (DateTime)c["CreatedOnUtc"],
                                        MercatorId = mercatorId,
                                        Address1 = a,

                                    };

                                        //Fetch Tva
                                        JToken[] tvaNumber = ParserJSon.ParseResultToJTokenList(WebService.Get(new UrlBuilder("GenericAttribute")
                                            .FilterEq("EntityId", (int)c["Id"])
                                            .FilterEq("KeyGroup", ENTITY)
                                            .FilterEq("Key", KEY_VATNUMBER)
                                            .BuildQuery()));
                                        
                                    if (tvaNumber.Length > 0)
                                    {
                                        cli.Tva = tvaNumber.First()["Value"].ToString();
                                    }

                                    toSync.Add(cli);
                                }
                            }
                        }                        
						}catch(Exception e)
	                    {
	                        Program.log(e);                            
	                    }
                    }

                    foreach (Customer c in toSync)
                    {
                        try
                        {
                            //Check if has to be edited or added
                            SigCli sigCli = new SigCli();
                            bool exists = sigCli.ReadWhere(String.Format("C_ID_WEB = '{0}'", c.Id));

                            string newMercatorId = "";

                            if (!exists)
                            {
                                sigCli = createMClient(c);
                                newMercatorId = sigCli.GetField("C_ID").Value.ToString();
                            }
                            else if ((bool)sigCli.GetField("C_FROM_WEB").Value)
                            //On l'update s'il a été créé sur le site, pas s'il vient de Mercator
                            {
                                if(c.MercatorId == null)
                                {
                                    newMercatorId = sigCli.GetField("C_ID").Value.ToString().TrimEnd();
                                    c.MercatorId = newMercatorId;                                    
                                }
                                updateMClient(c);
                            }

                            //Add the newly created mercator id to the nopCommerce customer
                            if (!String.IsNullOrWhiteSpace(newMercatorId))
                            {
                                JToken[] result = ParserJSon.ParseResultToJTokenList(WebService.Get(new UrlBuilder("GenericAttribute").And().FilterEq("KeyGroup", ENTITY).FilterEq("Key", KEY_MERCATORID).FilterEq("EntityId", c.Id).BuildQuery()));
                                if (result.Length > 0)
                                {
                                    if (result.FirstOrDefault()["Value"].ToString() != "")
                                    {
                                        Program.log(String.Format("Client {0} already has a MercatorId : {1}", c.Id, result[0]["Value"]));
                                    }                                   
                                }
                                else
                                {
                                    JObject mercatorIdGA = ParserJSon.GetGenericAttributeJSon(c.Id, ENTITY, KEY_MERCATORID, newMercatorId);

                                    WebService.Post(WebApiEntities.GENERIC_ATTRIBUTE, mercatorIdGA);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Program.log(e);
                        }
                        
                    }                    
                }
                catch (Exception e)
                {
                    Program.log(e.Message);
                    Program.log(e.StackTrace);
                    return false;
                }
				finally
				{
					connection.Close();
				}

            }

            return true;


        }

        /// <summary>
        /// Extract the address from a Jtoken
        /// </summary>
        /// <param name="addressJ"></param>
        /// <returns></returns>
        private static Address getAddressFromToken(JToken addressJ)
        {
            JObject countryO = JObject.Parse(WebService.Get("Country(" + addressJ["CountryId"].ToString() + ")"));
            //Prend la première adresse
            Address a = new Address(addressJ["FirstName"].ToString(), addressJ["LastName"].ToString(), addressJ["Address1"].ToString(), addressJ["ZipPostalCode"].ToString()
                , addressJ["City"].ToString(), countryO["Name"].ToString(), addressJ["PhoneNumber"].ToString(), addressJ["Company"].ToString());
            return a;

        }
        

       
        
        #region Mercator CLI

        /// <summary>
        /// Create a Mercator Client
        /// </summary>
        /// <param name="c"></param>
        /// <param name="context"></param>
        /// <returns>Mercator client initiated with Nop client's values</returns>
        private SigCli createMClient(Customer c)
        {
            SigCli sigCli = new SigCli();
            
            sigCli.SetValueForField("C_NOM", c.Name);
            sigCli.SetValueForField("C_CLE1", c.Id);
            sigCli.SetValueForField("C_TYP_FORM", 1);
            sigCli.SetValueForField("C_ADRESSE", c.Address1.Street);
			sigCli.SetValueForField("C_ADRESSE2", c.Address1.Street2);
            string zipPostalCode = c.Address1.ZipPostalCode;
            if (zipPostalCode.Length > 9)
            {
                zipPostalCode = zipPostalCode.Substring(0, 9);
            }
            sigCli.SetValueForField("C_CODEP", zipPostalCode);
            sigCli.SetValueForField("C_VILLE", c.Address1.City);
            sigCli.SetValueForField("C_PAYS", c.Address1.Country);
            sigCli.SetValueForField("C_NUM_TEL", c.Address1.PhoneNumber);
            sigCli.SetValueForField("C_EMAIL", c.Email);
            sigCli.SetValueForField("C_REGIME", 1);
            sigCli.SetValueForField("C_NUM_TVA", c.Tva ?? "");
            sigCli.SetValueForField("C_TARIF", Convert.ToInt32(OptionsMercator.GetOptionValue("NOP_TARIF")));
            sigCli.SetValueForField("C_CREATION", c.CreatedOnUtc);
            sigCli.SetValueForField("C_MODIF", DateTime.Now);
            sigCli.SetValueForField("C_ID_WEB", c.Id);
            sigCli.SetValueForField("C_FROM_WEB", true);                       
            sigCli.Create();

            return sigCli;
        }

        /// <summary>
        /// Update Mercator client values
        /// </summary>
        /// <param name="exist"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private static void updateMClient(Customer c)
        {
            SigCli sigCli = new SigCli();
            sigCli.Read(c.MercatorId);

            if (sigCli.GetField("C_NOM").Value.ToString().TrimEnd() != c.Name)
            {
                sigCli.SetValueForField("C_NOM", c.Address1.Street);
            }
            if (sigCli.GetField("C_ADRESSE").Value.ToString().TrimEnd() != c.Address1.Street)
            {
                sigCli.SetValueForField("C_ADRESSE", c.Address1.Street);
            }
			if (sigCli.GetField("C_ADRESSE2").Value.ToString().TrimEnd() != c.Address1.Street2)
            {
                sigCli.SetValueForField("C_ADRESSE2", c.Address1.Street2);
            }
            if (sigCli.GetField("C_CODEP").Value.ToString().TrimEnd() != c.Address1.ZipPostalCode)
            {
                string zipPostalCode = c.Address1.ZipPostalCode;
                if (zipPostalCode.Length > 9)
                {
                    zipPostalCode = zipPostalCode.Substring(0, 9);
                }
                sigCli.SetValueForField("C_CODEP", zipPostalCode);
            }
            if (sigCli.GetField("C_VILLE").Value.ToString().TrimEnd() != c.Address1.City)
            {
                sigCli.SetValueForField("C_VILLE", c.Address1.City);
            }
            if (sigCli.GetField("C_PAYS").Value.ToString().TrimEnd() != c.Address1.Country)
            {
                sigCli.SetValueForField("C_PAYS", c.Address1.Country);
            }
            if (sigCli.GetField("C_NUM_TEL").Value.ToString().TrimEnd() != c.Address1.PhoneNumber)
            {
                sigCli.SetValueForField("C_NUM_TEL", c.Address1.PhoneNumber);
            }
            if (sigCli.GetField("C_EMAIL").Value.ToString().TrimEnd() != c.Email)
            {
                sigCli.SetValueForField("C_EMAIL", c.Email);
            }
            
            sigCli.SetValueForField("C_MODIF", DateTime.Now);

            sigCli.Update();
        }

        #endregion



        /// <summary>
        /// Get a new Id of nbChar characters
        /// </summary>
        /// <param name="nbChar"></param>
        /// <returns></returns>
        private static string newId(int nbChar)
        {
            string web = "WEB";
            string[] tabId = MercatorApi.Api.NewId().Split('-');
            return web + tabId[tabId.Count() - 1].Substring(0, nbChar);
        }

        #region check User Existence 

        /// <summary>
        /// Check if the Mercator client's email already exists in NopCommerce
        /// If so, it throws an Exception
        /// </summary>
        /// <param name="c">Mercator Client</param>
        private void checkEmailExistence(CLI c)
        {
            string emailResult = WebService.Get(urlBuilder.FilterEq("Email", c.C_EMAIL.TrimEnd()).BuildQuery());
            JToken[] emails = ParserJSon.ParseResultToJTokenList(emailResult);
            if (emails.Count() > 0)
            {
                JToken user = emails.First();
                if (user["Deleted"].ToString() == "False")
                {
                    string mercatorId = user["MercatorId"].ToString().TrimEnd();
                    if (mercatorId != c.C_ID.TrimEnd() && mercatorId != "")
                        throw new Exception("User with email : " + c.C_EMAIL.TrimEnd() + " already exists");
                }
            }
        }

        /// <summary>
        /// Check if the Mercator client's username (PILE+id) already exists in NopCommerce
        /// If so, it throws an Exception
        /// </summary>
        /// <param name="c">Mercator Client</param>
        private void checkUsernameExistence(CLI c)
        {
            string userNameResult = WebService.Get(urlBuilder.FilterEq("Username",c.C_ID.TrimEnd()).BuildQuery());
            JToken[] usernames = ParserJSon.ParseResultToJTokenList(userNameResult);

            if (usernames.Count() > 0)
            {
                JToken user = usernames.First();
                if (user["Deleted"].ToString() == "False")
                {
                    string mercatorId = user["MercatorId"].ToString().TrimEnd();
                    if (mercatorId != c.C_ID.TrimEnd() && mercatorId != "")
                        throw new DuplicateNameException("User with username : " + /*mercatorPrefix +*/ c.C_ID.TrimEnd() + " already exists");
                }

            }
        }

        #endregion

    }
}
