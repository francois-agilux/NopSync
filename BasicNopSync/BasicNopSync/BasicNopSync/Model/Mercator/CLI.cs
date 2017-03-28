using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.Model
{
    class CLI
    {
        public string C_NOM { get; internal set; }
        public string C_REGIME { get; internal set; }
        public string C_EMAIL { get; internal set; }
        public string C_CAT1{ get; internal set; }
        public string C_TARIF { get; internal set; }
        public string C_ID_DEV { get; internal set; }
        public string C_ID { get; internal set; }
        public string C_ADRESSE { get; internal set; }
        public string C_ADRESSE2 { get; internal set; }
        public string C_CODEP { get; internal set; }
        public string C_VILLE { get; internal set; }
        public string C_NUM_TEL { get; internal set; }
        public string C_NUM_GSM { get; internal set; }
        public string C_PAYS { get; internal set; }
        public string C_NUM_TVA { get; internal set; }
        public string C_IDCOMPTA { get; internal set; }
        public string C_CLE1 { get; internal set; }
		public string C_FROM_WEB { get; internal set; }
        public int C_ID_WEB { get; internal set; }
        public decimal C_MODIFTAG { get; internal set; }
        public DateTime C_MODIF { get; internal set; }
        public DateTime C_CREATION{ get; internal set; }
    }
}
