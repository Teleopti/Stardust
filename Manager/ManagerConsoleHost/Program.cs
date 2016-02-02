using System;
using System.Configuration;
using log4net;
using log4net.Config;
using Stardust.Manager;
using Stardust.Manager.Models;

namespace ManagerConsoleHost
{
    internal class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (ManagerController));

        private static string WhoAmI { get; set; }

        private static void Main(string[] args)
        {
            WhoAmI = "[MANAGER, " + Environment.MachineName.ToUpper() + "]";

            XmlConfigurator.Configure();

            Logger.Info(WhoAmI +  " : started.");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var config = new ManagerConfiguration
            {
                BaseAdress = ConfigurationManager.AppSettings["baseAddress"],
                ConnectionString = ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString
            };
            new ManagerStarter().Start(config);
        }

        private static void CurrentDomain_UnhandledException(object sender,
            UnhandledExceptionEventArgs e)
        {
            Logger.Error(WhoAmI + ": Unhandeled Exception in ManagerConsoleHost");
        }
    }
}