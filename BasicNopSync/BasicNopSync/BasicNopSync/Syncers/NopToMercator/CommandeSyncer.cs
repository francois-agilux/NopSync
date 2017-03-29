using BasicNopSync.Model.NopCommerce;
using BasicNopSync.OData;
using BasicNopSync.Utils;
using MercatorORM;
using MercatorUi;
using MercatorUi.Engine.Gescom;
using Newtonsoft.Json.Linq;
using NopSync.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Xml;

namespace BasicNopSync.Syncers.NopToMercator
{

    public class CommandeSyncer : Syncer
    {
        //Name of checkout_attributes as placed in NopCommerce
        //private static string ATTRIBUT_COMM = ConfigurationManager.AppSettings["AttCommId"];
        //TODO Mettre dans config au début
        private static string directory;
        private static string journal;
        private bool useGenericArticle;
        private const string REF_WEB = "WEB";

        public const string ENTITY = "Order";
        public const string KEY_SYNCED = "Synced";

        public CommandeSyncer() : base()
        {
            urlBuilder = new UrlBuilder(ENTITY);

            OptionsMercator rep = new OptionsMercator();
            directory = rep.GetOptionValue("NOP_REP_M").ToString();

            OptionsMercator journalOption = new OptionsMercator();
            journal = journalOption.GetOptionValue("NOP_JOURN").ToString();

            OptionsMercator genericArticle = new OptionsMercator();
            useGenericArticle = genericArticle.GetOptionValue("NOP_GEN_A")?.ToString()?.TrimEnd() == "1";

        }

        public override bool Sync()
        {
            //Get orders placed since last sync (based on Order's id) from Nop using Web Service 

            using (Main Mercator = new Main(directory, null, ConfigurationManager.AppSettings["MercatorLogin"], ConfigurationManager.AppSettings["MercatorMdp"]))
            {
                //@"\\serveur\dossier_mercator"
                using (SqlConnection connection = new SqlConnection(dataSettings.DataConnectionString))
                {
                    try
                    {
                        connection.Open();

                        if (Mercator != null)
                        {
                            MercatorApi.Api.IsWeb = true;
                            string jsonCommandes = "";

                            jsonCommandes = WebService.Get(new UrlBuilder("GenericAttribute").And().FilterEq("KeyGroup", ENTITY).FilterEq("Key", KEY_SYNCED).FilterEq("Value", "0").BuildQuery());
                            //jsonCommandes = WebService.Get(WebServiceUrls.ORDER_UNSYNCED);

                            JToken[] commandes = ParserJSon.ParseResultToJTokenList(jsonCommandes);
                            if (commandes.Length == 0)
                            {
                                Program.WriteLine("Pas de nouvelles commandes");
                            }

                            foreach (JToken co in commandes)
                            {
                                try
                                {
                                    string orderResult = WebService.Get(urlBuilder
                                        .Id((int)co["EntityId"])
                                        .Expand("OrderItems")
                                        .BuildQuery());
                                        
                                    //JToken[] orderToken = ParserJSon.ParseResultToJTokenList(orderResult);
                                    JObject order = JObject.Parse(orderResult);

                                    //if (order.FirstOrDefault() != null)
                                    //{
                                    //    JToken c = orderToken.FirstOrDefault();
									if (((int)order["PaymentStatusId"] == 30 || (int)order["PaymentStatusId"] == 20) || (order["PaymentMethodSystemName"].ToString() == "Payments.CheckMoneyOrder"))
                                    {
	
	
	                                    JObject syncMarker = new JObject();
	
	                                    List<int> syncedOrderIds = new List<int>();
	
	                                    var items = (order["OrderItems"]).ToArray();
	                                    int id = int.Parse(order["CustomerId"].ToString());
	                                    //Get web client
	                                    SigCli sigCli = new SigCli();
	                                    bool exists = sigCli.ReadWhere(String.Format("C_ID_WEB = '{0}'", id));
	
	                                    if (!exists)
	                                    {
	                                        Program.log("Commande n°" + order["Id"].ToString() + " non synchronisée car le client " + id + " n'a pas été trouvé dans Mercator.");
	                                    }
	                                    else
	                                    {
	                                        //Get the journal in which the order has to be placed                                            
	                                        using (BillingEngine be = BillingEngine.InitNew(Billing.TypeVAEnum.V, 3, journal))
	                                        {
	                                            be.ApplyCustomerSupplier(sigCli.GetField("C_ID").Value.ToString());
	                                            if (be.PIEDS != null)
	
	                                            {
	                                                be.PIEDS["DATE"] = DateTime.Parse(order["CreatedOnUtc"].ToString());
	                                            }
	                                            else
	                                            {
	                                                throw new Exception("An error occured - please check that the journal exists");
	                                            }
	
	
	
	
	                                            foreach (JToken i in items)
	                                            {
                                                    JToken[] o = ParserJSon.ParseResultToJTokenList(WebService.Get(new UrlBuilder(WebApiEntities.GENERIC_ATTRIBUTE).FilterEq("KeyGroup", "Product").FilterEq("Key", "MercatorId").FilterEq("EntityId", i["ProductId"]).BuildQuery()));

                                                    string mercatorId = o.FirstOrDefault()?["Value"]?.ToString();

                                                    int index = be.AppendLine();

                                                    if (String.IsNullOrWhiteSpace(mercatorId) && useGenericArticle)
                                                    {
                                                        //Article generique
                                                        mercatorId = "NOPGENERIQ";
                                                        be.InsertItem(mercatorId, index, 1);

                                                        JObject product = JObject.Parse(WebService.Get(new UrlBuilder("Product").Id((int)i["ProductId"]).BuildQuery()));
                                                        be.LIGNES.Rows[index]["DESIGNATIO"] = product["Name"].ToString();
                                                        
                                                        be.LIGNES.Rows[index]["TAUX_TVA"] = Math.Floor(((i["UnitPriceInclTax"].Value<Decimal>() / i["UnitPriceExclTax"].Value<Decimal>()) - 1) * 100);
                                                        string[] attributes = i["AttributeDescription"].ToString().Split(new string[] { "<br/>", "<br />" }, StringSplitOptions.None);
                                                        if (attributes.Length > 0)
                                                        {
                                                            foreach (string s in attributes)
                                                            {
                                                                int attributeData = be.AppendLine();
                                                                be.LIGNES.Rows[attributeData]["DESIGNATIO"] = s;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //get from db stock using cle_1 then Add ID
                                                        if (!String.IsNullOrWhiteSpace(mercatorId))
                                                        {
                                                            be.InsertItem(mercatorId, index, 1);
                                                            //be.LIGNES.Rows[index]["PU"] = i["UnitPriceExclTax"].Value<Decimal>();
                                                        }
                                                        else
                                                        {
                                                            Program.log("Article does not exist in Mercator, you might want to activate the generic article option");
                                                        }
                                                    }


                                                    //Use price/vat used when the order was placed : might have been some changes in Mercator since then
                                                    be.LIGNES.Rows[index]["Q"] = i["Quantity"].Value<Double>() /** condit*/ ;
                                                    be.LIGNES.Rows[index]["PU"] = i["UnitPriceInclTax"].Value<Decimal>();
                                                    /* * (1 + (i["Remise"].Value<Decimal>() / 100));*/
                                                }
	
	                                            //Shipping tax
	                                            //TODO: faire un doc avec les étapes de l'installation + vérifications à faire 
	                                            //TODO: Verif: Présence d'un article frais de livraison dans le Mercator cible
	                                            if (Double.Parse(order["OrderShippingInclTax"].ToString()) > 0)
	                                            {
	                                                string fraislivraisonId = OptionsMercator.GetOptionValue("NOP_LIV_ID").ToString(); 
	
	                                                SigStock sigStock = new SigStock();
	                                                bool fraisLivFound = sigStock.ReadWhere(String.Format("S_CLE1 = '{0}'", fraislivraisonId));
	
	                                                if (fraisLivFound)
	                                                {
	                                                    int n = be.AppendLine();
	                                                    be.InsertItem(sigStock.GetField("S_ID").Value.ToString(), n, 1);
	                                                    be.LIGNES.Rows[n]["PU"] = Double.Parse(order["OrderShippingInclTax"].ToString()) / 1.21;
	                                                    be.LIGNES.Rows[n]["TAUX_TVA"] = 21;
	
	                                                }
	                                            }
	
	                                            //CODE PROMO SUR TOTAL COMMANDE
	                                            //TODO:CODE PROMO - UNCOMMENT IF NECESSARY
	                                            //if (Double.Parse(c["OrderDiscount"].ToString()) > 0)
	                                            //{
	                                            //    string codepromocommandecle1 = ConfigurationManager.AppSettings["CODEPROMOCOMMANDE"];
	                                            //    SigStock sigStock = new SigStock();
	                                            //    bool codePromoFound = sigStock.ReadWhere(String.Format("S_CLE1 = '{0}'", codepromocommandecle1));
	                                            //    if (codePromoFound)
	                                            //    {   
	                                            //        int n = be.AppendLine();
	                                            //        be.InsertItem(sigStock.GetField("S_ID").Value.ToString().S_ID.TrimEnd(), n, 1);
	                                            //        be.LIGNES.Rows[n]["PU"] = (Double.Parse(c["OrderDiscount"].ToString())) * -1;
	                                            //        be.LIGNES.Rows[n]["TAUX_TVA"] = 0;
	                                            //    }
	                                            //}
	
	                                            be.UpdateAmounts();
	
	                                            #region payments
	
	                                            //Check if order is paid : c["PaymentStatusId"] -> 10 pending, 30 paid
	                                            if (order["PaymentStatusId"].ToString() == "10")
	                                            {
	                                                switch (order["PaymentMethodSystemName"].ToString())
	                                                {
	                                                    case PaymentMethods.NOP_VIREMENT: be.PIEDS["TYP_PAIEM1"] = PaymentMethods.M_VIREMENT; break;
	                                                    case PaymentMethods.NOP_PAYPAL: be.PIEDS["TYP_PAIEM1"] = PaymentMethods.M_PAYPAL; break;
	                                                    default:
	                                                        be.PIEDS["TYP_PAIEM1"] = PaymentMethods.M_VIREMENT; break;
	                                                }
	                                            }
	                                            else if (order["PaymentStatusId"].ToString() == "30")
	                                            {
	                                                double tot = Convert.ToDouble(be.PIEDS["TOT_TTC_DV"].ToString());
	
	                                                switch (order["PaymentMethodSystemName"].ToString())
	                                                {
	                                                    case PaymentMethods.NOP_VIREMENT: be.PIEDS["TYP_PAIEM1"] = PaymentMethods.M_VIREMENT; break;	                                                    
	                                                    case PaymentMethods.NOP_PAYPAL: be.PIEDS["TYP_PAIEM1"] = PaymentMethods.M_PAYPAL; break;
	                                                    default:
	                                                        be.PIEDS["TYP_PAIEM1"] = PaymentMethods.M_VIREMENT; break;
	                                                }
	                                                //be.PIEDS["TYP_PAIEM1"] = 10;
	                                                be.PIEDS["TOT_PAIEM1"] = tot;
	                                                be.PIEDS["NET_DV"] = tot;
	                                                be.PIEDS["NET_FB"] = tot;
	                                            }
	
	                                            #endregion
	
	                                            #region shipment                                        
	                                            if (order["PickUpInStore"].ToString() == "True")
	                                            {
	                                                be.AppendLine();
	                                                int nAdresse = be.AppendLine();
	                                                be.LIGNES.Rows[nAdresse]["DESIGNATIO"] = "Retrait en magasin";
	                                            }
	                                            else
	                                            {
	                                                Address billingA = AddressConverter.ParseJsonToAddress(WebService.Get(new UrlBuilder("Address").Id((int)order["BillingAddressId"]).BuildQuery()));
	                                                Address shippingA = AddressConverter.ParseJsonToAddress(WebService.Get(new UrlBuilder("Address").Id((int)order["ShippingAddressId"]).BuildQuery()));
	                                                if (!billingA.Equals(shippingA))
	                                                {
	                                                    //be.ApplyCliLiv("ID_CLI_LIV");
	                                                    be.AppendLine();
	                                                    int nAdresse = be.AppendLine();
	                                                    be.LIGNES.Rows[nAdresse]["DESIGNATIO"] = "Adresse de livraison: ";
	                                                    int nN = be.AppendLine();
	                                                    be.LIGNES.Rows[nN]["DESIGNATIO"] = shippingA.FirstName + " " + shippingA.LastName;
	                                                    int nA = be.AppendLine();
	                                                    be.LIGNES.Rows[nA]["DESIGNATIO"] = shippingA.Street;
	                                                    int nP = be.AppendLine();
	                                                    be.LIGNES.Rows[nP]["DESIGNATIO"] = shippingA.ZipPostalCode;
	                                                    int nV = be.AppendLine();
	                                                    be.LIGNES.Rows[nV]["DESIGNATIO"] = String.Format("{0}, {1}", shippingA.City, shippingA.Country);
	                                                    int nPh = be.AppendLine();
	                                                    be.LIGNES.Rows[nPh]["DESIGNATIO"] = shippingA.PhoneNumber;
	
	                                                    //TODO: Mode de livraison UPS + suppléments
	
	                                                }
	                                            }
	
	                                            #endregion
	
	
	
	                                            #region CheckoutAttributes
	                                            //Récupère les attributs de commandes associés à la commande et les affiches dans les lignes_v
	                                            //if (order["CheckoutAttributeDescription"].ToString() != "" && order["CheckoutAttributeDescription"].ToString() != "null")
	                                            //{
	                                            //    Dictionary<string, string> attributes = extractAttributes(order["CheckoutAttributesXml"].ToString());
	
	                                            //    if (attributes.ContainsKey(ATTRIBUT_COMM))
	                                            //    {
	                                            //        if (attributes[ATTRIBUT_COMM] != "")
	                                            //        {
	                                            //            be.AppendLine();
	                                            //            int n = be.AppendLine();
	                                            //            be.LIGNES.Rows[n]["DESIGNATIO"] = "Commentaires: ";
	
	                                            //            string[] commLines = attributes[ATTRIBUT_COMM].Split(new string[] { "\r" }, StringSplitOptions.None);
	                                            //            foreach (string s in commLines)
	                                            //            {
	                                            //                int nLine = be.AppendLine();
	                                            //                be.LIGNES.Rows[nLine]["DESIGNATIO"] = s;	
	                                            //            }
	                                            //        }
	                                            //    }
	                                            //}
	                                            #endregion
	
	                                            be.PIEDS["ID_WEB"] = order["Id"].ToString();
	                                            be.PIEDS["REFERENCE"] = REF_WEB;	
	
	                                            if (be.Save())
	                                            {
	                                                be.Close();
	                                                syncMarker.Add("Value", "1");
	                                                //WebService.Patch(String.Format(WebServiceUrls.ORDER_ID, (int)c["Id"]), syncMarker.ToString());
	                                                WebService.Patch(String.Format(new UrlBuilder("GenericAttribute").Id((int)co["Id"]).BuildQuery()), syncMarker.ToString());
	                                            }
	                                            else
	                                            {
                                                    //Erreur                                                
                                                    Program.log(be.LastError);
	                                                return false;	
	                                            }	
	                                        }
                                            //}
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Program.log(e.Message);
                                    Program.log(e.StackTrace);
                                    return false;
                                }
                            }

                            //****** UPDATE STATUS ********//
                            Program.WriteLine("Updating Nop Order status...");
                            string ordersUncomplete = WebService.Get(urlBuilder.FilterEq("OrderStatusId",20).BuildQuery());
                            JToken[] orders = ParserJSon.ParseResultToJTokenList(ordersUncomplete);

                            if (orders.Count() > 0)
                            {
                                foreach (JToken o in orders)
                                {
                                    int oId = (int)o["Id"];
                                    
                                    string synced = WebService.Get(new UrlBuilder(WebApiEntities.GENERIC_ATTRIBUTE).FilterEq("KeyGroup",ENTITY).FilterEq("Key",KEY_SYNCED).FilterEq("EntityId",oId).BuildQuery());
                                    JToken[] syncedToken = ParserJSon.ParseResultToJTokenList(synced);

                                    if (syncedToken.Length == 0)
                                        continue;
                                    if (syncedToken[0]["Value"].ToString() == "0")
                                        continue;

                                    PiedsV piedsV = new PiedsV();
                                    bool exists = piedsV.Read(oId, "ID_WEB");

                                    if (exists)
                                    {
                                        JObject patch = new JObject();
                                        patch.Add("OrderStatusId", 30);
                                        if (Convert.ToInt32(piedsV.GetField("TYPE").Value) == 2)
                                        {
                                            patch.Add("PaymentStatusId", 30);
                                            patch.Add("ShippingStatusId", 30);
                                        }
                                        else
                                        if (Convert.ToInt32(piedsV.GetField("TYPE").Value) == 1)
                                        {
                                            patch.Add("OrderStatusId", 30);
                                            patch.Add("PaymentStatusId", 30);
                                            patch.Add("ShippingStatusId", 40);
                                        }

                                        WebService.Patch(urlBuilder.Id(oId).BuildQuery(), patch.ToString());
                                    }
                                }

                            }
                            Program.WriteLine("Updated");
                        }

                    }
                    catch (Exception e)
                    {
                        Program.log(e.Message);
                        Program.log(e.StackTrace);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
                //context.SaveChanges();

                return true;
            }
        }

        /// <summary>
        /// Extract checkout attributes from xml into a dictionnary (key, value)
        /// </summary>
        /// <param name="attributesString"></param>
        /// <returns></returns>
        private static Dictionary<string, string> extractAttributes(string attributesString)
        {
            Dictionary<string, string> attributesAndValue = new Dictionary<string, string>();

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(attributesString);

            string xpath = "Attributes/CheckoutAttribute";
            XmlNodeList nodes = xml.SelectNodes(xpath);

            foreach (XmlNode childrenNode in nodes)
            {
                string id = childrenNode.Attributes["ID"].Value;
                string value = childrenNode.InnerText;

                attributesAndValue.Add(id, value);
            }

            return attributesAndValue;
        }
    }
}
