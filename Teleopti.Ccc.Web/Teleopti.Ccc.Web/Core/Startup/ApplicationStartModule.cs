using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Web.Broker;
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

		public static bool HasStartupError
		{
			get { return ErrorAtStartup != null; }
		}

		public void Dispose()
		{
		}

		public void Init(HttpApplication application)
		{
			// this will run on every HttpApplication initialization in the application pool
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

			if (HasStartupError)
				application.BeginRequest += onEveryRequest;
		}

		private void onEveryRequest(object sender, EventArgs e)
		{
			if (HasStartupError)
			{
				var startupException = new ApplicationException("Failure on start up", ErrorAtStartup);
				PreserveStack.ForInnerOf(startupException);
				
throw startupException;
			}
		}
	}

	public class ActionThrottleObject : IRegisteredObject
	{
		public void Stop(bool immediate)
		{
			if (SignalRConfiguration.ActionScheduler is IDisposable)
			{
				var actionThrottle = SignalRConfiguration.ActionScheduler as ActionThrottle;
				if (actionThrottle != null) actionThrottle.Dispose();
			}
			HostingEnvironment.UnregisterObject(this);
		}
	}
}
