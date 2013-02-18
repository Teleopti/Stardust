using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using log4net;

[assembly: PreApplicationStartMethod(typeof(ApplicationStartModule), "RegisterModule")]

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class ApplicationStartModule : IHttpModule
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (ApplicationStartModule));

		public static void RegisterModule()
		{
			DynamicModuleUtility.RegisterModule(typeof(ApplicationStartModule));
		}

		public static Exception ErrorAtStartup { get; set; }

		public static bool HasStartupError
		{
			get { return ErrorAtStartup != null; }
		}

		private static bool _applicationStarted;
		private static readonly object ApplicationStartLock = new object();

		public void Dispose()
		{
		}

		public void Init(HttpApplication application)
		{
			if (!_applicationStarted)
			{
				lock (ApplicationStartLock)
				{
					if (!_applicationStarted)
					{
						// this will run only once per application start
						OnStart(application);
						_applicationStarted = true;
					}
				}
			}
			// this will run on every HttpApplication initialization in the application pool
			OnInit(application);
		}

		private IBootstrapper _bootstrapper = new Bootstrapper();
		private IContainerConfiguration _containerConfiguration = new ContainerConfiguration();
		private bool _testMode;

		public void HackForTest(IBootstrapper injectedBootstrapper, IContainerConfiguration injectedContainerConfiguration)
		{
			_bootstrapper = injectedBootstrapper;
			_containerConfiguration = injectedContainerConfiguration;
			_testMode = true;
			_applicationStarted = false;
		}

		public void OnInit(HttpApplication application)
		{
			//todo: mem leak? how to unhook this eventhandler?
			application.PostAuthenticateRequest += delegate
			                                       	{
			                                       		if (HasStartupError) return;

														// exclude TestController from principal stuff
			                                       		if (HttpContext.Current.Request.Url.AbsolutePath.Contains("/Test/")) return;

			                                       		var requestContextInitializer = DependencyResolver
			                                       			.Current
			                                       			.GetService<IRequestContextInitializer>()
			                                       			;
			                                       		requestContextInitializer.SetupPrincipalAndCulture();
			                                       	};

		}

		public void OnStart(HttpApplication application)
		{
			ErrorAtStartup = null;
			try
			{
				var container = _containerConfiguration.Configure();
				if (!_testMode)
				{
					DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

                    var resolver = new Areas.Anywhere.Core.AutofacDependencyResolver(container.BeginLifetimeScope());
					GlobalHost.DependencyResolver = resolver;
					GlobalHost.HubPipeline.AddModule(container.Resolve<IHubPipelineModule>());
					RouteTable.Routes.MapHubs(new HubConfiguration());
				}
				_bootstrapper.Run(container.Resolve<IEnumerable<IBootstrapperTask>>());
			}
			catch (Exception ex)
			{
				log.Error(ex);
				ErrorAtStartup = ex;
			}
		}
	}
}