using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.MultipleConfig;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.Core
{
	public class WfmAdminModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterModule<TenantServerModule>();
			builder.RegisterApiControllers(typeof(HomeController).Assembly).ApplyAspects();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new AppConfigReader()), new FalseToggleManager())));
			builder.RegisterType<AdminTenantAuthentication>().As<ITenantAuthentication>().SingleInstance();
			builder.RegisterType<DatabaseHelperWrapper>().SingleInstance();

			// OPTIONAL: Enable property injection into action filters.
			builder.RegisterFilterProvider();
		}
	}
}