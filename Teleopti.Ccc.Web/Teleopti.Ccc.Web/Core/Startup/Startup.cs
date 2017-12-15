using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Services;
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
using Owin;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class Startup
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Startup));

		private readonly IBootstrapper _bootstrapper = new Bootstrapper();
		private readonly IContainerConfiguration _containerConfiguration = new ContainerConfiguration();

		private static bool _applicationStarted;
		private static readonly object applicationStartLock = new object();

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
			MvcHandler.DisableMvcResponseHeader = true;
			ApplicationStartModule.ErrorAtStartup = null;
			try
			{
				var container = _containerConfiguration.Configure(pathToToggle(), config);

				AutofacHostFactory.Container = container;

				ApplicationStartModule.RequestContextInitializer = container.Resolve<IRequestContextInitializer>();

				DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
				GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

				GlobalHost.DependencyResolver =
					new Autofac.Integration.SignalR.AutofacDependencyResolver(container);
				container.Resolve<IEnumerable<IHubPipelineModule>>().ForEach(m => GlobalHost.HubPipeline.AddModule(m));

				ApplicationStartModule.TasksFromStartup =
					_bootstrapper.Run(container.Resolve<IEnumerable<IBootstrapperTask>>(), application).ToArray();

				SignalRConfiguration.Configure(SignalRSettings.Load(), () => application.MapSignalR(new HubConfiguration()));
				FederatedAuthentication.WSFederationAuthenticationModule.SignedIn += WSFederationAuthenticationModule_SignedIn;
				FederatedAuthentication.WSFederationAuthenticationModule.SignInError +=
					WSFederationAuthenticationModule_SignInError;
				FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers.AddOrReplace(
					new MachineKeySessionSecurityTokenHandler());

				application.UseAutofacMiddleware(container);
				application.UseAutofacMvc();
				application.UseAutofacWebApi(config);
			}

			catch (Exception ex)
			{
				log.Error(ex);
				ApplicationStartModule.ErrorAtStartup = ex;
			}
		}

		private static string pathToToggle()
		{
			return Path.Combine(HttpContext.Current.Server.MapPath("~/"), ConfigurationManager.AppSettings["FeatureToggle"]);
		}

		private void WSFederationAuthenticationModule_SignInError(object sender,
			System.IdentityModel.Services.ErrorEventArgs e)
		{
			// http://stackoverflow.com/questions/15904480/how-to-avoid-samlassertion-notonorafter-condition-is-not-satisfied-errors
			if (e.Exception.Message.StartsWith("ID4148") ||
				e.Exception.Message.StartsWith("ID4243") ||
				e.Exception.Message.StartsWith("ID4223"))
			{
				FederatedAuthentication.SessionAuthenticationModule.DeleteSessionTokenCookie();
				e.Cancel = true;
			}
		}

		private void WSFederationAuthenticationModule_SignedIn(object sender, EventArgs e)
		{
			WSFederationMessage wsFederationMessage =
				WSFederationMessage.CreateFromFormPost(new HttpRequestWrapper(HttpContext.Current.Request));
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