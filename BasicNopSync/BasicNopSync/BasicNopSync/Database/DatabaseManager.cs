using MercatorORM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.Database
{
    //This code is from NopCommerce, not mine
    public partial class DatabaseManager
    {
        protected const char separator = ':';
        protected const string filename = "Settings.txt";

        /// <summary>
        /// Parse settings
        /// </summary>
        /// <param name="text">Text of settings file</param>
        /// <returns>Parsed data settings</returns>
        private static DataSettings ParseSettings(string text)
        {
            var shellSettings = new DataSettings();
            if (String.IsNullOrEmpty(text))
                return shellSettings;
                        
            var settings = new List<string>();
            using (var reader = new StringReader(text))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                    settings.Add(str);
            }

            foreach (var setting in settings)
            {
                var separatorIndex = setting.IndexOf(separator);
                if (separatorIndex == -1)
                {
                    continue;
                }
                string key = setting.Substring(0, separatorIndex).Trim();
                string value = setting.Substring(separatorIndex + 1).Trim();

                switch (key)
                {                    
                    case "DataConnectionString":
                        shellSettings.DataConnectionString = value;
                        break;
                    default:
                        shellSettings.RawDataSettings.Add(key, value);
                        break;
                }
            }

            return shellSettings;
        }

        /// <summary>
        /// Convert data settings to string representation
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <returns>Text</returns>
        protected virtual string ComposeSettings(DataSettings settings)
        {
            if (settings == null)
                return "";

            return string.Format("DataConnectionString: {0}{1}",                                 
                                 settings.DataConnectionString,
                                 Environment.NewLine
                );
        }

        /// <summary>
        /// Load settings
        /// </summary>
        /// <param name="filePath">File path; pass null to use default settings file path</param>
        /// <returns></returns>
        public static DataSettings LoadSettings(string filePath = null)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            }
            if (File.Exists(filePath))
            {
                string text = File.ReadAllText(filePath);

                return ParseSettings(text);
            }
            else
            {
                return null;
            }

            return new DataSettings();
        }

        
        /// <summary>
        /// Save settings to a file
        /// </summary>
        /// <param name="settings"></param>
        public virtual void SaveSettings(DataSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            if (!File.Exists(filePath))
            {
                using (File.Create(filePath))
                {
                    //we use 'using' to close the file after it's created
                }
            }

            var text = ComposeSettings(settings);
            File.WriteAllText(filePath, text);
        }

        public static bool CheckTableExistence(string tableName)
        {
            bool exists;

            try
            {   
                exists = true;
                DBContextFactory.DBContext.NonQuery("select 1 from " + tableName + " where 1 = 0");                
}
            catch
            {
                exists = false;
            }

            return exists;
        }

        public static bool CheckTableExistence(string connectionString, string tableName)
        {
            bool exists;

            try
            {
                exists = true;
                DBContextFactory.SetConnection(connectionString);
                DBContextFactory.DBContext.OpenConnection();
                DBContextFactory.DBContext.NonQuery("select 1 from " + tableName + " where 1 = 0");
                DBContextFactory.DBContext.CloseConnection();
            }
            catch(Exception e)
            {
                exists = false;
                DBContextFactory.DBContext.CloseConnection();
            }

            return exists;
        }

        public static bool CheckTableHasData(string tableName)
        {
            string sql = "select top 1 * from " + tableName + " where 1 = 1";
            bool exists;

            try
            {
                exists = true;                
                using (SqlCommand sqlCommand = new SqlCommand(sql))
                //using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {

                    try
                    {
                        DataTable dt = DBContextFactory.DBContext.Query(sqlCommand);
                        if (dt.Rows[0] != null)
                            return true;
                    }catch(Exception e)
                    {
                        return false;
                    }
                }                    
            }
            catch
            {
                exists = false;
            }

            return exists;
        }


        public static bool CheckColumnExistence(string col, string table)
        {
            string query = "select {0} from {1} where 1 = 0";

            bool exists;

            try
            {
                exists = true;
                DBContextFactory.DBContext.NonQuery(String.Format(query,col,table));
            }
            catch
            {
                exists = false;
            }

            return exists;
        }
    }
}
