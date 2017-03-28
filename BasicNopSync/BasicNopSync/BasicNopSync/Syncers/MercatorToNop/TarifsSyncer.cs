//using Newtonsoft.Json.Linq;
//using NopSync.Datas.Mercator;
//using NopSync.Utils;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BasicNopSync.Syncers.MercatorToNop
//{
//    class Tarifs
//    {
//        private static readonly string TARIFSWS = "CustomerRole";

//        //For now : Tarifs are synced as CustomerRoles
//        //When the stocks are syncing : Tarifs are mapped to Products as TierPrice 
//        //Tier Prices are displayed only when a customer has the appropriate role

//        //By default the displayed price is the one wanted by the client (S_TARHT_2, to be modified in ParseJson.cs). 

//        /// <summary>
//        /// Sync Mercator's tarifs as Nop's Customer Role
//        /// 
//        /// NOTE: To automatically assign a Customer Role to a customer on registration
//        ///     NopCommerce's file : Libraries\Nop.Services\Customers\CustomerRegistrationService.cs
//        ///     Method : RegisterCustomer
//        /// </summary>
//        /// <returns></returns>
//        public static bool SyncTarifs()
//        {

//            using (var context = new MERCATORDE_BRY_S_AEntities())
//            {
//                var tarifs = from p in context.TARIFS
//                             select p;

//                IDictionary<String, String> existingsRoles = getExistingCustomerRoles(tarifs);

//                List<String> rolesToKeep = new List<string>();

//                //ADD/EDIT Tarifs
//                foreach (TARIFS t in tarifs)
//                {
//                    //Format to JSON
//                    string json = ParserJSon.ParseTarifToNopCustomerRole(t.NOM.TrimEnd(), WebServiceUrls.TARIF_ + t.NUMERO.ToString());
//                    string roleID = "";
//                    try
//                    {
//                        if (existingsRoles == null)
//                        {
//                            //Send via POST (new ones)
//                            string result = WebService.Post(TARIFSWS, json);
//                            JObject newRole = JObject.Parse(result);
//                            roleID = newRole["Id"].ToString();
//                        }
//                        else
//                        {
//                            //ADD Tarif TO NOP                        
//                            if (existingsRoles.ContainsKey(t.NUMERO.ToString()))
//                            {
//                                //Send via PATCH/PUT (existing ones)                                
//                                roleID = existingsRoles[t.NUMERO.ToString()];
//                                WebService.Patch(TARIFSWS + "(" + roleID + ")", json);


//                                //No need to add url record, it will always be the same (Mercator's S_ID)

//                            }
//                            else
//                            {
//                                //Send via POST (new ones)
//                                string result = WebService.Post(TARIFSWS, json);
//                                JObject newRole = JObject.Parse(result);
//                                roleID = newRole["Id"].ToString();
//                            }
//                        }
//                        //Store cat to keep 
//                        rolesToKeep.Add(roleID);
//                    }
//                    catch (Exception e)
//                    {
//                        Program.log("Error while syncing a tarif, role Id: " + roleID);
//                        Program.log(e.Message);
//                        Program.log(e.StackTrace);
//                    }
//                }

//                //Exclude tarifs that aren't in Mercator
//                List<string> existings = new List<string>();

//                JObject nopCustomerRoles = JObject.Parse(WebService.Get(TARIFSWS));
//                var values = nopCustomerRoles["value"].ToArray();
//                foreach (JToken v in values)
//                {
//                    if (v["IsSystemRole"].ToString() == "False")
//                    {
//                        existings.Add(v["Id"].ToString());
//                    }
//                }


//                var toDelete = existings.Except(rolesToKeep);
//                toDelete = toDelete.ToList();
//                if (toDelete.Count() > 0)
//                {
//                    foreach (string s in toDelete)
//                    {
//                        //Deactivate the deleted category and the corresponding urlRecord
//                        WebService.Delete(TARIFSWS + "(" + s + ")");
//                    }
//                }
//            }

//            return true;
//        }

//        /// <summary>
//        /// Return existing customer roles that are not system roles (i.e tarifs added by us)
//        /// </summary>
//        /// <param name="tarifs"></param>
//        /// <returns>if no customer roles that aren't system role return null, otherwise returns existing customer roles</returns>
//        private static IDictionary<String, String> getExistingCustomerRoles(IQueryable<TARIFS> tarifs)
//        {
//            Dictionary<String, String> existingRoles = new Dictionary<string, string>();
//            string json = WebService.Get("CustomerRole?$filter=IsSystemRole+eq+false");
//            JObject roles = JObject.Parse(json);
//            var values = roles["value"].ToArray();

//            if (values.Count() > 0)
//            {
//                foreach (TARIFS t in tarifs)
//                {
//                    foreach (JToken v in values)
//                    {
//                        if (WebServiceUrls.TARIF_ + t.NUMERO.ToString() == v["SystemName"].ToString())
//                        {
//                            //r.ID => urlRecordId       
//                            existingRoles.Add(t.NUMERO.ToString(), v["Id"].ToString());
//                        }
//                    }
//                }

//                return existingRoles;
//            }
//            else
//            {
//                return null;
//            }


//        }
//    }

//}
