using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.WsFederation;
using Owin;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
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
		private static readonly object ApplicationStartLock = new object();

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
				lock (ApplicationStartLock)
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

				SignalRConfiguration.Configure(SignalRSettings.Load(),
					() => application.MapSignalR(new HubConfiguration { EnableJSONP = true }));
				
				application.UseAutofacMiddleware(container);
				application.UseAutofacMvc();
				application.UseAutofacWebApi(config);

				application.SetDefaultSignInAsAuthenticationType(WsFederationAuthenticationDefaults.AuthenticationType);
				application.UseCookieAuthentication(
					new CookieAuthenticationOptions
					{
						AuthenticationType =
						   WsFederationAuthenticationDefaults.AuthenticationType
					});
				var wreply = ConfigurationManager.AppSettings["ReplyAddress"].ReplaceWithRelativeUriWhenEnabled().ToString();
				var defaultIdentityProvider = container.Resolve<IIdentityProviderProvider>();
				application.UseWsFederationAuthentication(
					new WsFederationAuthenticationOptions
					{
						MetadataAddress = ConfigurationManager.AppSettings["AuthenticationBridgeMetadata"].ReplaceWithCustomEndpointHostOrLocalhost().ToString(),
						Wtrealm = ConfigurationManager.AppSettings["Realm"],
						Wreply = wreply,

						AuthenticationMode = AuthenticationMode.Active,
						Notifications = new WsFederationAuthenticationNotifications
						{
							RedirectToIdentityProvider = notification =>
							{
								notification.ProtocolMessage.IssuerAddress =
									notification.ProtocolMessage.IssuerAddress.ReplaceWithRelativeUriWhenEnabled().ToString();
								if (!notification.ProtocolMessage.IsSignOutMessage)
								{
									if (!(notification.OwinContext.Request.QueryString.HasValue && notification.OwinContext.Request.QueryString.Value.EndsWith("nowhr")))
									{
										notification.ProtocolMessage.Whr = defaultIdentityProvider.DefaultProvider();
									}
									notification.ProtocolMessage.Wctx += "&ru=" + Uri.EscapeDataString(notification.OwinContext.Request.Uri.PathAndQuery);
								}
								else
								{
									notification.ProtocolMessage.Wreply = wreply + "?nowhr";
								}
								
								if (notification.OwinContext.Request.Path.StartsWithSegments(new PathString("/api")) || isAjaxRequest(notification.Request))
								{
									notification.HandleResponse();
									return Task.FromResult(false);
								}

								notification.Response.Redirect(VirtualPathUtility.ToAbsolute("~/Start/Return/Hash?redirectUrl=" + Uri.EscapeDataString(notification.ProtocolMessage.BuildRedirectUrl())));
								notification.HandleResponse();
								return Task.FromResult(false);
							},
							SecurityTokenValidated = notification =>
							{
								var contextData = notification.ProtocolMessage.Wctx;
								var items = HttpUtility.ParseQueryString(contextData);
								var returnUrl = items["ru"];
								if (!string.IsNullOrEmpty(returnUrl))
								{
									notification.AuthenticationTicket.Properties.RedirectUri = returnUrl;
								}
								return Task.FromResult(false);
							}
						}
					});

				application.Map("/login", map =>
				{
					map.Run(ctx =>
					{
						if (ctx.Authentication.User == null ||
							!ctx.Authentication.User.Identity.IsAuthenticated)
						{
							ctx.Response.StatusCode = 401;
						}
						else
						{
							ctx.Response.Redirect("?done");
						}
						return Task.FromResult(true);
					});
				});

				application.Map("/logout", map =>
				{
					map.Run(ctx =>
					{
						ctx.Authentication.SignOut(WsFederationAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
						ctx.Response.Redirect(VirtualPathUtility.ToAbsolute("~/login?nowhr"));
						return Task.FromResult(true);
					});
				});
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

		private bool isAjaxRequest(IOwinRequest request)
		{
			var query = request.Query;

			if ((query != null) && (query["X-Requested-With"] == "XMLHttpRequest"))
			{
				return true;
			}

			var headers = request.Headers;
			return ((headers != null) && (headers["X-Requested-With"] == "XMLHttpRequest"));
		}

		private static bool inTestEnvironement()
		{
			return HttpContext.Current == null;
		}
	}
}