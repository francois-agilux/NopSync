using BasicNopSync.Model.Mercator;
using BasicNopSync.OData;
using BasicNopSync.Utils;
using MercatorORM;
using Newtonsoft.Json.Linq;
using NopSync.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.Syncers.MercatorToNop
{
    public class SCatSyncer : Syncer
    {
        private const string formatCatStckType = "NOP_CSTCK{0}";
        private Dictionary<int, string> catStckTypes;

        private const string ENTITY = "SpecificationAttribute";
        private const string KEY_TYPE = "Type";

        public SCatSyncer() : base()
        {
            urlBuilder = new UrlBuilder(ENTITY);
        }

        public override bool Sync()
        {
            #region CAT_STCK types
            //Get all the cat stock types in app.config and sync them
            catStckTypes = new Dictionary<int, string>();
            bool keepGoing = false;
            int i = 1;
            do
            {
                string value = OptionsMercator.GetOptionValue(String.Format(formatCatStckType, i))?.ToString();
                keepGoing = !String.IsNullOrWhiteSpace(value);
                if (keepGoing)
                {
                    catStckTypes.Add(i, value.TrimEnd());
                }
                i++;
            } while (keepGoing);

            SyncCatStckTypes(catStckTypes);
            #endregion


            //Get Stock From Mercator where "Vente En Ligne" is setted
            using (SqlConnection connection = new SqlConnection(dataSettings.DataConnectionString))
            {
                connection.Open();
                //IEnumerable<CAT_STCK> catStckList = GetMercatorCATSTCK(connection);

                //Array of existing attSpecOp
                IDictionary<String, int> attSpecOp = GetAllNopSpecOpt();

                List<int> specsToKeep = new List<int>();

                //Sync options for every S_CAT{x}, x being the type id in catStckTypes
                foreach (var catStckTyp in catStckTypes)
                {
                    try
                    {
                        int nopId = 0;
                        var cstResult = ParserJSon.ParseResultToJTokenList(WebService.Get(urlBuilder.FilterEq("Name", catStckTyp.Value).BuildQuery()));
                        if (cstResult.Length > 0)
                        {
                            nopId = (int)cstResult.First()["Id"];
                            SyncSpecAttOps(attSpecOp, specsToKeep, catStckTyp.Key, nopId);
                        }

                    }
                    catch (Exception e)
                    {
                        Program.log(e);
                    }
                }

                //Exclude scat that aren't in Mercator
                Delete(attSpecOp, specsToKeep);


                connection.Close();
            }
            return true;
        }

        private void SyncCatStckTypes(Dictionary<int, string> catStckTypes)
        {
            //1. Get existings specification attributes
            //2. Check existences 
            //2b (opt) Check if names has changed
            //3. Add or udpates
            //4. Delete if necessary

            JToken[] specAtts = ParserJSon.ParseResultToJTokenList(WebService.Get(ENTITY));

            List<int> toKeep = new List<int>();

            foreach (KeyValuePair<int, string> s in catStckTypes)
            {
                JToken j = null;
                if (specAtts.Length > 0)
                    j = specAtts[s.Key - 1];
                if (j == null)
                {
                    //ADD
                    CAT_STCK cs = new CAT_STCK();
                    cs.NOM = s.Value;
                    cs.TYPE = s.Key;
                    string result = WebService.Post(ENTITY, ParserJSon.ParseScatToSpecificationAttribute(cs).ToString());
                    //JToken[] resultJ = ParserJSon.ParseResultToJTokenList(result);
                    JObject jsonResult = JObject.Parse(result);
                    toKeep.Add((int)jsonResult["Id"]);

                    string gaResult = WebService.Post(WebApiEntities.GENERIC_ATTRIBUTE, ParserJSon.GetGenericAttributeJSon((int)jsonResult["Id"], ENTITY, KEY_TYPE, cs.TYPE.ToString()).ToString());

                    Console.WriteLine("Cat Stock:" + cs.NOM + " inserted");
                }
                else
                {
                    if (j["Name"].ToString() != s.Value)
                    {
                        //Update
                        JObject jo = new JObject();
                        jo.Add("Name", s.Value);
                        //jo.Add("Type", s.Key);
                        WebService.Patch(urlBuilder.Id((int)j["Id"]).BuildQuery(), jo.ToString());
                    }
                    toKeep.Add((int)j["Id"]);
                }
            }

            IEnumerable<int> idsExisting = specAtts.Select(x => (int)x["Id"]);
            IEnumerable<int> toDelete = idsExisting.Except(toKeep);

            toDelete.ToList().ForEach(x => WebService.Delete(urlBuilder.Id(x).BuildQuery()));
        }

        public void SyncSpecAttOps(IDictionary<String, int> attSpecOp, List<int> specsToKeep, int catStockTypeId, int specAttId)
        {
            string sql = String.Format("SELECT nom,id_web, id FROM CAT_STCK WHERE TYPE = {0}", catStockTypeId, StockSyncer.JOIN_CLAUSE, StockSyncer.ALL_STOCKS_WHERE_CLAUSE);
            string sqlUpdateIdWeb = "UPDATE CAT_STCK SET ID_WEB = {0} WHERE ID = '{1}'";

            List<string> catStocks = new List<string>();

            SqlCommand sqlCommand = new SqlCommand(sql);

            try
            {
                DataTable dt = DBContextFactory.DBContext.Query(sqlCommand);

                foreach (DataRow dr in dt.Rows)
                {
                    string scatVal = Convert.ToString(dr["nom"]).TrimEnd();
                    string scatId = Convert.ToString(dr["id"]).TrimEnd();
                    int scatIdWeb = Convert.ToInt32(dr["id_web"]);

                    //Format to JSON
                    JObject json = ParserJSon.ParseSCATToSpecificationAttributeOption(specAttId, scatVal);
                    int specId = 0;

                    if (!attSpecOp.Any(x => x.Value == scatIdWeb))
                    {
                        //Send via POST (new ones)
                        string result = WebService.Post(WebApiEntities.SPEC_ATT_OPT, json);
                        JObject newValue = JObject.Parse(result);
                        specId = (int)newValue["Id"];
                        //Update
                        sqlUpdateIdWeb = String.Format(sqlUpdateIdWeb, specId, scatId);
                        dbContext.NonQuery(sqlUpdateIdWeb);
                    }
                    else
                    {
                        //Exists; we keep the id
                        specId = scatIdWeb;
                        JObject nameUpdate = new JObject();
                        nameUpdate.Add("Name", scatVal);
                        WebService.Patch(new UrlBuilder("SpecificationAttributeOption").Id(scatIdWeb).BuildQuery(), nameUpdate.ToString());

                    }
                    //Store cat to keep 
                    specsToKeep.Add(specId);
                }

            }
            catch (Exception e)
            {
                Program.log(e);
            }
            
        }


        /// <summary>
        /// Get all SpecAttributeOptions
        /// <String, String> => <Name+Type, Id>
        /// </summary>
        /// <returns>A Dictionnary of strings : Key = Name+TypeId, Value = specId</returns>
        public static Dictionary<string, int> GetAllNopSpecOpt()
        {
            Dictionary<string, int> nopSpecOpt = new Dictionary<string, int>();
            string json = WebService.Get(WebApiEntities.SPEC_ATT_OPT);
            var values = ParserJSon.ParseResultToJTokenList(json);

            foreach (JToken v in values)
            {
                //Name+typeID, cause 2 equal names can be in 2 differents cat
                try
                {

                    nopSpecOpt.Add(v["Name"].ToString() + v["SpecificationAttributeId"], (int)v["Id"]);
                }
                catch (Exception e)
                {
                    Program.log(e.Message);
                    Program.log(String.Format("Valeur déjà présente : {0}{1}", v["Name"].ToString(), v["SpecificationAttributeId"].ToString()));
                    Program.log(e.StackTrace);
                }

            }

            return nopSpecOpt;
        }

        /// <summary>
        /// Delete scat
        /// </summary>
        /// <param name="attSpecOp"></param>
        /// <param name="specToKeep"></param>
        private void Delete(IDictionary<string, int> attSpecOp, List<int> specToKeep)
        {
            //Exclude scat that aren't in Mercator
            List<int> existings = new List<int>();

            existings = attSpecOp.Select(x => x.Value).ToList();

            List<int> toDelete = existings.Except(specToKeep).ToList();
            toDelete = toDelete.ToList<int>();
            if (toDelete.Count() > 0)
            {
                toDelete.ForEach(x => WebService.Delete(new UrlBuilder("SpecificationAttributeOption").Id(x).BuildQuery()));
            }
        }



    }
}
