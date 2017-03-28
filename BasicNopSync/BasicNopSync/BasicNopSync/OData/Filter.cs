using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.OData
{
    public class Filter : Statement
    {
        private List<Conditions> Conditions { get; set; }
        private string Combinator { get; set; }

        /// <summary>
        /// Filter with one condition
        /// </summary>
        /// <param name="c"></param>
        public Filter(Conditions c) : base()
        {
            Operation = "filter";

            Conditions = new List<Conditions>();
            Conditions.Add(c);
            Combinator = null;
        }

        /// <summary>
        /// Multiple condition using one combinator
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="combinator"></param>
        public Filter(List<Conditions> conditions, Combinator? combinator) : base()
        {
            Operation = "filter";

            Conditions = conditions;
            Combinator = GetCombinatorValue(combinator);
        }

        /// <summary>
        /// Build a filter query using the Filter object datas
        /// </summary>
        /// <returns></returns>
        public override string BuildQuery()
        {
            string afterEqual = "";

            if (Conditions.Count > 1 && String.IsNullOrWhiteSpace(Combinator))
                throw new NullReferenceException("More than one condition but combinator is null");

            foreach (Conditions c in Conditions)
            {
                if (!Conditions.First().Equals(c))
                    afterEqual += "+" + Combinator + "+";

                afterEqual += c.ToString();
            }

            return String.Format(StatementFormat, Operation, afterEqual);
        }

        private string GetCombinatorValue(Combinator? combinator)
        {
            switch (combinator)
            {
                case OData.Combinator.AND: return "and";
                case OData.Combinator.OR: return "or";
                default: return null;
            }
        }
    }
}
