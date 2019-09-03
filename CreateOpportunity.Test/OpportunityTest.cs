using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using CreateOpportunity.Enterprise;
using CreateOpportunity.CustomWebService;
using CreateOpportunity.BAL;
using CreateOpportunity.Entity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace CreateOpportunity.Test
{
    [TestClass]
    public class OpportunityTest
    {
        /// <summary>
        /// Post Missing Records to Salesforce
        /// </summary>
        [TestMethod]
        public void Post_Missing_Records_To_Salesforce()
        {
            //Opportunity opportunity = new Opportunity();

            //var userName = ConfigurationManager.AppSettings["userName"];
            //var password = ConfigurationManager.AppSettings["password"];
            //var passwordSecurityToken = ConfigurationManager.AppSettings["passwordSecurityToken"];

            //var binding = new SforceService();
            //var Session = binding.login(userName, password + passwordSecurityToken);

            //var opportunities = opportunity.GetOpportunities();

            //var missingRecords = opportunity.GetMissingObjectRecords(opportunities, Session.sessionId);

            //if (missingRecords.Count > 0)
            //{
            //    var responsePostMissingRecords = opportunity.PostMissingObjectRecords(missingRecords, Session.sessionId);

            //    Assert.AreEqual(responsePostMissingRecords.salesforceErrorMessage,null);
            //}
        }
      /// <summary>
      /// Create Opportunity in Salesforce
      /// </summary>
        [TestMethod]
        public void Create_Opportunity()
        {
            //Opportunity opportunity = new Opportunity();

            //var userName = ConfigurationManager.AppSettings["userName"];
            //var password = ConfigurationManager.AppSettings["password"];
            //var passwordSecurityToken = ConfigurationManager.AppSettings["passwordSecurityToken"];

            //var binding = new SforceService();
            //var Session = binding.login(userName, password + passwordSecurityToken);

            ////Get Opportunity
            //var opportunities = opportunity.GetOpportunities();

            ////Get Missing records and find order ID to remove from the list collection
            //var missingRecords = opportunity.GetMissingObjectRecords(opportunities, Session.sessionId);

            //if (missingRecords.Count > 0)
            //{
            //    //Find thg Order ID(s) and remove from the collection
            //    var uniqueOrderIDs = missingRecords.Select(row => row.OrderID).Distinct();
            //    foreach (var ID in uniqueOrderIDs)
            //    {
            //        opportunities.RemoveAll(opp => opp.OrderID == ID);
            //    }
            //}

            ////Create Opportunity
            //var responseCreateOpportunities = opportunity.CreateOpportunities(opportunities, Session.sessionId);

            //if (String.IsNullOrEmpty(responseCreateOpportunities.salesforceErrorMessage))
            //{

            //    int receivedResponseCount = 0;
            //    var otisIDs = opportunities.Select(row => row.ExternalID).ToList<String>();

            //    var postedOpportunityCount = opportunities.Count;

            //    do
            //    {
            //        //Wait for 30 seconds to get the response
            //        System.Threading.Thread.Sleep(30000);
            //        //4. Get response from opportunity
            //        var responseOpportunities = opportunity.GetOpportunitiesAsynchResponse(otisIDs, Session.sessionId);

            //        if (String.IsNullOrEmpty(responseOpportunities.salesforceErrorMessage))
            //        {
            //            int insertedCount = 0;
            //            int RejectedCount = 0;
            //            var responseInserted = responseOpportunities.inserted;
            //            var responseRejected = responseOpportunities.rejected;
            //            if(responseInserted!=null)
            //            {
            //                insertedCount=responseInserted.Count();
            //            }
            //            if (responseRejected != null)
            //            {
            //                RejectedCount = responseRejected.Count();
            //            }
            //            receivedResponseCount = receivedResponseCount + insertedCount + RejectedCount;
            //        }
            //        else
            //        {
            //            //Stop the loop and fail the test method
            //            receivedResponseCount = postedOpportunityCount;
            //            Assert.Fail();
            //        }

            //    } while (postedOpportunityCount != receivedResponseCount);

            //    Assert.AreEqual(receivedResponseCount, postedOpportunityCount);
            //}
            //else
            //{
            //    Assert.Fail();
            //}
           
        }
        [TestMethod]
        public void Get_Opportunities_From_DM()
        {
            //Opportunity opportunity = new Opportunity();

            //var opportunities = opportunity.GetOpportunities();
        }
        private List<OpportunityEntity> GetMockOpportunities()
        {
            List<OpportunityEntity> opportunityEntities = new List<OpportunityEntity>();
            OpportunityEntity opportunityEntity = new OpportunityEntity();

            opportunityEntity.AccountSalesforceReference = "DS110162896";
            opportunityEntity.BookerSalesforceReference = "CON-000528067";
            opportunityEntity.EndCustomerSalesforceReference = "CON-000528067";

            List<OpportunityLineItemEntity> opportunityLineItemEntities = new List<OpportunityLineItemEntity>();
            OpportunityLineItemEntity opportunityLineItemEntity = new OpportunityLineItemEntity();
            opportunityLineItemEntity.AutoRenewal = true;
            opportunityLineItemEntities.Add(opportunityLineItemEntity);

            opportunityEntity.OpportunityLineItemEntities = opportunityLineItemEntities;
            opportunityEntities.Add(opportunityEntity);
            
            return opportunityEntities;
        }
    }
}
