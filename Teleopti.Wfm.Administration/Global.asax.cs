using System;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.MultipleConfig;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces.Infrastructure;
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

			// Register your API controllers.
			//builder.RegisterApiControllers(typeof(TenantList).Assembly).ApplyAspects();

			//builder.RegisterType<TenantList>().As<ITenantList>();

			//builder.RegisterType<ConfigReader>().As<IConfigReader>().SingleInstance();
			//builder.Register(c =>
			//{
			//	var configReader = c.Resolve<IConfigReader>();
			//	var connStringToTenant = configReader.ConnectionStrings[tenancyConnectionStringKey];
			//	var connstringAsString = connStringToTenant == null ? null : connStringToTenant.ConnectionString;
			//	return TenantUnitOfWorkManager.CreateInstanceForWeb(connstringAsString);
			//})
			//	.As<ITenantUnitOfWork>()
			//	.As<ICurrentTenantSession>()
			//	.SingleInstance();
			//builder.RegisterType<TenantUnitOfWorkAspect>().As<ITenantUnitOfWorkAspect>().SingleInstance();

			//// OPTIONAL: Register model binders that require DI.
			//builder.RegisterModelBinders(Assembly.GetExecutingAssembly());
			//builder.RegisterModelBinderProvider();

			//// OPTIONAL: Register web abstractions like HttpContextBase.
			//builder.RegisterModule<AutofacWebTypesModule>();

			//// OPTIONAL: Enable property injection in view pages.
			//builder.RegisterSource(new ViewRegistrationSource());

			// OPTIONAL: Enable property injection into action filters.
			builder.RegisterFilterProvider();

			// Set the dependency resolver to be Autofac.
			var container = builder.Build();
			config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
		}
	}
}