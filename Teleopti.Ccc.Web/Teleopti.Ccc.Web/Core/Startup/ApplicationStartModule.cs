using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Ccc.Web.Core.Startup;

[assembly: PreApplicationStartMethod(typeof(ApplicationStartModule), "RegisterModule")]

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class ApplicationStartModule : IHttpModule
	{
		public static void RegisterModule()
		{
			DynamicModuleUtility.RegisterModule(typeof(ApplicationStartModule));
		}

		public static Exception ErrorAtStartup { get; set; }
		public static Task[] TasksFromStartup { get; set; }

		private readonly object _taskWaitLockObject = new object();
		private bool _noStartupErrors;

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
		}

		private static IEnumerable<string> ignoreStartupErrors()
		{
			yield return "/togglehandler/";
		}

		private bool _isDisposed;

		public void Init(HttpApplication application)
		{
			// this will run on every HttpApplication initialization in the application pool
			application.BeginRequest += (s, e) =>
			{
				if (_isDisposed) return;
				checkForStartupErrors();
				validateClientVersion();
			};
			application.PostAuthenticateRequest += (s, e) =>
			{
				if (_isDisposed) return;
				setupPrincipal();
			};
		}

		private static void validateClientVersion()
		{
			var clientVersion = HttpContext.Current.Request.Headers["X-Client-Version"];
			var versionValidationDisabled = string.IsNullOrEmpty(clientVersion);
			if (versionValidationDisabled)
				return;
			var clientVersionIsOk = clientVersion == SystemVersion.Version();
			if (clientVersionIsOk)
				return;

			HttpContext.Current.Response.StatusCode = 418;
			HttpContext.Current.Response.End();
		}

		public static IRequestContextInitializer RequestContextInitializer { get; set; }
		public static SystemVersion SystemVersion { get; set; }

		private void setupPrincipal()
		{
			if (requestMatching(withoutPrincipal())) return;
			RequestContextInitializer.SetupPrincipalAndCulture(onlyUseGregorianCalendar(HttpContext.Current));
		}

		private static bool requestMatching(IEnumerable<string> patterns)
		{
			var url = HttpContext.Current.Request.Url.AbsolutePath.ToLowerInvariant();
			return patterns.Any(url.Contains);
		}

		private void checkForStartupErrors()
		{
			if (_noStartupErrors)
				return;

			if (requestMatching(ignoreStartupErrors()))
				return;

			if (TasksFromStartup != null)
			{
				lock (_taskWaitLockObject)
				{
					if (TasksFromStartup != null)
					{
						Task.WaitAll(TasksFromStartup);
						TasksFromStartup = null;
					}
				}
			}

			if (ErrorAtStartup != null)
			{
				var startupException = new ApplicationException("Failure on start up", ErrorAtStartup);
				PreserveStack.ForInnerOf(startupException);
				throw startupException;
			}

			_noStartupErrors = true;
		}

		private bool onlyUseGregorianCalendar(HttpContext context)
		{
			var useGregorianCalendar = string.Empty;
			var headers = context.Request.Headers;
			useGregorianCalendar = headers["X-Use-GregorianCalendar"] ?? useGregorianCalendar;
			return !string.IsNullOrEmpty(useGregorianCalendar) && bool.Parse(useGregorianCalendar);
		}

		public void Dispose()
		{
			_isDisposed = true;
		}
	}
}