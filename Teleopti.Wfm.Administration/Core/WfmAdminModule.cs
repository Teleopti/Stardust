using System;
using System.Configuration;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Support.Security;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core.Hangfire;
using Teleopti.Wfm.Administration.Core.Stardust;

namespace Teleopti.Wfm.Administration.Core
{
	public class WfmAdminModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			var iocArgs = new IocArgs(new ConfigReader());
			var toggleManager = CommonModule.ToggleManagerForIoc(iocArgs);
			var iocConf = new IocConfiguration(iocArgs, toggleManager);

			builder.RegisterModule(new TenantServerModule(iocConf));
			builder.RegisterApiControllers(typeof(HomeController).Assembly).ApplyAspects();
			builder.RegisterModule(new CommonModule(iocConf));
			builder.RegisterType<AdminTenantAuthentication>().As<ITenantAuthentication>().SingleInstance();
			builder.RegisterType<DatabaseHelperWrapper>().As<IDatabaseHelperWrapper>().SingleInstance();
			builder.RegisterType<UpdateCrossDatabaseView>().As<IUpdateCrossDatabaseView>().SingleInstance();
			builder.RegisterType<CheckDatabaseVersions>().As<ICheckDatabaseVersions>().SingleInstance();
			builder.RegisterType<RunWithUnitOfWork>().As<IRunWithUnitOfWork>().SingleInstance();
			builder.RegisterType<GetImportUsers>().As<IGetImportUsers>().SingleInstance();
			builder.RegisterType<LoadAllPersonInfos>().SingleInstance();
			builder.RegisterType<LoadAllTenants>().As<ILoadAllTenants>().SingleInstance();
			builder.RegisterType<Import>().SingleInstance();
			builder.RegisterType<SaveTenant>().SingleInstance();
			builder.RegisterType<CreateBusinessUnit>().As<ICreateBusinessUnit>().InstancePerDependency();
			builder.RegisterType<DbPathProvider>().As<IDbPathProvider>().SingleInstance();
			builder.RegisterType<CheckPasswordStrength>().As<ICheckPasswordStrength>().SingleInstance();
			builder.RegisterType<DeleteTenant>().SingleInstance();
			builder.RegisterType<UpdateCrossDatabaseView>().SingleInstance();
			builder.RegisterType<DatabaseUpgrader>().SingleInstance();
			builder.RegisterType<DatabasePatcher>().SingleInstance();
			builder.RegisterType<TenantUpgrader>().SingleInstance();
			builder.RegisterType<NullLog>().As<IUpgradeLog>();
			builder.RegisterType<UpgradeRunner>().SingleInstance();
			builder.RegisterType<UpgradeLogRetriever>().As<IUpgradeLogRetriever>().SingleInstance();
			builder.RegisterType<HangfireCookie>().As<IHangfireCookie>().SingleInstance();
			builder.Register(c => new LoadPasswordPolicyService(ConfigurationManager.AppSettings["ConfigurationFilesPath"])).SingleInstance().As<ILoadPasswordPolicyService>();
			builder.RegisterType<PasswordPolicy>().SingleInstance().As<IPasswordPolicy>();
			builder
				.Register(c => new HangfireStatisticsViewModelBuilder(c.Resolve<HangfireRepository>(), ConfigurationManager.ConnectionStrings["Hangfire"].ConnectionString))
				.SingleInstance().AsSelf();
			builder.RegisterType<HangfireRepository>().SingleInstance();
		
			builder.Register(c => new StardustRepository(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)).SingleInstance();

			builder.Register<Func<ICurrentUnitOfWork, IBusinessUnitRepository>>(context => uow => new BusinessUnitRepository(uow));
			builder.Register<Func<ICurrentUnitOfWork, IPersonRepository>>(context => uow => new PersonRepository(uow));
			builder.Register<Func<ICurrentUnitOfWork, IScenarioRepository>>(context => uow => new ScenarioRepository(uow));
			builder.Register<Func<ICurrentUnitOfWork, IApplicationRoleRepository>>(context => uow => new ApplicationRoleRepository(uow));
			builder.Register<Func<ICurrentUnitOfWork, IAvailableDataRepository>>(context => uow => new AvailableDataRepository(uow));
			builder.Register<Func<ICurrentUnitOfWork, IKpiRepository>>(context => uow => new KpiRepository(uow));
			builder.Register<Func<ICurrentUnitOfWork, ISkillTypeRepository>>(context => uow => new SkillTypeRepository(uow));
			builder.Register<Func<ICurrentUnitOfWork, IRtaStateGroupRepository>>(context => uow => new RtaStateGroupRepository(uow));
			// OPTIONAL: Enable property injection into action filters.
			builder.RegisterFilterProvider();
		}
	}
}