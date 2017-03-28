using BasicNopSync;
using BasicNopSync.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopSync.Utils
{
    class AclRecords
    {
        private static readonly string wsCustomerRoleMapping = "CustomerRole";
        private static readonly string wsAclRecordMapping = "AclRecord";

        public static void addAclMapping(int entityId, string entityNameMapping)
        {
            //ADD ACL RECORD
            var allowedRoles = ConfigurationManager.AppSettings["CRRestrictedAccessAllowed"].Split(';');
            foreach (String s in allowedRoles)
            {
                JObject ids = JObject.Parse(WebService.Get(wsCustomerRoleMapping + "?$filter=Name+eq+'" + s + "'&select=Id"));
                var idsValue = ids["value"].ToArray();
                if (idsValue.Count() > 0)
                {
                    foreach (JToken id in idsValue)
                    {
                        JObject aclJson = ParserJSon.parseNewAclToJson(entityId, entityNameMapping, int.Parse(id["Id"].ToString()));
                        WebService.Post(wsAclRecordMapping, aclJson.ToString());
                    }
                }
            }
        }

        //Used for products
        public static void addAclMapping(int entityId, string entityNameMapping, JArray ja)
        {

            //ADD ACL RECORD
            var allowedRoles = ConfigurationManager.AppSettings["CRRestrictedAccessAllowed"].Split(';');
            JObject jo = new JObject();
            foreach (String s in allowedRoles)
            {
                JObject ids = JObject.Parse(WebService.Get(wsCustomerRoleMapping + "?$filter=Name+eq+'" + s + "'&select=Id"));
                var idsValue = ids["value"].ToArray();
                if (idsValue.Count() > 0)
                {
                    foreach (JToken id in idsValue)
                    {
                        ja.Add(ParserJSon.parseNewAclToJson(entityId, entityNameMapping, int.Parse(id["Id"].ToString())));
                    }
                }
            }

        }
    }
}
