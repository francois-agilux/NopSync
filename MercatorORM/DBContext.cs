using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace MercatorORM
{
    /// <summary>
    /// Classe permettant un accès paresseux à un singleton DBContext
    /// </summary>
    public static class DBContextFactory
    {

        public static DBContext _DBContext;
         
        public static DBContext DBContext {
            get {
                return GetDBContext();
            }
        }

        public static string ConnectionString;

        public static void SetConnection(string ConnectionString)
        {
            DBContextFactory.ConnectionString = ConnectionString;
        }

        private static DBContext GetDBContext()
        {
            if (_DBContext == null)
                _DBContext = new DBContext(ConnectionString);

            return _DBContext;
        }

        public static void Init(string Connectionstring)
        {
            if(_DBContext == null)
                _DBContext = new DBContext(ConnectionString);
        }
    }

    /// <summary>
    /// Représente la base de données Mercator
    /// </summary>
    public class DBContext
    {
        public Dictionary<string, Lazy<SigDescriptor>> Descriptors { get; set; }
        public Dictionary<string, Entity> Entities { get; set; }
        public SqlTransaction Transaction { get; set; }

        public SqlConnection Connection { get; set; }

        public string ConnectionString { get;  }

        /// <summary>
        /// Initialise le contexte de base de données
        /// </summary>
        /// <param name="ConnectionString">ConnectionString SQL</param>
        internal DBContext(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
            this.Init();
        }

        /// <summary>
        /// Initialise les descripteurs de signalétiques disponibles
        /// </summary>
        private void Init()
        {
            Descriptors = new Dictionary<string, Lazy<SigDescriptor>>();
            Descriptors[SigEnum.CLI] = new Lazy<SigDescriptor>(() => new SigDescriptor(SigEnum.CLI, "C_ID"));
            Descriptors[SigEnum.FOU] = new Lazy<SigDescriptor>(() => new SigDescriptor(SigEnum.FOU, "F_ID"));
            Descriptors[SigEnum.STOCK] = new Lazy<SigDescriptor>(() => new SigDescriptor(SigEnum.STOCK, "S_ID"));           
            Descriptors[EntityEnum.RAYONS] = new Lazy<SigDescriptor>(() => new SigDescriptor(EntityEnum.RAYONS, "ID"));
            Descriptors[EntityEnum.FAMILLES] = new Lazy<SigDescriptor>(() => new SigDescriptor(EntityEnum.FAMILLES, "ID"));
            Descriptors[EntityEnum.SS_FAMIL] = new Lazy<SigDescriptor>(() => new SigDescriptor(EntityEnum.SS_FAMIL, "ID"));            
            Descriptors[EntityEnum.OPTIONS] = new Lazy<SigDescriptor>(() => new SigDescriptor(EntityEnum.OPTIONS, "TYPE", "RECNO"));
            Descriptors[EntityEnum.CAT_STCK] = new Lazy<SigDescriptor>(() => new SigDescriptor(EntityEnum.CAT_STCK, "ID"));
            Descriptors[EntityEnum.PIEDS_V] = new Lazy<SigDescriptor>(() => new SigDescriptor(EntityEnum.PIEDS_V, "ID"));
            Descriptors[EntityEnum.GAMENUM] = new Lazy<SigDescriptor>(() => new SigDescriptor(EntityEnum.GAMENUM, "ID"));

        }

        public void AddDescriptor(string Name, string Id)
        {
            Descriptors[Name] = new Lazy<SigDescriptor>(() => new SigDescriptor(Name, Id));
        }


        public DataTable Query(string req)
        {
            using (SqlCommand command = new SqlCommand(req))
            {
                return Query(command);
            }
        }

        public DataTable Query(SqlCommand command)
        {
            //using (SqlConnection conn = new SqlConnection(this.ConnectionString))
            //{
            //    conn.Open();
            //    command.Connection = conn;
            //    using(var reader = command.ExecuteReader())
            //    {
            //        var tb = new DataTable();
            //        tb.Load(reader);
            //        return tb;
            //    }
            //}
            var tb = new DataTable();
            OpenConnection();
            command.Connection = Connection;
            command.Transaction = Transaction;
            using (var reader = command.ExecuteReader())
            {                
                tb.Load(reader);                
            }
            CloseConnection();
            return tb;
        }

        public int NonQuery(string req, params object[] parameters)
        {
            using (SqlCommand command = new SqlCommand(req))
            {
                if(parameters.Count() > 0)
                {
                    var parametersKeys = Regex.Matches(req, "@p[0-9]+");//.OfType<Match>().OrderBy(x => x.Value);

                    foreach (Match key in parametersKeys)
                        command.Parameters.AddWithValue(key.Value, parameters[Convert.ToInt16(key.Value.Substring(2))]);
                }
                return NonQuery(command);
            }   
        }

        public int NonQuery(SqlCommand command)
        {
            //using (SqlConnection conn = new SqlConnection(this.ConnectionString))
            //{
            //    conn.Open();
            //    command.Connection = conn;
            //    return command.ExecuteNonQuery();                
            //}
            
            OpenConnection();
            command.Connection = Connection;
            command.Transaction = Transaction;

            var res = command.ExecuteNonQuery();
            CloseConnection();

            return res;
        }

        /// <summary>
        /// Renvoie une valeur simple depuis la DB
        /// </summary>
        /// <typeparam name="T">Type de la valeur à renvoyer</typeparam>
        /// <param name="command">Commande SQL à exécuter</param>
        /// <returns>La valeur de type T si la requête renvoie un résultat</returns>
        public T GetValue<T>(SqlCommand command)
        {
            if (!command.CommandText.StartsWith("SELECT", StringComparison.CurrentCultureIgnoreCase))
                throw new Exception("La requête doit être de type SELECT.");

            DataTable dt = this.Query(command);
            if (dt.Rows.Count > 0)
                return (T) Convert.ChangeType(dt.Rows[0][0], typeof(T));
            else
                return default(T);
        }

        /// <summary>
        /// Renvoie une valeur simple depuis la DB
        /// </summary>
        /// <typeparam name="T">Type de la valeur à renvoyer</typeparam>
        /// <param name="req">Chaîne contenant la requête à exécuter</param>
        /// <returns>La valeur de type T si la requête renvoie un résultat</returns>
        public T GetValue<T>(string req)
        {
            using (SqlCommand command = new SqlCommand(req))
            {
                return this.GetValue<T>(command);
            }
        }
        
        public void OpenConnection()
        {
            if(Connection == null)
            {
                Connection = new SqlConnection(ConnectionString);
                Connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (Transaction == null && Connection != null)
            {                
                Connection.Close();
                Connection = null;
            }
        }

        public void BeginTransaction()
        {
            if (Transaction == null)
            {
                if (Connection == null)
                    OpenConnection();
                Transaction = Connection.BeginTransaction();
            }
        }

        public void CommitTransaction()
        {
            if (Transaction != null && Connection?.State == ConnectionState.Open)
            {
                Transaction.Commit();
                Transaction = null;
                CloseConnection();
            }
        }

        public void RollbackTransaction()
        {
            if (Transaction != null && Connection?.State == ConnectionState.Open)
            {
                Transaction.Rollback();
                Transaction = null;
                CloseConnection();
            }
        }
    }
}
