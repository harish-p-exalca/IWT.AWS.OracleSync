using IWT.OracleSync.Business;
using IWT.OracleSync.Data;
using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;

namespace WeighSyncService
{
    public partial class WeighSyncService : ServiceBase
    {
        private Timer Schedular;
        private static bool Starter = false;
        public WeighSyncService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteLog.WriteToFile("Starting Weigh Sync Service");
            this.ScheduleService();
        }

        public void ScheduleService() //schdule timing
        {
            try
            {
                if (Starter)
                {
                    new OracleDBSync().GetOracleData();
                }

                Schedular = new Timer(new TimerCallback(SchedularCallback));

                //Set the Default Time.
                DateTime scheduledTime = DateTime.MinValue;

                //Get the Interval in Minutes from AppSettings.
                int intervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalMinutes"]);

                //Set the Scheduled Time by adding the Interval to Current Time.
                scheduledTime = DateTime.Now.AddMinutes(intervalMinutes);
                if (DateTime.Now > scheduledTime)
                {
                    //If Scheduled Time is passed set Schedule for the next Interval.
                    scheduledTime = scheduledTime.AddMinutes(intervalMinutes);
                }

                TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);
                string schedule = string.Format("{0} day(s) {1} hour(s) {2} minute(s) {3} seconds(s)", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

                WriteLog.WriteToFile("Weigh Sync Service scheduled to run after: " + schedule);
                //Get the difference in Minutes between the Scheduled and Current Time.
                int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);

                //Change the Timer's Due Time.
                Schedular.Change(dueTime, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Exception:-"+ex.Message);
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

        protected override void OnStop()
        {
            WriteLog.WriteToFile("Stopping Weigh Sync Service");
        }
    }
}
