using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.Model.NopCommerce
{
    class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Tva { get; set; }
        public string MercatorId { get; set; }
		public string FromMercator { get; set; }

        public Address Address1 { get; set; }
        #region NopCommerce code         

        public string Username { get; set; }        
        public string Email { get; set; }        
        public string Password { get; set; }
        public int PasswordFormatId { get; set; }
        public string PasswordSalt { get; set; }
        public string AdminComment { get; set; }
        public bool IsTaxExempt { get; set; }
        public int AffiliateId { get; set; }
        public int VendorId { get; set; }
        public bool HasShoppingCartItems { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public bool IsSystemAccount { get; set; }
        public string SystemName { get; set; }
        public string LastIpAddress { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? LastLoginDateUtc { get; set; }
        public DateTime LastActivityDateUtc { get; set; }

        #endregion 

        public Customer() { }

		public Customer(string mercatorId, string email, string username, string name, DateTime createdOnUtc, int id, Address a)
        {
            MercatorId = mercatorId;
            Email = email;
            Username = username;
            Name = name;
            CreatedOnUtc = createdOnUtc;
            Id = id;

            Address1 = a;

        }

    }
}
