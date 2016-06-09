using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.Wcf;
using Autofac.Integration.WebApi;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Microsoft.IdentityModel.Web;
using Owin;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class Startup
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ApplicationStartModule));

		private IBootstrapper _bootstrapper = new Bootstrapper();
		private IContainerConfiguration _containerConfiguration = new ContainerConfiguration();
		private bool _testMode;

		private static bool _applicationStarted;
		private static readonly object applicationStartLock = new object();

		public void InjectForTest(IBootstrapper injectedBootstrapper, IContainerConfiguration injectedContainerConfiguration)
		{
			_bootstrapper = injectedBootstrapper;
			_containerConfiguration = injectedContainerConfiguration;
			_testMode = true;
			_applicationStarted = false;
		}

		public void Configuration(IAppBuilder app)
		{
			if (!_applicationStarted)
			{
				lock (applicationStartLock)
				{
					if (!_applicationStarted)
					{
						// this will run only once per application start
						OnStart(app, new HttpConfiguration());
						_applicationStarted = true;
					}
				}
			}
		}

		public void OnStart(IAppBuilder application, HttpConfiguration config)
		{
			GlobalConfiguration.Configuration.Filters.Add(new NoCacheFilter());

			MvcHandler.DisableMvcResponseHeader = true;
			ApplicationStartModule.ErrorAtStartup = null;
			try
			{
				var pathToToggle = Startup.pathToToggle();
				var container = _containerConfiguration.Configure(pathToToggle, config);

				AutofacHostFactory.Container = container;
				if (!_testMode)
				{
					DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
					GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

					GlobalHost.DependencyResolver =
						new Autofac.Integration.SignalR.AutofacDependencyResolver(container.BeginLifetimeScope());
					container.Resolve<IEnumerable<IHubPipelineModule>>().ForEach(m => GlobalHost.HubPipeline.AddModule(m));
				}

				ApplicationStartModule.TasksFromStartup =
					_bootstrapper.Run(container.Resolve<IEnumerable<IBootstrapperTask>>(), application).ToArray();

				SignalRConfiguration.Configure(SignalRSettings.Load(), () => application.MapSignalR(new HubConfiguration()));
				FederatedAuthentication.WSFederationAuthenticationModule.SignedIn += WSFederationAuthenticationModule_SignedIn;
				FederatedAuthentication.ServiceConfiguration.SecurityTokenHandlers.AddOrReplace(
					new MachineKeySessionSecurityTokenHandler());

				application.UseAutofacMiddleware(container);
				application.UseAutofacMvc();
				application.UseAutofacWebApi(config);

				//var managerConfiguration = new ManagerConfiguration
				//{
				//	ConnectionString =
				//	ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString,
				//	Route = ConfigurationManager.AppSettings["RouteName"]
				//};


				//new ManagerStarter().Start(managerConfiguration, container);
			}

			catch (Exception ex)
			{
				log.Error(ex);
				ApplicationStartModule.ErrorAtStartup = ex;
			}
		}

		private static string pathToToggle()
		{
			return inTestEnvironement() ? "inTest" : Path.Combine(HttpContext.Current.Server.MapPath("~/"), ConfigurationManager.AppSettings["FeatureToggle"]);
		}

		private static bool inTestEnvironement()
		{
			return HttpContext.Current == null;
		}

		void WSFederationAuthenticationModule_SignedIn(object sender, EventArgs e)
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