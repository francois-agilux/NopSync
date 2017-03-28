using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.Database
{
    public class DataSettings
    {
        public static string DCSFormat = "Data Source={0};Initial Catalog={1};Integrated Security=False;User={2};Password={3};";
		public string DataSource { get; set; }
        public string Catalog { get; set; } 
        public string User { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        public DataSettings()
        {
            RawDataSettings = new Dictionary<string, string>();
        }

        /// <summary>
        /// Connection string
        /// </summary>
        public string DataConnectionString { get; set; }

        /// <summary>
        /// Raw settings file
        /// </summary>
        public IDictionary<string, string> RawDataSettings { get; private set; }

        /// <summary>
        /// A value indicating whether entered information is valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return !String.IsNullOrEmpty(this.DataConnectionString);
        }

        public void BuildConnectionString(string dataSource, string catalog, string user, string password)
        {
            DataConnectionString = String.Format(DCSFormat, dataSource, catalog, user, password);
        }

        public void ExtractDatasFromConnectionString()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(this.DataConnectionString);
            this.Catalog = builder.InitialCatalog;
            this.DataSource = builder.DataSource;
            this.User = builder.UserID;
            this.Password = builder.Password;
        }
    }
}
