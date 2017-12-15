using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Teleopti.Ccc.Domain;
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


		private bool _isDisposed;

		public void Init(HttpApplication application)
		{
			// this will run on every HttpApplication initialization in the application pool
			application.PostAuthenticateRequest += setupPrincipal;
			application.BeginRequest += checkForStartupErrors;
		}

		public static IRequestContextInitializer RequestContextInitializer { get; set; }

		private void setupPrincipal(object sender, EventArgs e)
		{
			if (requestWithoutPrincipal()) return;
			if (_isDisposed) return;
			RequestContextInitializer.SetupPrincipalAndCulture(onlyUseGregorianCalendar(HttpContext.Current));
		}

		private static bool requestWithoutPrincipal()
		{
			// exclude TestController from principal stuff
			var url = HttpContext.Current.Request.Url.AbsolutePath.ToLowerInvariant();
			return new[]
			{
				"/togglehandler/",
				"/test/",
				"/content/",
				"/signalr/ping",
				"/js/",
				"/css/",
				"/html/",
				"/vendor/"
			}.Any(url.Contains);
		}

		private void checkForStartupErrors(object sender, EventArgs e)
		{
			if (_noStartupErrors) return;

			var url = HttpContext.Current.Request.Url.AbsolutePath.ToLowerInvariant();
			if (url.Contains("/togglehandler/")) return;

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