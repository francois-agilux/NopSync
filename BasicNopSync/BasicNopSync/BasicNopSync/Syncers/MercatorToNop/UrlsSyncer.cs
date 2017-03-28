using BasicNopSync.Model;
using BasicNopSync.Model.Mercator;
using BasicNopSync.Model.NopCommerce;
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

namespace BasicNopSync.Syncers.MercatorToNop
{
    //BasicNopSync URL Format: {Rayon}-{S_MODELE}-{S_ID}
    public class UrlsSyncer : Syncer
    {
        private const string ENTITY = "UrlRecord";
        
        public UrlsSyncer() : base() {            
            OptionsMercator = new OptionsMercator();
            urlBuilder = new UrlBuilder(ENTITY);
        }


        //Update every product url and sync new ones
        public override bool Sync()
        {
            using (SqlConnection connection = new SqlConnection(dataSettings.DataConnectionString))
            {
                try
                {
                    List<STOCK> prodList = new List<STOCK>();

                    connection.Open();

                    string whereClause = String.Format(StockSyncer.WHERE_CLAUSE, -1);

                    prodList = GetMercatorProductListWithUrlDatas(connection, whereClause).ToList();


                    JToken[] products = ParserJSon.ParseResultToJTokenList(WebService.Get(new UrlBuilder("Product")
                                                                                            .FilterEq("Published", true)
                                                                                            .Select("Id", "MercatorId")
                                                                                            .BuildQuery()));

                    foreach (JToken j in products)
                    {
                        try
                        {
                            STOCK s = prodList.Where(x => x.S_ID.TrimEnd() == j["MercatorId"].ToString().TrimEnd()).FirstOrDefault();
                            if (s != null)
                            {
                                string newUrl = BuildProductUrl(s, connection);
                                JToken[] urls = ParserJSon.ParseResultToJTokenList(WebService.Get(new UrlBuilder("UrlRecord")
                                                                .FilterEq("EntityId", (int)j["Id"])
                                                                .FilterEq("EntityName", "Product")
                                                                .BuildQuery()));
                                if (urls.Length > 0)
                                {
                                    int urlId = (int)urls.FirstOrDefault()["Id"];

                                    JObject newUrlObject = new JObject();
                                    newUrlObject.Add("Slug", newUrl);

                                    WebService.Patch(new UrlBuilder("UrlRecord").Id(urlId).BuildQuery(), newUrlObject.ToString());
                                }

                            }
                        }
                        catch (Exception e)
                        {
                            Program.log(e.Message);
                            Program.log(e.StackTrace);
                            continue;
                        }
                    }

                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
                finally
                {
                    if(connection != null)
                        connection.Close();
                }
            }

        }

        /// <summary>
        /// Get a stock list with only datas useful to build the url:
        /// S_ID, S_MODELE, S_ID_RAYON, S_ID_FAMIL, S_ID_SSFAM
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="whereClause"></param>
        /// <returns></returns>
        private IEnumerable<STOCK> GetMercatorProductListWithUrlDatas(SqlConnection connection, string whereClause = "")
        {

            string sql = @"SELECT s_id, s_modele, s_id_rayon, s_id_famil, s_id_ssfam
                            FROM stock" + (whereClause.Length > 0 ? " WHERE " + whereClause : "");
            List<STOCK> prodList = new List<STOCK>();
            using (SqlCommand sqlCommand = new SqlCommand(sql))
            //using (SqlDataReader reader = sqlCommand.ExecuteReader())
            {
                try
                {
                    DataTable dt = DBContextFactory.DBContext.Query(sqlCommand);

                    foreach (DataRow dr in dt.Rows)
                    {
                        STOCK prod = new STOCK();

                        prod.S_ID = Convert.ToString(dr["s_id"]).TrimEnd();
                        prod.S_MODELE = Convert.ToString(dr["s_modele"]).TrimEnd();                        
                        prod.S_ID_RAYON = Convert.ToString(dr["s_id_rayon"]).TrimEnd();
                        prod.S_ID_FAMIL = Convert.ToString(dr["s_id_famil"]).TrimEnd();
                        prod.S_ID_SSFAM = Convert.ToString(dr["s_id_ssfam"]).TrimEnd();                        

                        prodList.Add(prod);
                    }

                }
                catch (Exception e)
                {
                    Program.log(e.Message);
                    Program.log(e.StackTrace);
                }
                return prodList;
            }
        }

        public static string BuildCategoryUrl(RFS rfs)
        {
            string finalUrl = "";
            List<string> urlMembers = new List<string>();

            string tempUrl = rfs.ID.TrimEnd()+"-"+rfs.NOM.TrimEnd();
            finalUrl = Utils.Utils.GetFriendlyUrl(tempUrl);
            return finalUrl;
        }

        public static string BuildCategoryUrl(Category cat)
        {
            string finalUrl = "";
            List<string> urlMembers = new List<string>();

            string tempUrl = cat.MercatorId.TrimEnd() + "-" + cat.Name.TrimEnd();
            finalUrl = Utils.Utils.GetFriendlyUrl(tempUrl);
            return finalUrl;
        }

        /// <summary>
        /// Url format : {Rayon}-{Modele}-{Id}
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string BuildProductUrl(STOCK s, SqlConnection connection)
        {
            string finalUrl = "";
            List<string> urlMembers = new List<string>();

            string firstMember = RFS.GetRFSNameById(RFS.TYPE.RAYON, s.S_ID_RAYON, connection);
            if (!String.IsNullOrWhiteSpace(firstMember))
                urlMembers.Add(firstMember);            

            string secondMember = s.S_MODELE.TrimEnd();
            if (!String.IsNullOrWhiteSpace(secondMember))
                urlMembers.Add(secondMember);

            string thirdMember = s.S_ID.TrimEnd();
            urlMembers.Add(thirdMember);

            foreach (string u in urlMembers)
            {
                if (u == urlMembers.First())
                    finalUrl = u;
                else
                    if (!String.IsNullOrWhiteSpace(u))
                    finalUrl += " " + u;
            }

            finalUrl = Utils.Utils.GetFriendlyUrl(finalUrl);

            return finalUrl;
        }
    }
}
