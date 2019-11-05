using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CreateOpportunity.CustomWebService;
using CreateOpportunity.Entity;
using CreateOpportunity.DAL;
using System.Data;
using System.Data.SqlClient;
namespace CreateOpportunity.BAL
{
    public class Opportunity
    {
        otisSoapServicesService otisSoapServices;
        NLog.Logger Logger;

        public Opportunity(NLog.Logger Logger)
        {
            this.Logger = Logger;
        }
        #region public functions

        /// <summary>
        /// Get the opportunities from DM of today's billing date
        /// </summary>
        /// <returns></returns>
        public List<OpportunityEntity> GetOpportunities()
        {
            List<OpportunityEntity> opportunityEntities = null;
            Logger.Info("Function called: GetOpportunities");
            try
            {
                var result = new OpportunityDAL().GetOpportunities();
                List<OpportunityEntity> newOpportunityEntities = null;
                List<OpportunityEntity> creditOpportunityEntities = null;
 
                //New Opportunity
                var newOpportunitiesRows = result.AsEnumerable().Where(myRow => !myRow.Field<decimal>("SalesPrice").ToString().StartsWith("-"));
                if (newOpportunitiesRows != null)
                {
                    if (newOpportunitiesRows.Count() > 0)
                    {
                        newOpportunityEntities = MarshalOpportunityFromDB(newOpportunitiesRows, "New Business");
                    }
                }

                //Credit Opportunity
                var creditOpportunitiesRows = result.AsEnumerable().Where(myRow => myRow.Field<decimal>("SalesPrice").ToString().StartsWith("-") && !String.IsNullOrEmpty(myRow.Field<String>("OriginalBookingSalesforceReference")));
                if (creditOpportunitiesRows != null)
                {
                    if (creditOpportunitiesRows.Count() > 0)
                    {
                        creditOpportunityEntities = MarshalOpportunityFromDB(creditOpportunitiesRows, "Credit");
                    }
                }

                if (newOpportunityEntities != null)
                {
                    Logger.Info("New Business opportunities found are:" + newOpportunityEntities.Count().ToString());
                    if (creditOpportunityEntities != null)
                    {
                        Logger.Info("Credit opportunities found are:" + creditOpportunityEntities.Count().ToString());
                        opportunityEntities = newOpportunityEntities.Concat(creditOpportunityEntities).ToList<OpportunityEntity>();
                    }
                    else
                    {
                        Logger.Info("No credit opportunities found.");
                        opportunityEntities = newOpportunityEntities;
                    }
                }
                else
                {
                    Logger.Info("No New Business opportunities found.");

                    if (creditOpportunityEntities != null)
                    {
                        Logger.Info("Credit opportunities found are:" + creditOpportunityEntities.Count().ToString());
                        opportunityEntities = creditOpportunityEntities;
                    }
                    else
                    {
                        Logger.Info("No credit opportunities found.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return opportunityEntities;
        }

        /// <summary>
        /// Get missing records
        /// Either might be the new one with empty Salesforce Reference in DM database OR wrong reference updatd in DM
        /// </summary>
        /// <param name="opportunityEntities"></param>
        /// <param name="SessionID"></param>
        /// <returns></returns>
        public List<MissingRecordEntity> GetMissingObjectRecords(List<OpportunityEntity> opportunityEntities, String SessionID)
        {
            Logger.Info("Function called: GetMissingObjectRecords");
            List<RecordEntity> recordEntities =null;
            List<MissingRecordEntity> missingRecords = null;
            try
            {
                var missingDMRecords = ObjectRecordsCheckInDM(opportunityEntities);

                if (missingDMRecords != null)
                {
                    
                    //var uniqueOrderIDs = missingDMRecords.Select(row => row.OrderID).Distinct();

                    //foreach (var uniqueID in uniqueOrderIDs)
                    //{
                    //    var data=missingDMRecords.Where(row=>row.OrderID==uniqueID).Select(row=>row.RecordEntities).;
                    //    opportunityEntities.RemoveAll(opp => opp.OrderID == uniqueID);
                    //}

                    foreach (var missing in missingDMRecords)
                    {
                        if (recordEntities==null)
                        {
                            recordEntities = missing.RecordEntities;
                        }
                        else
                        {
                            recordEntities=recordEntities.Concat(missing.RecordEntities).ToList<RecordEntity>();
                        }
                        opportunityEntities.RemoveAll(opp => opp.OrderID == missing.OrderID);
                    }
                    if (recordEntities != null)
                    {
                        Logger.Info("Empty salesforce reference(s) found in DM. Count:" + recordEntities.Select(row=>row.ObjectID).Distinct().Count().ToString());
                        recordEntities = null;
                    }
                }
                else
                {
                    Logger.Info("No empty salesforce reference found in DM.");
                }

                var missingSFRecords = ObjectRecordsCheckInSalesforce(opportunityEntities, SessionID);

                if (missingSFRecords != null)
                {
                    
                    foreach (var missing in missingSFRecords)
                    {
                        if (recordEntities == null)
                        {
                            recordEntities = missing.RecordEntities;
                        }
                        else
                        {
                            recordEntities = recordEntities.Concat(missing.RecordEntities).ToList<RecordEntity>(); ;
                        }
                    }
                    if (recordEntities != null)
                    {
                        Logger.Info("Salesforce reference(s) entered in DM don't match with Salesforce. Count:" + recordEntities.Select(row => row.ObjectID).Distinct().Count().ToString());
                        
                    }

                    if (missingDMRecords != null)
                    {
                        missingRecords = missingDMRecords.Concat(missingSFRecords).ToList<MissingRecordEntity>();
                    }
                    else
                    {
                        missingRecords = missingSFRecords;
                    }
                }
                else
                {
                    missingRecords = missingDMRecords;
                    Logger.Info("All salesforce reference(s) enetred in DM match with Salesforce.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return missingRecords;
        }

        /// <summary>
        /// Post any missing records to Salesforce if any found in DM
        /// </summary>
        /// <param name="missingRecords"></param>
        /// <param name="SessionID"></param>
        /// <returns></returns>
        public postMissingObjectRecordsResponse PostMissingObjectRecords(List<MissingRecordEntity> missingRecords, String SessionID)
        {
            postMissingObjectRecordsResponse postMissingObjectRecordsResponse=null;
            Logger.Info("Function called: PostMissingObjectRecords");
            try
            {
                if (missingRecords.Count > 0)
                {
                    missingObjectRecord missingObjectRecord;
                    List<missingObjectRecord> missingObjectRecords = new List<missingObjectRecord>();
                    List<String> uniqueMissingRecords = new List<String>();

                    foreach (var missingRecord in missingRecords)
                    {
                        if (missingRecord.RecordEntities.Count > 0)
                        {
                            foreach (var recordEntity in missingRecord.RecordEntities)
                            {
                                if (!uniqueMissingRecords.Contains(recordEntity.ObjectID + "_" + recordEntity.ObjectName))
                                {
                                    uniqueMissingRecords.Add(recordEntity.ObjectID + "_" + recordEntity.ObjectName);
                                    missingObjectRecord = new missingObjectRecord();
                                    missingObjectRecord.objectName = recordEntity.ObjectName;
                                    missingObjectRecord.otisId = recordEntity.ObjectID;
                                    missingObjectRecord.name = recordEntity.ObjectValue;
                                    missingObjectRecord.description = recordEntity.Description;
                                    missingObjectRecords.Add(missingObjectRecord);
                                }
                            }
                        }
                    }
                    if (missingObjectRecords.Count > 0)
                    {
                        otisSoapServices = new otisSoapServicesService();
                        var sessionHeader = new SessionHeader();
                        sessionHeader.sessionId = SessionID;
                        otisSoapServices.SessionHeaderValue = sessionHeader;

                        //Call SF Web Service function
                        postMissingObjectRecordsResponse = otisSoapServices.postMissingObjectRecords(missingObjectRecords.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return postMissingObjectRecordsResponse;
        }

        /// <summary>
        /// Post New and Credit opportunities Async
        /// </summary>
        /// <param name="opportunityEntities"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public addOpportunitiesAsynchResponse CreateOpportunities(List<OpportunityEntity> opportunityEntities, String sessionID)
        {
            Logger.Info("Function called: CreateOpportunities");
            addOpportunitiesAsynchResponse sfOpportunityResponse;
            otisSoapServices = new otisSoapServicesService();
            var sessionHeader = new SessionHeader();
            sessionHeader.sessionId = sessionID;
            otisSoapServices.SessionHeaderValue = sessionHeader;
            try
            {
                var opportunities = MarshalDMEntityToSFEntity(opportunityEntities);

                sfOpportunityResponse = otisSoapServices.addOpportunitiesAsynch(opportunities.ToArray<opportunityOtis>());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //Its been discussed with Jack that when salesforceErrorMessage is null or empty then the batch has been accepted
            return sfOpportunityResponse;
        }

        /// <summary>
        /// Once the opportunites are posted then get the response back
        /// </summary>
        /// <param name="otisIDs"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public addOpportunitiesResponse GetOpportunitiesAsynchResponse(List<String> otisIDs,String sessionID)
        {
            Logger.Info("Function called: GetOpportunitiesAsynchResponse");
            addOpportunitiesResponse response;
            otisSoapServices = new otisSoapServicesService();
            var sessionHeader = new SessionHeader();
            sessionHeader.sessionId = sessionID;
            otisSoapServices.SessionHeaderValue = sessionHeader;
            try
            {
                response = otisSoapServices.getAddOpportunitiesAsynchResponse(otisIDs.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public int UpdateInvoiceNumber(string SessionID)
        {
            Logger.Info("Function called: UpdateInvoiceNumber");
            int recordAffected = 0;
            try
            {
                otisSoapServices = new otisSoapServicesService();
                var sessionHeader = new SessionHeader();
                sessionHeader.sessionId = SessionID;
                otisSoapServices.SessionHeaderValue = sessionHeader;
                getCompletedInvoicesRequest invoicesRequest = new getCompletedInvoicesRequest();
                var lastInvoicedDate = new OpportunityDAL().GetLastDateInvoiced();

                if (lastInvoicedDate == null)
                {
                    lastInvoicedDate = DateTime.Today.AddDays(-1);
                }

                invoicesRequest.earliestCompletedInvoiceDateTime = lastInvoicedDate;
                invoicesRequest.earliestCompletedInvoiceDateTimeSpecified = true;
                Logger.Info("Last invoice date requested is: " + Convert.ToString(lastInvoicedDate));

                var invoiceResponse = otisSoapServices.getCompletedInvoices(invoicesRequest);
                if (invoiceResponse != null)
                {
                    if(String.IsNullOrEmpty(invoiceResponse.salesforceErrorMessage))
                    {
                        if (invoiceResponse.invoices != null)
                        {
                            Logger.Info("Number of invoices received from Salesforce are:" + invoiceResponse.invoices.Count());
                            foreach (var response in invoiceResponse.invoices)
                            {
                                if (!String.IsNullOrEmpty(response.opportunityKey) )
                                {
                                    if (response.completedDateTime != null)
                                    {
                                        recordAffected = recordAffected + new OpportunityDAL().UpdateSalesforceOpportunity(response.opportunityKey, "INVOICE", response.invoiceNumber, response.completedDateTime);
                                    }
                                    else
                                    {
                                        Logger.Info("Completed DateTime is msissing from Salesforce for the invoice number:" + response.invoiceNumber);
                                    }
                                }
                                else
                                {
                                    Logger.Info("No Salesforce Reference number found for the invoice number:" + response.invoiceNumber);
                                }
                            }
                        }
                        else
                        {
                            Logger.Info("No invoice data received.");
                        }
                    }
                    else
                    {
                        Logger.Info("Error received from Salesforce. Error Message:" + invoiceResponse.salesforceErrorMessage);
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return recordAffected;
        }

        /// <summary>
        /// Update Opportunity record(s) in DM
        /// e.g Opportunity Salesforce Reference, Invoice Number, Opportunity PostedDateTime and Opportunity ResponseDateTime 
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="message"></param>
        /// <param name="recordType"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public int UpdateOpportunity(string orderID,String message,string recordType,DateTime billingDate)
        {
            return new OpportunityDAL().UpdateSalesforceOpportunity(orderID, recordType, message, billingDate);
        }

        #endregion
        
        #region private functions

        private List<MissingRecordEntity> ObjectRecordsCheckInDM(List<OpportunityEntity> opportunityEntities)
        {
            List<MissingRecordEntity> missingDMRecords = null;
            try
            {
                Logger.Info("Function called: ObjectRecordsCheckInDM");
                missingDMRecords = MarshalMissingRecordsReceivedFromDM(opportunityEntities);
            }

            catch (Exception ex)
            {    
                throw ex;
            }
            return missingDMRecords;
        }

        private List<MissingRecordEntity> ObjectRecordsCheckInSalesforce(List<OpportunityEntity> opportunities, String sessionID)
        {
            List<MissingRecordEntity> missingSFRecords = null;
            try
            {
                Logger.Info("Function called: ObjectRecordsCheckInSalesforce");
                if (opportunities.Count > 0)
                {
                    var keyChecks = PrepareKeyCheckRequest(opportunities);

                    otisSoapServices = new otisSoapServicesService();
                    var sessionHeader = new SessionHeader();
                    sessionHeader.sessionId = sessionID;
                    otisSoapServices.SessionHeaderValue = sessionHeader;

                    //Call Salesforce and receive KeyCheck response
                    var keyCheckResult = otisSoapServices.keyCheck(keyChecks.ToArray<keyCheck>());

                    if (keyCheckResult.notFoundKeys != null)
                    {
                        if (keyCheckResult.notFoundKeys.Length > 0)
                        {
                            missingSFRecords = MarshalKeyCheckResponse(keyCheckResult, opportunities);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return missingSFRecords;
        }

        private List<keyCheck> PrepareKeyCheckRequest(List<OpportunityEntity> opportunities)
        {
            List<keyCheck> keyChecks = new List<keyCheck>();
            keyCheck kc;
            List<string> uniqueRecords = new List<string>();
            try
            {
                Logger.Info("Function called: PrepareKeyCheckRequest");
                //Start - Key Check Request
                foreach (var opportunity in opportunities)
                {
                    if (!uniqueRecords.Contains(opportunity.AccountSalesforceReference + "_Account_" + Convert.ToString(opportunity.OrderID)))
                    {
                        uniqueRecords.Add(opportunity.AccountSalesforceReference + "_Account_" + Convert.ToString(opportunity.OrderID));
                        kc = new keyCheck();
                        kc.objectName = "Account";
                        kc.keyValue = opportunity.AccountSalesforceReference;
                        kc.otisId = Convert.ToString(opportunity.OrderID);
                        keyChecks.Add(kc);
                    }
                    if (!uniqueRecords.Contains(opportunity.BookerSalesforceReference + "_Contact_" + Convert.ToString(opportunity.OrderID)))
                    {
                        uniqueRecords.Add(opportunity.BookerSalesforceReference + "_Contact_" + Convert.ToString(opportunity.OrderID));
                        kc = new keyCheck();
                        kc.objectName = "Contact";
                        kc.keyValue = opportunity.BookerSalesforceReference;
                        kc.otisId = Convert.ToString(opportunity.OrderID);
                        keyChecks.Add(kc);
                    }
                    if (!uniqueRecords.Contains(opportunity.EndCustomerSalesforceReference + "_Account_" + Convert.ToString(opportunity.OrderID)))
                    {
                        uniqueRecords.Add(opportunity.EndCustomerSalesforceReference + "_Account_" + Convert.ToString(opportunity.OrderID));
                        kc = new keyCheck();
                        kc.objectName = "Account";
                        kc.keyValue = opportunity.EndCustomerSalesforceReference;
                        kc.otisId = Convert.ToString(opportunity.OrderID);
                        keyChecks.Add(kc);
                    }
                    // This will be for the credit opportunity
                    if (!String.IsNullOrEmpty(opportunity.OriginalBookingSalesforceReference))
                    {
                        if (!uniqueRecords.Contains(opportunity.OriginalBookingSalesforceReference + "_Opportunity_" + Convert.ToString(opportunity.OrderID)))
                        {
                            uniqueRecords.Add(opportunity.OriginalBookingSalesforceReference + "_Opportunity_" + Convert.ToString(opportunity.OrderID));
                            kc = new keyCheck();
                            kc.objectName = "Opportunity";
                            kc.keyValue = opportunity.OriginalBookingSalesforceReference;
                            kc.otisId = Convert.ToString(opportunity.OrderID);
                            keyChecks.Add(kc);
                        }
                    }
                    if (opportunity.OpportunityLineItemEntities.Count > 0)
                    {
                        foreach (var lineItem in opportunity.OpportunityLineItemEntities)
                        {
                            // As we are building the KeyCheck collection. Ignore a duplicate Product for the OTIS ID
                            var productKeys = keyChecks.Where(key => key.otisId == Convert.ToString(opportunity.OrderID) && key.keyValue == lineItem.ProductSalesforceReference && key.objectName == "Product2");

                            if (productKeys.Count<keyCheck>() == 0)
                            {
                                if (!uniqueRecords.Contains(lineItem.ProductSalesforceReference + "_Product2_" + Convert.ToString(opportunity.OrderID)))
                                {
                                    uniqueRecords.Add(lineItem.ProductSalesforceReference + "_Product2_" + Convert.ToString(opportunity.OrderID));
                                    kc = new keyCheck();
                                    kc.objectName = "Product2";
                                    kc.keyValue = lineItem.ProductSalesforceReference;
                                    kc.otisId = Convert.ToString(opportunity.OrderID);
                                    keyChecks.Add(kc);
                                }
                            }

                            // As we are building the KeyCheck collection. Ignore a duplicate Contact for the OTIS ID
                            var contactKeys = keyChecks.Where(key => key.otisId == Convert.ToString(opportunity.OrderID) && key.keyValue == lineItem.ContactSalesforceReference && key.objectName == "Contact");

                            if (contactKeys.Count<keyCheck>() == 0)
                            {
                                if (!uniqueRecords.Contains(lineItem.ContactSalesforceReference + "_Contact_" + Convert.ToString(opportunity.OrderID)))
                                {
                                    uniqueRecords.Add(lineItem.ContactSalesforceReference + "_Contact_" + Convert.ToString(opportunity.OrderID));
                                    kc = new keyCheck();
                                    kc.objectName = "Contact";
                                    kc.keyValue = lineItem.ContactSalesforceReference;
                                    kc.otisId = Convert.ToString(opportunity.OrderID);
                                    keyChecks.Add(kc);
                                }
                            }

                            if (!String.IsNullOrEmpty(lineItem.InstanceType))
                            {
                                // As we are building the KeyCheck collection. Ignore a duplicate Instance for the OTIS ID
                                var instanceKeys = keyChecks.Where(key => key.otisId == Convert.ToString(opportunity.OrderID) && key.keyValue == lineItem.InstanceSalesforceReference && key.objectName == lineItem.InstanceType);

                                if (instanceKeys.Count<keyCheck>() == 0)
                                {
                                    if (!uniqueRecords.Contains(lineItem.InstanceSalesforceReference + "_" + lineItem.InstanceType + Convert.ToString(opportunity.OrderID)))
                                    {
                                        uniqueRecords.Add(lineItem.InstanceSalesforceReference + "_" + lineItem.InstanceType + Convert.ToString(opportunity.OrderID));
                                        kc = new keyCheck();
                                        kc.objectName = lineItem.InstanceType;
                                        kc.keyValue = lineItem.InstanceSalesforceReference;
                                        kc.otisId = Convert.ToString(opportunity.OrderID);
                                        keyChecks.Add(kc);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return keyChecks;
            //End - Key Check Request
        }

        private List<MissingRecordEntity> MarshalKeyCheckResponse(keyCheckResponse keyCheckResult, List<OpportunityEntity> opportunities)
        {
            MissingRecordEntity missingRecordEntity;
            RecordEntity recordEntity;
            List<RecordEntity> recordEntities =null;
            List<MissingRecordEntity> missingRecordEntities = new List<MissingRecordEntity>();

            List<string> uniqueRecords = new List<string>();
            List<int> OtisIDs = new List<int>();
            
            try
            {
                Logger.Info("Function called: MarshalKeyCheckResponse");
                foreach (var notFoundKey in keyCheckResult.notFoundKeys)
                {
                    if (!OtisIDs.Contains(Convert.ToInt32(notFoundKey.otisId)))
                    {
                        
                        OtisIDs.Add(Convert.ToInt32(notFoundKey.otisId));
                        var keys = keyCheckResult.notFoundKeys.Where(key => key.otisId == notFoundKey.otisId);
                        var opportunity = opportunities.Where(row => row.OrderID == Convert.ToInt32(notFoundKey.otisId)).First<OpportunityEntity>();

                        recordEntities = new List<RecordEntity>();
                        missingRecordEntity = new MissingRecordEntity();
                        missingRecordEntity.OrderID = Convert.ToInt32(notFoundKey.otisId);

                        foreach (var key in keys)
                        {
                            if (key.objectName == "Account" && key.keyValue == opportunity.AccountSalesforceReference)
                            {
                                if (!uniqueRecords.Contains(opportunity.AccountID + "_Account"))
                                {
                                    uniqueRecords.Add(opportunity.AccountID + "_Account");
                                    recordEntity = new RecordEntity();
                                    recordEntity.ObjectID = opportunity.AccountID + "_" + opportunity.AccountXRefID;
                                    recordEntity.ObjectName = key.objectName;
                                    recordEntity.ObjectValue = opportunity.AccountName;

                                    string description;
                                    if (String.IsNullOrEmpty(notFoundKey.errorMessage))
                                    {
                                        description = "Posted account reference number is not found in Salesforce. Reference Number:";
                                    }
                                    else
                                    {
                                        description = notFoundKey.errorMessage;
                                    }
                                    recordEntity.Description = description
                                                                + opportunity.AccountSalesforceReference + ", Account Name:" + opportunity.AccountName  +  ", DM AccountID:"+ opportunity.AccountID 
                                                                + ", OTIS AccountID:" + opportunity.AccountXRefID;
                                    recordEntities.Add(recordEntity);
                                }
                            }
                            else if (key.objectName == "Contact" && key.keyValue == opportunity.BookerSalesforceReference)
                            {
                                if (!uniqueRecords.Contains(opportunity.BookerID + "_Contact"))
                                {
                                    uniqueRecords.Add(opportunity.BookerID + "_Contact");
                                    recordEntity = new RecordEntity();
                                    recordEntity.ObjectID = opportunity.BookerID + "_" + opportunity.BookerXrefID;
                                    recordEntity.ObjectName = key.objectName;
                                    recordEntity.ObjectValue = opportunity.BookerName;
                                    recordEntity.Description = "Posted contact reference number is not found in Salesforce. Reference Number:"
                                                                + opportunity.BookerSalesforceReference + ", Contact Name:" + opportunity.BookerName + ", DM ContactID:" + opportunity.BookerID
                                                                + ", OTIS ContactID:" + opportunity.BookerXrefID;
                                    recordEntities.Add(recordEntity);
                                }
                            }
                            if (key.objectName == "Account" && key.keyValue == opportunity.EndCustomerSalesforceReference)
                            {
                                if (!uniqueRecords.Contains(opportunity.EndCustomerID + "_Account"))
                                {
                                    uniqueRecords.Add(opportunity.EndCustomerID + "_Account");
                                    recordEntity = new RecordEntity();

                                    recordEntity.ObjectID = opportunity.EndCustomerID + "_" + opportunity.EndCustomerXRefID;
                                    recordEntity.ObjectName = key.objectName;
                                    recordEntity.ObjectValue = opportunity.EndCustomerName;
                                    recordEntity.Description = "Posted account reference number is not found in Salesforce. Reference Number:"
                                                                + opportunity.EndCustomerSalesforceReference + ", Account Name:" + opportunity.EndCustomerName + ", DM AccountID:" + opportunity.EndCustomerID
                                                                + ", OTIS AccountID:" + opportunity.EndCustomerXRefID;
                                    recordEntities.Add(recordEntity);
                                }
                            }
                            foreach (var OpplineItem in opportunity.OpportunityLineItemEntities)
                            {
                                if (key.objectName == "Product2" && key.keyValue == OpplineItem.ProductSalesforceReference)
                                {
                                    if (!uniqueRecords.Contains(OpplineItem.ProductID + "_Product2"))
                                    {
                                        uniqueRecords.Add(OpplineItem.ProductID + "_Product2");
                                        recordEntity = new RecordEntity();
                                        recordEntity.ObjectID = OpplineItem.ProductID + "_" + OpplineItem.ProductXRefID;
                                        recordEntity.ObjectName = key.objectName;
                                        recordEntity.ObjectValue = OpplineItem.ProductName;
                                        recordEntity.Description = "Posted product reference number is not found in Salesforce. Reference Number:"
                                                                    + OpplineItem.ProductSalesforceReference + ", Product Name:" + OpplineItem.ProductName + ", DM ProductID:" + OpplineItem.ProductID
                                                                    + ", OTIS ProductID:" + OpplineItem.ProductXRefID;
                                        recordEntities.Add(recordEntity);
                                    }
                                }
                                else if (key.objectName == "Contact" && key.keyValue == OpplineItem.ContactSalesforceReference)
                                {
                                    if (!uniqueRecords.Contains(OpplineItem.ContactID + "_Contact"))
                                    {
                                        uniqueRecords.Add(OpplineItem.ContactID + "_Contact");
                                        recordEntity = new RecordEntity();
                                        recordEntity.ObjectID = OpplineItem.ContactID + "_" + OpplineItem.ContactXRefId;
                                        recordEntity.ObjectName = key.objectName;
                                        recordEntity.ObjectValue = OpplineItem.ContactName;
                                        recordEntity.Description = "Posted contact reference number is not found in Salesforce. Reference Number:"
                                                                 + OpplineItem.ContactSalesforceReference + ", Contact Name:" + OpplineItem.ContactName +  ", DM ContactID:" + OpplineItem.ContactID
                                                                 + ", OTIS ContactID:" + OpplineItem.ContactXRefId;
                                        recordEntities.Add(recordEntity);
                                    }
                                }
                                //Instance Level Objects
                                else if (key.objectName == OpplineItem.InstanceType && key.keyValue == OpplineItem.InstanceSalesforceReference)
                                {
                                    if (!uniqueRecords.Contains(OpplineItem.InstanceID + "_" + OpplineItem.InstanceType))
                                    {
                                        uniqueRecords.Add(OpplineItem.InstanceID + "_" + OpplineItem.InstanceType);
                                        recordEntity = new RecordEntity();
                                        recordEntity.ObjectID = OpplineItem.InstanceID + "_" + OpplineItem.InstanceXrefId;
                                        recordEntity.ObjectName = key.objectName;
                                        recordEntity.ObjectValue = OpplineItem.InstanceTypeDescription;
                                        recordEntity.Description = "Posted " + OpplineItem.InstanceType + " reference number is not found in Salesforce. Reference Number:"
                                                                     + OpplineItem.InstanceSalesforceReference + ", Instance Name:" + OpplineItem.InstanceTypeDescription + ", DM InstanceID:" + OpplineItem.InstanceID
                                                                    + ", OTIS InstanceID:" + OpplineItem.InstanceXrefId;
                                        recordEntities.Add(recordEntity);
                                    }
                                }
                            }
                        }
                     
                            missingRecordEntity.RecordEntities = recordEntities;
                            missingRecordEntities.Add(missingRecordEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return missingRecordEntities;
        }

        private List<opportunityOtis> MarshalDMEntityToSFEntity(List<OpportunityEntity> dmOpportunities)
        {
            opportunityOtis sfOpportunity = null;
            lineItem sfLineItem = null;

            List<opportunityOtis> sfOpportunities = new List<opportunityOtis>();
            List<lineItem> sfLineItems = new List<lineItem>();

            try
            {
                Logger.Info("Function called: MarshalDMEntityToSFEntity");
                foreach (var dmOpportunity in dmOpportunities)
                {
                    sfOpportunity = new opportunityOtis();
                    if (dmOpportunity.ExternalID.Length > 250)
                    {
                        sfOpportunity.otisId = dmOpportunity.ExternalID.Substring(0, 250);
                    }
                    else
                    {
                        sfOpportunity.otisId = dmOpportunity.ExternalID;
                    }
                    sfOpportunity.accountKey = dmOpportunity.AccountSalesforceReference;

                    sfOpportunity.bookerContactKey = dmOpportunity.BookerSalesforceReference;

                    sfOpportunity.closeDate = dmOpportunity.CloseDate;
                    sfOpportunity.closeDateSpecified = true;

                    sfOpportunity.currencyIsoCode = dmOpportunity.Currency;

                    sfOpportunity.endCustomerAccountKey = dmOpportunity.EndCustomerSalesforceReference;

                    sfOpportunity.brandName = dmOpportunity.Brand;

                    if (dmOpportunity.Name.Length > 120)
                    {
                        sfOpportunity.opportunityName = dmOpportunity.Name.Substring(0, 120);
                    }
                    else
                    {
                        sfOpportunity.opportunityName = dmOpportunity.Name;
                    }

                    sfOpportunity.originalBookingKey = dmOpportunity.OriginalBookingSalesforceReference;

                    sfOpportunity.paymentType = dmOpportunity.PaymentType;
                    if (!String.IsNullOrEmpty(sfOpportunity.purchaseOrderNumber))
                    {
                        if (dmOpportunity.PurchaseOrderNumber.Length > 255)
                        {
                            sfOpportunity.otisId = dmOpportunity.PurchaseOrderNumber.Substring(0, 255);
                        }
                        else
                        {
                            sfOpportunity.otisId = dmOpportunity.PurchaseOrderNumber;
                        }
                    }
                    sfOpportunity.stageName = dmOpportunity.Stage;

                    sfOpportunity.type = dmOpportunity.Type;
                    sfOpportunity.amount = dmOpportunity.Amount;
                    sfOpportunity.amountSpecified = true;

                    foreach (var dmLineitem in dmOpportunity.OpportunityLineItemEntities)
                    {
                        sfLineItem = new lineItem();
                        sfLineItem.autoRenewalTerms = dmLineitem.AutoRenewal;
                        sfLineItem.autoRenewalTermsSpecified = true;

                        sfLineItem.contactKey = dmLineitem.ContactSalesforceReference;

                        sfLineItem.endDate = dmLineitem.EndDate;
                        sfLineItem.endDateSpecified = true;

                        if (dmLineitem.InstanceType == "Event__c")
                        {
                            sfLineItem.eventKey = dmLineitem.InstanceSalesforceReference;

                        }
                        else if (dmLineitem.InstanceType == "Publication_Issue__c")
                        {
                            sfLineItem.publicationIssueKey = dmLineitem.InstanceSalesforceReference;
                        }

                        sfLineItem.invoiceDate = dmLineitem.InvoiceDate;
                        sfLineItem.invoiceDateSpecified = true;

                        sfLineItem.productKey = dmLineitem.ProductSalesforceReference;

                        sfLineItem.quantity = dmLineitem.Quantity;
                        sfLineItem.quantitySpecified = true;
                        sfLineItem.serviceDate = dmLineitem.ServiceDate;
                        sfLineItem.serviceDateSpecified = true;
                        sfLineItem.unitPrice = dmLineitem.UnitPrice;
                        sfLineItem.unitPriceSpecified = true;
                        sfLineItem.description = dmLineitem.Description;
                        sfLineItems.Add(sfLineItem);
                    }
                    sfOpportunity.lineItems = sfLineItems.ToArray<lineItem>();
                    sfOpportunities.Add(sfOpportunity);
                    sfLineItems.Clear();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return sfOpportunities;
        }

        private List<OpportunityEntity> MarshalOpportunityFromDB(EnumerableRowCollection<DataRow> result, string type)
        {
            List<int> lstOrders = new List<int>();
            List<OpportunityEntity> opportunityEntities = new List<OpportunityEntity>();
            
            string externalID=String.Empty;
            try
            {
                Logger.Info("Function called: MarshalOpportunityFromDBToEntity for Type- " + type.ToString());
                if (result.Count() > 0)
                {
                    foreach (DataRow order in result)
                    {
                        int orderID = Convert.ToInt32(order["OrderId"]);
                        if ((type == "New Business") || (type == "Credit" && !String.IsNullOrEmpty(Convert.ToString(order["OriginalBookingSalesforceReference"]))))
                        {
                            //The records have bought at once from the database and the business logic to extract the line item details is below
                            //Don't iterate the second record for the same order id
                            if (!lstOrders.Contains(orderID))
                            {
                                lstOrders.Add(orderID);

                                var opportunityEntity = MarshalOpportunityFromDBToEntity(order, type);

                                //Prepare the line item based on the order id
                                var lineItemResult = result.AsEnumerable().Where(myRow => myRow.Field<int>("OrderID") == Convert.ToInt32(order["OrderID"]));

                                decimal amount = 0;
                                List<OpportunityLineItemEntity> opportunityLineItemEntities = new List<OpportunityLineItemEntity>();

                                DateTime billingDateTime = DateTime.Now;

                                //Line item starts here
                                if (lineItemResult.Count() > 0)
                                {
                                    foreach (var lineItem in lineItemResult)
                                    {
                                        OpportunityLineItemEntity opportunityLineItemEntity = null;
                                        //if (type == "Credit" && opportunityEntity.OriginalBookingSalesforceReference != null)
                                        //{
                                        //    //Original Line Item
                                        //    opportunityLineItemEntity = MarshalOpportunityLineItemFromDBToEntity(result, lineItem,"OriginalSalePrice");
                                        //    amount = amount + opportunityLineItemEntity.UnitPrice;
                                        //    opportunityLineItemEntity.InvoiceDate = Convert.ToDateTime(lineItem["OriginalBillingDate"]);

                                        //    opportunityLineItemEntities.Add(opportunityLineItemEntity);

                                        //    //Credit Line Item
                                        //    opportunityLineItemEntity = MarshalOpportunityLineItemFromDBToEntity(result, lineItem,"SalePrice");
                                        //    billingDateTime = opportunityLineItemEntity.InvoiceDate;
                                        //}
                                        //else
                                        //{
                                            opportunityLineItemEntity = MarshalOpportunityLineItemFromDBToEntity(result, lineItem,"SalePrice");
                                            billingDateTime = opportunityLineItemEntity.InvoiceDate;
                                        //}
                                        amount = amount + opportunityLineItemEntity.UnitPrice;
                                        opportunityLineItemEntities.Add(opportunityLineItemEntity);
                                    }
                                }
                                //It has been assumed that the billing date will be the same for all the opportunities picked up during that run
                                opportunityEntity.ExternalID = opportunityEntity.ExternalID + "_" + billingDateTime.ToString("dd-MM-yyyy");
                                opportunityEntity.Amount = amount;
                                opportunityEntity.OpportunityLineItemEntities = opportunityLineItemEntities;

                                opportunityEntities.Add(opportunityEntity);
                            }
                        }
                        else
                        {
                            Logger.Info("Original Salesforce Reference is missing for the credit opportunity: Order ID - " + orderID.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return opportunityEntities;
        }

        private OpportunityEntity MarshalOpportunityFromDBToEntity(DataRow order,string type)
        {
            OpportunityEntity opportunityEntity= new OpportunityEntity();

            opportunityEntity.Name = (Convert.ToString(order["CompanyName"]) + "-" + Convert.ToString(order["ProductName"]) + "-" + Convert.ToString(order["CloseDate"]));
            opportunityEntity.OrderID = Convert.ToInt32(order["OrderId"]);

            //Account
            opportunityEntity.AccountSalesforceReference = Convert.ToString(order["AccountSalesforceReference"]);
            opportunityEntity.AccountID = Convert.ToString(order["AccountID"]);
            opportunityEntity.AccountName = Convert.ToString(order["AccountName"]);
            opportunityEntity.AccountXRefID = Convert.ToString(order["AccountXRefID"]);
            //Booker
            opportunityEntity.BookerSalesforceReference = Convert.ToString(order["BookerSalesforceReference"]);
            opportunityEntity.BookerID = Convert.ToString(order["BookerID"]);
            opportunityEntity.BookerXrefID = Convert.ToString(order["BookerXRefID"]);

            if (!String.IsNullOrEmpty(Convert.ToString(order["IndividualName"])))
            {
                opportunityEntity.BookerName = Convert.ToString(order["IndividualName"]);
            }
            else
            {
                opportunityEntity.BookerName = Convert.ToString(order["GroupName"]);
            }

            opportunityEntity.CloseDate = Convert.ToDateTime(order["CloseDate"]);
            opportunityEntity.EndCustomerSalesforceReference = Convert.ToString(order["EndCustomerSalesforceReference"]);
            opportunityEntity.EndCustomerID = Convert.ToString(order["EndCustomerID"]);
            opportunityEntity.EndCustomerName = Convert.ToString(order["EndCustomerName"]);
            opportunityEntity.EndCustomerXRefID = Convert.ToString(order["EndCustomerXRefID"]);
            opportunityEntity.PurchaseOrderNumber = Convert.ToString(order["PurchaseOrderNumber"]);
            opportunityEntity.Type = type;

            opportunityEntity.Currency = Convert.ToString(order["CurrencyCode"]);

            opportunityEntity.Brand = Convert.ToString(order["Brand"]);

            if (type == "New Business")
            {
                opportunityEntity.Stage = "Won";
                opportunityEntity.PaymentType = "On Invoice";
                //As the new and credit opportunity lying under same order id. The external id field in SF which requires a unique id will be prefixed with "New_"
                opportunityEntity.ExternalID = "New_" + Convert.ToString(order["OrderId"]);
            }
            else if (type == "Credit")
            {
                opportunityEntity.Stage = "Credit Approved";

               //As the new and credit opportunity lying under same order id. The external id field in SF which requires a unique id will be prefixed with "Credit_"
                opportunityEntity.ExternalID = "Credit_" + Convert.ToString(order["OrderId"]);
                opportunityEntity.OriginalBookingSalesforceReference = Convert.ToString(order["OriginalBookingSalesforceReference"]);
            }
            return opportunityEntity;
        }

        private OpportunityLineItemEntity MarshalOpportunityLineItemFromDBToEntity(EnumerableRowCollection<DataRow> result, DataRow lineItem,string salesType)
        {
            //We need to create a separate opportunity if there are more than one contact in line item. This has been handled in the SQL itself 
            //The below line will be used to get the count so that the unit price can be equally splitted
            var lineItemContact = result.AsEnumerable().Where(myRow => myRow.Field<int>("InvoiceItemId") == Convert.ToInt32(lineItem["InvoiceItemId"]));

            OpportunityLineItemEntity opportunityLineItemEntity = new OpportunityLineItemEntity();

            //Account
            opportunityLineItemEntity.ProductID = Convert.ToString(lineItem["ProductID"]);
            opportunityLineItemEntity.ProductName = Convert.ToString(lineItem["ProductName"]);
            opportunityLineItemEntity.ProductSalesforceReference = Convert.ToString(lineItem["ProductSalesforceReference"]);
            opportunityLineItemEntity.ProductXRefID = Convert.ToString(lineItem["ProductXRefID"]);
            opportunityLineItemEntity.Description= Convert.ToString(lineItem["ItemDescription"]);
            //Contact
            opportunityLineItemEntity.ContactSalesforceReference = Convert.ToString(lineItem["ContactSalesforceReference"]);
            opportunityLineItemEntity.ContactID = Convert.ToString(lineItem["LineItemContactID"]);
            opportunityLineItemEntity.ContactXRefId = Convert.ToString(lineItem["LineItemContactXrefID"]);

            if (!String.IsNullOrEmpty(Convert.ToString(lineItem["IndividualName2"])))
            {
                opportunityLineItemEntity.ContactName = Convert.ToString(lineItem["IndividualName2"]);
            }
            else
            {
                opportunityLineItemEntity.ContactName = Convert.ToString(lineItem["GroupName2"]);
            }

            opportunityLineItemEntity.AutoRenewal = false;
            opportunityLineItemEntity.BillingCycle = "One Off";
            if(!String.IsNullOrEmpty(Convert.ToString(lineItem["EndDate"])))
            {
                opportunityLineItemEntity.EndDate = Convert.ToDateTime(lineItem["EndDate"]);
            }
            else
            {
                opportunityLineItemEntity.EndDate = DateTime.Now.AddDays(10);
                //opportunityLineItemEntity.EndDate = null;
            }
            if (!String.IsNullOrEmpty(Convert.ToString(lineItem["ServiceDate"])))
            {
                opportunityLineItemEntity.ServiceDate = Convert.ToDateTime(lineItem["ServiceDate"]);
            }
            else
            {
                opportunityLineItemEntity.ServiceDate = DateTime.Now;
                //opportunityLineItemEntity.ServiceDate = null;
            }

            opportunityLineItemEntity.Quantity = Convert.ToInt32(lineItem["Quantity"]);
            opportunityLineItemEntity.AutoRenewal = false;

            //The unit price splitted below based on the number of contacts are found.
            //Also in the case of credit, the original line items will also be added in the opportunity
            if (salesType=="SalePrice")
            {
                opportunityLineItemEntity.UnitPrice = Convert.ToDecimal(lineItem["SalesPrice"]) / lineItemContact.Count();
            }
            else if(salesType=="OriginalSalePrice")
            {
                opportunityLineItemEntity.UnitPrice = Convert.ToDecimal(lineItem["OriginalSalesPrice"]) / lineItemContact.Count();
            }
            opportunityLineItemEntity.InvoiceDate = Convert.ToDateTime(lineItem["BillingDate"]);
          
            string InstanceSalesforceReference = Convert.ToString(lineItem["InstanceSalesforceReference"]);
            string InstanceXrefTableID = Convert.ToString(lineItem["InstanceXrefTableID"]);
            string InstanceID = Convert.ToString(lineItem["InstanceID"]);
            string InstanceName = Convert.ToString(lineItem["InstanceName"]);

            //Dont hook up for Subscription InstanceXrefTableID == "-102" and Royalities InstanceXrefTableID == "-101"
            if (!String.IsNullOrEmpty(InstanceXrefTableID))
            {
                if (InstanceXrefTableID == "-102" || InstanceXrefTableID == "137")
                {
                    //Subscription and Publication
                    //Hook the Publication
                    opportunityLineItemEntity.InstanceID = InstanceID;
                    opportunityLineItemEntity.InstanceTypeDescription = InstanceName;
                    opportunityLineItemEntity.InstanceSalesforceReference = InstanceSalesforceReference;
                    opportunityLineItemEntity.InstanceType = "Publication_Issue__c";
                    opportunityLineItemEntity.InstanceXrefId = Convert.ToString(lineItem["InstanceXrefID"]);
                }
                else if (InstanceXrefTableID == "46" || InstanceXrefTableID == "254")
                {
                    //Event and Reecording
                    //Hook to the event
                    opportunityLineItemEntity.InstanceID = InstanceID;
                    opportunityLineItemEntity.InstanceTypeDescription = InstanceName;
                    opportunityLineItemEntity.InstanceSalesforceReference = InstanceSalesforceReference;
                    opportunityLineItemEntity.InstanceType = "Event__c";
                    opportunityLineItemEntity.InstanceXrefId = Convert.ToString(lineItem["InstanceXrefID"]);
                }
            }
            return opportunityLineItemEntity;
        }

        private List<MissingRecordEntity> MarshalMissingRecordsReceivedFromDM(List<OpportunityEntity> opportunityEntities)
        {
            MissingRecordEntity missingRecordEntity=null;
            RecordEntity recordEntity;
            List<RecordEntity> recordEntities = null;

            List<MissingRecordEntity> missingRecordEntities = new List<MissingRecordEntity>();
            try
            {
                Logger.Info("Function called: MarshalMissingRecordsReceivedFromDM");
                foreach (var opportunityEntity in opportunityEntities)
                {
                    recordEntities = new List<RecordEntity>();
                    missingRecordEntity = new MissingRecordEntity();
                    missingRecordEntity.OrderID = opportunityEntity.OrderID;
                    if (String.IsNullOrEmpty(opportunityEntity.AccountSalesforceReference))
                    {
                            recordEntity = new RecordEntity();
                            recordEntity.ObjectID = opportunityEntity.AccountID + "_" + opportunityEntity.AccountXRefID;
                            recordEntity.ObjectName = "Account";
                            recordEntity.ObjectValue = opportunityEntity.AccountName;
                            recordEntity.Description = "No salesforce reference number found for the Account (Office). Account Name:" + opportunityEntity.AccountName +  ", DM AccountID:" + opportunityEntity.AccountID
                                                               + ", OTIS AccountID:" + opportunityEntity.AccountXRefID;
                        recordEntities.Add(recordEntity);
                    }
                    if (String.IsNullOrEmpty(opportunityEntity.BookerSalesforceReference))
                    {
                            recordEntity = new RecordEntity();
                            recordEntity.ObjectID = opportunityEntity.BookerID + "_" + opportunityEntity.BookerXrefID;
                            recordEntity.ObjectName = "Contact";
                            recordEntity.ObjectValue = opportunityEntity.BookerName;
                            recordEntity.Description = "No salesforce reference number found for the Booker (Contact). Contact Name:" + opportunityEntity.BookerName + ", DM ContactID:" + opportunityEntity.BookerID
                                   + ", OTIS ContactID:" + opportunityEntity.BookerXrefID;
                        recordEntities.Add(recordEntity);
                    }
                    if (String.IsNullOrEmpty(opportunityEntity.EndCustomerSalesforceReference))
                    {
                            recordEntity = new RecordEntity();

                            recordEntity.ObjectID = opportunityEntity.EndCustomerID + "_" + opportunityEntity.EndCustomerXRefID;
                            recordEntity.ObjectName = "Account";
                            recordEntity.ObjectValue = opportunityEntity.EndCustomerName;
                        recordEntity.Description = "No salesforce reference number found for the End Customer Name (Account). Account Name:" + opportunityEntity.EndCustomerName + ", DM AccountID:" + opportunityEntity.EndCustomerID
                                                   + ", OTIS AccountID:" + opportunityEntity.EndCustomerXRefID;
                        recordEntities.Add(recordEntity);
                    }

                    foreach (var oppLineItem in opportunityEntity.OpportunityLineItemEntities)
                    {
                        if (String.IsNullOrEmpty(oppLineItem.ProductSalesforceReference))
                        {
                                recordEntity = new RecordEntity();
                                recordEntity.ObjectID = oppLineItem.ProductID + "_" + oppLineItem.ProductXRefID;
                                recordEntity.ObjectName = "Product2";
                                recordEntity.ObjectValue = oppLineItem.ProductName;
                                recordEntity.Description = "No salesforce reference number found for the Product. Product Name:" + oppLineItem.ProductName + ", DM ProductID:" + oppLineItem.ProductID
                                                        + ", OTIS ProductID:" + oppLineItem.ProductXRefID;
                            recordEntities.Add(recordEntity);
                        }
                        if (String.IsNullOrEmpty(oppLineItem.ContactSalesforceReference))
                        {
                                recordEntity = new RecordEntity();
                                recordEntity.ObjectID = oppLineItem.ContactID + "_" + oppLineItem.ContactXRefId;
                                recordEntity.ObjectName = "Contact";
                                recordEntity.ObjectValue = oppLineItem.ContactName;
                                recordEntity.Description = "No salesforce reference number found for the Contact. Contact Name:" + oppLineItem.ContactName + ", DM ContactID:" + oppLineItem.ContactID
                                                       + ", OTIS ContactID:" + oppLineItem.ContactXRefId;
                            recordEntities.Add(recordEntity);
                        }
                        if (!String.IsNullOrEmpty(oppLineItem.InstanceType) && String.IsNullOrEmpty(oppLineItem.InstanceSalesforceReference))
                        {
                                recordEntity = new RecordEntity();
                                recordEntity.ObjectID = oppLineItem.InstanceID + "_" + oppLineItem.InstanceXrefId;
                                recordEntity.ObjectName = oppLineItem.InstanceType;
                                recordEntity.ObjectValue = oppLineItem.InstanceTypeDescription;
                                recordEntity.Description = "No salesforce reference number found for the " + oppLineItem.InstanceType + ". Instance Name:" + oppLineItem.InstanceTypeDescription + ", DM InstanceID:" + oppLineItem.InstanceID
                                                   + ", OTIS InstanceID:" + oppLineItem.InstanceXrefId;
                            recordEntities.Add(recordEntity);
                        }
                    }
                    if (recordEntities.Count > 0)
                    {
                        missingRecordEntity.RecordEntities = recordEntities;
                        missingRecordEntities.Add(missingRecordEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return missingRecordEntities;
        }
        #endregion
    }
}