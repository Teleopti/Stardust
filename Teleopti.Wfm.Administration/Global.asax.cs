using System;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
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
		}
	}
}