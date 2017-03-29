using BasicNopSync;
using BasicNopSync.Model;
using BasicNopSync.Model.NopCommerce;
using BasicNopSync.OData;
using BasicNopSync.Syncers;
using BasicNopSync.Utils;
using MercatorORM;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.Syncers.NopToMercator
{
    public class ClientMToNSyncer : Syncer
    {
        public const string ENTITY = "Customer";
        public const string KEY_MERCATORID = "MercatorId";
        public const string KEY_MODIFTAG = "Modiftag";

        public const string WHERE_CLAUSE = "c_email <> '' and c_from_web = 0 and c_sommeil = 0 and c_email like '%noctis%' ";
        
        public ClientMToNSyncer() : base()
        {
            urlBuilder = new UrlBuilder(ENTITY);
        }
        
        /// <summary>
        /// Sync Clients from Mercator to Nop
        /// </summary>
        /// <returns></returns>
        public override bool Sync()
        {
            using (SqlConnection connection = new SqlConnection(dataSettings.DataConnectionString))
            {
                try
                {
                    connection.Open();
                    //TODO Get clients to be registered => DEFINIR CONDITION
                    List<CLI> mclis = GetMercatorClientsList(connection, WHERE_CLAUSE);                     

                    //List<CLI> mclis = mClients.Select(x => x.Cli).ToList();
                    string nopClient = "";
                    IEnumerable<JToken> clis = null;
                    foreach (CLI c in mclis)
                    {
                        try
                        {
                            if (c.C_ID_WEB != 0)
                            //a déjà été synchro : vérifie si on doit modifier quelque chose
                            {
                                //Get the customer addresses for the given customer Id if it is not deleted 
                                nopClient = WebService.Get(urlBuilder
                                    .Id(c.C_ID_WEB)
                                    .FilterEq("Deleted", false)
                                    .Expand("Addresses")
                                    .BuildQuery());

                                clis = ParserJSon.ParseResultToJTokenList(nopClient);

                                if (clis.Count() > 0)
                                {
                                    JToken client = clis.FirstOrDefault();

                                    string modiftagResult = WebService.Get(new UrlBuilder("GenericAttribute").And().FilterEq("KeyGroup", ENTITY).FilterEq("Key", KEY_MODIFTAG).FilterEq("EntityId", (int)client["Id"]).BuildQuery());
                                    JToken[] modiftagJtokens = ParserJSon.ParseResultToJTokenList(modiftagResult);
                                    string modiftagS = modiftagJtokens.FirstOrDefault()?["Value"]?.ToString();
                                    decimal modiftag = Decimal.Parse(modiftagS);

                                    if (modiftag < c.C_MODIFTAG)
                                    {

                                        var address = client["Addresses"].ToArray();

                                        if (address.Count() > 0)
                                        {
                                            Address a = GetAddressFromToken(address.Last());

                                            Customer customer = new Customer(c.C_ID, client["Email"].ToString(), client["Username"].ToString(), c.C_NOM.TrimEnd(), (DateTime)client["CreatedOnUtc"], (int)client["Id"], a);

                                            customer.Tva = c.C_NUM_TVA?.TrimEnd();

                                            customer.PasswordSalt = client["PasswordSalt"].ToString();
                                            customer.Password = client["Password"].ToString();

                                            updateNopClient(c, customer, modiftagJtokens.First());

                                        }
                                    }
                                }
                                else
                                {
                                    //Doesn't exist or has been deleted
                                }
                            }
                            else
                            {
                                createNopClient(c);
                            }
                        }catch(Exception e)
                        {
                            Program.log(e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Program.log(e);
                    return false;       

                }
                finally
                {
                    connection.Close();
                }
            
             
                return true;

            }
        }

        /// <summary>
        /// Get the CLI list from Mercator 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="whereClause"></param>
        /// <returns></returns>
        private List<CLI> GetMercatorClientsList(SqlConnection connection, string whereClause)
        {
            string sql = @"select c_id, c_nom, c_email, c_adresse, c_adresse2, c_codep, c_ville, c_pays, c_num_tel, c_num_gsm, 
                c_cat1, c_modiftag, c_tarif, c_id_web from CLI" + (whereClause.Length > 0 ? " WHERE " + whereClause : "");
            
            List<CLI> cliList = new List<CLI>();
            using (SqlCommand sqlCommand = new SqlCommand(sql))            
            {
                try
                {
                    DataTable dt = DBContextFactory.DBContext.Query(sqlCommand);

                    foreach (DataRow dr in dt.Rows)
                    {
                        CLI cli = new CLI();

                        cli.C_ID = Convert.ToString(dr["c_id"]).TrimEnd();
                        cli.C_EMAIL = Convert.ToString(dr["c_email"]).TrimEnd();
                        cli.C_NOM = Convert.ToString(dr["c_nom"]).TrimEnd();
                        cli.C_ADRESSE = Convert.ToString(dr["c_adresse"]).TrimEnd();
                        cli.C_ADRESSE2 = Convert.ToString(dr["c_adresse2"]).TrimEnd();
                        cli.C_CODEP = Convert.ToString(dr["c_codep"]).TrimEnd();
                        cli.C_VILLE = Convert.ToString(dr["c_ville"]).TrimEnd();
                        cli.C_PAYS = Convert.ToString(dr["c_pays"]).TrimEnd();
                        cli.C_NUM_TEL = Convert.ToString(dr["c_num_tel"]).TrimEnd();
                        cli.C_NUM_GSM = Convert.ToString(dr["c_num_gsm"]).TrimEnd();
                        cli.C_CAT1 = Convert.ToString(dr["c_cat1"]).TrimEnd();
                        cli.C_TARIF = Convert.ToString(dr["c_tarif"]).TrimEnd();
                        cli.C_MODIFTAG = Convert.ToDecimal(dr["c_modiftag"]);
                        cli.C_ID_WEB = Convert.ToInt32(dr["c_id_web"]);

                        cliList.Add(cli);
                    }

                    return cliList;
                }
                catch (Exception e)
                {
                    Program.log(e);
                    return cliList;
                }
            }
        }

        /// <summary>
        /// Extract the address from a Jtoken
        /// </summary>
        /// <param name="addressJ"></param>
        /// <returns></returns>
        private Address GetAddressFromToken(JToken addressJ)
        {
            JObject countryO = JObject.Parse(WebService.Get(new UrlBuilder("Country").Id((int)addressJ["CountryId"]).BuildQuery()));
            //Prend la première adresse
            Address a = new Address(addressJ["FirstName"].ToString(), 
                addressJ["LastName"].ToString(), 
                addressJ["Address1"].ToString(), 
                addressJ["ZipPostalCode"].ToString(),
                addressJ["City"].ToString(), 
                countryO["Name"].ToString(), 
                addressJ["PhoneNumber"].ToString(), 
                addressJ["Company"].ToString());
            return a;

        }

        /// <summary>
        /// Create a new Customer in NopCommerce from a Mercator CLI
        /// </summary>
        /// <param name="c"></param>
        /// <param name="context"></param>
        private void createNopClient(CLI c)
        {
            //Check if email/username already exists
            CheckEmailExistence(c);
            //checkUsernameExistence(c);

            //Generation random password, or use C_ID?
            //string password = PasswordManagement.CreatePassword();
            string password = c.C_ID;

            //Add address
            string addressResult = WebService.Post(WebApiEntities.ADDRESS, ParserJSon.ParseAddressToJson(c, OptionsMercator).ToString());
            JObject newAddress = JObject.Parse(addressResult);
            int addressId = (int)newAddress["Id"];


            string clientJson = ParserJSon.ParseClientToJson(c.C_EMAIL.TrimEnd(), c.C_ID.TrimEnd(), c.C_MODIFTAG).ToString();

            string clientResult = WebService.Post(ENTITY, clientJson);

            JObject newCli = JObject.Parse(clientResult);
            int id = (int)newCli["Id"];
            c.C_ID_WEB = id;

            //Password
            JObject clientPassword = ParserJSon.ParseClientPassword(c.C_ID.TrimEnd(), int.Parse(newCli["Id"].ToString()));
            WebService.Post(WebApiEntities.CUSTOMER_PASSWORD, clientPassword.ToString());

            //Add TVA Number (if not_empty)
            //VatNumberStatudId : 10 = empty, 20 = valid
            if (!String.IsNullOrWhiteSpace(c.C_NUM_TVA))
            {
                List<JObject> entries = ParserJSon.ParseClientCompanyToJson(id, c.C_NUM_TVA.TrimEnd(), c.C_NOM.TrimEnd());
                foreach (JObject e in entries)
                {
                    WebService.Post(WebApiEntities.GENERIC_ATTRIBUTE, e.ToString());
                }
            }

            JObject linkCliToAddress = new JObject();
            linkCliToAddress.Add("@odata.id", WebService.GetStoreAddress() + new UrlBuilder("Address").Id(addressId).BuildQuery());
            //link address            
            WebService.Put(urlBuilder.BuildQueryRef(c.C_ID_WEB,"Addresses"), linkCliToAddress.ToString());

            //link address as billing address
            //WebService.Put(String.Format(WebServiceUrls.LINK_CUSTOMER_TO_BILLING_ADDRESS, c.C_ID_WEB.ToString()), linkCliToAddress.ToString());

            //link role                                        
            //3 = Registered (so that he can connect)
            JObject linkCliToCR = new JObject();
            linkCliToCR.Add("@odata.id", WebService.GetStoreAddress() + new UrlBuilder("CustomerRole").Id(3).BuildQuery());
            WebService.Put(urlBuilder.BuildQueryRef(c.C_ID_WEB, "CustomerRoles"), linkCliToCR.ToString());
            
            JObject genericAttributeMercatorId = ParserJSon.GetGenericAttributeJSon(id, ENTITY, KEY_MERCATORID, c.C_ID);
            WebService.Post(WebApiEntities.GENERIC_ATTRIBUTE, genericAttributeMercatorId);
            JObject genericAttributeModiftagId = ParserJSon.GetGenericAttributeJSon(id, ENTITY, KEY_MODIFTAG, c.C_MODIFTAG.ToString());
            WebService.Post(WebApiEntities.GENERIC_ATTRIBUTE, genericAttributeModiftagId);
            //From Mercator, tarif indiqué dans fiche article
            //string roleIdResult = WebService.Get(String.Format(WebServiceUrls.CUSTOMER_ROLE_BY_SYSTEM_NAME, WebServiceUrls.TARIF_ + c.C_TARIF));
            //JToken[] roleId = ParserJSon.ParseResultToJTokenList(roleIdResult);

            //linkCliToCR = "{\"@odata.id\":\"" + ConfigurationManager.AppSettings["storeAddress"] + String.Format(WebServiceUrls.CUSTOMER_ROLE_ID, roleId.FirstOrDefault()["Id"].ToString()) + "\"}";
            //WebService.Put(String.Format(WebServiceUrls.LINK_CUSTOMER_TO_CR_WS, c.C_ID_WEB.ToString()), linkCliToCR);
            SigCli sigCli = new SigCli();
            sigCli.Read(c.C_ID);
            
            sigCli.SetValueForField("C_ID_WEB", c.C_ID_WEB);
            sigCli.Update();

            //context.SaveChanges();
        }


        /// <summary>
        /// Update Nop's Customer datas from Mercator 
        /// </summary>
        /// <param name="cli"></param>
        /// <param name="cl"></param>
        /// <param name="context"></param>
        private void updateNopClient(CLI cli, Customer cl, JToken GenericAttributeModiftag)
        {
            //Patch Customer
            JObject toPatch = new JObject();
            if (cli.C_EMAIL.TrimEnd() != cl.Email)
            {
                CheckEmailExistence(cli);
                toPatch.Add("Username", cli.C_EMAIL.TrimEnd());
                toPatch.Add("Email", cli.C_EMAIL.TrimEnd());
            }
            
            WebService.Patch(urlBuilder.Id(cl.Id).BuildQuery(), toPatch.ToString());

            //Tarif update
            //string crJson = WebService.Get(String.Format(WebServiceUrls.GET_CUSTOMER_AND_CR_BY_ID, cl.IdNop));
            //JToken[] customer = ParserJSon.ParseResultToJTokenList(crJson);
            //var crs = customer.FirstOrDefault();
            //var customerRoles = crs != null ? crs["CustomerRoles"].ToArray() : null;

            //syncTarCli(cli, context);
            
            //Update Tag            
            toPatch = new JObject();
            toPatch.Add("Value", cli.C_MODIFTAG.ToString());
            WebService.Patch(String.Format(new UrlBuilder("GenericAttribute").Id((int)GenericAttributeModiftag["Id"]).BuildQuery()), toPatch.ToString());
            
            if (cli.C_ADRESSE.TrimEnd() != cl.Address1.Street.TrimEnd() || cli.C_ADRESSE2.TrimEnd() != cl.Address1.Street2.TrimEnd())
            {
                string addressJson = ParserJSon.ParseAddressToJson(cli, OptionsMercator).ToString();
                string addressResult = WebService.Post(WebApiEntities.ADDRESS, addressJson);
                JObject newAddress = JObject.Parse(addressResult);
                int addressId = (int)newAddress["Id"];

                //link address
                JObject linkCliToAddress = new JObject();
                linkCliToAddress.Add("@odata.id",WebService.GetStoreAddress() + new UrlBuilder("Address").Id(addressId).BuildQuery());
                
                WebService.Put(urlBuilder.BuildQueryRef(cli.C_ID_WEB,"Addresses"), linkCliToAddress);
                //link address as billing address
                //WebService.Put(String.Format(WebServiceUrls.LINK_CUSTOMER_TO_BILLING_ADDRESS, cli.C_ID_WEB.ToString()), linkCliToAddress);
            }
        }

        #region Utils

        /// <summary>
        /// Check if the Mercator client's email already exists in NopCommerce
        /// If so, it throws an Exception
        /// </summary>
        /// <param name="c">Mercator Client</param>
        private void CheckEmailExistence(CLI c)
        {
            string emailResult = WebService.Get(urlBuilder.FilterEq("Email",c.C_EMAIL.TrimEnd()).BuildQuery());
            JToken[] emails = ParserJSon.ParseResultToJTokenList(emailResult);
            if (emails.Count() > 0)
            {
                JToken user = emails.First();
                if (user["Deleted"].ToString() == "False")
                {
                    string mercatorIdResult = WebService.Get(String.Format(new UrlBuilder("GenericAttribute").And().FilterEq("KeyGroup", ENTITY).FilterEq("Key", KEY_MERCATORID).FilterEq("EntityId", (int)user["Id"]).BuildQuery()));
                    JObject mercatorIdJObject = JObject.Parse(mercatorIdResult);
                    string mercatorId = mercatorIdJObject["Value"]?.ToString();
                    if (mercatorId != c.C_ID.TrimEnd() && !String.IsNullOrWhiteSpace(mercatorId))
                        throw new Exception("User with email : " + c.C_EMAIL.TrimEnd() + " already exists");
                }
            }
        }


        ///// <summary>
        ///// Check if the Mercator client's username (PILE+id) already exists in NopCommerce
        ///// If so, it throws an Exception
        ///// </summary>
        ///// <param name="c">Mercator Client</param>
        //private void CheckUsernameExistence(CLI c)
        //{
        //    string userNameResult = WebService.Get(String.Format(WebServiceUrls.GET_CUSTOMER_BY_USERNAME, /*mercatorPrefix+*/c.C_ID.TrimEnd()));
        //    JToken[] usernames = ParserJSon.ParseResultToJTokenList(userNameResult);

        //    if (usernames.Count() > 0)
        //    {
        //        JToken user = usernames.First();
        //        if (user["Deleted"].ToString() == "False")
        //        {
        //            string mercatorId = user["MercatorId"].ToString().TrimEnd();
        //            if (mercatorId != c.C_ID.TrimEnd() && mercatorId != "")
        //                throw new DuplicateNameException("User with username : " + /*mercatorPrefix +*/ c.C_ID.TrimEnd() + " already exists");
        //        }

        //    }
        //}


        #endregion
    }
}
