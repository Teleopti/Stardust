using System.ServiceProcess;

namespace Teleopti.Ccc.Sdk.ServiceBus.Host
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun = new ServiceBase[] 
                                              { 
                                                  new ServiceBusHost() 
                                              };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
