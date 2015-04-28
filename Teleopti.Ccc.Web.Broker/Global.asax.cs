using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
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

			var builder = new ContainerBuilder();
			builder.RegisterModule(new MessageBrokerWebModule());
			builder.RegisterModule(new MessageBrokerServerModule());
			_container = builder.Build();

			var lifetimeScope = _container.BeginLifetimeScope();
			GlobalHost.DependencyResolver = new AutofacDependencyResolver(lifetimeScope); 
			
			var hubConfiguration = new HubConfiguration { EnableCrossDomain = true };
			SignalRConfiguration.Configure(hubConfiguration);

            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
			
			DependencyResolver.SetResolver(new Autofac.Integration.Mvc.AutofacDependencyResolver(lifetimeScope));
			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
				);
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
	            Logger.Debug("An error occured, please review the error and take actions necessary.",
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