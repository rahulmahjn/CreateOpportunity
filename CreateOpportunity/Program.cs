using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using CreateOpportunity.CustomWebService;
using CreateOpportunity.Enterprise;
using System.Net;
using CreateOpportunity.BAL;
using System.Configuration;
namespace CreateOpportunity
{
    static class Program
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        static void Main()
        {
            #if (DEBUG)
                OpportunityOperation opportunityOperation = new OpportunityOperation(Logger);

                opportunityOperation.Process();
            
            #else
                 ServiceBase[] ServicesToRun;

                ServicesToRun = new ServiceBase[]
                 {
                     new CreateOpportunity()
                 };
                 ServiceBase.Run(ServicesToRun);
            #endif
        }
    }
}
