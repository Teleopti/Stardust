using System.Configuration;
using Autofac;
using Autofac.Integration.WebApi;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Support.Security.Library;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core.Hangfire;
using Teleopti.Wfm.Azure.Common;

namespace Teleopti.Wfm.Administration.Core.Modules
{
	public class WfmAdminAppModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			var iocArgs = new IocArgs(new ConfigReader())
			{
				TeleoptiPrincipalForLegacy = true
			};
			var toggleManager = CommonModule.ToggleManagerForIoc(iocArgs);
			var iocConf = new IocConfiguration(iocArgs, toggleManager);

			builder.RegisterModule(new CommonModule(iocConf));
			builder.RegisterModule(new WfmAdminModule(iocConf));

		}
	}

	public class WfmAdminModule : Module
	{
		private readonly IocConfiguration _configuration;

		public WfmAdminModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{

			builder.RegisterModule(new StardustModule(_configuration));
			builder.RegisterModule(new EtlToolModule());
			builder.RegisterApiControllers(typeof(HomeController).Assembly).ApplyAspects();
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
			builder.RegisterType<HangfireStatisticsViewModelBuilder>().SingleInstance();
			builder.RegisterType<HangfireRepository>().SingleInstance();
			builder.RegisterType<HangfireUtilities>().AsSelf().As<IManageFailedHangfireEvents>().SingleInstance();
			builder.RegisterType<AdminAccessTokenRepository>().AsSelf();
			builder.RegisterType<RecurrentEventTimer>().SingleInstance();
			builder.RegisterType<InitializeApplicationInsight>().SingleInstance();
			builder.RegisterType<PurgeOldSignInAttempts>().As<IPurgeOldSignInAttempts>().SingleInstance();
			builder.RegisterType<WfmInstallationEnvironment>().As<IInstallationEnvironment>();

			builder.RegisterType<PurgeNoneEmployeeData>().As<IPurgeNoneEmployeeData>().SingleInstance();

			builder.RegisterType<RestorePersonInfoOnDetach>().SingleInstance();

			builder.RegisterType<SkillForecastJobStartTimeRepository>().As<ISkillForecastJobStartTimeRepository>().SingleInstance();

		}
	}

}