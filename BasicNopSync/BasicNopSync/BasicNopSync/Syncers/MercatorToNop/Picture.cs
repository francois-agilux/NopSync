using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;
using BasicNopSync.Model;
using MercatorORM;
using BasicNopSync.Utils;
using BasicNopSync.OData;

namespace BasicNopSync.Syncers.MercatorToNop
{
    //Don't forget to add S_IMAGE fields in db and to update EF Model
    class Picture
    {
        private OptionsMercator om;
        private string Rep;

        private UrlBuilder urlBuilder;
        private const string ENTITY = "Picture";

        public Picture()
        {
            urlBuilder = new UrlBuilder(ENTITY);
        }

        //Return the picture repository
        public void SetRep(OptionsMercator om)
        {
            Rep = Convert.ToString(om.GetOptionValue("REP_BMP")) ?? "";            
        }

        /// <summary>
        /// Link product to picture
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public JArray Sync(STOCK stock, OptionsMercator om, int productId = 0)
        {
            this.om = om;

            JArray picturesLinks = new JArray();
            if (productId != 0)
            {
                //check if product already has picture
                JObject productPicture = JObject.Parse(WebService.Get(new UrlBuilder("ProductPicture").FilterEq("ProductId",productId).BuildQuery()));
                JToken[] pp = productPicture["value"].ToArray();                
                
                foreach (JToken p in pp)
                {   
                    //Product has picture
                    if ((int)p["ProductId"] == productId)
                    {
                        //Delete pictures
                        int pictureId = (int)p["PictureId"];
                        //Delete link
                        WebService.Delete(new UrlBuilder("ProductPicture").Id((int)p["Id"]).BuildQuery());
                        //Delete picture
                        WebService.Delete(urlBuilder.Id((int)p["PictureId"]).BuildQuery());                            

                    }
                }
            }

            
            //To do for every S_IMAGE            
            SyncPic(stock.S_IMAGE1, picturesLinks, productId);
            //SyncPic(stock.S_IMAGE2, picturesLinks, productId);
            //SyncPic(stock.S_IMAGE3, picturesLinks, productId);
            //SyncPic(stock.S_IMAGE4, picturesLinks, productId);
            //SyncPic(stock.S_IMAGE5, picturesLinks, productId);
            //SyncPic(stock.S_IMAGE6, picturesLinks, productId);
            //SyncPic(stock.S_IMAGE7, picturesLinks, productId);
            //SyncPic(stock.S_IMAGE8, picturesLinks, productId);
            //SyncPic(stock.S_IMAGE9, picturesLinks, productId);
            //SyncPic(stock.S_IMAGE10, picturesLinks, productId);
            //SyncPic(stock.S_IMAGE11, picturesLinks, productId);
            //SyncPic(stock.S_IMAGE12, picturesLinks, productId);

            return picturesLinks;
        }
        

        /// <summary>
        /// Go through every Product Picture for product with id=productId
        /// If does not exist we add it otherwise we save the PP Id 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="productId"></param>
        /// <param name="found"></param>
        /// <param name="pps"></param>
        /// <param name="ppsIds"></param>
        private void SyncPic(string image, JArray pictureLinks, int productId = 0)
        {
            if (String.IsNullOrWhiteSpace(image))
                return;

            JObject json = new JObject();  
            try
            {
                Model.Picture pm = GetPicBase64(image);
                if(pm != null){
                    json = ParserJSon.GetPictureJson(pm.Base64, pm.ImageType, pm.Name);                    

                    string result = WebService.Post(ENTITY, json);
                    JObject newPicture = JObject.Parse(result);
                    int picId = (int)newPicture["Id"];
                    JObject jsonResult = ParserJSon.getProductPictureJson(picId, productId);

                    pictureLinks.Add(jsonResult);
                }
               
                return;
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Error parsing picture:" + json.ToString());
                Program.log("Error parsing picture:" + json.ToString());
                return;
            }
        }


        /// <summary>
        /// Find a picture and convert it in Base64
        /// </summary>
        /// <param name="image"></param>
        /// <returns>A Model.Picture object</returns>
        public Model.Picture GetPicBase64(string image)
        {
            bool exists = false;            
            string base64 = "";            
            string[] datas = image.Split(new string[] { "." }, StringSplitOptions.None);

            if (!(datas.Count() > 1))
            {
                Program.log("Error parsing a picture - missing extension : " + datas[0]); return null;
            }
            string[] tokens = datas[0].Split(new string[] { "\\" }, StringSplitOptions.None);
            //If there is more than one token : absolute path, otherwise we need the rep
            if (tokens.Count() > 1)
            {
                if (File.Exists(image))
                {
                    base64 = FileConverter.convertPicture(image);
                    exists = true;
                }
            }
            else
            {
                if (File.Exists(Rep + "\\" + image))
                {
                    base64 = FileConverter.convertPicture(Rep + "\\" + image);
                    exists = true;
                }
            }

            return exists ? new Model.Picture { Base64 = base64, ImageType = datas[1], Name = tokens.Last() } : null;
        }
    }
    
}

