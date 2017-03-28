using Newtonsoft.Json.Linq;
using BasicNopSync.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data.SqlClient;
using BasicNopSync.Model.NopCommerce;
using MercatorORM;
using BasicNopSync.Utils;
using System.Data;
using System.Windows.Forms;
using System.Configuration;
using BasicNopSync.Syncers.MercatorToNop;
using BasicNopSync.OData;

namespace BasicNopSync.Syncers.MercatorToNop
{
    public class StockSyncer : Syncer
    {   
        private static int count = 0;
        private OptionsMercator OptionsMercator;
        decimal lastUpdate = 0;
        string whereClause = "";

        public const string JOIN_CLAUSE = "dispo on s_id = id_stock ";
        public const string WHERE_CLAUSE = "S_WEB = 1 and S_SOMMEIL = 0 and S_ID_RAYON <> '' and S_MODELE <> '' and S_MODIFTAG > {0} and dispo.id_magasin = '0000000001'";
        public const string ALL_STOCKS_WHERE_CLAUSE = "S_WEB = 1 and S_SOMMEIL = 0 and S_ID_RAYON <> '' and S_MODELE <> ''  and dispo.id_magasin = '0000000001'";
        private static int limitBeforeSync = Int32.Parse(ConfigurationManager.AppSettings["limitBeforeSync"]);

        public const string ENTITY = "Product";
        public const string KEY_MERCATORID = "MercatorId";
        public const string KEY_MODIFTAG = "Modiftag";

        private Picture pic;
        
        public StockSyncer() : base()
        {            
            pic = new Picture();
            OptionsMercator = new OptionsMercator();
            urlBuilder = new UrlBuilder(ENTITY);
            //Category mapping
            Categories = RFSSyncer.GetMapIdNopId();
            count = 0;
            lastUpdate = Convert.ToDecimal(OptionsMercator.GetOptionValue("NOP_MDFTAG"));
            whereClause = String.Format(WHERE_CLAUSE, lastUpdate);
        }

        /// <summary>
        /// Synchronize Mercator's Stock as Nop's product
        /// </summary>        
        /// <returns>True if the syncing went well, false otherwise</returns>
        public override bool Sync()
        {
            using (SqlConnection connection = new SqlConnection(dataSettings.DataConnectionString))
            {
                try
                {
                    //Get mercator product list and cancel the sync if list is empty
                    List<STOCK> prodList = new List<STOCK>();
                    prodList = GetMercatorProductsList(connection, JOIN_CLAUSE, whereClause).ToList();
                    
                    //Get the products from NopCommerce with their Mercator Id et with the modiftag saved on the last sync
                    List<ProductDatas> productsMIdMtag = GetProductMercatorDatas();

                    if (prodList.Count() == 0)
                    {
                        Console.WriteLine("Pas de produits à synchroniser");
                        DeleteProducts(connection, productsMIdMtag);
                        return true;
                    }

                    //Get all spec options
                    Dictionary<string, int> specopts = SCatSyncer.GetAllNopSpecOpt();

                    JArray products = new JArray();

                    //ADD/EDIT STOCKS
                    foreach (STOCK stock in prodList)
                    {
                        ProcessProduct(stock, connection, productsMIdMtag, specopts, products);

                        //Update of the modiftag
                        lastUpdate = stock.S_MODIFTAG > lastUpdate ? stock.S_MODIFTAG : lastUpdate;

                        if (count == limitBeforeSync)
                        {
                            SendProducts(products, stocks: prodList, connection: connection);                            
                            count = 0;
                        }
                    }

                    #region add all products, urls and attribute values
                    //Send the lasts products if they have not been synced
                    if (count < limitBeforeSync)
                    {
                        SendProducts(products, stocks: prodList, connection: connection);                  
                        count = 0;                        
                    }
                    #endregion

                    DeleteProducts(connection, productsMIdMtag);

                    Console.WriteLine("Update Modiftag");                    
                    //OptionsMercator.
                    OptionsMercator.SetValueForField("VALEUR", lastUpdate);
                    OptionsMercator.Update();
                    Console.WriteLine("Synchro terminée");

                    connection.Close();
                }
                catch (Exception e)
                {
                    Program.WriteLine(e.Message);
                    Program.WriteLine(e.StackTrace);
                    connection.Close();
                    return false;
                }
            }

            return true;
        }


        #region CRUD
        private IEnumerable<STOCK> GetMercatorProductsList(SqlConnection connection, string joinClause = "", string whereClause = "")
        {
            
            string sql = @"SELECT s_id, s_modele, s_memo, s_cle1, s_cle2, s_cle3, s_prix_ht, s_prix_ti, 
                            s_taux_tva, s_modiftag, s_id_rayon, s_id_famil, s_id_ssfam, s_image1, s_image2, s_image3, s_image4, s_image5, s_image6, s_image7, s_image8, s_image9, s_image10, s_image11, 
                            s_cat1, s_cat2, s_cat3, s_condit_v, dispo.dispo as stck
                            FROM stock" + (joinClause.Length>0 ? " JOIN " + joinClause : "")  + (whereClause.Length > 0 ? " WHERE " + whereClause : "");
            List<STOCK> prodList = new List<STOCK>();
            using (SqlCommand sqlCommand = new SqlCommand(sql))
            //using (SqlDataReader reader = sqlCommand.ExecuteReader())
            {
               
                try
                {
                    DataTable dt = DBContextFactory.DBContext.Query(sqlCommand);
                    
                    foreach(DataRow dr in dt.Rows)
                    {
                        STOCK prod = new STOCK();

                        prod.S_ID = Convert.ToString(dr["s_id"]).TrimEnd();
                        prod.S_MODELE = Convert.ToString(dr["s_modele"]).TrimEnd();
                        prod.S_MEMO = Convert.ToString(dr["s_memo"]).TrimEnd();
                        prod.S_CLE1 = Convert.ToString(dr["s_cle1"]).TrimEnd();
                        prod.S_CLE2 = Convert.ToString(dr["s_cle2"]).TrimEnd();
                        prod.S_CLE3 = Convert.ToString(dr["s_cle3"]).TrimEnd();
                        prod.S_PRIX_HT = Convert.ToDecimal(dr["s_prix_ht"]);
                        prod.S_PRIX_TI = Convert.ToDecimal(dr["s_prix_ti"]);
                        prod.S_TAUX_TVA= Convert.ToDecimal(dr["s_taux_tva"]);
                        prod.S_MODIFTAG = Convert.ToInt32(dr["s_modiftag"]);
                        prod.S_ID_RAYON = Convert.ToString(dr["s_id_rayon"]).TrimEnd();
                        prod.S_ID_FAMIL = Convert.ToString(dr["s_id_famil"]).TrimEnd();
                        prod.S_ID_SSFAM = Convert.ToString(dr["s_id_ssfam"]).TrimEnd();
                        prod.S_IMAGE1 = Convert.ToString(dr["s_image1"]).TrimEnd();
                        prod.S_IMAGE2 = Convert.ToString(dr["s_image2"]).TrimEnd();
                        prod.S_IMAGE3 = Convert.ToString(dr["s_image3"]).TrimEnd();
                        prod.S_IMAGE4 = Convert.ToString(dr["s_image4"]).TrimEnd();
                        prod.S_IMAGE5 = Convert.ToString(dr["s_image5"]).TrimEnd();
                        prod.S_IMAGE6 = Convert.ToString(dr["s_image6"]).TrimEnd();
                        prod.S_IMAGE7 = Convert.ToString(dr["s_image7"]).TrimEnd();
                        prod.S_IMAGE8 = Convert.ToString(dr["s_image8"]).TrimEnd();
                        prod.S_IMAGE9 = Convert.ToString(dr["s_image9"]).TrimEnd();
                        prod.S_IMAGE10 = Convert.ToString(dr["s_image10"]).TrimEnd();
                        prod.S_IMAGE11 = Convert.ToString(dr["s_image11"]).TrimEnd();
                        //prod.S_IMAGE12 = Convert.ToString(dr["s_image12"]).TrimEnd();
                        prod.S_CAT1 = Convert.ToString(dr["s_cat1"]).TrimEnd();
                        prod.S_CAT2 = Convert.ToString(dr["s_cat2"]).TrimEnd();
                        prod.S_CAT3 = Convert.ToString(dr["s_cat3"]).TrimEnd();
                        prod.S_DISPO = Convert.ToInt32(dr["stck"]);
                        prod.S_CONDIT_V = Convert.ToDecimal(dr["s_condit_v"]);
                        
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

        /// <summary>
        /// Add or edit a product
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="context"></param>
        /// <param name="productsMIdMtag"></param>
        /// <param name="productsToUpdate"></param>
        /// <param name="products"></param>
        /// <param name="s_id"></param>
        /// <returns>The last modiftag</returns>
        private void ProcessProduct(STOCK stock, SqlConnection connection, List<ProductDatas> productsMIdMtag, Dictionary<string,int> specopts, JArray products, int reapproCategory = 0)
        {
            JObject product = new JObject();

            List<ProductDatas> pId = productsMIdMtag.Where(x => x.MercatorId == stock.S_ID.TrimEnd()).ToList();

            int productId = pId.Count > 0 ? pId.First().Id : 0;

            //Product exists, check if it needs to be edited                   
            //if (existingUrlRecordsSlugAndProductId.ContainsKey(mID + WebServiceUrls.SLUG))
            if (productId != 0)
            {                
                if (IsProductToUpdate(stock, productsMIdMtag)) 
                {
                    //EDIT PRODUCT
                    product = GetProduct(stock, Categories, connection, specopts, productId);
                    if (product != null)
                    {
                        products.Add(product);

                        //IS this needed?
                        //productsToUpdate.Remove(stock.S_ID.TrimEnd());

                        Console.WriteLine("Produit : " + stock.S_MODELE.TrimEnd() + " mis à jour");
                        count++;
                    }
                }
            }
            else
            {
                //ADD PRODUCT                            
                product = GetProduct(stock, Categories, connection, specopts);
                product.Remove("Id");
                products.Add(product);
                Program.WriteLine("Produit : " + stock.S_MODELE.TrimEnd() + " prêt");
                count++;
            }
        }


        /// <summary>
        /// Send the products to the website by WS
        /// </summary>
        /// <param name="products"></param>
        /// <returns>False if an exception has been caught, true if products have been succesfully synced</returns>
        private void SendProducts(JArray products, bool publish = true, List<STOCK> stocks = null, SqlConnection connection = null)
        {
            JObject container = new JObject();
            string addedProductsResult = "";
            JObject addedProductsResultO = new JObject();

            try
            {

                //Add the products to the JSON
                container.Add("products", products);
                ParserJSon.checkJson(container.ToString());

                //Post everything
                Console.WriteLine("Envoi des produits");
                addedProductsResult = WebService.Post(urlBuilder.BuildQueryFct("AddProducts"), container.ToString());

                addedProductsResultO = JObject.Parse(addedProductsResult);

                Console.WriteLine("Produits synchros");
                
                if (publish)
                {
                    Console.WriteLine("Ajouts URLS && Generic Attributes");

                    JArray urlList = new JArray();
                    JArray genericAttributeList = new JArray();

                    foreach (var j in addedProductsResultO)
                    {
                        STOCK s = stocks.Where(x => x.S_ID.TrimEnd() == j.Value.ToString()).FirstOrDefault();

                        string url = UrlsSyncer.BuildProductUrl(s, connection);
                        JObject urlJSon = ParserJSon.GetUrlRecordJson(ENTITY, Int32.Parse(j.Key), url);
                        JObject genericAttribute = ParserJSon.GetGenericAttributeJSon(Int32.Parse(j.Key), ENTITY, KEY_MODIFTAG, s.S_MODIFTAG.ToString());

                        urlList.Add(urlJSon);
                        genericAttributeList.Add(genericAttribute);
                    }

                    JObject urlContainer = new JObject();
                    JObject gaContainer = new JObject();

                    urlContainer.Add("urls", urlList);
                    gaContainer.Add("genericAttributes", genericAttributeList);
                    
                    WebService.Post(new UrlBuilder("UrlRecord").BuildQueryFct("AddUrlRecords"), urlContainer.ToString());
                    WebService.Post(new UrlBuilder("GenericAttribute").BuildQueryFct("AddGenericAttributes"), gaContainer.ToString());

                    Console.WriteLine("URLS et GA ajoutés");
                }                
            }
            catch (Exception e)
            {
                Program.log("Erreur lors de l'envoi des produits");
                Program.log(e.Message);
                Program.log(e.StackTrace);                

                Program.log("These products have failed syncing : ");
                foreach (JObject jo in products)
                {
                    Program.WriteLine(jo["Name"].ToString());
                    Program.log(jo.ToString(), false);
                }
            }

            products.Clear();
        }


        /// <summary>
        /// Initiate a Product to be synced
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="qteStock"></param>
        /// <param name="categories"></param>
        /// <param name="specopts"></param>
        /// <param name="context"></param>
        /// <param name="productId"></param>
        /// <returns>A Json object corresponding to a NopCommerce Product</returns>
        private JObject GetProduct(STOCK stock, IDictionary<string, int> categories, SqlConnection connection, IDictionary<String, int> specopts, int productId = 0)
        {
               
            JObject p = new JObject();            
            Product product = new Product(stock.S_MODELE.TrimEnd(),
                stock.S_MEMO.TrimEnd(),
                stock.S_CLE2.TrimEnd(),
                stock.S_CLE1.TrimEnd(),
                stock.S_DISPO, stock.S_PRIX_HT, condit: stock.S_CONDIT_V,
                mercatorId: stock.S_ID.TrimEnd(),
                modiftag: stock.S_MODIFTAG);


            p = ParserJSon.ParseStockToNopProduct(product, productId);
            //p = ParserJSon.ParseStockToNopProduct(stock, additionalShippingCharge, false, qteStock, productId);
            if (!AddTVA(p, stock)) return null;
            p.Add(WebApiEntities.PRODUCT_CATEGORIES, MapCategory(stock, categories, productId));
            p.Add(WebApiEntities.PRODUCT_PICTURES, pic.Sync(stock, OptionsMercator, productId));
            try {
                p.Add(WebApiEntities.PRODUCT_SPEC_ATTS, MapSpecs(stock, specopts, productId));
            }catch(KeyNotFoundException knfe)
            {
                Program.log(knfe.Message);
                Program.log(knfe.StackTrace);
            }
            //p.Add(WebServiceUrls.TIER_PRICES, MapTierPrices(stock, connection, productId));
            //p.Add(WebServiceUrls.PRODUCT_FILES, MapFiles(stock, connection, productId));
            //p.Add(WebServiceUrls.PRODUCT_ATTRIBUTE_MAPPINGS, syncTaxAndCondit(stock, productId));            

            return p;
        }


        /// <summary>
        /// Delete the products in Nop that are no longer suppose to be on it
        /// </summary>
        /// <param name="context"></param>
        /// <param name="productsMIdMtag"></param>
        /// <param name="reappro"></param>
        private void DeleteProducts(SqlConnection connection, List<ProductDatas> productsMIdMtag)
        {
            Console.WriteLine("Suppression produits...");
            //First gather products that were created on the website and have no MercatorId
            string jsonNoSkus = WebService.Get(urlBuilder
                .Select("Id", "Sku", "Published")
                .FilterEq("Sku", null)
                .BuildQuery());
            JToken[] productsNoSku = ParserJSon.ParseResultToJTokenList(jsonNoSkus);

            //Then, get the mercator products that are supposed to be on the website
            List<STOCK> prodList = new List<STOCK>();
            prodList = GetMercatorProductsList(connection, JOIN_CLAUSE, ALL_STOCKS_WHERE_CLAUSE).ToList();

            List<string> all_stocks_ids = new List<string>();

            //get stocks (if "Vente en ligne" checked)            
            all_stocks_ids = prodList.Select(x => x.S_ID.TrimEnd()).ToList();

            //Get the products that are on NopCommerce
            List<int> existsInNop = productsMIdMtag.Where(x => all_stocks_ids.Any(y => y == x.MercatorId)).Select(z => z.Id).ToList();

            //If some are on Nop but not in the prodList from Mercator, we mark them as deleted
            List<int> toEject = productsMIdMtag.Select(x => x.Id).Except(existsInNop).ToList();
			//We also add the product that were created on the website (Tournesols products don't have any sku)
            toEject.AddRange(productsNoSku.Select(x => (int)x["Id"]));

            if (toEject.Count() > 0)
            {
                JObject unpublish = new JObject();
                unpublish.Add("Deleted", true);

                JObject urlDeactivate = new JObject();
                urlDeactivate.Add("IsActive", false);
                foreach (int id in toEject)
                {
                    JObject urls = JObject.Parse(WebService.Get(new UrlBuilder("UrlRecord")
                                                                .FilterEq("EntityId", id)
                                                                .FilterEq("EntityName", ENTITY)
                                                                .BuildQuery()));
                        
                    var urlsValues = urls["value"].ToArray();
                    if (urlsValues.Count() > 0)
                    {
                        WebService.Patch(new UrlBuilder("UrlRecord").Id((int)urlsValues[0]["Id"]).BuildQuery(), urlDeactivate.ToString());
                    }
                    
                    WebService.Patch(urlBuilder.Id(id).BuildQuery(),unpublish.ToString());
                }
            }

            Console.WriteLine("Produits supprimés");
        }

        #endregion

        #region miscellaneous

        /// <summary>
        /// Add a tax category to a product
        /// </summary>
        /// <param name="productJo"></param>
        /// <param name="stock"></param>
        /// <remarks>
        ///     Tax categories have to be created in Nop's admin panel      
        ///     Tax categories : Belgique 6, Belgique 12, Belgique 21
        /// </remarks>
        /// <returns></returns>
        private bool AddTVA(JObject productJo, STOCK stock)
        {   
            string resultTC = WebService.Get("TaxCategory");
            JToken[] tvaValues = ParserJSon.ParseResultToJTokenList(resultTC);

            Dictionary<string, int> tcIds = tvaValues.ToDictionary(x => x["Name"].ToString().Split(' ').Last(), x => (int)x["Id"]);            
            
            int taxId;
            tcIds.TryGetValue(stock.S_TAUX_TVA.ToString(), out taxId);
            if (taxId != 0)
            {
                if ((int)productJo["TaxCategoryId"] == 0) {
                    productJo["TaxCategoryId"] = taxId;
                }
                else
                {
                    productJo.Add("TaxCategoryId", taxId);
                }
            }
            else
            {   

                Program.log(String.Format("Le taux de TVA {0} n'existe pas dans NopCommerce. ID du produit concerné : {1}. \nVeuillez contacter l'administrateur pour régler le problème.", stock.S_TAUX_TVA, stock.S_ID.TrimEnd()));                
                return false;
            }
            return true;
        }

        /// <summary>
        /// Add taxes to products (i.e product attributes) (e.g ecotaxe)
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="productId"></param>
        //private static JArray syncTaxAndCondit(STOCK stock, int productId = 0)
        //{
        //    JArray ja = new JArray();
        //    JObject attributeMapping;
        //    JArray attributeValues;
        //    string productVAttributeId;    
        //    //Attributes
        //    foreach (ARTLIENS a in stock.ARTLIENS)
        //    {
        //        if (a.VEN)
        //        {
        //            int attributeId = 0;
        //            productVAttributeId = "";                    

        //            //Create attribute if it doesn't exist yet
        //            JObject paO = JObject.Parse(WebService.Get(WebServiceUrls.PRODUCT_ATTRIBUTE + "?$filter=Name+eq+'" + a.STOCK1.S_MODELE.TrimEnd() + "'"));
        //            var pa = paO["value"].ToArray();
        //            if (pa.Count() == 0)
        //            {
        //                string result = WebService.Post(WebServiceUrls.PRODUCT_ATTRIBUTE, ParserJSon.ParseTaxToProductAttribute(a.STOCK1.S_MODELE.TrimEnd()));
        //                JObject idO = JObject.Parse(result);
        //                attributeId = (int)idO["Id"];
        //            }
        //            else
        //            {
        //                attributeId = (int)pa[0]["Id"];
        //            }

        //            attributeMapping = ParserJSon.ParseArtLien1ToProductVariantAttribute(attributeId, productId);

        //            //Add value to attribute
        //            attributeValues = new JArray();
        //            //JObject pvavO = JObject.Parse(WebService.Get(WebServiceUrls.PRODUCT_VARIANT_ATTRIBUTE_VALUE + "?$filter=ProductAttributeMappingId+eq+" + productVAttributeId));
        //            //var pvav = pvavO["value"].ToArray();

        //            attributeValues.Add(ParserJSon.ParseArtLien1ToProductVariantAttributeValue(a.STOCK1.S_MODELE.TrimEnd(), a.STOCK1.S_PRIX_TI));

        //            attributeMapping.Add("ProductAttributeValues", attributeValues);
        //            ja.Add(attributeMapping);
        //        }

        //    }

        //    if (stock.S_CONDIT_V > 1)
        //    {
        //        attributeMapping = ParserJSon.ParseArtLien1ToProductVariantAttribute(1, productId);
        //        attributeValues = new JArray();            
        //        attributeValues.Add(ParserJSon.ParseArtLien1ToProductVariantAttributeValue(stock.S_CONDIT_V.ToString() + " PC", 0));
        //        attributeMapping.Add("ProductAttributeValues", attributeValues);
        //        ja.Add(attributeMapping);
        //    }
            
        //    return ja;
        //}

        /// <summary>
        /// Map product to tier prices (Tarifs in Mercator)
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="productId"></param>
        /// <param name="connection"></param>
        //private static JArray MapTierPrices(STOCK stock, SqlConnection connection, int productId)
        //{
        //    JArray array = new JArray();
        //    string json = WebService.Get("TierPrice?$filter=ProductId+eq+" + productId);
        //    JToken[] valuesTP = ParserJSon.ParseResultToJTokenList(json);

        //    List<String> tpToKeep = new List<String>();

        //    //Get existing customer roles
        //    string jsonCR = WebService.Get("CustomerRole?$filter=IsSystemRole+eq+false");
        //    JToken[] valuesCR = ParserJSon.ParseResultToJTokenList(jsonCR);

        //    //CustomerRoleID => Price in Nop
        //    IDictionary<String, String> tpMap = new Dictionary<String, String>();
        //    foreach (JToken v in valuesTP)
        //    {
        //        tpMap.Add(v["CustomerRoleId"].ToString()+"KEY", v["Id"].ToString());
        //        tpMap.Add(v["CustomerRoleId"].ToString(), v["Price"].ToString());

        //    }
            
        //    foreach (JToken v in valuesCR)
        //    {
        //        string tarId = v["SystemName"].ToString().Split('_').Last();
        //        double tarti = (double)connection.Entry(stock).Property("S_PRIXHT" + tarId).CurrentValue;
        //        decimal remise = GetRemise(stock,connection,tarId);
        //        string concernedPrice;
        //        tpMap.TryGetValue(v["Id"].ToString(), out concernedPrice);
        //        //If TierPrice contains customer role's id => Check if it's the same price                    
        //        if (concernedPrice != null)
        //        {
        //            //Price have changed => Patch
        //            if (tarti.ToString() != concernedPrice)
        //            {
        //                array.Add(ParserJSon.GetTierPriceJson((int)v["Id"], tarti, remise, productId));
        //            }
        //        }
        //        //If it's not in it, we add it
        //        else
        //        {
        //            array.Add(ParserJSon.GetTierPriceJson((int)v["Id"], tarti, remise, productId));
        //        }
        //    }
        //    return array;
        //    //DELETE DONE AUTOMATICALLY WHEN TARIF IS REMOVED
        //}

        /// <summary>
        /// Get remise by stock and tarif
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="context"></param>
        /// <param name="tarId"></param>
        /// <returns></returns>
        //private static decimal GetRemise(STOCK stock, SqlConnection connection, string tarId)
        //{
        //    string scat2 = stock.S_CAT2;
        //    var cats = from c in context.CAT_STCK
        //                   where c.NOM.TrimEnd() == scat2.TrimEnd()
        //                   select c;

        //    if(cats.FirstOrDefault() != null)
        //    {
        //        CAT_STCK cs = cats.FirstOrDefault();
        //        object tarti = context.Entry(cs).Property("CAT" + tarId).CurrentValue;
        //        return (decimal)(double)tarti;
        //    }   

        //    return 0;
        //}
                

        /// <summary>
        /// Add a new entry in Nop Product_Category_Mapping Table 
        /// </summary>
        private JArray MapCategory(STOCK stock, IDictionary<string, int> categories, int productId = 0)
        {
            string ray = stock.S_ID_RAYON.TrimEnd();
            string fam = stock.S_ID_FAMIL.TrimEnd();
            string ssfam = stock.S_ID_SSFAM.TrimEnd();
            //Get CatId
            string rfsId = (ssfam == "") ? ((fam == "") ? ray : fam) : ssfam;

            int catId;
            bool catExists = categories.TryGetValue(rfsId, out catId);

            //If catID is empty, the rfs have not been syncrhonised. Sync it or do nothing ?
            if (catExists)
            {
                JArray cats = new JArray();
                cats.Add(ParserJSon.getProductCategoryJson(catId, productId));
                return cats;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="context"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        //private JArray MapFiles(STOCK stock, SqlConnection connection, int productId = 0)
        //{
        //    JArray ja = new JArray();

        //    string location = FileConverter.REP_FILES_LOCATION + stock.S_ID.TrimEnd();
        //    //string[] dirs = Directory.GetFiles(location);
        //    DirectoryInfo directorySelected = new DirectoryInfo(location);

        //    foreach (FileInfo fileToCompress in directorySelected.GetFiles())
        //    {
        //        string path = location + "\\" + fileToCompress.Name;
        //        ja.Add(ParserJSon.GetProductFileJson(fileToCompress.Name, FileConverter.ConvertFileToB64(path), productId));                
        //        //File.Delete(path);
        //    }

        //    return ja;
        //}


        /// <summary>
        /// ! Specifications have to be synchronized first (S_CAT) !
        /// Map the Mercator's Stock's S_CAT with Nop's Products's Specs
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="productId"></param>
        /// <param name="specopts"></param>
        private JArray MapSpecs(STOCK stock, IDictionary<String, int> specopts, int productId = 0)
        {
            JArray ja = new JArray();


            string typesResult = WebService.Get(new UrlBuilder("GenericAttribute")
                                                    .FilterEq("KeyGroup", "SpecificationAttribute")
                                                    .FilterEq("Key", "Type")
                                                    .BuildQuery());

            JToken[] types = ParserJSon.ParseResultToJTokenList(typesResult);
            int type1 = types.Where(x => (int)x["Value"] == 1).Select(x =>(int)x["EntityId"]).FirstOrDefault();
            int type2 = types.Where(x => (int)x["Value"] == 2).Select(x =>(int)x["EntityId"]).FirstOrDefault();
            int type3 = types.Where(x => (int)x["Value"] == 3).Select(x =>(int)x["EntityId"]).FirstOrDefault();

            if(type1 != 0)
                AddSpec(ja, specopts, stock.S_CAT1, type1, productId);
            if (type2 != 0)
                AddSpec(ja, specopts, stock.S_CAT2, type2, productId);
            if (type3 != 0)
                AddSpec(ja, specopts, stock.S_CAT3, type3, productId);

            //JToken[] prod = ParserJSon.ParseResultToJTokenList(WebService.Get(String.Format(WebServiceUrls.GET_PRODUCT_BY_MERCATOR_ID,stock.S_ID.TrimEnd())));
            JToken[] toDelete = ParserJSon.ParseResultToJTokenList(WebService.Get(new UrlBuilder("ProductSpecificationAttribute")
                                                                                    .FilterEq("ProductId", productId)
                                                                                    .BuildQuery()));
                
            //TODO: revoir delete
            //foreach (JToken j in toDelete)
            //{
            //    if ((int)j["SpecificationAttributeOptionId"] != value)
            //    {
            //        WebService.Delete(String.Format(WebServiceUrls.PRODUCT_SPEC_ATT_BY_ID, (int)j["Id"]));
            //        WebService.Delete(String.Format(WebServiceUrls.SPEC_ATT_OPT_BY_ID, (int)j["SpecificationAttributeOptionId"]));
            //    }
            //}

            return ja;

        }

        private void AddSpec(JArray ja, IDictionary<String,int> specopts, string scat, int number, int productId)
        {
            int value;
            if (!String.IsNullOrWhiteSpace(scat))
            {
                if (!specopts.TryGetValue(scat + number , out value))
                {
                    throw new KeyNotFoundException("Key: " + scat + number +" not found in the dictionary");
                }
                ja.Add(ParserJSon.getProductSpecificationAttributeJson(value, true, productId));
            }

        }

        //Récupère les id mercator, les modiftag et les id des produits Nop
        private List<ProductDatas> GetProductMercatorDatas()
        {
            Dictionary<string, int> map = new Dictionary<string, int>();
            
            string jsonPublishedIds = WebService.Get(new UrlBuilder(ENTITY).Select("Id","Sku","Published").BuildQuery());

            JToken[] products = ParserJSon.ParseResultToJTokenList(jsonPublishedIds);

            string jsonModiftagIds = WebService.Get(new UrlBuilder("GenericAttribute")
                                                    .FilterEq("KeyGroup", ENTITY)
                                                    .FilterEq("Key", KEY_MODIFTAG)
                                                    .BuildQuery());                
            JToken[] gaModiftagIdsValues = ParserJSon.ParseResultToJTokenList(jsonModiftagIds);

			if(gaModiftagIdsValues.Length == 0)
            {
                return new List<ProductDatas>();
            }
            var productsMIds = from p in products
                        join merc in gaModiftagIdsValues on p["Id"] equals merc["EntityId"]                                                
                        into z
                        from q in z.DefaultIfEmpty()                                    
                        select new { Id = p["Id"], Published = p["Published"], MercatorId = p["Sku"], Modiftag = q["Value"] };

            var productsDatas = from p in productsMIds
                           join modif in gaModiftagIdsValues on p.Id equals modif["EntityId"]
                        into z
                        from q in z.DefaultIfEmpty()
                        select new ProductDatas { Id = (int)p.Id, Published = (bool)p.Published, MercatorId = p.MercatorId.ToString(), Modiftag = (int)q["Value"] };

            //return map;
            return productsDatas.ToList();
        }

        private bool IsProductToUpdate(STOCK s, List<ProductDatas> productsMIdMtag)
        {   
            return productsMIdMtag.Any(p => p.MercatorId.TrimEnd() == s.S_ID.TrimEnd() && s.S_MODIFTAG > p.Modiftag);
        }

        #endregion

        private static IDictionary<string,int> Categories { get; set; }       
    }


    class ProductDatas
    {
        public int Id { get; set; }
        public bool Published { get; set; }
        public string MercatorId { get; set; }
        public int Modiftag { get; set; }
    }

}
