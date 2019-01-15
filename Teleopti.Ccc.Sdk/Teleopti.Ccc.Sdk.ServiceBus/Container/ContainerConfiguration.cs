using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;

namespace Teleopti.Ccc.Sdk.ServiceBus.Container
{
	public class ContainerConfiguration
	{
		private readonly IContainer _container;
		private readonly IToggleManager _toggleManager;

		public ContainerConfiguration(IContainer container, IToggleManager toggleManager)
		{
			_container = container;
			_toggleManager = toggleManager;
		}

		public void Configure()
		{
			Configure(null);
		}

		public void Configure(IContainer sharedContainer)
		{
			const string configKey = "FCM";
			const string configValue = "key=AAAANvMkWNA:APA91bG1pR8ZVsp-S98uWsFUE5lnQiC8UnsQL3DgN6Vyw5HyaKuqVt86kdeurfLfQkWt_7kZTgXcTuAaxvcVUkjtE8jFo72loTy6UYrLrVbYnqCXVI4mWCYhvLQnU3Sv0sIfW1k-eZCu";
			var build = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(new ConfigOverrider(new ConfigReader(),
				new Dictionary<string, string> {{configKey, configValue}}))
			{
				SharedContainer = sharedContainer,
				DataSourceApplicationName = DataSourceApplicationName.ForServiceBus(),
				OptimizeScheduleChangedEvents_DontUseFromWeb = true,
				TeleoptiPrincipalForLegacy = true
			}, _toggleManager);
			build.RegisterModule(new CommonModule(configuration));
			
			build.RegisterModule<AuthorizationModule>();
			build.RegisterModule<ServiceBusCommonModule>();
			build.RegisterModule(new PayrollContainerInstaller(configuration));
			build.RegisterModule<SchedulingModule>();
			build.Register(c => StatisticRepositoryFactory.Create()).As<IStatisticRepository>();
			build.RegisterModule(new NotificationModule(_toggleManager));
			build.RegisterModule<IntraIntervalSolverServiceModule>();

			build.RegisterType<AgentBadgeCalculator>().As<IAgentBadgeCalculator>();
			build.RegisterType<AgentBadgeWithRankCalculator>().As<IAgentBadgeWithRankCalculator>();

			build.Register(c =>
			{
				var configReader = c.Resolve<IConfigReader>();
				var connstringAsString = configReader.ConnectionString("Tenancy");
				return TenantUnitOfWorkManager.Create(connstringAsString);
			}).As<ITenantUnitOfWork>().As<ICurrentTenantSession>().SingleInstance();
			build.RegisterType<LoadAllTenants>().As<ILoadAllTenants>().SingleInstance();
			build.RegisterType<FindTenantByNameWithEnsuredTransaction>().As<IFindTenantByNameWithEnsuredTransaction>().SingleInstance();
			build.RegisterType<FindTenantByName>().As<IFindTenantByName>().SingleInstance();

#pragma warning disable 618
			build.Update(_container);
#pragma warning restore 618
		}
	}
}