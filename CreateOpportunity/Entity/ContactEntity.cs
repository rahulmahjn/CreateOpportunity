using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateOpportunity.Entity
{
    public class ContactEntity
    {
        public int ContactID { get; set; }
        public int AccountID { get; set; }
        public string AccountSRN { get; set; }
        public string ContactSRN { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string StreetLine1 { get; set; }
        public string StreetLine2 { get; set; }
        public string Town { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool DoNotEmail { get; set; }
        public bool DoNotCall { get; set; }
        public bool DoNotMail { get; set; }
    }
}
