using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateOpportunity.Entity
{
    public class OpportunityLineItemEntity
    {
        public int Quantity { get; set; }
        public string ProductSalesforceReference { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductXRefID { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime ServiceDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ContactSalesforceReference { get; set; }
        public string ContactID { get; set; }
        public string ContactName { get; set; }
        public string ContactXRefId { get; set; }
        public bool AutoRenewal { get; set; }
        public string InstanceSalesforceReference { get; set; }
        public string InstanceID { get; set; }
        public string InstanceType { get; set; }
        public string InstanceTypeDescription { get; set; }
        public string InstanceXrefId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string BillingCycle { get; set; }
    }
}
