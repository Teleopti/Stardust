using System.Configuration;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.Core
{
	public class WfmAdminModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			var iocConf = new IocConfiguration(new IocArgs(new ConfigReader()), new FalseToggleManager());

			builder.RegisterModule(new TenantServerModule(iocConf));
			builder.RegisterApiControllers(typeof(HomeController).Assembly).ApplyAspects();
			builder.RegisterModule(new CommonModule(iocConf));
			builder.RegisterType<AdminTenantAuthentication>().As<ITenantAuthentication>().SingleInstance();
			builder.RegisterType<DatabaseHelperWrapper>().SingleInstance();
			builder.RegisterType<CheckDatabaseVersions>().SingleInstance();
			builder.RegisterType<GetImportUsers>().SingleInstance();
			builder.RegisterType<LoadAllPersonInfos>().SingleInstance();
			builder.RegisterType<Import>().SingleInstance();
			builder.RegisterType<SaveTenant>().SingleInstance();
			builder.RegisterType<DbPathProvider>().As<IDbPathProvider>().SingleInstance();
			builder.RegisterType<CheckPasswordStrength>().As<ICheckPasswordStrength>().SingleInstance();
			builder.RegisterType<FindTenantAdminUserByEmail>().SingleInstance();
			
			builder.Register(c => new LoadPasswordPolicyService(ConfigurationManager.AppSettings["ConfigurationFilesPath"])).SingleInstance().As<ILoadPasswordPolicyService>();
			builder.RegisterType<PasswordPolicy>().SingleInstance().As<IPasswordPolicy>();

			// OPTIONAL: Enable property injection into action filters.
			builder.RegisterFilterProvider();
		}
	}
}