using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Microsoft.IdentityModel.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using log4net;
using AutofacDependencyResolver = Autofac.Integration.Mvc.AutofacDependencyResolver;

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
		public static Task[] TasksFromStartup { get; set; }

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

		public void InjectForTest(IBootstrapper injectedBootstrapper, IContainerConfiguration injectedContainerConfiguration)
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

					GlobalHost.DependencyResolver = new Autofac.Integration.SignalR.AutofacDependencyResolver(container.BeginLifetimeScope()); 
					container.Resolve<IEnumerable<IHubPipelineModule>>().ForEach(m => GlobalHost.HubPipeline.AddModule(m));
					SignalRConfiguration.Configure(new HubConfiguration {EnableCrossDomain = true});
				}
				TasksFromStartup = _bootstrapper.Run(container.Resolve<IEnumerable<IBootstrapperTask>>()).ToArray();
				application.Disposed += (s, e) =>
					{
						if (SignalRConfiguration.ActionScheduler is IDisposable)
						{
							var actionThrottle = SignalRConfiguration.ActionScheduler as ActionThrottle;
							if (actionThrottle != null) actionThrottle.Dispose();
						}
					};

				FederatedAuthentication.WSFederationAuthenticationModule.SignedIn += new System.EventHandler(WSFederationAuthenticationModule_SignedIn);
			}
			catch (Exception ex)
			{
				log.Error(ex);
				ErrorAtStartup = ex;
			}
		}

		void WSFederationAuthenticationModule_SignedIn(object sender, System.EventArgs e)
		{
			WSFederationMessage wsFederationMessage = WSFederationMessage.CreateFromFormPost(HttpContext.Current.Request);
			if (wsFederationMessage.Context != null)
			{
				var wctx = HttpUtility.ParseQueryString(wsFederationMessage.Context);
				string returnUrl = wctx["ru"];

				// TODO: check for absolute url and throw to avoid open redirects
				HttpContext.Current.Response.Redirect(returnUrl, false);
				HttpContext.Current.ApplicationInstance.CompleteRequest();
			}
		}
	}
}