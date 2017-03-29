using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using BasicNopSync.Utils;
using BasicNopSync.Model.NopCommerce;
using BasicNopSync.Model.Mercator;
using BasicNopSync.Syncers.MercatorToNop;
using BasicNopSync.OData;
using MercatorORM;

namespace BasicNopSync.Syncers.MercatorToNop
{
    public class RFSSyncer : Syncer
    {   
        public const string ENTITY = "Category";
        public const string KEY_MERCATORID = "MercatorId";
        private bool useGenericArticle = false;

        public RFSSyncer() : base()
        {
            urlBuilder = new UrlBuilder(ENTITY);
            OptionsMercator om = new OptionsMercator();
            useGenericArticle = om.GetOptionValue("NOP_GEN_A")?.ToString()?.TrimEnd() == "1";
        }


        public override bool Sync()
        {
            if (Sync(RFS.TYPE.RAYON))
            {
                if (Sync(RFS.TYPE.FAMILLE))
                {
                    if (Sync(RFS.TYPE.SSFAMIL))
                    {
                        Program.WriteLine("RFS Sync OK");
                        return true;
                    }
                    else
                    {
                        Program.log("Sous-Familles - Sync Failed");
                        return false;
                    }
                }
                else
                {
                    Program.log("Familles - Sync Failed");
                    return false;
                }
            }
            else
            {
                Program.log("Rayons - Sync Failed");
                return false;
            }
        }

        /// <summary>
        /// Synchronize RFS with Nop's categories
        /// Sync E-Commerce first (access to everyone)
        /// Then Sync Restricted access
        /// </summary>
        /// <note>RFS to excludes are to add in appconfig file</note>
        /// <param name="niveau"></param>
        /// <returns></returns>
        private bool Sync(RFS.TYPE niveau)
        {
            //Get Rayons From Mercator
            using (SqlConnection connection = new SqlConnection(dataSettings.DataConnectionString))
            {

                IEnumerable<Category> categories = new List<Category>();

                //map mercator's ids with nop commerce's existing ids
                IDictionary<string, int> mapMIdNopId = GetMapIdNopId();

                categories = GetRFSAsCategories(connection, mapMIdNopId, niveau);

                //ID Mercator (url) + id category
                //rayonID => urlRecordID
                //rayonID + "SLUG" => catID
                //var existingUrlRecordsSlugAndCatId = GetExistingUrlRecords(categories);
                

                List<int> categoriesToKeep = new List<int>();

                //ADD/EDIT RAYONS
                foreach (Category category in categories)
                {
                    try
                    {
                        //Format to JSON
                        JObject json = ParserJSon.ParseRFSToNopCategory(category);
                        int catId = 0;

                        //ADD CATEGORY TO NOP                            
                        if (mapMIdNopId.ContainsKey(category.MercatorId))
                        {
                            try
                            {
                                catId = mapMIdNopId[category.MercatorId];

                                //Send via PATCH/PUT (existing ones)                                                           
                                WebService.Patch(urlBuilder.Id(catId).BuildQuery(), json.ToString());

                                //Activate url (in case it was deactivated)
                                JObject urlActivate = new JObject();
                                urlActivate.Add("IsActive", true);
                                JObject urls = JObject.Parse(WebService.Get(new UrlBuilder("UrlRecord").And().FilterEq("EntityId", catId).FilterEq("EntityName", ENTITY).BuildQuery()));

                                var urlsValues = urls["value"].ToArray();
                                if (urlsValues.Count() > 0)
                                    WebService.Patch(new UrlBuilder("UrlRecord").Id((int)urlsValues[0]["Id"]).BuildQuery(), urlActivate.ToString());

                            }
                            catch (Exception e)
                            {
                                Program.log("Exception lors de la mise à jour de la categorie : " + category.Name.TrimEnd());
                                Program.log("NopId de la catégorie :" + catId);
                                Program.log(e.Message);
                                Program.log(e.StackTrace);
                            }
                        }
                        else
                        {
                            //Send via POST (new ones)

                            string result = WebService.Post(ENTITY, json.ToString());
                            JObject newCat = JObject.Parse(result);
                            catId = (int)newCat["Id"];

                            //ADD URL RECORD
                            JObject urlJson = ParserJSon.GetUrlRecordJson(ENTITY, catId, UrlsSyncer.BuildCategoryUrl(category));
                            WebService.Post(WebApiEntities.URL_RECORD, urlJson.ToString());

                            //ADD GENERIC ATTRIBUTE MERCATOR ID
                            JObject genericAttributeJSon = ParserJSon.GetGenericAttributeJSon(catId, ENTITY, KEY_MERCATORID, category.MercatorId);
                            WebService.Post(WebApiEntities.GENERIC_ATTRIBUTE, genericAttributeJSon.ToString());

                            Console.WriteLine(category.Name + " inserted");

                        }

                        //Store cat to keep 
                        categoriesToKeep.Add(catId);

                    }
                    catch (Exception e)
                    {
                        Program.log(String.Format("Error syncing category : {0}.", category.Name));
                        Program.log(e.Message);
                        Program.log(e.StackTrace);
                    }
                }

                //Delete categories that aren't in Mercator
                List<int> existings = new List<int>();

                List<JToken> values = niveau == RFS.TYPE.RAYON ? JObject.Parse(WebService.Get(urlBuilder.And().FilterEq("Published",true).FilterEq("ParentCategoryId",0).BuildQuery()))["value"].ToList()
                    : (niveau == RFS.TYPE.FAMILLE ? GetNopFamilles() : GetNopSousFamilles());

                foreach (JToken v in values)
                {
                    if(!(bool)v["Deleted"])
                        existings.Add((int)v["Id"]);
                }

                var toDelete = existings.Except(categoriesToKeep);
                toDelete = toDelete.ToList();

                JObject deletedJson = new JObject();
                deletedJson.Add("Deleted", true);

                JObject urlDeactivated = new JObject();
                urlDeactivated.Add("IsActive", false);

                foreach (int i in toDelete)
                {
                    //If cat has a genericattribute with the lvl, it comes from mercator. If it does not come from Mercator, don't delete it
                    if (useGenericArticle)
                    {   
                        JToken[] fromMercator = ParserJSon.ParseResultToJTokenList(WebService.Get(new UrlBuilder(WebApiEntities.GENERIC_ATTRIBUTE).FilterEq("KeyGroup",ENTITY).FilterEq("Key",KEY_MERCATORID).FilterEq("EntityId",i).BuildQuery()));
                        if (fromMercator.Length == 0)
                            continue;
                    }
                    //Deactivate the deleted category and delete corresponding urlRecord
                    //Instead of published : false => Delete : false ou vraiment la supprimer ? 
                    //WebService.Patch(wsCategoryMapping + "(" + s + ")", "{\"Published\":false}");
                    JObject urls = JObject.Parse(WebService.Get(new UrlBuilder("UrlRecord").And().FilterEq("EntityId",i).FilterEq("EntityName","Category").BuildQuery()));
                    var urlsValues = urls["value"].ToArray();
                    //WebService.Patch("UrlRecord("+urlsValues[0]["Id"].ToString()+")","{\"IsActive\":false}");
                    
                    if (urlsValues.Count() > 0)
                    {
                        Program.log(i + " deleted");
                        //deleteResult = WebService.Delete(String.Format(WebServiceUrls.URL_RECORD_ID, (int)urlsValues[0]["Id"]));
                        WebService.Patch(new UrlBuilder("UrlRecord").Id((int)urlsValues[0]["Id"]).BuildQuery(), urlDeactivated.ToString());
                    }
                    //deleteResult = WebService.Delete(String.Format(WebServiceUrls.CATEGORY_ID, i));
                    WebService.Patch(urlBuilder.Id(i).BuildQuery(), deletedJson.ToString());
                }
            }

            return true;

        }

        #region Get Mercator datas       

        /// <summary>
        /// Get the RFS to be synced from Mercator and transorm them into Categories
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="MercIdNopId"></param>
        /// <param name="niveau"></param>
        /// <returns></returns>
        private static IEnumerable<Category> GetRFSAsCategories(SqlConnection connection, IDictionary<string,int> MercIdNopId, RFS.TYPE niveau)
        {
            connection.Open();

            string sqlRayon = @"SELECT id, nom FROM RAYONS WHERE id in (SELECT distinct(s_id_rayon) FROM stock where S_WEB = 1 and S_SOMMEIL = 0 and S_ID_RAYON <> '' and S_MODELE <> '')";
            string sqlFam = @"SELECT id, nom, id_rayon FROM FAMILLES WHERE id in (SELECT distinct(s_id_famil) FROM stock where S_WEB = 1 and S_SOMMEIL = 0 and S_ID_RAYON <> '' and S_MODELE <> '')";
            string sqlSsFam = @"SELECT id, nom, id_famille FROM SS_FAMIL WHERE id in (SELECT distinct(s_id_ssfam) FROM stock where S_WEB = 1 and S_SOMMEIL = 0 and S_ID_RAYON <> '' and S_MODELE <> '')";

            string sql= "";
            switch (niveau)
            {
                case RFS.TYPE.RAYON: sql = sqlRayon; break;
                case RFS.TYPE.FAMILLE: sql = sqlFam; break;
                case RFS.TYPE.SSFAMIL: sql = sqlSsFam; break;
                default: return null;

            }

            List<Category> catList = new List<Category>();
            using (SqlCommand sqlCommand = new SqlCommand(sql, connection))
            using (SqlDataReader reader = sqlCommand.ExecuteReader())
            {
                try
                {
                    while (reader.Read())
                    {   
                        int parentCategoryId = 0;
                        bool exists = false;
                        if (niveau == RFS.TYPE.FAMILLE)
                        {
                            exists = MercIdNopId.TryGetValue(reader["id_rayon"].ToString().Trim(), out parentCategoryId);                            
                        }
                        else if (niveau == RFS.TYPE.SSFAMIL)
                        {
                            exists = MercIdNopId.TryGetValue(reader["id_famille"].ToString().Trim(), out parentCategoryId);                            
                        }
                        
                        Category cat = new Category(
                            reader["nom"].ToString().Trim(),
                            parentCategoryId,
                            reader["id"].ToString().Trim()
                            );

                        if (!exists && niveau != RFS.TYPE.RAYON)
                        {
                            Program.log("Clé non trouvée.");
                            Program.log("Parent de la catégorie: " + cat.Name + " non trouvé");
                        }

                        catList.Add(cat);

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

                return catList;
            }
            
        }
        

        #endregion

        #region Get Existing Entities

        /// <summary>
        /// Get existing url records
        /// </summary>
        /// <param name="rayons"></param>
        /// <returns></returns>
        //private static IDictionary<String, int> GetExistingUrlRecords(IEnumerable<Category> categories) 
        //{
        //    Dictionary<String, int> existingIds = new Dictionary<string, int>();
        //    string json = WebService.Get(WebServiceUrls.GET_URL_RECORDS_ACTIVE_CATEGORIES);
        //    JObject urls = JObject.Parse(json);
        //    var values = urls["value"].ToArray();

        //    foreach(Category c in categories)
        //    {
        //        foreach(JToken v in values)
        //        {
        //            if(c.MercatorId == v["Slug"].ToString())
        //            {                 
        //                //r.ID => urlRecordId       
        //                existingIds.Add(c.MercatorId, (int)v["Id"]);
        //                //r.IDSlug =>catId
        //                existingIds.Add(c.MercatorId + SLUG, (int)v["EntityId"]);
        //            }
        //        }
        //    }

        //    return existingIds;

        //}

        /// <summary>
        /// Get a mapping of Mercator's RFS IDs and Nop's CatIds
        /// </summary>
        /// <returns>A Dictionnary with RFS IDs as key and Nop Ids as values</returns>
        public static IDictionary<string, int> GetMapIdNopId()
        {
            Dictionary<string, int> map = new Dictionary<string, int>();                  

            //Get the categories mercator Ids
            string json = WebService.Get(new UrlBuilder("GenericAttribute").And().FilterEq("KeyGroup",ENTITY).FilterEq("Key","MercatorId").BuildQuery());
            JToken[] gaValues = ParserJSon.ParseResultToJTokenList(json);            
            
            //Get the categories mercator Ids of the active categories
            foreach(JToken v in gaValues)
            {   
                //if(catsValues.Any(x=>(int)x["Id"] == (int)v["EntityId"]))
                    map.Add(v["Value"].ToString(), (int)v["EntityId"]);
            }

            return map;
        }

        /// <summary>
        /// Get all categories corresponding to Mercator's FAMILLES
        /// </summary>
        /// <returns></returns>
        private List<JToken> GetNopFamilles()
        {
            //Get all parent categories
            JObject nopCategoriesR = JObject.Parse(WebService.Get(urlBuilder
                                                                .FilterEq("Published",true)
                                                                .FilterEq("ParentCategoryId",0)
                                                                .BuildQuery()));
            var cr = nopCategoriesR["value"].ToArray();
            List<string> nopCategoriesRId = new List<string>();
            foreach (JToken r in cr)
            {
                nopCategoriesRId.Add(r["Id"].ToString());
            }

            //Get familles based on parent id 
            List<JToken> nopFamilles = GetNopFamilles(nopCategoriesRId);

            return nopFamilles;
        }

        /// <summary>
        /// Get categories corresponding to Mercator's FAMILLES with the parentIds in the given list
        /// </summary>
        /// <returns></returns>
        private static List<JToken> GetNopFamilles(List<string> nopCategoriesParentId)
        {
            //Get familles based on parent id 
            JObject nopAllCategories = JObject.Parse(WebService.Get(ENTITY));
            var all = nopAllCategories["value"].ToArray();
            List<JToken> nopFamilles = new List<JToken>();
            foreach (JToken c in all)
            {
                if (nopCategoriesParentId.Contains(c["ParentCategoryId"].ToString()))
                {
                    nopFamilles.Add(c);
                }
            }

            return nopFamilles;
        }

        /// <summary>
        /// Get all categories corresponding to Mercator's SOUS-FAMILLES
        /// </summary>
        /// <returns></returns>
        private List<JToken> GetNopSousFamilles()
        {
            //Get Familles
            List<JToken> familles = GetNopFamilles();
            List<string> nopCategoriesFId = new List<string>();
            foreach(JToken f in familles)
            {
                nopCategoriesFId.Add(f["Id"].ToString());
            }

            //Get SousFamilles based on parent id 
            JObject nopAllCategories = JObject.Parse(WebService.Get(ENTITY));
            var all = nopAllCategories["value"].ToArray();
            List<JToken> nopSousFamilles = new List<JToken>();
            foreach (JToken c in all)
            {
                if (nopCategoriesFId.Contains(c["ParentCategoryId"].ToString()))
                {
                    nopSousFamilles.Add(c);
                }
            }

            return nopSousFamilles;
        }


        #endregion

     
    }
}
