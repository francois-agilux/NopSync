using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.Model.NopCommerce
{
    class Address
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street { get; set; }
		public string Street2 { get; set; }
        public string ZipPostalCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
        public string Company { get; set; }

        public Address(string firstName, string lastName, string street, string street2, string zipPostalCode, string city, string country, string phoneNumber, string company = "")
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Street = street;
			this.Street2 = street2;
            this.ZipPostalCode = zipPostalCode;
            this.City = city;
            this.Country = country;
            this.PhoneNumber = phoneNumber;
            this.Company = company;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Address))
                return false;
            Address addressObj = obj as Address;

            if (this.FirstName != addressObj.FirstName)
                return false;

            if (this.LastName != addressObj.LastName)
                return false;

            if (this.Street != addressObj.Street)
                return false;

            if (this.Street2 != addressObj.Street2)
                return false;

            if (this.Country != addressObj.Country)
                return false;

            if (this.PhoneNumber != addressObj.PhoneNumber)
                return false;

            return true;
        }

    }
}
