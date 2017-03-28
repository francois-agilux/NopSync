using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.OData
{
    public class Conditions
    {
        public Conditions(string attribute, Operators op, object value)
        {
            Attribute = attribute;
            Op = op;
            Value = GetFormatValue(value);         
        }

        private string Attribute { get; set; }
        private Operators Op { get; set; }
        private string Value { get; set; }


        private string GetFormatValue(object v)
        {
            if(v == null)
            {
                return "null";
            }
            else if(v.GetType() == typeof(String))
            {
                return "'"+v.ToString()+"'";
            }
            else if(v.GetType() == typeof(bool))
            {
                return (bool)v ? "true" : "false";
            }            

            return v.ToString();

        }


        public override string ToString()
        {
            switch (Op)
            {
                case Operators.EQ:
                    return Attribute + "+eq+" + Value;
                case Operators.NE:
                    return Attribute + "+ne+" + Value;
                case Operators.GT:
                    return Attribute + "+gt+" + Value;
                case Operators.LT:
                    return Attribute + "+lt+" + Value;
                case Operators.STARTSWITH:
                    return String.Format("startswith({0},{1})", Attribute, Value);
                default:
                    throw new Exception("Operator not implemented");
            }
        }

    }
}
