using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IWT.OracleSync.Business;
using IWT.OracleSync.Data;
using IWT.OracleSync.Console;

namespace Oracle_Data_Synchronization
{
    public partial class Oracle_Data_Synchronization : ServiceBase
    {
        private Timer Schedular;
        private static bool Starter = false;
        public Oracle_Data_Synchronization()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteLog.WriteToFile("Oracle data synchronization service has started");
            this.ScheduleService();
        }

        protected override void OnStop()
        {
            WriteLog.WriteToFile("Oracle data synchronization service has stopped");
        }

        public void ScheduleService() //schdule timing
        {
            try
            {

                OracleDBSync oracleSync = new OracleDBSync();
                oracleSync.GetOracleData();

                Schedular = new Timer(new TimerCallback(SchedularCallback));

                //Set the Default Time.
                DateTime scheduledTime = DateTime.MinValue;



                //Get the Interval in Minutes from AppSettings.
                int intervalTime = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalTime"]);

                //Set the Scheduled Time by adding the Interval to Current Time.
                scheduledTime = DateTime.Now.AddSeconds(intervalTime);
                if (DateTime.Now > scheduledTime)
                {
                    //If Scheduled Time is passed set Schedule for the next Interval.
                    scheduledTime = scheduledTime.AddSeconds(intervalTime);
                }


                TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);
                string schedule = string.Format("{0} day(s) {1} hour(s) {2} minute(s) {3} seconds(s)", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

                WriteLog.WriteToFile("Oracle data synchronization Service scheduled to run after: " + schedule);
                //Get the difference in Minutes between the Scheduled and Current Time.
                int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);

                //Change the Timer's Due Time.
                Schedular.Change(dueTime, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile(ex.Message);

                //Stop the Windows Service.
                using (System.ServiceProcess.ServiceController serviceController = new System.ServiceProcess.ServiceController("SimpleService"))
                {
                    serviceController.Stop();
                }
            }
        }

        private void SchedularCallback(object e)
        {
            Starter = true;
            this.ScheduleService();
        }
    }
}
