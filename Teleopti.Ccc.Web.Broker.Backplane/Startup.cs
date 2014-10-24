using System.Threading.Tasks;
using Contrib.SignalR.SignalRMessageBus.Backend;
using log4net;
using Microsoft.Owin;
using Owin;
using Teleopti.Ccc.Web.Broker.Backplane;

[assembly: OwinStartup(typeof(Startup))]

namespace Teleopti.Ccc.Web.Broker.Backplane
{
	public class Startup
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Startup));

		public void Configuration(IAppBuilder app)
		{
			log4net.Config.XmlConfigurator.Configure();

			var storage = new IdStorage();
			storage.OnStart();

			app.MapSignalR<SignalRBackplane>("/backplane");

			TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
		}

		private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
		{
			if (!unobservedTaskExceptionEventArgs.Observed)
			{
				Logger.Debug("An error occured, please review the error and take actions necessary.",
							 unobservedTaskExceptionEventArgs.Exception);
				unobservedTaskExceptionEventArgs.SetObserved();
			}
		}
	}
}