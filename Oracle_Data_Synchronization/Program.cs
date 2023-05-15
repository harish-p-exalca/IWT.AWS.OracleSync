﻿using System.ServiceProcess;

namespace Oracle_Data_Synchronization
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Oracle_Data_Synchronization()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
