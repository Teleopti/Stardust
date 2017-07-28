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
			DynamicModuleUtility.RegisterModule(typeof (ApplicationStartModule));
		}

		public static Exception ErrorAtStartup { get; set; }
		public static Task[] TasksFromStartup { get; set; }
		private readonly object _taskWaitLockObject = new object();

		private Action unsubscribeStartupErrorsAction;
		private Action unsubscribePostAuthenticateAction;

		private static readonly string[] keyWords = {
			"/togglehandler/",
			"/test/",
			"/content/",
			"/signalr/ping",
			"/js/",
			"/css/",
			"/html/",
			"/vendor/"
		};

		public void Init(HttpApplication application)
		{
			// this will run on every HttpApplication initialization in the application pool
			application.PostAuthenticateRequest += setupPrincipal;
			application.BeginRequest += checkForStartupErrors;

			unsubscribeStartupErrorsAction = () => { application.BeginRequest -= checkForStartupErrors; };
			unsubscribePostAuthenticateAction = () => { application.PostAuthenticateRequest -= setupPrincipal; };
		}

		private void setupPrincipal(object sender, EventArgs e)
		{
			// exclude TestController from principal stuff
			var url = HttpContext.Current.Request.Url.AbsolutePath.ToLowerInvariant();
			if (isTestController(url)) return;
			
			var requestContextInitializer = DependencyResolver.Current.GetService<IRequestContextInitializer>();

			requestContextInitializer.SetupPrincipalAndCulture(onlyUseGregorianCalendar(HttpContext.Current));
		}

		private static bool isTestController(string url)
		{
			return keyWords.Any(url.Contains);
		}

		private void checkForStartupErrors(object sender, EventArgs e)
		{
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

			unsubscribeStartupErrorsAction?.Invoke();
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
			unsubscribeStartupErrorsAction?.Invoke();
			unsubscribePostAuthenticateAction?.Invoke();

			unsubscribeStartupErrorsAction = null;
			unsubscribePostAuthenticateAction = null;
		}
	}
}