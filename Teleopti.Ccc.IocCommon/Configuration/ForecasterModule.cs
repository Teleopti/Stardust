using Autofac;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ForecasterModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<QuickForecaster>()
				.SingleInstance()
				.As<IQuickForecaster>();
			builder.RegisterType<HistoricalDataProvider>()
				.SingleInstance()
				.As<IHistoricalDataProvider>();
			builder.RegisterType<LoadStatistics>()
				.SingleInstance()
				.As<ILoadStatistics>();
			builder.RegisterType<StatisticHelperFactory>()
				.SingleInstance()
				.As<IStatisticHelperFactory>();
			builder.RegisterType<LoadSkillDaysInDefaultScenario>()
				.SingleInstance()
				.As<ILoadSkillDaysInDefaultScenario>();
			builder.RegisterType<DailyStatisticsAggregator>()
				.SingleInstance()
				.As<IDailyStatisticsAggregator>();
		}
	}
}