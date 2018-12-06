using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using Autofac;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Web.Auth;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Ccc.Web.Core.Startup;

[assembly: PreApplicationStartMethod(typeof(ApplicationModule), "RegisterModule")]

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class ApplicationModule : IHttpModule
	{
		public static void RegisterModule()
		{
			DynamicModuleUtility.RegisterModule(typeof(ApplicationModule));
		}

		private static IEnumerable<string> withoutPrincipal()
		{
			yield return "/togglehandler/";
			yield return "/test/";
			yield return "/content/";
			yield return "/signalr/ping/";
			yield return "/js/";
			yield return "/css/";
			yield return "/html/";
			yield return "/vendor/";
			yield return "/rta/state/";
		}

		private static IEnumerable<string> ignoreStartupErrors()
		{
			yield return "/togglehandler/";
		}

		public void Init(HttpApplication application)
		{
			// this will run on every HttpApplication initialization in the application pool
			application.BeginRequest += (s, e) =>
			{
				if (_isDisposed) return;
				checkForStartupErrors();
				appendServerVersionHeader();
			};
			application.PostAuthenticateRequest += (s, e) =>
			{
				if (_isDisposed) return;
				setupPrincipal();
			};
			application.Error += (s, e) =>
			{
				var error = HttpContext.Current.Server.GetLastError();
				if (error is CryptographicException)
				{
					FederatedAuthentication.WSFederationAuthenticationModule.SignOut();
					HttpContext.Current.Server.ClearError();
				}
			};
			application.EndRequest += (s, e) =>
			{
				var error = HttpContext.Current.Server.GetLastError();
				var response = HttpContext.Current.Response;
				if (error != null) response.ContentType = "text/html; charset=utf-8";
			};
		}

		private static IRequestContextInitializer _requestContextInitializer;
		private static SystemVersion _systemVersion;

		public static void Inject(ILifetimeScope container)
		{
			_requestContextInitializer = container.Resolve<IRequestContextInitializer>();
			_systemVersion = container.Resolve<SystemVersion>();
		}

		private static void appendServerVersionHeader()
		{
			var serverVersion = _systemVersion.Version();
			var response = HttpContext.Current.Response;
			response.AddOnSendingHeaders(context =>
			{
				if (context.Response.Headers["X-Server-Version"] == null)
				{
					context.Response.AppendHeader("X-Server-Version", serverVersion);
				}
			});
		}

		private static void setupPrincipal()
		{
			if (requestMatching(withoutPrincipal())) return;
			_requestContextInitializer.SetupPrincipalAndCulture(onlyUseGregorianCalendar(HttpContext.Current));
		}

		private static bool requestMatching(IEnumerable<string> patterns)
		{
			var url = HttpContext.Current.Request.UrlConsideringLoadBalancerHeaders().AbsolutePath.ToLowerInvariant();
			return patterns.Any(url.Contains);
		}

		private readonly object _taskWaitLockObject = new object();
		private bool _noStartupErrors;

		private void checkForStartupErrors()
		{
			if (_noStartupErrors)
				return;

			if (requestMatching(ignoreStartupErrors()))
				return;

			if (Startup.TasksFromStartup != null)
			{
				lock (_taskWaitLockObject)
				{
					if (Startup.TasksFromStartup != null)
					{
						Task.WaitAll(Startup.TasksFromStartup);
						Startup.TasksFromStartup = null;
					}
				}
			}

			if (Startup.ErrorAtStartup != null)
			{
				var startupException = new ApplicationException("Failure on start up", Startup.ErrorAtStartup);
				PreserveStack.ForInnerOf(startupException);
				throw startupException;
			}

			_noStartupErrors = true;
		}

		private static bool onlyUseGregorianCalendar(HttpContext context)
		{
			var useGregorianCalendar = string.Empty;
			var headers = context.Request.Headers;
			useGregorianCalendar = headers["X-Use-GregorianCalendar"] ?? useGregorianCalendar;
			return !string.IsNullOrEmpty(useGregorianCalendar) && bool.Parse(useGregorianCalendar);
		}

		private bool _isDisposed;
		public void Dispose() => _isDisposed = true;
	}
}