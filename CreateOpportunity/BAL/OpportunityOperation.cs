using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using CreateOpportunity.CustomWebService;
using CreateOpportunity.Enterprise;
using System.Net;
using CreateOpportunity.BAL;
using System.Configuration;

namespace CreateOpportunity.BAL
{
   public class OpportunityOperation
    {
        NLog.Logger Logger;

        public OpportunityOperation(NLog.Logger Logger)
        {
            this.Logger = Logger;
        }

        public void Process()
        {
            try
            {
                Logger.Info("******************************************Process Started************************************");

                var userName = ConfigurationManager.AppSettings["userName"];
                var password = ConfigurationManager.AppSettings["password"];
                var passwordSecurityToken = ConfigurationManager.AppSettings["passwordSecurityToken"];

                bool IsLoginToSF = false;
                int retryCount = 0;

                LoginResult Session = null;
                do
                {
                    try
                    {
                        if (retryCount > 0)
                        {
                            Logger.Info("Re-trying to login to Salesforce after 30 secs. Retry Count:" + retryCount.ToString());
                            System.Threading.Thread.Sleep(30000);
                        }
                        var binding = new SforceService();

                        Logger.Info("Logging into Salesforce to retreive the Session ID.");
                        Session = binding.login(userName, password + passwordSecurityToken);
                        IsLoginToSF = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Info("Logging into Salesforce is failed. Exception raised:" + ex.Message);
                        retryCount++;
                    }
                } while (!IsLoginToSF && retryCount != 3);

                if (Session != null)
                {
                    if (!String.IsNullOrEmpty(Session.sessionId))
                    {
                        var otisSoapServicesService = new otisSoapServicesService();
                        SessionHeader sessionHeader = new SessionHeader();
                        
                        otisSoapServicesService.SessionHeaderValue.sessionId = Session.sessionId;
                        Opportunity opportunity = new Opportunity(Logger, otisSoapServicesService);

                        Logger.Info("*****************Started Create Opportunity Process*****************");
                        CreateOpportunityProcess(Session, opportunity);
                        Logger.Info("*****************Finished Create Opportunity Process*****************");

                        Logger.Info("*****************Started Invoice Process*****************");
                        UpdateInvoiceNumber(Session, opportunity);
                        Logger.Info("*****************Finished Invoice Process*****************");
                    }
                    else
                    {
                        Logger.Info("No session id received. Process can't be continued.");
                    }
                }
                else
                {
                    Logger.Info("No session id received. Process can't be continued.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception Message Raised: " + ex.Message);

                if (ex.InnerException != null)
                {
                    Logger.Error("Inner Exception Message: " + ex.InnerException.Message);
                }
            }
            finally
            {
                Logger.Info("******************************************Process Finished************************************");
            }
        }

        void CreateOpportunityProcess(LoginResult Session, Opportunity opportunity)
        {
            try
            {
                Dictionary<int, string> errMessage = new Dictionary<int, string>();

                Logger.Info("------------Started getting opportunities from DM------------");
                //1. Get opportunities from DM
                var opportunities = opportunity.GetOpportunities();

                if (opportunities != null)
                {
                    if (opportunities.Count > 0)
                    {
                        Logger.Info("Total number of opportunities found are: " + opportunities.Select(row => row.OrderID).Distinct().Count().ToString());
                        Logger.Info("------------Finished getting opportunities from DM------------");

                        //1.Process will run through with following three 

                        MismatchRecords(opportunities, Session, opportunity);

                        MissingRecords(opportunities, Session, opportunity);
                                               

                        //Opportunities are filetred through DM and Salesforce validation check. Its important to check the count before creating the opportunities
                        CreateOpportunity(opportunities, Session, opportunity);

                    }
                    else
                    {
                        Logger.Info("No opportunities found from DM.");
                        Logger.Info("------------Finished getting opportunities from DM------------");
                    }
                }
                else
                {
                    Logger.Info("No opportunities found from DM.");
                    Logger.Info("------------Finished getting opportunities from DM------------");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void MismatchRecords(List<Entity.OpportunityEntity> opportunities, LoginResult Session, Opportunity opportunity)
        {
            if (opportunities.Count > 0)
            {
                opportunity.PostMismatchRecords(opportunities, Session.sessionId);
            }
        }
        void MissingRecords(List<Entity.OpportunityEntity> opportunities, LoginResult Session, Opportunity opportunity)
        {
            Logger.Info("------------Started checking missing records------------");

            //2. Find missing records
            //2.1 New records in DM which is not yet created in SF
            //2.2 Wrong Salesforce number is updated in DM

            var missingRecords = opportunity.GetMissingObjectRecords(opportunities, Session.sessionId);



            if (missingRecords.Count > 0)
            {
                Logger.Info("------------Finished checking missing records------------");

                Logger.Info("------------Started posting missing records-----------");
                var responsePostMissingRecords = opportunity.PostMissingObjectRecords(missingRecords, Session.sessionId);


                if (String.IsNullOrEmpty(responsePostMissingRecords.salesforceErrorMessage))
                {
                    Logger.Info("------------Finished posting missing records: " + responsePostMissingRecords.responseSummary + "-------------");
                }
                else
                {
                    Logger.Info("Posting missing records error received from Salesforce: " + responsePostMissingRecords.salesforceErrorMessage);
                    Logger.Info("------------Finished posting missing records-----------");
                }

                //update the unique Order IDs
                var uniqueOrderIDs = missingRecords.Select(row => row.OrderID).Distinct();
                foreach (var ID in uniqueOrderIDs)
                {
                    opportunities.RemoveAll(opp => opp.OrderID == ID);
                }
            }
            else
            {
                Logger.Info("No missing record(s) found.");
                Logger.Info("------------Finished checking missing records------------");
            }
        }

        void CreateOpportunity(List<Entity.OpportunityEntity> opportunities, LoginResult Session, Opportunity opportunity)
        {
            if (opportunities.Count > 0)
            {
                Logger.Info("Remaining opportunities which are ready to be posted to Salesforce are: " + opportunities.Count.ToString());
                //3. Create Opportunity
                Logger.Info("------------Started posting opportunities------------");
                var responseCreateOpportunities = opportunity.CreateOpportunities(opportunities, Session.sessionId);


                if (String.IsNullOrEmpty(responseCreateOpportunities.salesforceErrorMessage))
                {
                    Logger.Info("------------Finished posting opportunities. Count:" + opportunities.Count.ToString() + "-------------");

                    //List<String> otisIDs = new List<string>();
                    int receivedResponseCount = 0;

                    var postedOpportunityCount = opportunities.Count;
                    string OrderIDs = String.Empty;
                    var otisIDs = opportunities.Select(row => row.OrderID).ToList<int>();

                    if (otisIDs != null)
                    {
                        foreach (var otisID in otisIDs)
                        {
                            OrderIDs = OrderIDs + Convert.ToString(otisID) + ",";
                        }
                        if (!String.IsNullOrEmpty(OrderIDs))
                        {
                            OrderIDs = "(" + OrderIDs.TrimEnd(',') + ")";

                            Logger.Info("------------Started updating posted opportunity Timestamp in DM------------");
                            //Record the Posted DateTime stamp in Order Table
                            opportunity.UpdateOpportunity(OrderIDs, String.Empty, "POSTEDDATETIME", DateTime.Now);
                            Logger.Info("------------Finished updating posted opportunity Timestamp in DM------------");
                        }
                    }
                    //Log mention the number of opportunities records posted are
                    var externalIDs = opportunities.Select(row => row.ExternalID).ToList<String>();
                    int responseRetryCount = 0;

                    addOpportunitiesResponse responseOpportunities = null;
                    var RetryCount = Convert.ToInt32(ConfigurationManager.AppSettings["RetryCount"]);
                    //Continue the loop until the count posted doesn't meet
                    do
                    {
                        if (responseRetryCount == RetryCount)
                        {
                            Logger.Info("Number of re-tries are exhausted. Process treminated. Retry count: " + RetryCount.ToString());
                            break;
                        }

                        Logger.Info("Wait for 30 secs to receive the opportunity response. Retry Count:" + responseRetryCount.ToString());
                        //Wait for 30 seconds to get the response
                        System.Threading.Thread.Sleep(30000);

                        Logger.Info("------------Started requesting opportunities response------------");
                        //4. Get response from opportunity
                        responseOpportunities = opportunity.GetOpportunitiesAsynchResponse(externalIDs, Session.sessionId);
                        if (responseOpportunities != null)
                        {
                            var responseInserted = responseOpportunities.inserted;
                            var responseRejected = responseOpportunities.rejected;
                            var overCapacity = responseOpportunities.overCapacity;

                            if (String.IsNullOrEmpty(responseOpportunities.salesforceErrorMessage))
                            {
                                int insertedCount = 0;

                                int RejectedCount = 0;

                                if (responseInserted != null)
                                {
                                    insertedCount = responseInserted.Count();
                                }
                                if (responseRejected != null)
                                {
                                    RejectedCount = responseRejected.Count();
                                }
                                receivedResponseCount = insertedCount + RejectedCount;
                                Logger.Info("Opportunities posted are: " + postedOpportunityCount.ToString() + " and received resoponse are: " + receivedResponseCount.ToString());

                                if (overCapacity != null)
                                {
                                    Logger.Info("Record(s) are still awaiting to be processed in Salesforce. Count: " + overCapacity.Count().ToString());
                                }
                            }
                            else
                            {
                                receivedResponseCount = postedOpportunityCount;
                            }

                            responseRetryCount = responseRetryCount + 1;
                        }
                        //Keep looping through until the number of records posted are not met with response received count
                    } while (postedOpportunityCount != receivedResponseCount);

                    if (postedOpportunityCount == receivedResponseCount)
                    {
                        if (responseOpportunities != null)
                        {
                            if (String.IsNullOrEmpty(responseOpportunities.salesforceErrorMessage))
                            {
                                if (responseOpportunities.inserted != null)
                                {
                                    Logger.Info("Successfull response count received are: " + responseOpportunities.inserted.Count().ToString());

                                    string insertedOtisIDs = String.Empty;

                                    foreach (var inserted in responseOpportunities.inserted)
                                    {
                                        insertedOtisIDs = insertedOtisIDs + inserted.otisId + ",";
                                        var identifiers = inserted.otisId.Split('_');
                                        opportunity.UpdateOpportunity(identifiers[1], inserted.opportunityKey, "REFERENCE", Convert.ToDateTime(identifiers[2]));
                                    }

                                    insertedOtisIDs = insertedOtisIDs.TrimEnd(',');
                                    Logger.Info("Successfull OtisIDS are: " + insertedOtisIDs.ToString());
                                }

                                if (responseOpportunities.rejected != null)
                                {

                                    Logger.Info("Rejected response count received are: " + responseOpportunities.rejected.Count().ToString());

                                    string rejectedOtisIDs = String.Empty;
                                    foreach (var rejected in responseOpportunities.rejected)
                                    {
                                        rejectedOtisIDs = rejectedOtisIDs + rejected.otisId + ",";
                                        var identifiers = rejected.otisId.Split('_');
                                        opportunity.UpdateOpportunity(identifiers[1],
                                            "Opportunity failed to create in Salesforce, error received: " + rejected.salesforceErrorMessage, "ERROR", Convert.ToDateTime(identifiers[2]));
                                    }
                                    rejectedOtisIDs = rejectedOtisIDs.TrimEnd(',');
                                    Logger.Info("Rejected OtisIDS are: " + rejectedOtisIDs.ToString());
                                }
                            }
                            else
                            {
                                Logger.Info("No record processed. Salesforce error received in response: " + responseCreateOpportunities.salesforceErrorMessage);
                                Logger.Info("------------Finished requesting opportunities response-------------");
                            }
                            Logger.Info("------------Finished requesting opportunities response-------------");
                        }
                    }


                }
                else
                {
                    Logger.Info("-------------Posting opportunities error received from Salesforce: " + responseCreateOpportunities.salesforceErrorMessage + "------------");
                    Logger.Info("------------Finished successfully posting opportunities-------------");
                }
            }
            else
            {
                Logger.Info("No opportunities are found or left to post to Salesforce.");
            }
        }

        void UpdateInvoiceNumber(LoginResult Session, Opportunity opportunity)
        {
            try
            {
                var result = opportunity.UpdateInvoiceNumber(Session.sessionId);
                Logger.Info("Number of records updated in DM are :" + result.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
