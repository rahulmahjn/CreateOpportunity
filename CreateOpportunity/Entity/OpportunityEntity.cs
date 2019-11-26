using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CreateOpportunity.Entity
{
    public class OpportunityEntity
    {
        public int OrderID { get; set; }
        public string ExternalID { get; set; }
        public string Name { get; set; }
        public string InvoiceItemId { get; set; }
        public string OtisInvoiceId { get; set; }
        public string AccountSalesforceReference { get; set; }
        public string AccountID { get; set; }
        public string AccountName { get; set; }
        public string AccountXRefID { get; set; }
        public string StreetLine1 { get; set; }
        public string StreetLine2 { get; set; }
        public string PostCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string CompanyID { get; set; }
        public string Type { get; set; }
        public string Currency { get; set; }
        public string BookerSalesforceReference { get; set; }
        public string BookerName { get; set; }
        public string BookerID { get; set; }
        public string BookerXrefID { get; set; }
        public string BookerEmailAddress { get; set; }
        public Decimal Amount { get; set; }
        public DateTime? CloseDate { get; set; }
        public string Stage { get; set; }
        public string EndCustomerSalesforceReference { get; set; }
        public string EndCustomerName { get; set; }
        public string EndCustomerID { get; set; }
        public string EndCustomerXRefID { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string OwnerID { get; set; }
        public string PaymentType { get; set; }
        public string Brand { get; set; }
        public string OriginalBookingSalesforceReference { get; set; }
        
        public List<OpportunityLineItemEntity> OpportunityLineItemEntities { get; set; }
    }
}
