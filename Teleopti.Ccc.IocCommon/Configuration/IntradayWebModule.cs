using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Intraday;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class IntradayWebModule : Module
	{
		private IIocConfiguration _configuration;

		public IntradayWebModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ForecastedStaffingProvider>().SingleInstance();
			builder.RegisterType<FetchSkillInIntraday>().SingleInstance();
			builder.RegisterType<FetchSkillArea>().SingleInstance();
			builder.RegisterType<CreateSkillArea>().SingleInstance();
			builder.RegisterType<DeleteSkillArea>().SingleInstance();
			builder.RegisterType<LoadSkillInIntradays>().As<ILoadAllSkillInIntradays>().SingleInstance();
			builder.RegisterType<IntradayMonitorDataLoader>().As<IIntradayMonitorDataLoader>().SingleInstance();
			builder.RegisterType<LatestStatisticsIntervalIdLoader>().As<ILatestStatisticsIntervalIdLoader>().SingleInstance();
			builder.RegisterType<MonitorSkillsProvider>().SingleInstance();
			builder.RegisterType<LatestStatisticsTimeProvider>().SingleInstance();
			builder.RegisterType<ScheduleForecastSkillReadModelRepository>().As<IScheduleForecastSkillReadModelRepository>().SingleInstance();
			builder.RegisterType<ScheduleForecastSkillProvider>().As<IScheduleForecastSkillProvider>().SingleInstance();
			if (_configuration.Toggle(Toggles.AddActivity_TriggerResourceCalculation_39346))
				builder.RegisterType<AddActivityWithResourceCalculation>().As<IPersonAssignmentAddActivity>().SingleInstance();
			else
				builder.RegisterType<AddActivityWithoutResourceCalculation>().As<IPersonAssignmentAddActivity>().SingleInstance();
		}
	}
}