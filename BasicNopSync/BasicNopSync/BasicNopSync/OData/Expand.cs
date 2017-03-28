using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.OData
{
    public class Expand : Statement
    {
        private string Collection { get; set; }

        public Expand(string collection) : base()
        {
            Operation = "expand";

            Collection = collection;
        }

        public override string BuildQuery()
        {
            return String.Format(StatementFormat, Operation, Collection);
        }
    }
}
