using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateOpportunity.Entity
{
    public class AccountEntity
    {
        public int AccountID { get; set; }
        public int AccountSRN { get; set; }
        public string Name { get; set; }
        public string BillingAddressLine1 { get; set; }
        public string BillingAddressLine2 { get; set; }
        public string BillingCity { get; set; }
        public string BillingPostCode { get; set; }
        public string BillingCountry { get; set; }
        public string BillingPhone { get; set; }
        public string BillingFax { get; set; }
        public string BillingWebsite { get; set; }
        public string MCAAccount { get; set; }

    }
}
