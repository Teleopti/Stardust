using System;
using System.Threading.Tasks;
using System.Web;
using Autofac;
using Autofac.Integration.SignalR;
using Microsoft.AspNet.SignalR;
using log4net;

namespace Teleopti.Ccc.Web.Broker
{
	public class Global : HttpApplication
	{
	    private static readonly ILog Logger = LogManager.GetLogger(typeof (Global));
		private static IContainer _container;

		protected void Application_Start(object sender, EventArgs e)
		{
			log4net.Config.XmlConfigurator.Configure();

			var containerBuilder = new ContainerBuilder();
			containerBuilder.Register(c => SignalRConfiguration.ActionScheduler).As<IActionScheduler>();
			containerBuilder.RegisterType<SubscriptionPassThrough>().As<IBeforeSubscribe>();
			containerBuilder.RegisterHubs(typeof (MessageBrokerHub).Assembly);
			_container = containerBuilder.Build();
			GlobalHost.DependencyResolver = new AutofacDependencyResolver(_container.BeginLifetimeScope()); 
			
			var hubConfiguration = new HubConfiguration { EnableCrossDomain = true };
			SignalRConfiguration.Configure(hubConfiguration);

            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
		}

		protected void Application_End(object sender, EventArgs e)
		{
			if (SignalRConfiguration.ActionScheduler is IDisposable)
			{
				var actionThrottle = SignalRConfiguration.ActionScheduler as ActionThrottle;
				if (actionThrottle != null) actionThrottle.Dispose();
			}
		}

	    private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
	    {
	        if (!unobservedTaskExceptionEventArgs.Observed)
	        {
	            Logger.Error("An error occured, please review the error and take actions necessary.",
	                         unobservedTaskExceptionEventArgs.Exception);
	            unobservedTaskExceptionEventArgs.SetObserved();
	        }
	    }

	    protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{

		}

		protected void Application_Error(object sender, EventArgs e)
		{

		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

	}
}