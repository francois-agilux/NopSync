using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.OData
{
    public class UrlBuilder
    {
        private string EntityIdFormat = "{0}({1})";        
        private string EntityName = "";
        private int? EntityId;

        private List<Conditions> filters;
        private List<string> select;
        private string expand;

        private Combinator? combinator;

        /// <summary>
        /// Initiate filters (AND by default) and select attribute
        /// </summary>
        public UrlBuilder()
        {
            filters = new List<Conditions>();
            select = new List<string>();

            combinator = Combinator.AND;
        }

        /// <summary>
        /// Initiate filters (AND by default) and select attribute and entity Name
        /// </summary>
        public UrlBuilder(string entityName)
        {
            filters = new List<Conditions>();
            select = new List<string>();
            EntityName = entityName;

            combinator = Combinator.AND;
        }
        
        public UrlBuilder Entity(string entityName)
        {
            EntityName = entityName;
            return this;
        }

        public UrlBuilder Id(int id)
        {
            EntityId = id;

            return this;
        }
        
        public UrlBuilder Select(params string[] attributes)
        {
            select = attributes.ToList();

            return this;
        }        

        public UrlBuilder Expand(string collection)
        {
            expand = collection;

            return this;
        }
        
        /// <summary>
        /// filter=attribute+eq+value
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public UrlBuilder FilterEq(string attribute, object value)
        {
            Conditions c = new Conditions(attribute, Operators.EQ, value);

            filters.Add(c);

            return this;
        }

        /// <summary>
        /// filter=attribute+ne+value
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public UrlBuilder FilterNe(string attribute, object value)
        {
            Conditions c = new Conditions(attribute, Operators.NE, value);

            filters.Add(c);

            return this;
        }

        /// <summary>
        /// filter=attribute+gt+value
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public UrlBuilder FilterGt(string attribute, object value)
        {
            Conditions c = new Conditions(attribute, Operators.GT, value);

            filters.Add(c);

            return this;
        }

        /// <summary>
        /// filter=attribute+lt+value
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public UrlBuilder FilterLt(string attribute, object value)
        {
            Conditions c = new Conditions(attribute, Operators.LT, value);

            filters.Add(c);

            return this;
        }

        /// <summary>
        /// filter=startswith(attribute,value)
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public UrlBuilder FilterStartsWith(string attribute, object value)
        {
            Conditions c = new Conditions(attribute, Operators.STARTSWITH, value);

            filters.Add(c);

            return this;
        }
        
        /// <summary>
        /// Specify that the conditions in the filter must be combined with the and keyword
        /// </summary>
        /// <returns></returns>
        public UrlBuilder And()
        {
            combinator = Combinator.AND;
            return this;
        }

        /// <summary>
        /// Specify that the conditions in the filter must be combined with the or keyword
        /// </summary>
        /// <returns></returns>
        public UrlBuilder Or()
        {
            combinator = Combinator.OR;
            return this;
        }


        /// <summary>
        /// Build the query 
        /// </summary>
        /// <returns></returns>
        public string BuildQuery()
        {
            List<Statement> statements = new List<Statement>();

            if (String.IsNullOrWhiteSpace(EntityName))
                throw new NullReferenceException("No entity");

            if (select.Count > 0)
                statements.Add(new Select(select));

            if (filters.Count >0)
                statements.Add(new Filter(filters, combinator));

            if (!String.IsNullOrWhiteSpace(expand))
                statements.Add(new Expand(expand));
            
            return BuildQuery(statements);
        }
       
        /// <summary>
        /// Return the following query : entity/[list of statement]
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="st"></param>
        /// <returns></returns>
        public string BuildQuery(List<Statement> st)
        {   
            string query = EntityName + (EntityId == null ? "" : String.Format("({0})", EntityId)) + "?";

            foreach(Statement s in st)
            {
                if (st.First() != s)
                    query += "&";
                query += s.BuildQuery();
            }

            filters?.Clear();
            select?.Clear();
            expand = "";
            EntityId = null;            

            return query;
        }

        /// <summary>
        /// Return the following query : EntityName(Id)/Collection/$ref
        /// Or an exception if EntityName is not initialiazed
        /// </summary>
        /// <param name="id"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public string BuildQueryRef(int id, string collection)
        {
            if (String.IsNullOrWhiteSpace(EntityName))
                throw new NullReferenceException("EntityName has not been initialized");

            return BuildQueryRefWithEntity(EntityName, id, collection);
        }

        /// <summary>
        /// Return the following query : entity/Default.Function/
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public string BuildQueryFct(string function)
        {
            if (String.IsNullOrWhiteSpace(EntityName))
                throw new NullReferenceException("EntityName has not been initialized");

            return String.Format("{0}/Default.{1}/", EntityName, function);
        }

        /// <summary>
        /// Return the following query : entity(id)/collection/$ref
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public string BuildQueryRefWithEntity(string entity, int id, string collection)
        {
            return String.Format("{0}({1})/{2}/$ref",entity,id,collection);
        }
    }
}
