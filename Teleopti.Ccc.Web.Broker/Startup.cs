﻿using System;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.SignalR;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Broker;
using RegistrationExtensions = Autofac.Integration.Mvc.RegistrationExtensions;

[assembly: OwinStartup(typeof(Startup))]

namespace Teleopti.Ccc.Web.Broker
{
	public class Startup
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Startup));

		public void Configuration(IAppBuilder app)
		{
			log4net.Config.XmlConfigurator.Configure();

			var settings = SignalRSettings.Load();

			var builder = new ContainerBuilder();
			builder.RegisterModule<MessageBrokerWebModule>();
			builder.RegisterModule(new MessageBrokerServerModule(settings.ThrottleMessages, settings.MessagesPerSecond));
			builder.RegisterType<SubscriptionPassThrough>().As<IBeforeSubscribe>().SingleInstance();
			builder.RegisterHubs(typeof(MessageBrokerHub).Assembly);
			RegistrationExtensions.RegisterControllers(builder, typeof(MessageBrokerController).Assembly);
			var container = builder.Build();

			HostingEnvironment.RegisterObject(new actionThrottleStopper(container));

			var lifetimeScope = container.BeginLifetimeScope();
			GlobalHost.DependencyResolver = new AutofacDependencyResolver(lifetimeScope); 
			
			var hubConfiguration = new HubConfiguration { EnableJSONP = true };
			SignalRConfiguration.Configure(settings, () => app.MapSignalR(hubConfiguration));

			TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

			DependencyResolver.SetResolver(new Autofac.Integration.Mvc.AutofacDependencyResolver(lifetimeScope));
			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);
		}

		private class actionThrottleStopper : IRegisteredObject
		{
			private readonly IComponentContext _container;

			public actionThrottleStopper(IComponentContext container)
			{
				_container = container;
			}

			public void Stop(bool immediate)
			{
				var actionScheduler = _container.Resolve<IActionScheduler>();
				if (actionScheduler is IDisposable)
					(actionScheduler as IDisposable).Dispose();
				HostingEnvironment.UnregisterObject(this);
			}
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