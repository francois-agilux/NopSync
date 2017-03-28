using BasicNopSync.Model.NopCommerce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.WebApi.Datas
{   
    public class AuthParameters
    {
        //public string ServerUrl { get; set; }
        //public string Code { get; set; }
        //public string Error { get; set; }
        //public string ClientId { get; set; }
        //public string ClientSecret { get; set; }
        //public string RedirectUrl { get; set; }
        //public string GrantType { get; set; }
        
        //public AuthorizationModel authModel { get; set; }        
        public string ClientName { get; set; }
        public string SecretToken { get; set; }
        public string PublicToken { get; set; }
        public string StoreAddress { get; set; }
    }
}
