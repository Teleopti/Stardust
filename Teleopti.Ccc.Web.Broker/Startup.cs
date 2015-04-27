using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.SignalR;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Teleopti.Ccc.Web.Broker;
using RegistrationExtensions = Autofac.Integration.Mvc.RegistrationExtensions;

[assembly: OwinStartup(typeof(Startup))]

namespace Teleopti.Ccc.Web.Broker
{
	public class Startup
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (Global));
		private static IContainer _container;

		public void Configuration(IAppBuilder app)
		{
			log4net.Config.XmlConfigurator.Configure();

			var builder = new ContainerBuilder();
			builder.RegisterModule<MessageBrokerWebModule>();
			builder.RegisterModule<MessageBrokerServerModule>();
			_container = builder.Build();

			var lifetimeScope = _container.BeginLifetimeScope();
			GlobalHost.DependencyResolver = new AutofacDependencyResolver(lifetimeScope); 
			
			var hubConfiguration = new HubConfiguration { EnableJSONP = true };
			SignalRConfiguration.Configure(()=>app.MapSignalR(hubConfiguration));

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