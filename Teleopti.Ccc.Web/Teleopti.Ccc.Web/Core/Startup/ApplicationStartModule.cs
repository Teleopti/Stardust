using System;
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

				requestContextInitializer.SetupPrincipalAndCulture(onlyUseGregorianCalendar(HttpContext.Current));

			};

			if (HasStartupError)
				application.BeginRequest += mayThrowStartupException;
		}

		private bool onlyUseGregorianCalendar(HttpContext context)
		{
			var useGregorianCalendar = String.Empty;
			var headers = context.Request.Headers;
			useGregorianCalendar = headers["X-Use-GregorianCalendar"] ?? useGregorianCalendar;
			if (string.IsNullOrEmpty(useGregorianCalendar)) return false;
			return Boolean.Parse(useGregorianCalendar);
		}

		private void mayThrowStartupException(object sender, EventArgs e)
		{
			if (!HasStartupError) return;
			var startupException = new ApplicationException("Failure on start up", ErrorAtStartup);
			PreserveStack.ForInnerOf(startupException);
			throw startupException;
		}
	}
}