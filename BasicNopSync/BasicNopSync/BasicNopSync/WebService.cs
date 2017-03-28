using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using BasicNopSync.WebApi.Datas;
using BasicNopSync.WebAPi.Controllers;
using Newtonsoft.Json.Linq;

namespace BasicNopSync
{
    public class WebService
    {
        //private ResourceManager rm;
        private static AuthParameters authParams;
        
        //TODO: check piette pour repeter patch et get x 3

        public WebService()
        {
        }

		public static string GetStoreAddress()
        {
            if(authParams == null)
                authParams = AuthorizationController.GetAuthParams();

            return authParams.StoreAddress;
        }

        /// <summary>
        /// GET method
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string Get(string uri)
        {
            bool success = true;
            int count = 0;
            string finalResult;

            string sig = Signature();
            string authInfo = authParams.ClientName + ":" + sig;

            do
            {
                HttpClient client = new HttpClient();                

                client.BaseAddress = new Uri(authParams.StoreAddress);

                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                client.DefaultRequestHeaders.Add("PublicKey", authParams.PublicToken);
                client.DefaultRequestHeaders.Add("User-Agent", "WebApiFornopCommerce");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
                finalResult = "";
                try
                {
                    HttpResponseMessage response = client.GetAsync(uri).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        client.Dispose();
                        return result;
                    }
                    else
                    {
                        string result = String.Format("{0} - {1} - {2} \n Failed Request: {3}", response.StatusCode.ToString(), response.ReasonPhrase, response.Content.ToString(), uri);
                        client.Dispose();
                        finalResult = result;
                        throw new Exception(result);
                    }
                }
                catch (Exception e)
                {
                    count++;
                    success = false;
                    if (count < 3)
                        Program.log(String.Format("Get failed #{0}, try again", count));
                    else
                        Program.log(String.Format("Get failed #3"));
                }
            } while (!success && count < 3);
            throw new Exception(finalResult);

        }

		public static string Put(string uri, JObject jsonRequest)
        {
            return Put(uri, jsonRequest.ToString());
        }

        public static string Put(string uri, string request)
        {
            string sig = Signature();
            string authInfo = authParams.ClientName + ":" + sig;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(authParams.StoreAddress);
            
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
            client.DefaultRequestHeaders.Add("PublicKey", authParams.PublicToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpContent content = new StringContent(request);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = client.PutAsync(uri, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                client.Dispose();
                return result;

            }
            else
            {
                string result = String.Format("{0} - {1} - {2}", response.StatusCode.ToString(), response.ReasonPhrase, response.Content.ToString());
                client.Dispose();
                throw new Exception(result);

            }
        }

        public static string Post(string uri, JObject jsonRequest)
        {
            return Post(uri, jsonRequest.ToString());
        }

        public static string Post(string uri, string request)
        {
            string sig = Signature();
            string authInfo = authParams.ClientName + ":" + sig;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(authParams.StoreAddress);
            
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
            client.DefaultRequestHeaders.Add("PublicKey", authParams.PublicToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpContent content = new StringContent(request);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = client.PostAsync(uri, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                client.Dispose();
                return result;
            }
            else
            {
                string result = String.Format("{0} - {1} - {2}", response.StatusCode.ToString(), response.ReasonPhrase, response.Content.ToString());
                client.Dispose();
                throw new Exception(result);

            }
        }

		public static string Patch(string uri, JObject jsonRequest)
        {
            return Patch(uri, jsonRequest.ToString());
        }

        public static string Patch(string uri, string request)
        {
            bool success = true;
            int count = 0;
            string finalResult = "";

            string sig = Signature();
            string authInfo = authParams.ClientName + ":" + sig;

            do
            {

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(authParams.StoreAddress);

                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                client.DefaultRequestHeaders.Add("PublicKey", authParams.PublicToken);
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpContent content = new StringContent(request);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                content.Headers.ContentType.CharSet = "utf-8";
                try
                {
                    HttpRequestMessage patch = new HttpRequestMessage
                    {
                        Method = new HttpMethod("PATCH"),
                        RequestUri = new Uri(client.BaseAddress + uri),
                        Content = content,
                    };
                    HttpResponseMessage response = client.SendAsync(patch).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        client.Dispose();
                        return result;
                    }
                    else
                    {
                        string result = String.Format("{0} - {1} - {2} \n Failed Request:\n uri: {3} \n Body: {4} ", response.StatusCode.ToString(), response.ReasonPhrase, response.Content.ToString(), uri, request);
                        client.Dispose();
                        finalResult = result;
                        throw new Exception(result);
                    }
                }
                catch (Exception e)
                {
                    count++;
                    success = false;
                    if (count < 3)
                        Program.log(String.Format("Patch failed #{0}, try again", count));
                    else
                        Program.log(String.Format("Patch failed #3"));
                }
            } while (!success && count < 3);
            throw new Exception(finalResult);
        }

        public static string Delete(string uri)
        {
            string sig = Signature();
            string authInfo = authParams.ClientName + ":" + sig;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(authParams.StoreAddress);
         
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
            client.DefaultRequestHeaders.Add("PublicKey", authParams.PublicToken);
            HttpResponseMessage response = client.DeleteAsync(uri).Result;
            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                client.Dispose();
                return result;
            }
            else
            {
                string result = String.Format("{0} - {1} - {2} - {3}", response.StatusCode.ToString(), response.ReasonPhrase, response.Content.ToString(), response.ToString());
                client.Dispose();
                throw new Exception(result);

            }
        }

        private static string Signature()
        {
            if (authParams == null)
                authParams = AuthorizationController.GetAuthParams();

            if (String.IsNullOrWhiteSpace(authParams.SecretToken) || String.IsNullOrWhiteSpace(authParams.PublicToken))
                return "";

            string signature;
            var secretBytes = Encoding.UTF8.GetBytes(authParams.SecretToken);
            var valueBytes = Encoding.UTF8.GetBytes(authParams.PublicToken);

            using (var hmac = new HMACSHA256(secretBytes))
            {
                var hash = hmac.ComputeHash(valueBytes);
                signature = Convert.ToBase64String(hash);
            }
            return signature;
        }
    }
}
