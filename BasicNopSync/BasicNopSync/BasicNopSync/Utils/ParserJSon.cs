using BasicNopSync.Model;
using BasicNopSync.Model.Mercator;
using BasicNopSync.Model.NopCommerce;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MercatorORM;
using BasicNopSync.OData;

namespace BasicNopSync.Utils
{
    class ParserJSon
    {
        /// <summary>
        /// Parse Mercator's RSF Item to NopCommerce's Category
        /// </summary>
        /// <returns>json string corresponding to the category</returns>
        public static JObject ParseRFSToNopCategory(Category cat)
        {
            JObject jo = JObject.Parse(JsonConvert.SerializeObject(cat));
            if (cat.Id == 0)
                jo.Remove("Id");
           
            return jo;
                      
        }
        
        /// <summary>
        /// Parse Mercator's Stock to NopCommerce's Product (UPDATE PRODUCT)        
        /// </summary>
        /// <param name="s"></param>
        /// <param name="additionalShippingCharge"></param>
        /// <param name="isTaxExempt"></param>
        /// <returns></returns>
        public static JObject ParseStockToNopProduct(Product p, int id)
        {
            DateTime dto = DateTime.Now;
            //JToken date = JObject.Parse(WebService.Get("Product?$select=CreatedOnUtc&$filter=MercatorId+eq+'" + p.Id + "'"))["value"];

            p.UpdatedOnUtc = dto;

            //if (date.ToArray().Count() > 0)
            //{                
            //    dto = (DateTime)Convert.ToDateTime(date[0]["CreatedOnUtc"].ToString());                             
            //}            

            p.CreatedOnUtc = dto;
            p.Id = id;            

            JObject j = JObject.Parse(JsonConvert.SerializeObject(p));

            return j;
        }
        
        public static JObject ParseScatToSpecificationAttribute(CAT_STCK cs)
        {
            JObject jo = new JObject();
            jo.Add("Name",cs.NOM.TrimEnd());
            jo.Add("DisplayOrder", 0);
            //jo.Add("Type", cs.TYPE);
            return jo;
        }

        /// <summary>
        /// Return the json adding a new specification attribute option
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        public static JObject ParseSCATToSpecificationAttributeOption(CAT_STCK cs, int specAttId)
        {
            JObject jo = new JObject();
            jo.Add("SpecificationAttributeId", specAttId);
            jo.Add("Name", cs.NOM.TrimEnd());
            jo.Add("DisplayOrder", 0);
            
            return jo;
        }

        /// <summary>
        /// Return the json adding a new specification attribute option
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        public static JObject ParseSCATToSpecificationAttributeOption(int specAttId, string val)
        {
            //cs.Type : Dépend de l'id dans la base Nop Commerce (idéalement 1,2,3)
            JObject jo = new JObject();
            jo.Add("SpecificationAttributeId", specAttId);
            jo.Add("Name", val);
            jo.Add("DisplayOrder", 0);

            return jo;
        }

        /// <summary>
        /// Return the json adding a new customer role
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ParseTarifToNopCustomerRole(string name, string systemName)
        {
            JObject jo = new JObject();
            jo.Add("Name", name);
            jo.Add("FreeShipping", false);
            jo.Add("TaxExempt", false);
            jo.Add("Active", true);
            jo.Add("IsSystemRole", false);
            jo.Add("SystemName", systemName);
            jo.Add("PurchasedWithProductId", 0);
            
            return jo.ToString();
        }
        
        /// <summary>
        /// Return the json string mapping a product with a category
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="catId"></param>
        /// <returns>json string</returns>
        public static JObject getProductCategoryJson(int catId, int productId = 0)
        {
            JObject mapping = new JObject();
            mapping.Add("CategoryId", catId);
            mapping.Add("IsFeaturedProduct", false);
            mapping.Add("DisplayOrder", 1);
            if (productId != 0)
            {
                mapping.Add("ProductId", productId);
            }

            return mapping;
        }

        /// <summary>
        /// Return the json string mapping an entity and a url
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="entityId"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static JObject GetUrlRecordJson(string entityName, int entityId, string url)
        {
            JObject jo = new JObject();
            jo.Add("EntityId", entityId);
            jo.Add("EntityName", entityName);
            jo.Add("Slug", url);
            jo.Add("IsActive", true);
            jo.Add("LanguageId", 0);

            //string json = "{\"EntityId\":" + entityId + ",\"EntityName\":\""+entityName+"\",\"Slug\":\""+url+"\",\"IsActive\":true,\"LanguageId\":0}";
            
            return jo;
        }


        
        /// <summary>
        /// Return the json string used to add a picture in the db
        /// </summary>
        /// <param name="base64"></param>
        /// <param name="imgType"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static JObject GetPictureJson(string base64, string imgType, string name)
        {
            JObject jo = new JObject();

            jo.Add("PictureBinary", base64);
            jo.Add("MimeType", String.Format("image/{0}", imgType.TrimEnd()));
            jo.Add("SeoFilename", name);
            jo.Add("AltAttribute", null);
            jo.Add("TitleAttribute", null);
            jo.Add("IsNew", false);

            return jo;
        }

        /// <summary>
        /// Return the json string mapping a picture and a product in the db
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="pictureId"></param>
        /// <returns></returns>
        public static JObject getProductPictureJson(int pictureId, int productId = 0)
        {
            JObject jo = new JObject();
            if (productId != 0)
            {
                jo.Add("ProductId", productId);
            }
            jo.Add("PictureId", pictureId);
            jo.Add("DisplayOrder", 0);

            //string json = "{\"ProductId\":"+productId+",\"PictureId\":"+pictureId+",\"DisplayOrder\":0}";
            //checkJson(json);
            return jo;
        }

        /// <summary>
        /// Return the json string mapping a product and a spec attribute 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="specId"></param>
        /// <param name="allowFilter">true if client want his customers to filter on that S_CAT</param>
        /// <returns></returns>
        public static JObject getProductSpecificationAttributeJson(int specId, bool allowFilter, int productId = 0)
        {
            //string allowFilterS = allowFilter ? "true" : "false";

            JObject jo = new JObject();
            if (productId != 0)
            {
                jo.Add("ProductId", productId);
            }
            jo.Add("AttributeTypeId", 0);
            jo.Add("SpecificationAttributeOptionId", specId);
            jo.Add("CustomValue", "");
            jo.Add("AllowFiltering", allowFilter);
            jo.Add("ShowOnProductPage", true);
            jo.Add("DisplayOrder", 0);
            jo.Add("AttributeType", "Option");            
            //checkJson(json);
            return jo;
        }

        public static JObject GetProductFileJson(string name, string file, int productId = 0)
        {
            JObject jo = new JObject();
            
            List<string> parts = name.Split('.').ToList();
            string finalName = String.Join("-", parts);
            jo.Add("FileExtension", parts.Last());
            jo.Add("FileName", finalName);
            jo.Add("File", file);

            if (productId != 0)
            {
                jo.Add("ProductId", productId);
            }


            return jo;

        }

        /// <summary>
        /// Return the json string mapping a product and a tarif
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerRoleId"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public static JObject GetTierPriceJson(int customerRoleId, double price, decimal remise, int productId = 0)
        {
            JObject jo = new JObject();
            if (productId != 0)
            {
                jo.Add("ProductId", productId);
            }
            jo.Add("StoreId", 0);
            jo.Add("CustomerRoleId", customerRoleId);
            jo.Add("Quantity", 1);
            jo.Add("Remise", remise);
            jo.Add("Price", price);
            //string json = "{\"ProductId\":"+productId+",\"StoreId\":0,\"CustomerRoleId\":"+customerRoleId+",\"Quantity\":1,\"Price\":"+price+"}";
            //checkJson(json);
            return jo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="discountTypeId">2 = produits, 5 = catégories</param>
        /// <param name="price"></param>
        /// <returns></returns>
        public static JObject ParseDiscountToJson(string name, int discountTypeId, bool percentage, double amount, DateTimeOffset beginDate, DateTimeOffset endDate, string idMercator, string idMercatorVPied)
        {   
            JObject jo = new JObject();
            jo.Add("Name", name);
            jo.Add("DiscountTypeId", discountTypeId);
            if (percentage)
            {
                jo.Add("UsePercentage", true);
                jo.Add("DiscountPercentage", amount);
                jo.Add("DiscountAmount", 0);
            }
            else
            {
                jo.Add("UsePercentage", false);
                jo.Add("DiscountPercentage", 0);
                jo.Add("DiscountAmount", amount);
            }

            jo.Add("MaximumDiscountAmount", null);
            jo.Add("StartDateUtc", beginDate);
            jo.Add("EndDateUtc", endDate);

            jo.Add("RequiresCouponCode", false);
            jo.Add("CouponCode", null);
            jo.Add("DiscountLimitationId", 0);
            jo.Add("LimitationTimes", 1);
            jo.Add("MaximumDiscountedQuantity", null);
            jo.Add("DiscountLimitation", "Unlimited");
            jo.Add("IdMercator", idMercatorVPied + "-" + idMercator);

            return jo;
        }

        /// <summary>
        /// Get the Json body to create a new discount requirement rule
        /// </summary>
        /// <param name="discountId"></param>
        /// <returns></returns>
        public static JObject ParseDiscountRequirementToJson(string discountId, string rule)
        {
            JObject jo = new JObject();
            jo.Add("DiscountId", discountId);
            jo.Add("DiscountRequirementRuleSystemName", rule);

            return jo;
        }
        
        /// <summary>
        /// Returns the json string creating a new acl record
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityName"></param>
        /// <param name="customerRoleId"></param>
        /// <returns></returns>
        public static JObject parseNewAclToJson(int entityId, string entityName, int customerRoleId)
        {
            JObject jo = new JObject();
            jo.Add("EntityId", entityId);
            jo.Add("EntityName", entityName);
            jo.Add("CustomerRoleId", customerRoleId);
            //string json = " {\"EntityId\":" + entityId + ",\"EntityName\":\"" + entityName + "\",\"CustomerRoleId\":" + customerRoleId + "}";
            //checkJson(json);
            return jo;
        }

        #region Tax

        /// <summary>
        /// Add the tax name as an attribute
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static JObject ParseTaxToProductAttribute(string name)
        {
            JObject jo = new JObject();
            jo.Add("Name", name);
            return jo;
        }

        /// <summary>
        /// Link attribute (tax) with taxed product
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="attributeId"></param>
        /// <returns></returns>
        public static JObject ParseArtLien1ToProductVariantAttribute(int attributeId, int productId = 0)
        {
            JObject jo = new JObject();
            if (productId != 0)
            {
                jo.Add("ProductId", productId);
            }
            jo.Add("ProductAttributeId", attributeId);
            jo.Add("TextPrompt", "Frais Additionnels");
            jo.Add("IsRequired", false);
            jo.Add("AttributeControlTypeId", 50);
            jo.Add("DisplayOrder", 0);
            //string json = "{\"ProductId\":" + productId + ",\"ProductAttributeId\":" + attributeId + ",\"TextPrompt\":\"null\",\"IsRequired\":false,\"AttributeControlTypeId\":50,\"DisplayOrder\":0}";
            //checkJson(json);
            return jo;
        }

        /// <summary>
        /// Add the price adjustment for the attribute and the product
        /// </summary>
        /// <param name="productAttributeId">Id of attribute_product_mapping</param>
        /// <param name="name"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public static JObject ParseArtLien1ToProductVariantAttributeValue(/*string productAttributeId, */string name, double price)
        {
            JObject jo = new JObject();
            jo.Add("AttributeValueTypeId", 0);
            jo.Add("AssociatedProductId", 0);
            jo.Add("Name", name);
            jo.Add("PriceAdjustment", price);
            jo.Add("WeightAdjustment", 0.0000);
            jo.Add("Cost", 0.0000);
            jo.Add("Quantity", 0);
            jo.Add("IsPreSelected", true);
            jo.Add("DisplayOrder", 0);
            jo.Add("PictureId", 0);

            //string json = "{\"ProductAttributeMappingId\":" + productAttributeId + ",\"AttributeValueTypeId\":0,\"AssociatedProductId\":0,\"Name\":\""+name+"\",\"PriceAdjustment\":" + price + ",";
            //json += "\"WeightAdjustment\":0.0000,\"Cost\":0.0000,\"Quantity\":0,\"IsPreSelected\":true,\"DisplayOrder\":0,\"PictureId\":0}";
            //checkJson(json);
            return jo;
        }

        #endregion Tax


        /// <summary>
        /// Parse a Client to be sync to Nop in Json Format
        /// </summary>
        /// <param name="email"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static JObject ParseClientToJson(string email, DateTimeOffset date, string password, string mercatorId, decimal modiftag)
        {
            string salt = "";                        
            salt = PasswordManagement.CreateSaltKey(6);            
            password = PasswordManagement.HashPassword(password.TrimEnd(), salt);
           
            JObject jo = new JObject();
            jo.Add("Username", email);
            jo.Add("Email", email);
            jo.Add("IsTaxExempt", false);
            jo.Add("AffiliateId", 0);
            jo.Add("VendorId", 0);
            jo.Add("HasShoppingCartItems", false);
            jo.Add("Active", true);
            jo.Add("Deleted", false);
            jo.Add("IsSystemAccount", false);
            jo.Add("CreatedOnUtc", date);
            jo.Add("LastActivityDateUtc", date);
            jo.Add("Password", password);
            jo.Add("PasswordSalt", salt);
            jo.Add("PasswordFormatId", 1);            
            
            return jo;
        }
        
        /// <summary>
        /// Parse data to add a VAT Number to a client into Json strings
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vatNumber"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        public static List<JObject> ParseClientCompanyToJson(int customerId, string vatNumber, string company)
        {
            List<JObject> entries = new List<JObject>();
            entries.Add(GetGenericAttributeJSon(customerId, "Customer", "VatNumber", vatNumber));
            entries.Add(GetGenericAttributeJSon(customerId, "Customer", "VatNumberStatusId", "20"));
            entries.Add(GetGenericAttributeJSon(customerId, "Customer", "Company", company));
            return entries;

        }

        /// <summary>
        /// Parse CLI address to Nop Address in Json String
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static JObject ParseAddressToJson(CLI c, OptionsMercator options)
        {
            //potentiel " dans champ : .Replace("\"", "\\\"") 
			string belgique = options.GetOptionValue("NOP_PAYS").ToString();

            //Si pas de pays, on met belgique par défaut
            string countryId = (c.C_PAYS.TrimEnd() != "") ? GetCountryIdByName(c.C_PAYS.TrimEnd()) : GetCountryIdByName(belgique);            
            
            JObject json = new JObject();
            json.Add("LastName", c.C_NOM.TrimEnd().Replace("\"", "\\\""));
            json.Add("Email", c.C_EMAIL.TrimEnd());
            json.Add("Company", c.C_NOM.TrimEnd().Replace("\"", "\\\""));
            json.Add("CountryId", countryId);
            json.Add("City", c.C_VILLE.TrimEnd());
            json.Add("Address1", c.C_ADRESSE.TrimEnd().Replace("\"", "\\\""));
            json.Add("Address2", c.C_ADRESSE2.TrimEnd().Replace("\"", "\\\""));
            json.Add("ZipPostalCode", c.C_CODEP.TrimEnd());
            json.Add("PhoneNumber", c.C_NUM_TEL.TrimEnd());
            //json.Add("MercatorId", c.C_ID.TrimEnd());
            
            return json;
            
        }

        public static JObject GetGenericAttributeJSon(int entityId, string keygroup, string key, string value, int storeId = 0)
        {
            JObject json = new JObject();
            json.Add("EntityId", entityId);
            json.Add("KeyGroup", keygroup);
            json.Add("Key", key);
            json.Add("Value", value);
            json.Add("StoreId", storeId);

            return json;
        }
        
        /// <summary>
        /// Convert Date Time To Json Format
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static string ConvertDateTimeOffsetToJsonDateFormat(DateTimeOffset dto)
        {
            string offset = dto.Offset.Hours.ToString();
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy-MM-ddTHH:mm:ss+0" + offset + ":00"//,
                //DateTimeStyles = DateTimeStyles.AdjustToUniversal
            });
            return JsonConvert.SerializeObject(dto, settings);
        }

        /// <summary>
        /// Get the product SKU (Mercator's CLE_1) by id in nop db
        /// </summary>
        /// <param name="pId"></param>
        /// <returns>Product SKU (string)</returns>
        public static string getSKUById(string pId)
        { 
            string PRODUCTIDBYSKU = "Product({0})?$select=Sku";
            JObject o = JObject.Parse(WebService.Get(string.Format(PRODUCTIDBYSKU,pId)));
            string sku = o["Sku"].ToString();
            return sku;

        }

        /// <summary>
        /// Get the country id in Nop Commerce by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetCountryIdByName(string name)
        {
            string countryS = WebService.Get(new UrlBuilder("Country").FilterEq("Name",name).BuildQuery());
            JObject countryO = JObject.Parse(countryS);
            var countryIds = countryO["value"].ToArray();
            if (countryIds.Count() > 0)
            {
                JToken country = countryIds.FirstOrDefault();
                return country["Id"].ToString();
            }
            else
            {
                throw new Exception("Undefined country : \"" + name + "\" in Nop DB.");
            }
        }

        /// <summary>
        /// Parse a json result into a JToken array to access the values
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static JToken[] ParseResultToJTokenList(string json)
        {
            JObject parsed = JObject.Parse(json);
            JToken[] values = parsed["value"].ToArray();
            return values;
        }


        public static void checkJson(string json)
        {
            try
            {
                JObject.Parse(json);
                
            }
            catch (JsonReaderException jex)
            {
                
                //Exception in parsing json
                Console.WriteLine(jex.Message);
                Console.WriteLine("Wrong Json : " + json);
            }

        }

        
    }
    
}
