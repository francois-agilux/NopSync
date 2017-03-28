using System;
using BasicNopSync.WebApi.Datas;
using System.Data.SqlClient;
using MercatorORM;
using System.Data;


namespace BasicNopSync.WebAPi.Controllers
{
    public class AuthorizationController
    {

        //public string RefreshAccessToken(string refreshToken, string clientId, string serverUrl)
        //{
        //    string json = string.Empty;

        //    if (!string.IsNullOrEmpty(refreshToken) &&
        //        !string.IsNullOrEmpty(clientId) &&
        //        !string.IsNullOrEmpty(serverUrl))
        //    {
        //        var model = new AccessModel();

        //        try
        //        {
        //            var authParameters = new AuthParameters()
        //            {
        //                ClientId = clientId,
        //                ServerUrl = serverUrl,
        //                RefreshToken = refreshToken,
        //                GrantType = "refresh_token"
        //            };

        //            var nopAuthorizationManager = new AuthorizationManager(authParameters.ClientId,
        //                authParameters.ClientSecret, authParameters.ServerUrl);

        //            string responseJson = nopAuthorizationManager.RefreshAuthorizationData(authParameters);

        //            AuthorizationModel authorizationModel =
        //                JsonConvert.DeserializeObject<AuthorizationModel>(responseJson);

        //            model.AuthorizationModel = authorizationModel;
        //            model.UserAccessModel = new UserAccessModel()
        //            {
        //                ClientId = clientId,
        //                ServerUrl = serverUrl
        //            };

        //            // Here we use the temp data because this method is called via ajax and here we can't hold a session.
        //            // This is needed for the GetCustomers method in the CustomersController.
        //            TempData["accessToken"] = authorizationModel.AccessToken;
        //            TempData["serverUrl"] = serverUrl;
        //        }
        //        catch (Exception ex)
        //        {
        //            json = string.Format("error: '{0}'", ex.Message);

        //            return Json(json, JsonRequestBehavior.AllowGet);
        //        }

        //        json = JsonConvert.SerializeObject(model.AuthorizationModel);
        //    }
        //    else
        //    {
        //        json = "error: 'something went wrong'";
        //    }

        //    return Json(json, JsonRequestBehavior.AllowGet);
        //}

        //public AuthParameters InitiateAuthorization(string serverUrl, string clientId, string clientSecret)
        //{
        //    Uri server = new Uri(serverUrl);
        //    Uri redirect = new Uri(Program.baseAddressToken);
        //    AuthParameters authParams = new AuthParameters
        //    {
        //        ClientId = clientId,
        //        ClientSecret = clientSecret,
        //        ServerUrl = server.ToString(),
        //        GrantType = "authorization_code",
        //        RedirectUrl = redirect.ToString()
        //    };


        //    ApiAuthorizer _apiAuthorizer = new ApiAuthorizer(authParams.ClientId, authParams.ClientSecret, authParams.ServerUrl);
        //    //1
        //    string codeUrl = _apiAuthorizer.GetAuthorizationUrl(Program.baseAddressToken, new string[] { });
        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri(codeUrl);            

        //    string code = client.GetAsync(codeUrl).Result.ToString();
        //    //Intercepted by AuthorizeController    
        //    if (string.IsNullOrWhiteSpace(code))
        //        throw new Exception("Empty code");
        //    authParams.Code = code;

        //    //2
        //    // get the access token
        //    string accessTokenJSon = _apiAuthorizer.AuthorizeClient(authParams.Code, authParams.GrantType, authParams.RedirectUrl);
        //    AuthorizationModel authorizationModel = JsonConvert.DeserializeObject<AuthorizationModel>(accessTokenJSon);
        //    authParams.authModel = authorizationModel;

        //    return authParams;
        //}

        public static AuthParameters GetAuthParams()
        {
            string query = "select clientName, publictoken, secrettoken, storeaddress from web_api_credentials";

            DataTable dt = DBContextFactory.DBContext.Query(query);
            foreach (DataRow dr in dt.Rows)
            {
                AuthParameters auth = new AuthParameters();

                auth.ClientName = Convert.ToString(dr["clientName"]).TrimEnd();
                auth.PublicToken= Convert.ToString(dr["publictoken"]).TrimEnd();
                auth.SecretToken = Convert.ToString(dr["secrettoken"]).TrimEnd();
                auth.StoreAddress = Convert.ToString(dr["storeaddress"]).TrimEnd();

                if (!auth.StoreAddress.Contains("odata"))
                {
                    if (!auth.StoreAddress.EndsWith("/"))
                    {
                        auth.StoreAddress += "/";
                    }
                    auth.StoreAddress += "odata/";
                }
                    

                return auth;
            }

            return null;
        }

        public void SaveAuthModelInDB(string connectionString, string clientName, string publicToken, string secretToken, string storeAddress/*AuthParameters authParams*/)
        {
            //string nonQuery = @"insert into web_api_credentials (client_id,client_secret,access_token,refresh_token,redirect_url,server_url) values
            //    ('{0}','{1}','{2}','{3}','{4}','{5}')";
            //SqlCommand command = new SqlCommand(String.Format(nonQuery, authParams.ClientId, authParams.ClientSecret,
            //    authParams.authModel.AccessToken, authParams.authModel.RefreshToken, authParams.RedirectUrl, authParams.ServerUrl));
            
            DBContextFactory.SetConnection(connectionString);

            string nonQuery = @"insert into web_api_credentials (clientName,publicToken,secretToken,storeaddress) values
                ('{0}','{1}','{2}','{3}')";
            SqlCommand command = new SqlCommand(String.Format(nonQuery, clientName, publicToken, secretToken, storeAddress));

            DBContextFactory.DBContext.BeginTransaction();

            int result = DBContextFactory.DBContext.NonQuery(command);

            if (result < 0)
                DBContextFactory.DBContext.RollbackTransaction();
            else
                DBContextFactory.DBContext.CommitTransaction();
        }

        private string BadRequest(string message = "Bad Request")
        {
            return message;
        }
    }
}