using Autofac;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Wfm.Administration.Core.EtlTool;

namespace Teleopti.Wfm.Administration.Core.Modules
{
	public class EtlToolModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<JobCollectionModelProvider>().SingleInstance();
			builder.RegisterType<TenantLogDataSourcesProvider>().SingleInstance();
			builder.RegisterType<AnalyticsConnectionsStringExtractor>().SingleInstance();
			builder.RegisterType<EtlModule.TenantLogonInfoLoader>().As<ITenantLogonInfoLoader>().SingleInstance();
			builder.RegisterType<FindTenantLogonInfoUnsecured>().As<IFindLogonInfo>().SingleInstance();
			builder.RegisterType<PmInfoProvider>().As<IPmInfoProvider>().SingleInstance();
			builder.RegisterType<ConfigurationHandler>().As<IConfigurationHandler>().SingleInstance();
			builder.RegisterType<GeneralFunctions>().As<IGeneralFunctions>().SingleInstance();
			builder.RegisterType<GeneralInfrastructure>().As<IGeneralInfrastructure>().SingleInstance();
			builder.RegisterType<BaseConfigurationRepository>().As<IBaseConfigurationRepository>().SingleInstance();
			builder.RegisterType<EtlJobScheduler>().SingleInstance();
			builder.RegisterType<JobScheduleRepository>().As<IJobScheduleRepository>().SingleInstance();
			builder.RegisterType<Now>().As<INow>().SingleInstance();
			builder.RegisterType<BaseConfigurationValidator>().SingleInstance();
			builder.RegisterType<JobHistoryRepository>().As<IJobHistoryRepository>().SingleInstance();
			builder.RegisterType<Tenants>().As<ITenants>().SingleInstance();
		}
	}
}