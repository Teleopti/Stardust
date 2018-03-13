using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Services;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;
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
		private static bool _applicationStarted;
		private static readonly object applicationStartLock = new object();
		private readonly IBootstrapper _bootstrapper = new Bootstrapper();
		private readonly IContainerConfiguration _containerConfiguration = new ContainerConfiguration();

		public void Configuration(IAppBuilder app)
		{
			if (_applicationStarted) return;
			lock (applicationStartLock)
			{
				if (_applicationStarted) return;
				// this will run only once per application start
				OnStart(app, new HttpConfiguration());
				_applicationStarted = true;
			}
		}

		public static Exception ErrorAtStartup { get; set; }
		public static Task[] TasksFromStartup { get; set; }

		public void OnStart(IAppBuilder application, HttpConfiguration config)
		{
			MvcHandler.DisableMvcResponseHeader = true;
			ErrorAtStartup = null;
			try
			{
				var container = new DontDisposeMe(_containerConfiguration.Configure(pathToToggle(), config));

				AutofacHostFactory.Container = container;

				ApplicationModule.Inject(container);

				DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
				GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

				GlobalHost.DependencyResolver = new Autofac.Integration.SignalR.AutofacDependencyResolver(container);
				container.Resolve<IEnumerable<IHubPipelineModule>>().ForEach(m => GlobalHost.HubPipeline.AddModule(m));

				TasksFromStartup = _bootstrapper.Run(container.Resolve<IEnumerable<IBootstrapperTask>>(), application).ToArray();

				SignalRConfiguration.Configure(SignalRSettings.Load(), () => application.MapSignalR(new HubConfiguration()));
				FederatedAuthentication.WSFederationAuthenticationModule.SignedIn += wsFederationAuthenticationModuleSignedIn;
				FederatedAuthentication.WSFederationAuthenticationModule.SignInError += wsFederationAuthenticationModuleSignInError;
				FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers.AddOrReplace(new MachineKeySessionSecurityTokenHandler());
			}
			catch (Exception ex)
			{
				log.Error(ex);
				ErrorAtStartup = ex;
			}
		}

		private static string pathToToggle() =>
			Path.Combine(HttpContext.Current.Server.MapPath("~/"), ConfigurationManager.AppSettings["FeatureToggle"]);

		private void wsFederationAuthenticationModuleSignInError(object sender, System.IdentityModel.Services.ErrorEventArgs e)
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

		private static void wsFederationAuthenticationModuleSignedIn(object sender, EventArgs e)
		{
			var wsFederationMessage = WSFederationMessage.CreateFromFormPost(new HttpRequestWrapper(HttpContext.Current.Request));
			if (wsFederationMessage.Context == null) return;

			var wctx = HttpUtility.ParseQueryString(wsFederationMessage.Context);
			var returnUrl = wctx["ru"];

			// TODO: check for absolute url and throw to avoid open redirects
			HttpContext.Current.Response.Redirect(returnUrl, false);
			HttpContext.Current.ApplicationInstance.CompleteRequest();
		}
	}
	
	public class DontDisposeMe : ILifetimeScope
	{
		private readonly IContainer _container;

		public DontDisposeMe(IContainer container)
		{
			_container = container;
		}

		public void Dispose()
		{
		}
		
		public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters) => _container.ResolveComponent(registration, parameters);
		public IComponentRegistry ComponentRegistry => _container.ComponentRegistry;
		public ILifetimeScope BeginLifetimeScope() => _container.BeginLifetimeScope();
		public ILifetimeScope BeginLifetimeScope(object tag) => _container.BeginLifetimeScope(tag);
		public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction) => _container.BeginLifetimeScope(configurationAction);
		public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction) => _container.BeginLifetimeScope(tag, configurationAction);
		public IDisposer Disposer => _container.Disposer;
		public object Tag => _container.Tag;
		
		public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning
		{
			add => _container.ChildLifetimeScopeBeginning += value;
			remove => _container.ChildLifetimeScopeBeginning -= value;
		}

		public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding
		{
			add => _container.CurrentScopeEnding += value;
			remove => _container.CurrentScopeEnding -= value;
		}

		public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning
		{
			add => _container.ResolveOperationBeginning += value;
			remove => _container.ResolveOperationBeginning -= value;
		}
	}
	
}