using System;
using System.IO;
using Autofac;
using Autofac.Integration.WebApi;
using log4net.Config;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.Modules;
using System.Web.Http;

namespace Teleopti.Wfm.Administration
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			// Code that runs on application startup
			GlobalConfiguration.Configure(WebApiConfig.Register);

			var config = GlobalConfiguration.Configuration;
			config.Filters.Add(new NoCacheFilterHttp());

			var builder = new ContainerBuilder();
			builder.RegisterModule<WfmAdminModule>();
			var container = builder.Build();

			// Set the dependency resolver to be Autofac.
			config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

			// Cookie only used with hangfire where we are not possible to change to requests with an token.
			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
			});

			container.Resolve<HangfireDashboardStarter>().Start(app);
		}
	}
}