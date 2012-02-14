using System.ServiceProcess;

namespace Teleopti.Messaging.Svc
{
    static class MessageBrokerStartUp
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun = new ServiceBase[] {  new MessageBrokerSvc() };
            ServiceBase.Run(ServicesToRun);
        }

    }
}
