using BasicNopSync.OData;
using BasicNopSync.Utils;
using MercatorORM;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.Model.Mercator
{
    /// <summary>
    /// Represent what we need to sync RSF from Mercator
    /// 
    /// Level :
    ///     Rayons       => 0
    ///     Famille      => 1
    ///     Sous-Famille =>2
    /// </summary>
    public class RFS
    {
        public string ID { get; set; }
        public string NOM { get; set; }
        public string ParentMercatorID { get; set; }
        //public string Level { get; set; }

        public const string RAYONS = "RAYONS";
        public const string FAMILLES = "FAMILLES";
        public const string SS_FAMIL = "SS_FAMIL";

        public enum TYPE { RAYON, FAMILLE, SSFAMIL }

        public RFS(string id="", string name="", string ParentID=""/*, string Level*/)
        {
            this.ID = id;
            this.NOM = name;
            this.ParentMercatorID = ParentID;
            //this.Level = Level;
        }

        /// <summary>
        /// Return product of this category and its child categories
        /// </summary>
        /// <param name="catId"></param>
        /// <returns></returns>
        public static List<JToken> GetAllProducts(int catId)
        {
            List<JToken> products = new List<JToken>();

            ///Category?$filter=ParentCategoryId+eq+64
            //Get this cat Products
            string catJson = WebService.Get(new UrlBuilder("ProductCategory").FilterEq("CategoryId",catId).BuildQuery());
            JObject catO = JObject.Parse(catJson);
            var values = catO["value"].ToArray();
            //Products exists
            if (values.Count() > 0)
            {
                foreach (JToken v in values)
                {
                    string proJson = WebService.Get(new UrlBuilder("Product").FilterEq("Id",(int)v["ProductId"]).And().FilterEq("Published",true).BuildQuery());
                    JObject proO = JObject.Parse(proJson);
                    var proValues = proO["value"].ToArray();
                    if (proValues.Count() > 0)
                    {
                        //Product is not published
                        products.Add(proValues[0]);
                    }
                }
            }

            string childJson = WebService.Get(new UrlBuilder("Category").FilterEq("ParentCategory",catId).BuildQuery());
            JObject childsO = JObject.Parse(childJson);
            var childs = childsO["value"].ToArray();

            foreach (JToken c in childs)
            {
                products.AddRange(GetAllProducts((int)c["Id"]));
            }

            return products;

        }


        public static string GetCombinedCategoryName(int id)
        {
            string result = WebService.Get(new UrlBuilder("Category").Id(id).Select("Name","ParentCategoryId").BuildQuery());
            JObject resultO = JObject.Parse(result);
            if (resultO["ParentCategoryId"].ToString() == "0")
            {
                string label = resultO["Name"].ToString().TrimEnd();
                return label;
            }
            else
            {
                return GetCombinedCategoryName((int)resultO["ParentCategoryId"]) + "/" + resultO["Name"].ToString().TrimEnd();
            }

        }

        /// <summary>
        /// Return the name of the rfs by its id and type
        /// Type:
        /// 1 => Rayon
        /// 2 => Famille
        /// 3 => Sous-famille
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetRFSNameById(RFS.TYPE t, string id, SqlConnection connection)
        {
            string trimId = id.TrimEnd();

            string sql = "select nom from {0} where ID = '{1}'";

            string type = GetRFSType(t);
            if (type != String.Empty)
            {
                string sqlCommand = String.Format(sql, type, trimId);

                DataTable dt = DBContextFactory.DBContext.Query(sqlCommand);

                if (dt.Rows.Count > 0)
                    return Convert.ToString(dt.Rows[0]["nom"]).TrimEnd();                
            }

            return String.Empty;
        }

        /// <summary>
        /// Get the type of RFS
        /// 0 => Rayons
        /// 1 => Familles
        /// 2 => Ss_famil        
        /// </summary>
        /// <param name="type"></param>
        /// <returns>The RFS matching the type, empty string if type out of range</returns>
        private static string GetRFSType(RFS.TYPE type)
        {
            switch (type)
            {
                case TYPE.RAYON: return RAYONS;
                case TYPE.FAMILLE: return FAMILLES;
                case TYPE.SSFAMIL: return SS_FAMIL;
                default: return String.Empty;
            }
        }
    }
}
