﻿using System.ServiceProcess;

namespace Teleopti.Analytics.Etl.ServiceHost
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
                                { 
                                    new Service1() 
                                };
            ServiceBase.Run(ServicesToRun);
        }
    }
}