using BasicNopSync.Model.NopCommerce;

namespace BasicNopSync.Utils
{
    class CountryMapping
    {

        /// <summary>
        /// Return the Nop Country matching the Mercator Country
        /// </summary>
        /// <param name="country">Mercator Country</param>
        /// <returns></returns>
        public static string getNopCountry(string country)
        {
            switch (country)
            {
                case Country.BELGIQUE: return "Belgique";                    
                case "BE": return "Belgique";
                case Country.FRANCE: return "France";
                case Country.LUXEMBOURG: return "Luxembourg";
                case Country.ANGLETERRE: return "Royaume-Uni";
                case Country.ALLEMAGNE: return "Allemagne";                
                default: return "";
            }

        }
    }
}
