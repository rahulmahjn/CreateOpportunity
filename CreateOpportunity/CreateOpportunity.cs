using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
    public partial class CreateOpportunity : ServiceBase
    {
        static Timer timer;
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        DateTime dateTime;

        public CreateOpportunity()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("******************************************Service Started************************************");
            schedule_Timer();
        }

        protected override void OnStop()
        {
            Logger.Info("******************************************Service Stopped************************************");
        }

        static void schedule_Timer()
        {
            DateTime nowTime = DateTime.Now;
            var hour = Convert.ToString(ConfigurationManager.AppSettings["hour"]);
            var minute = Convert.ToString(ConfigurationManager.AppSettings["minute"]);
            var second = Convert.ToString(ConfigurationManager.AppSettings["second"]);

            DateTime scheduledTime = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, Convert.ToInt32(hour), Convert.ToInt32(minute), Convert.ToInt32(second), 0); //Specify your scheduled time HH,MM,SS [8am and 42 minutes]

            Logger.Info("Service scheduled to push the opportunities at: " + hour + ":" + minute + ":" +  second);

            if (nowTime > scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            double tickTime = (double)(scheduledTime - DateTime.Now).TotalMilliseconds;
            timer = new Timer(tickTime);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }

        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            OpportunityOperation opportunityOperation = new OpportunityOperation(Logger);

            opportunityOperation.Process();

            schedule_Timer();
        }
    }
}
