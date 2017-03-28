using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.OData
{
    public abstract class Statement
    {
        protected const string StatementFormat = "${0}={1}";
        protected string Operation { get; set; }

        public abstract string BuildQuery();
    }
}
