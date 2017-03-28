using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BasicNopSync.Model.NopCommerce
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentCategoryId { get; set; }
        public int CategoryTemplateId { get; set; }
        public int PictureId { get; set; }
        public int PageSize { get; set; }
        public bool AllowCustomersToSelectPageSize { get; set; }
        public bool ShowOnHomePage { get; set; }
        public bool IncludeInTopMenu { get;set;}        
        public bool SubjectToAcl { get; set; }
        public bool LimitedToStores { get; set; }
        public bool Published { get; set; }
        public bool Deleted{ get; set; }
        public int DisplayOrder{ get; set; }     
        [JsonIgnore]   
        public string MercatorId { get; set; }

        public Category(string name = "",int parentId = 0, string mercatorId = "", int id = 0)
        {
            InitCategory();

            Id = id;
            Name = name;
            ParentCategoryId = parentId;
            MercatorId = mercatorId;
        }

        private void InitCategory()
        {
            CategoryTemplateId = 1;
            PictureId = 0;
            PageSize = 9;
            AllowCustomersToSelectPageSize = true;
            ShowOnHomePage = false;
            IncludeInTopMenu = true;
            SubjectToAcl = false;
            LimitedToStores = false;
            Published = true;
            Deleted = false;
            DisplayOrder = 0;

        }
    }
}
