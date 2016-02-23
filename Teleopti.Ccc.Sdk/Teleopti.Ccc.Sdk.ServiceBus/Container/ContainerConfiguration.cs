using Autofac;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.Sagas.Persisters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;
using Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers;

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
			var build = new ContainerBuilder();
			build.RegisterGeneric(typeof(InMemorySagaPersister<>)).As(typeof(ISagaPersister<>));

			build.RegisterModule(
				new CommonModule(
					new IocConfiguration(
						new IocArgs(new ConfigReader())
						{
							SharedContainer = sharedContainer,
							DataSourceConfigurationSetter =
								Infrastructure.NHibernateConfiguration.DataSourceConfigurationSetter.ForServiceBus(),
							OptimizeScheduleChangedEvents_DontUseFromWeb = true
						}, _toggleManager)));

			build.RegisterModule<ShiftTradeModule>();
			build.RegisterModule<AuthorizationContainerInstaller>();
			build.RegisterModule<SerializationContainerInstaller>();
			build.RegisterModule<ServiceBusCommonModule>();
			build.RegisterModule<PayrollContainerInstaller>();
			build.RegisterModule<RequestContainerInstaller>();
			build.RegisterModule<SchedulingContainerInstaller>();
			build.RegisterModule<ForecastContainerInstaller>();
			build.RegisterModule<CommandDispatcherModule>();
			build.RegisterModule<LocalServiceBusEventsPublisherModule>();
			build.RegisterModule<CommandHandlersModule>();
			build.RegisterModule(new NotificationModule(_toggleManager));
			build.RegisterModule<IntraIntervalSolverServiceModule>();
			build.RegisterModule<NodeHandlersModule>();
			build.RegisterModule<PersonAccountModule>();
			
			build.RegisterType<AgentBadgeCalculator>().As<IAgentBadgeCalculator>();
			build.RegisterType<AgentBadgeWithRankCalculator>().As<IAgentBadgeWithRankCalculator>();
			build.RegisterType<RunningEtlJobChecker>().As<IRunningEtlJobChecker>();

			build.RegisterType<NotifyTeleoptiRtaServiceToCheckForActivityChange>().As<INotifyRtaToCheckForActivityChange>().SingleInstance();

			build.Register(c =>
			{
				var configReader = c.Resolve<IConfigReader>();
				var connstringAsString = configReader.ConnectionString("Tenancy");
				return TenantUnitOfWorkManager.Create(connstringAsString);
			})
				.As<ITenantUnitOfWork>()
				.As<ICurrentTenantSession>()
				.SingleInstance();
			build.RegisterType<LoadAllTenants>().As<ILoadAllTenants>().SingleInstance();
			build.RegisterType<FindTenantByNameWithEnsuredTransaction>().As<IFindTenantByNameWithEnsuredTransaction>().SingleInstance();
			build.RegisterType<FindTenantByName>().As<IFindTenantByName>().SingleInstance();



			build.Update(_container);
		}

	}
}