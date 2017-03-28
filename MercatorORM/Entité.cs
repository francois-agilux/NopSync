using System;   
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Xml.Linq;
using MercatorApi;

namespace MercatorORM
{
    /// <summary>
    /// Classe abstraite fournissant l'implémentation de base des entités Mercator
    /// </summary>
    abstract public class Entity : IRecord
    {
        private string _Module;
        public string Module
        {
            get { return _Module; }
            internal set { _Module = value; }
        }

        public ICollection<Field> Fields { get; set; }

        private SigDescriptor _SigDescriptor;
        public SigDescriptor SigDescriptor
        {
            get { return _SigDescriptor; }
            internal set { _SigDescriptor = value; }
        }

        [Key]
        public Field ID { get; set; }

        public Field GetField(string name)
        {
            return this.Fields.FirstOrDefault(x => name.ToLower().Equals(x.FieldDescriptor.Name.ToLower()));
        }

        public bool SetValueForField(string name, object value)
        {
            Field aField = GetField(name);
            if (aField != null)
            {
                aField.SetValue(value);
                return true;
            }
            //TODO : log l'erreur
            return false;
        }

        public virtual void SetID()
        {
            Logging.log.DebugFormat("Entity.SetID : ID.Value 1 = {0}", this.ID.Value);
            if (string.IsNullOrEmpty(Convert.ToString(this.ID.Value)))
            {                
                this.ID.SetValue(Api.Ident());                
            }
            Logging.log.DebugFormat("Entity.SetID : ID.Value 2 = {0}", this.ID.Value);
        }

        public virtual bool Create()
        {
            SetID();

            //using (SqlConnection conn = new SqlConnection(DBContextFactory.DBContext.ConnectionString))
            using (SqlCommand comm = new SqlCommand())
            {
                //conn.Open();                
                //comm.Connection = conn;
                var InsertFields = SigDescriptor.FieldDescriptorCollection.Where(x => !this.SigDescriptor.ExcludedColumns.Contains(x.Name));
                string cmdText = "INSERT INTO " + SigDescriptor.Module;
                cmdText += " ( " + string.Join(", ", InsertFields.Select(x => x.Name)) + " ) ";
                cmdText += " VALUES ( " + string.Join(", ", InsertFields.Select(x => ToSQLParam(x.Name))) + " )";

                comm.CommandText = cmdText;
                Logging.log.DebugFormat("Entity.Create : {0} = '{1}'", this.ID.FieldDescriptor.
                    Name, this.ID.Value);                
                buildCreateParameters(comm);

                //return comm.ExecuteNonQuery() > 0;
                return DBContextFactory.DBContext.NonQuery(comm) > 0;
            }
        }

        /// <summary>
        /// Lit les données d'un enregistrement sur base du champ identité donné en paramètre.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="IDField"></param>
        /// <returns></returns>
        public bool Read(object ID, string IDField = null)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(DBContextFactory.DBContext.ConnectionString))
            using (SqlCommand comm = new SqlCommand())
            {
                conn.Open();
                comm.Connection = conn;
                string cmdText = "SELECT " + string.Join(", ", SigDescriptor.FieldDescriptorCollection.Select(x => x.Name));
                cmdText += " FROM " + SigDescriptor.Module;
                cmdText += " WHERE " + (IDField ?? SigDescriptor.ID) + " = @ID_Value";

                comm.Parameters.AddWithValue("@ID_Value", ID);
                comm.CommandText = cmdText;

                SqlDataReader reader = comm.ExecuteReader();
                if (reader.Read())
                {
                    foreach (var FieldDescriptor in SigDescriptor.FieldDescriptorCollection)
                    {
                        this.GetField(FieldDescriptor.Name).SetValue(reader[FieldDescriptor.Name]);
                        if (IsID(FieldDescriptor))
                            this.ID = this.GetField(FieldDescriptor.Name); // reader[FieldDescriptor.Name];
                    }
                    result = true;
                }
                reader.Close();
            }

            return result;
        }

        public virtual bool Update(SqlConnection conn)
        {            
            using (SqlCommand comm = new SqlCommand())
            {                
                comm.Connection = conn;
                string cmdText = "UPDATE " + SigDescriptor.Module;
                cmdText += " SET " + UpdateParameters();
                cmdText += " WHERE " + SigDescriptor.ID + " = @ID_Value";

                comm.Parameters.AddWithValue("@ID_Value", this.ID.Value);
                comm.CommandText = cmdText;
                buildUpdateParameters(comm);

                return comm.ExecuteNonQuery() > 0;
            }
        }

        public virtual bool Update()
        {
            using (SqlConnection conn = new SqlConnection(DBContextFactory.DBContext.ConnectionString))
            using (SqlCommand comm = new SqlCommand())
            {
                conn.Open();
                comm.Connection = conn;
                string cmdText = "UPDATE " + SigDescriptor.Module;
                cmdText += " SET " + UpdateParameters();
                cmdText += " WHERE " + SigDescriptor.ID + " = @ID_Value";

                comm.Parameters.AddWithValue("@ID_Value", this.ID.Value);
                comm.CommandText = cmdText;
                buildUpdateParameters(comm);

                return comm.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete()
        {
            using (SqlConnection conn = new SqlConnection(DBContextFactory.DBContext.ConnectionString))
            using (SqlCommand comm = new SqlCommand())
            {
                conn.Open();
                comm.Connection = conn;
                string cmdText = "DELETE FROM " + this.Module; ;
                cmdText += " WHERE " + this.SigDescriptor.ID + " = @ID_Value";
                comm.CommandText = cmdText;
                comm.Parameters.AddWithValue("@ID_Value", this.ID.Value);

                return comm.ExecuteNonQuery() > 0;
            }
        }

        public bool Exists(string IDField = null, object ID = null)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(DBContextFactory.DBContext.ConnectionString))
            using (SqlCommand comm = new SqlCommand())
            {
                conn.Open();
                comm.Connection = conn;
                string cmdText = "SELECT " + (IDField ?? SigDescriptor.ID);
                cmdText += " FROM " + SigDescriptor.Module;
                cmdText += " WHERE " + (IDField ?? SigDescriptor.ID) + " = @ID_Value";

                comm.Parameters.AddWithValue("@ID_Value", ID ?? this.ID.Value);
                comm.CommandText = cmdText;

                SqlDataReader reader = comm.ExecuteReader();                                    
                result = reader.HasRows;                
                reader.Close();
            }
            return result;
        }

        /// <summary>
        /// Détermine si le FieldDescriptor est le champ ID de la signalétique
        /// </summary>
        /// <param name="FieldDescriptor"></param>
        /// <returns></returns>
        private bool IsID(FieldDescriptor FieldDescriptor)
        {
            return FieldDescriptor.Name.Equals(this.SigDescriptor.ID);
        }

        /// <summary>
        /// Initialise une instance de signalétique.
        /// </summary>
        /// <param name="SigDescriptor">Descripteur de la signalétique à initialiser</param>
        public Entity(SigDescriptor SigDescriptor = null)
        {
            //if(DBContextFactory.DBContext.Entities.TryGetValue(SigDescriptor.Module)            
            if(SigDescriptor != null)
                CreateEntity(SigDescriptor);
        }

        internal void CreateEntity(SigDescriptor SigDescriptor)
        {
            this.SigDescriptor = SigDescriptor;
            this.Module = this.SigDescriptor.Module;
            this.Fields = new List<Field>();


            foreach (var FieldDescriptor in this.SigDescriptor.FieldDescriptorCollection)
            {
                Field Field = new Field();
                Field.FieldDescriptor = FieldDescriptor;
                ///Field.Sig = this;
                Field.SetValue(Field.FieldDescriptor.GetDBDefaultValue());
                Fields.Add(Field);
                if (IsID(FieldDescriptor))
                    this.ID = Field;
            }
        }

        /// <summary>
        /// Construit une chaîne avec les champs de la signalétique afin de les utiliser dans une commande SQL UPDATE
        /// </summary>
        /// <returns></returns>
        private string UpdateParameters()
        {
            List<string> valuesList = new List<string>();
            foreach (var aField in this.Fields.Where(x => !this.SigDescriptor.ExcludedColumns.Contains(x.FieldDescriptor.Name)))
            {
                //TODO : exclure les colonnes identité
                if (!IsID(aField.FieldDescriptor))
                    valuesList.Add(aField.FieldDescriptor.Name + "=" + ToSQLParam(aField.FieldDescriptor.Name));
            }
            return string.Join(",", valuesList);

        }

        /// <summary>
        /// Transforme une chaîne en paramètre SQL
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ToSQLParam(string value)
        {
            return "@" + value.ToUpper();
        }

        /// <summary>
        /// Fournit les paramètres à utiliser pour la commande SQL UPDATE sqlCommand
        /// </summary>
        /// <param name="sqlCommand">Commande SQL UPDATE</param>
        private void buildUpdateParameters(SqlCommand sqlCommand)
        {
            List<string> valuesList = new List<string>();
            foreach (var aField in this.Fields)
            {
                if (!IsID(aField.FieldDescriptor))
                    sqlCommand.Parameters.AddWithValue(ToSQLParam(aField.FieldDescriptor.Name), aField.Value);
            }
        }

        /// <summary>
        /// Fournit les paramètres à utiliser pour la commande SQL INSERT sqlCommand
        /// </summary>
        /// <param name="sqlCommand">Commande SQL INSERT</param>
        private void buildCreateParameters(SqlCommand sqlCommand)
        {
            List<string> valuesList = new List<string>();
            foreach (var aField in this.Fields)
            {
                sqlCommand.Parameters.AddWithValue(ToSQLParam(aField.FieldDescriptor.Name), aField.Value);
            }
        }

        /// <summary>
        /// Méthode basique renvoyant une DataTable qui contient les entrées correspondant aux critères définis par les paramètres
        /// </summary>
        /// <param name="field">Champ à vérifier dans la clause</param>
        /// <param name="value">Valeur à vérifier dans la clause</param>
        /// <returns></returns>
        public DataTable Where(string whereClause)
        {
            using (SqlCommand comm = new SqlCommand())
            {
                comm.CommandText = "SELECT " + string.Join(", ", SigDescriptor.FieldDescriptorCollection.Select(x => x.Name));
                comm.CommandText += " FROM "+this.Module+" WHERE " + whereClause;
                return DBContextFactory.DBContext.Query(comm);
            }
        }

        /// <summary>
        /// Lit les données d'un enregistrement sur base de la clause donnée en paramètre. Si la requête retourne plusieurs enregistrements, seul le premier est lu.
        /// </summary>
        /// <param name="whereClause"></param>
        /// <returns></returns>
        public bool ReadWhere(string whereClause)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(DBContextFactory.DBContext.ConnectionString))
            using (SqlCommand comm = new SqlCommand())
            {
                conn.Open();
                comm.Connection = conn;
                string cmdText = "SELECT " + string.Join(", ", SigDescriptor.FieldDescriptorCollection.Select(x => x.Name));
                cmdText += " FROM " + SigDescriptor.Module;
                cmdText += " WHERE " + whereClause;
                
                comm.CommandText = cmdText;

                SqlDataReader reader = comm.ExecuteReader();
                if (reader.Read())
                {
                    foreach (var FieldDescriptor in SigDescriptor.FieldDescriptorCollection)
                    {
                        this.GetField(FieldDescriptor.Name).SetValue(reader[FieldDescriptor.Name]);
                        if (IsID(FieldDescriptor))
                            this.ID = this.GetField(FieldDescriptor.Name); // reader[FieldDescriptor.Name];
                    }
                    result = true;
                }
                reader.Close();
            }

            return result;
        }

        public int SetValue(string fieldName, object value)
        {
            SqlCommand comm = new SqlCommand("UPDATE "+this.Module+ " SET "+fieldName +"=@VALUE WHERE "+this.ID.FieldDescriptor.Name + "=@ID");
            comm.Parameters.AddWithValue("@ID", this.ID.Value);
            comm.Parameters.AddWithValue("@VALUE", value);
            return DBContextFactory.DBContext.NonQuery(comm);
        }
    }

   
    public class Rayon : Entity
    {        
        public Rayon() : base(DBContextFactory.DBContext.Descriptors[EntityEnum.RAYONS].Value)
        { }

    }

    public class Famille : Entity
    {
        public Famille() : base(DBContextFactory.DBContext.Descriptors[EntityEnum.FAMILLES].Value)
        { }
    }

    public class SSFamil : Entity
    {
        public SSFamil() : base(DBContextFactory.DBContext.Descriptors[EntityEnum.SS_FAMIL].Value)
        { }
    }

    public class CatStock : Entity
    {
        public CatStock() : base(DBContextFactory.DBContext.Descriptors[EntityEnum.CAT_STCK].Value)
        { }
    }

    public class Cat : Entity
    {
        public Cat(string type) : base()
        {           
            Lazy<SigDescriptor> sig;
            if(!DBContextFactory.DBContext.Descriptors.TryGetValue(type, out sig))
            {
                DBContextFactory.DBContext.Descriptors[type] = new Lazy<SigDescriptor>(() => new SigDescriptor(type, "ID"));
            }
            CreateEntity(DBContextFactory.DBContext.Descriptors[type].Value);
        }
    }

    public class PiedsV : Entity
    {
        public PiedsV() : base(DBContextFactory.DBContext.Descriptors[EntityEnum.PIEDS_V].Value)
        { 
        }
    }

    public class Table : Entity
    {
        public Table(string tableName, string tableId) : base()
        {
            Lazy<SigDescriptor> sig;
            if (!DBContextFactory.DBContext.Descriptors.TryGetValue(tableName, out sig))
            {
                DBContextFactory.DBContext.Descriptors[tableName] = new Lazy<SigDescriptor>(() => new SigDescriptor(tableName, tableId));
            }
            CreateEntity(DBContextFactory.DBContext.Descriptors[tableName].Value);
        }
    }

    public class OptionsMercator : Entity
    {
        
        public OptionsMercator() : base(DBContextFactory.DBContext.Descriptors[EntityEnum.OPTIONS].Value)
        {
        }

        public object GetOptionValue(string option)
        {
            if (this.Read(option, "TYPE"))
                return this.GetField("VALEUR").Value;
            else
                return null;
        }

        public int SetOptionValue(string option, object value)
        {
            SqlCommand comm = new SqlCommand("UPDATE " + this.Module + " SET VALEUR=@VALUE WHERE TYPE=@ID");
            comm.Parameters.AddWithValue("@ID", option);
            comm.Parameters.AddWithValue("@VALUE", value);
            return DBContextFactory.DBContext.NonQuery(comm);
        }
    }

    public class GamEnum : Entity
    {
        public GamEnum() : base(DBContextFactory.DBContext.Descriptors[EntityEnum.GAMENUM].Value)
        { }
    }


    /// <summary>
    /// Enumération reprenant les types possibles de signalétiques
    /// </summary>
    public class EntityEnum
    {
        public const string RAYONS = "RAYONS";
        public const string FAMILLES = "FAMILLES";
        public const string SS_FAMIL = "SS_FAMIL";
        public const string OPTIONS = "OPTIONS";
        public const string CAT_STCK = "CAT_STCK";
        public const string PIEDS_V = "PIEDS_V";
        public const string GAMENUM = "GAMENUM";
    }
}
