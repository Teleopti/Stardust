using System;
using System.IO;
using Autofac;
using Autofac.Integration.WebApi;
using Hangfire;
using Hangfire.Dashboard;
using log4net.Config;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.Hangfire;
using GlobalConfiguration = System.Web.Http.GlobalConfiguration;

namespace Teleopti.Wfm.Administration
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			// Code that runs on application startup
			GlobalConfiguration.Configure(WebApiConfig.Register);

			var config = GlobalConfiguration.Configuration;

			var builder = new ContainerBuilder();
			builder.RegisterModule<WfmAdminModule>();

			// Set the dependency resolver to be Autofac.
			var container = builder.Build();
			config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

			setupHangfireDashboard(app, container.Resolve<IConfigReader>());
		}

		private static void setupHangfireDashboard(IAppBuilder app, IConfigReader config)
		{
			// Cookie only used with hangfire where we are not possible to change to requests with an token.
			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
			});

			Hangfire.GlobalConfiguration.Configuration.UseSqlServerStorage(config.ConnectionString("Hangfire"));
			var options = new DashboardOptions
			{
				AuthorizationFilters = new IAuthorizationFilter[]
				{
					new HangfireDashboardAuthorization()
				},
				AppPath = null
			};

			if (config.ReadValue("HangfireDashboard", true))
				app.UseHangfireDashboard("/hangfire", options);
		}
	}
}