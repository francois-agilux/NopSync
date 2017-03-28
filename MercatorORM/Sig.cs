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
    /// Classe abstraite fournissant l'implémentation de base des signalétiques
    /// </summary>
    [Serializable]
    abstract public class Sig : Entity
    {
        public string TypeID { get; set; }

        public override void SetID()
        {
            Logging.log.DebugFormat("Sig.SetID : ID.Value 1 = {0}", this.ID.Value);
            if (string.IsNullOrEmpty(Convert.ToString(this.ID.Value)))
            {              
                switch(TypeID)
                {
                    case "NON":
                        this.ID.SetValue(Api.Ident());
                        break;
                    case "CHRONO":
                        this.ID.SetValue(DBContextFactory.DBContext.GetValue<string>("SELECT ISNULL(MAX(CAST("+this.ID.FieldDescriptor.Name+" AS INT)),0)+1 FROM " + this.Module));
                        break;
                    default:
                        this.ID.SetValue(Api.Ident());
                        break;
                }
            }
            Logging.log.DebugFormat("Sig.SetID : ID.Value 2 = {0}", this.ID.Value);
        }

        public override bool Create()
        {
            //TODO : ajouter message d'erreur
            if (string.IsNullOrEmpty(this.GetNameField().Value.ToString()))
                return false;

            this.GetCreateField().SetValue(DateTime.Now);
            this.GetUpdateField().SetValue(DateTime.Now);
            DataSet DefaultValues = Api.DataSetFromXmlString(this.GetSigDefaultValues());
            DataRow dr = DefaultValues.Tables[0].Rows[0];
          
            foreach (DataColumn aColumn in DefaultValues.Tables[0].Columns)
            {                
                var Field = this.Fields.Where(x => x.FieldDescriptor.Name.Equals(aColumn.ColumnName)).DefaultIfEmpty(null).First();
                if (Field != null)                    
                    Field.SetValue(dr[aColumn.ColumnName]);
            }

            return base.Create();
        }

        public override bool Update()
        {
            //TODO : ajouter message d'erreur
            if (string.IsNullOrEmpty(this.GetNameField().Value.ToString()))
                return false;

            this.GetUpdateField().SetValue(DateTime.Now);
            return base.Update();
        }

        /// <summary>
        /// Récupère le XML reprenant les valeur par défaut de la signalétique en cours dans la DB Mercator
        /// </summary>
        /// <returns></returns>
        private string GetSigDefaultValues()
        {
            using (SqlConnection conn = new SqlConnection(DBContextFactory.DBContext.ConnectionString))
            using (SqlCommand comm = new SqlCommand())
            {
                conn.Open();
                comm.Connection = conn;
                string cmdText = "SELECT VALEUR";
                cmdText += " FROM PARAMSMEMO" ;
                cmdText += " WHERE TYPE = @DEF_" + this.Module;
                comm.CommandText = cmdText;
                comm.Parameters.AddWithValue("@DEF_" + this.Module, "DEF_" + this.Module);
                SqlDataReader reader = comm.ExecuteReader();
                if (reader.Read())
                {
                    return reader.GetString(0);
                }
            }

            return null;
        }
        
        /// <summary>
        /// Récupère le champ tampon de mise à jour de la signalétique
        /// </summary>
        /// <returns></returns>
        private Field GetUpdateField()
        {
            string ModuleLetter = this.Module.First().ToString();
            return this.Fields.Where(x => x.FieldDescriptor.Name.Equals(ModuleLetter + "_MODIF")).DefaultIfEmpty(null).First();
        }

        /// <summary>
        /// Récupère le champ tampon de création de la signalétique
        /// </summary>
        /// <returns></returns>
        private Field GetCreateField()
        {
            string ModuleLetter = this.Module.First().ToString();
            return this.Fields.Where(x => x.FieldDescriptor.Name.Equals(ModuleLetter + "_CREATION")).DefaultIfEmpty(null).First();

        }

        /// <summary>
        /// Récupère le champ nom de la signalétique
        /// </summary>
        /// <returns></returns>
        private Field GetNameField()
        {
            string ModuleLetter = this.Module.First().ToString();
            string NameSuffix = "_NOM";

            if (this.Module == SigEnum.STOCK)
            {
                NameSuffix = "_MODELE";
            }

            return this.Fields.Where(x => x.FieldDescriptor.Name.Equals(ModuleLetter + NameSuffix)).DefaultIfEmpty(null).First();

        }

        /// <summary>
        /// Initialise une instance de signalétique.
        /// </summary>
        /// <param name="SigDescriptor">Descripteur de la signalétique à initialiser</param>
        public Sig(SigDescriptor SigDescriptor) : base(SigDescriptor)
        {
            OptionsMercator optionsMercator = new OptionsMercator();

            switch (this.Module)
            {
                case SigEnum.CLI:
                    TypeID = optionsMercator.GetOptionValue("N_CLI_AUTO").ToString().Trim();
                    break;
                case SigEnum.FOU:
                    TypeID = optionsMercator.GetOptionValue("N_FOU_AUTO").ToString().Trim();
                    break;
                case SigEnum.STOCK:
                    TypeID = optionsMercator.GetOptionValue("N_ART_AUTO").ToString().Trim();
                    break;
                default:
                    TypeID = string.Empty;
                    break;
            }
        }        
    }

    /// <summary>
    /// Signalétique client
    /// </summary>
    public class SigCli : Sig
    {       
        //[Key]
        //string C_ID { get; set; }
        public SigCli() : base(DBContextFactory.DBContext.Descriptors[SigEnum.CLI].Value)
        { }        
    }

    /// <summary>
    /// Signalétique fournisseur
    /// </summary>
    public class SigFou : Sig
    {
        public SigFou() : base(DBContextFactory.DBContext.Descriptors[SigEnum.FOU].Value)
        { }
    }

    /// <summary>
    /// Signalétique article
    /// </summary>
    public class SigStock : Sig
    {
        public SigStock() : base(DBContextFactory.DBContext.Descriptors[SigEnum.STOCK].Value)
        { }
    }

    /// <summary>
    /// Enumération reprenant les types possibles de signalétiques
    /// </summary>
    public class SigEnum
    {
        public const string CLI = "CLI";
        public const string FOU = "FOU";
        public const string STOCK = "STOCK";
        public const string XLEAD = "XLEAD";
        public const string ANA = "ANA";
        public const string GEN = "GEN";
        public const string DEST = "DEST";
    }
}
