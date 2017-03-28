using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BasicNopSync.Utils
{  

    public class Utils
    {   

        /// <summary>
        /// Renvoie une chaîne utilisable dans une URL et SEO-friendly
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string GetFriendlyUrl(string val)
        {
            val = val.ToLower();
            //Suppression des accents
            val = RemoveDiacritics(val);
            //Remplacement de certains caractères spéciaux par un équivalent alphanumérique
            val = val.Replace("µ", "u");
            val = val.Replace("%", "pc");
            //Suppression d'une partie des caractère non utilisables dans une URL
            val = new Regex("[^a-z0-9,\\s_'\\./:&\"-]").Replace(val, "");
            //Remplacement des caractères considérés comme de l'espacement par "-"
            val = new Regex("[,\\s_'\\./:&\"]+").Replace(val, " ");
            val = val.Trim();
            val = new Regex("[ ]").Replace(val, "-");
            val = val.ToUpper();
            return val;
        }

        /// <summary>
        /// Renvoie une copie de la chaîne dépourvue d'accents
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

    }

}
