using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.Utils
{
    class PasswordManagement
    {
        //Code from NopCommerce 3.6


        public static string CreatePassword()
        {
            //Create random string hashed with customer id as salt => Password
            //Then hash it and send it 
            var rng = new RNGCryptoServiceProvider();
            string[] tabId = MercatorApi.Api.NewId().Split('-');
            //string passwordTemp = tabId[tabId.Count() - 1].Substring(0, 12);
            string id = "";
            foreach(string s in tabId) { id += s; }
            string passwordReal="";
            for (int i=0; i< 10; i++)
            {
                Random random = new Random(i);                
                passwordReal += id.ElementAt(random.Next(id.Length)); ;
            }
            return passwordReal;
        }

        /// <summary>
        /// Create salt key
        /// </summary>
        /// <param name="size">Key size</param>
        /// <returns>Salt key</returns>
        public static string CreateSaltKey(int size)
        {   
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[size];
            rng.GetBytes(buff);
            
            return Convert.ToBase64String(buff);
        }

        /// <summary>
        /// Create a password hash in SHA1
        /// </summary>
        /// <param name="password">{assword</param>
        /// <param name="saltkey">Salk key</param>        
        /// <returns>Password hash</returns>
        public static string HashPassword(string password, string saltkey)
        {   
            string passwordSaltCombined = String.Concat(password, saltkey);
            
            var algorithm = HashAlgorithm.Create("SHA1");

            var hashByteArray = algorithm.ComputeHash(Encoding.UTF8.GetBytes(passwordSaltCombined));
            return BitConverter.ToString(hashByteArray).Replace("-", "");
        }
    }
}
