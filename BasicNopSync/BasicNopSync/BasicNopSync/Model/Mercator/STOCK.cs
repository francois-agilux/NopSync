using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.Model
{
    public class STOCK
    {
        public string S_ID { get; set; }
        public string S_MODELE { get; set; }
        public string S_CLE1 { get; set; }
        public string S_CLE2 { get; set; }
        public string S_CLE3 { get; set; }
        public bool S_SOMMEIL { get; set; }
        public string S_MEMO { get; set; }        
        public string S_DESCRIPTION { get; set; }
        public decimal S_PRIX_HT { get; set; }
        public decimal S_TAUX_TVA { get; set; }
        public decimal S_PRIX_TI { get; set; }
        public string S_ID_RAYON { get; set; }
        public string S_ID_FAMIL { get; set; }
        public string S_ID_SSFAM { get; set; }
        public string S_CAT1 { get; set; }
        public string S_CAT2 { get; set; }
        public string S_CAT3 { get; set; }
        //TODO : une fois que ce sera géré convenablement avec les SIG STOCK, gérer la synchro des S_CAT (1 à x)
        public string S_IMAGE1 { get; set; }
        public string S_IMAGE2 { get; set; }
        public string S_IMAGE3 { get; set; }
        public string S_IMAGE4 { get; set; }
        public string S_IMAGE5 { get; set; }
        public string S_IMAGE6 { get; set; }
        public string S_IMAGE7 { get; set; }
        public string S_IMAGE8 { get; set; }
        public string S_IMAGE9 { get; set; }
        public string S_IMAGE10 { get; set; }
        public string S_IMAGE11 { get; set; }
        public string S_IMAGE12 { get; set; }
        public string S_REF_FOU { get; set; }
        public bool S_WEB { get; set; }
        public int S_DISPO { get; set; }
        public decimal S_CONDIT_V { get; set; }
        public int S_ID_WEB { get; set; }
        public int S_MODIFTAG { get; set; }
    }
}
