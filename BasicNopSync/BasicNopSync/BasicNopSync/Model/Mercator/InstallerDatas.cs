using BasicNopSync.WebApi.Datas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.Model.Mercator
{
    public class InstallerDatas
    {
        public string RepMercator { get; set; }
        public string JournalMercator { get; set; }
        public string ConnectionString { get; set; }
        public bool UseGenericArticles { get; set; }

        public AuthParameters authParameters { get; set; }

    }
}
