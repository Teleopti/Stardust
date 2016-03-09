using System;
using System.IO;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using log4net.Config;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration
{
	public class Global : HttpApplication
	{
		void Application_Start(object sender, EventArgs e)
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
		}
	}
}