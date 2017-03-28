using BasicNopSync.Model.NopCommerce;
using Newtonsoft.Json.Linq;

namespace BasicNopSync.Utils
{
    class AddressConverter
    {
        /// <summary>
        /// Convert a Json adress
        /// </summary>
        /// <param name="addressJson"></param>
        /// <returns></returns>
        public static Address ParseJsonToAddress(JObject addressJson)
        {
            if (addressJson == null)
                return null;

            string pays = JObject.Parse(WebService.Get("Country(" + addressJson["CountryId"].ToString().TrimEnd() + ")"))["Name"].ToString();

            return new Address(addressJson["FirstName"].ToString().TrimEnd(),
                addressJson["LastName"].ToString().TrimEnd(),
                addressJson["Address1"].ToString().TrimEnd(),
                addressJson["Address2"].ToString().TrimEnd(),
                addressJson["ZipPostalCode"].ToString().TrimEnd(),
                addressJson["City"].ToString().TrimEnd(),
                pays,
                addressJson["PhoneNumber"].ToString().TrimEnd());            
        }

        public static Address ParseJsonToAddress(string addressJson)
        {
            JObject address = JObject.Parse(addressJson);
            if (address == null)
                return null;

            return ParseJsonToAddress(address);

        }


    }
}
