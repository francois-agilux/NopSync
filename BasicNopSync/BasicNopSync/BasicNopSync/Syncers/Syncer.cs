using BasicNopSync.Database;
using BasicNopSync.OData;
using BasicNopSync.Utils;
using MercatorORM;
using System;
using System.Globalization;

namespace BasicNopSync.Syncers
{
    public abstract class Syncer
    {   
        protected DBContext dbContext;
        protected UrlBuilder urlBuilder;
        protected DataSettings dataSettings;// = ConfigurationManager.ConnectionStrings["Mercator"].ConnectionString;        
		protected TextInfo textInfo;
        protected OptionsMercator OptionsMercator;

        public Syncer()
        {
            dataSettings = DatabaseManager.LoadSettings();

            if(String.IsNullOrWhiteSpace(DBContextFactory.ConnectionString))
                DBContextFactory.SetConnection(dataSettings.DataConnectionString);

            if(dbContext == null)
                dbContext = DBContextFactory.DBContext;
           if(textInfo == null)
                textInfo = new CultureInfo("fr-FR", false).TextInfo;
            if (OptionsMercator == null)
                OptionsMercator = new OptionsMercator();
        }
        
        public abstract bool Sync();
              
        
    }
}
