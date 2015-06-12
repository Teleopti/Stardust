using System;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.MultipleConfig;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration
{
	public class Global : HttpApplication
	{
		void Application_Start(object sender, EventArgs e)
		{
			// Code that runs on application startup
			GlobalConfiguration.Configure(WebApiConfig.Register);

			var builder = new ContainerBuilder();
			var config = GlobalConfiguration.Configuration;

			builder.RegisterModule<TenantServerModule>();
			builder.RegisterApiControllers(typeof(TenantList).Assembly).ApplyAspects();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new AppConfigReader()), new FalseToggleManager())));
			builder.RegisterType<TenantList>().As<ITenantList>().SingleInstance();
			builder.RegisterType<AdminTenantAuthentication>().As<ITenantAuthentication>().SingleInstance();

			// OPTIONAL: Enable property injection into action filters.
			builder.RegisterFilterProvider();

			// Set the dependency resolver to be Autofac.
			var container = builder.Build();
			config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
		}
	}
}