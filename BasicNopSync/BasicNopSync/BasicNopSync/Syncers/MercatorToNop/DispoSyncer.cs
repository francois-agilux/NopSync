using BasicNopSync.Model;
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
    public class DispoSyncer : Syncer
    {   

        public DispoSyncer() : base() {
            urlBuilder = new UrlBuilder("Product");
            OptionsMercator = new OptionsMercator();
        }

        public override bool Sync()
        {
            //Get Stock From Mercator where "Vente En Ligne" is set
            using (SqlConnection connection = new SqlConnection(dataSettings.DataConnectionString))
            {
                try
                {
                    IDictionary<string, int> prodStockDico;

                    connection.Open();

                    string whereClause = String.Format(StockSyncer.WHERE_CLAUSE, -1);

                    prodStockDico = GetMercatorProductListWithDispoDatas(connection, whereClause, StockSyncer.JOIN_CLAUSE).ToList().ToDictionary(y =>y.S_ID,y=>y.S_DISPO);
                    
                    //Recupère les sku et leurs stocks respectifs
                    JToken[] skuStocks = ParserJSon.ParseResultToJTokenList(WebService.Get(urlBuilder.Select("Sku","Id","StockQuantity").BuildQuery()));
                    IDictionary<string, string> stocksToUpdateNop = skuStocks.ToDictionary(y => y["Sku"].ToString().TrimEnd(), y => (int)y["Id"] + "+" + (int)y["StockQuantity"]);

                    int stockUpdateCount = 0;
                    //Si stocks différents entre Mercator et site on update
                    if (stocksToUpdateNop.Count > 0 && prodStockDico.Count > 0)
                    {
                        foreach (var s in prodStockDico)
                        {
                            string stockValue = s.Value.ToString();

                            if (stocksToUpdateNop.ContainsKey(s.Key))
                            {
                                if (stockValue != stocksToUpdateNop[s.Key].Split('+').LastOrDefault())
                                {
                                    int pId = int.Parse(stocksToUpdateNop[s.Key].Split('+').FirstOrDefault());
                                    //Console.WriteLine(s.Key + " updated");                                    
                                    JObject request = new JObject();
                                    request.Add("StockQuantity", stockValue);
                                    WebService.Patch(urlBuilder.Id(pId).BuildQuery(), request.ToString());
                                    stockUpdateCount++;
                                }
                            }
                        }

                    }
                    Console.WriteLine(String.Format("Stocks de {0} produits mis à jour.",stockUpdateCount));

                    return true;
                }
                catch (Exception e)
                {
                    Program.log(e.Message);
                    Program.log(e.StackTrace);
                    return false;
                }
                finally
                {
                    if (connection != null)
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
        private IEnumerable<STOCK> GetMercatorProductListWithDispoDatas(SqlConnection connection, string whereClause = "", string joinClause = "")
        {

            string sql = @"SELECT s_id,  dispo.dispo as stck FROM stock" + (joinClause.Length > 0 ? " JOIN " + joinClause : "") + (whereClause.Length > 0 ? " WHERE " + whereClause : "");
            List<STOCK> prodList = new List<STOCK>();
            using (SqlCommand sqlCommand = new SqlCommand(sql))            
            {
                try
                {
                    DataTable dt = DBContextFactory.DBContext.Query(sqlCommand);

                    foreach (DataRow dr in dt.Rows)
                    {
                        STOCK prod = new STOCK();

                        prod.S_ID = Convert.ToString(dr["s_id"]).TrimEnd();
                        prod.S_DISPO = Convert.ToInt32(dr["stck"]);                       

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
    }

    class ProductDispo
    {
        public string Sku { get; set; }
        public int Stock { get; set; }
    }
}
