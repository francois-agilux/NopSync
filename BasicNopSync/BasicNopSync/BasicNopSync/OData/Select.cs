using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.OData
{
    public class Select : Statement
    {
        private List<string> Attributes { get; set; }

        public Select() : base()
        {
            Operation = "select";
        }

        public Select(List<string> attributes) : base()
        {
            Operation = "select";

            Attributes = attributes;

        }

        public Select(string attributes) : base()
        {
            Operation = "select";

            Attributes = attributes.Split(',').ToList();
        }

        public override string BuildQuery()
        {
            string terms = "";

            foreach (string s in Attributes)
            {
                if (s != Attributes.First())
                    terms += ",";

                terms += s;
            }

            return String.Format(StatementFormat, Operation, terms);
        }

    }
}
