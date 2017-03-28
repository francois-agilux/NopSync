using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MercatorORM
{
    /// <summary>
    /// Classe abstraite fournissant l'implémentation de base pour un descripteur de signalétique
    /// </summary>
    public class SigDescriptor
    {
        public string Module { get; }
        [Key]
        public string ID { get; }

        public string[] ExcludedColumns { get; }

        public ICollection<FieldDescriptor> FieldDescriptorCollection { get; }

        //TODO : déterminer l'id grâce à la primarey key 
        public SigDescriptor(string Module, string ID, params string[] ExcludedColumns)
        {
            this.Module = Module;
            this.ID = ID;
            this.ExcludedColumns = ExcludedColumns;
            this.FieldDescriptorCollection = new List<FieldDescriptor> ();
            this.Init();
        }        

        private void Init()
        {
            using (SqlConnection conn = new SqlConnection(DBContextFactory.ConnectionString))
            using (SqlCommand comm = new SqlCommand())
            {
                conn.Open();
                comm.Connection = conn;
                string cmdText = "SELECT COLUMN_NAME, COLUMN_DEFAULT, DATA_TYPE, ISNULL(CHARACTER_MAXIMUM_LENGTH,0)";
                cmdText += " FROM INFORMATION_SCHEMA.COLUMNS";
                cmdText += " WHERE TABLE_NAME = @Table AND DATA_TYPE <> 'timestamp'";
                cmdText += " ORDER BY ORDINAL_POSITION";
                comm.CommandText = cmdText;
                comm.Parameters.AddWithValue("@Table", this.Module);

                SqlDataReader reader = comm.ExecuteReader();
                
                while (reader.Read())
                {
                    string ColumnName = reader.GetString(0);
                    //string ColumnDefault = reader.GetString(1);
                    int CharLength = reader.GetInt32(3);
                    string DbType = reader.GetString(2);
                    if (DbType == "numeric")
                        DbType = "float";
                    SqlDbType DataType = (SqlDbType)Enum.Parse(typeof(SqlDbType), DbType, true);

                    this.FieldDescriptorCollection.Add(new FieldDescriptor(ColumnName, DataType, CharLength));
                }
            }
        }

        public IEnumerable<string> GetAllID()
        {
            using (SqlConnection conn = new SqlConnection(DBContextFactory.DBContext.ConnectionString))
            using (SqlCommand comm = new SqlCommand())
            {
                conn.Open();
                comm.Connection = conn;
                string cmdText = "SELECT " + this.ID;
                cmdText += " FROM " + this.Module;
                
                comm.CommandText = cmdText;

                
                SqlDataAdapter adapter = new SqlDataAdapter(comm);
                DataSet ds = new DataSet();
                adapter.Fill(ds);

                return ds.Tables[0].AsEnumerable().Select(x => Convert.ToString(x[0]).Trim());

                
            }
        }


    }

 
    /// <summary>
    /// Descripteur de champ de signalétique
    /// </summary>
    [Serializable]
    public class FieldDescriptor
    {
        public SqlDbType DbType { get; set; }
        public int Length { get; set; }
        public string Name { get; set; }

        public FieldDescriptor()
        {

        }

        public FieldDescriptor(string Name, SqlDbType DbType, int Length)
        {
            this.Name = Name;
            this.DbType = DbType;
            this.Length = Length;
        }

        //TODO : intégrer la dbdefaultvalue dans le fielddescriptor
        public object GetDBDefaultValue()
        {
            switch (this.DbType)
            {
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.VarChar:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                    return "";

                case SqlDbType.Bit:
                case SqlDbType.Int:
                case SqlDbType.SmallInt:
                case SqlDbType.Float:
                case SqlDbType.BigInt:
                case SqlDbType.Decimal:
                    return 0;

                case SqlDbType.DateTime:
                    return new DateTime(1900, 1, 1);

                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Champ de signalétique
    /// </summary>
    [Serializable]
    public class Field
    {
        
        public FieldDescriptor FieldDescriptor { get; set; }

        //public object Value { get { return  (_Value as this.FieldDescriptor.DbType.GetType()); } set { _Value = value; } }
        //public Value Value { get; set; }
        public object Value { get; set; }
        //public Sig Sig { get; set; }

        public Field()
        {
            FieldDescriptor = new FieldDescriptor();
            Value = null;
            
        }

        public void SetValue(object value)
        {
            //Value aValue = new Value();
            //aValue.RealValue = Convert.ToString(value);
            //Value = aValue;
            Value = value;
        }
    }

    public struct Value
    {
        private string _RealValue;
        public string RealValue
        {
            get { return _RealValue; }
            set { _RealValue = value; }
        }
    }
}
