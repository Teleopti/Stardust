using Autofac;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.Infrastructure.Forecasting.Angel;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ForecasterModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ForecastingTargetMerger>()
				.SingleInstance()
				.As<IForecastingTargetMerger>();
			builder.RegisterType<ForecastingWeightedMeanAbsolutePercentageError>()
				.SingleInstance()
				.As<IForecastingMeasurer>();
			builder.RegisterType<ForecastMethodProvider>()
				.SingleInstance()
				.As<IForecastMethodProvider>();
			builder.RegisterType<LinearRegressionTrend>()
				.SingleInstance()
				.As<ILinearRegressionTrend>();
			builder.RegisterType<QuickForecastSkillEvaluator>()
				.SingleInstance()
				.As<IQuickForecastSkillEvaluator>();
			builder.RegisterType<QuickForecastWorkloadEvaluator>()
				.SingleInstance()
				.As<IQuickForecastWorkloadEvaluator>();
			builder.RegisterType<IndexVolumes>()
				.SingleInstance()
				.As<IIndexVolumes>();
			builder.RegisterType<QuickForecasterWorkload>()
				.SingleInstance()
				.As<IQuickForecasterWorkload>();
			builder.RegisterType<QuickForecaster>()
				.SingleInstance()
				.As<IQuickForecaster>();
			builder.RegisterType<HistoricalData>()
				.SingleInstance()
				.As<IHistoricalData>();
			builder.RegisterType<LoadStatistics>()
				.SingleInstance()
				.As<ILoadStatistics>();
			builder.RegisterType<StatisticHelperFactory>()
				.SingleInstance()
				.As<IStatisticHelperFactory>();
			builder.RegisterType<FetchAndFillSkillDays>()
				.SingleInstance()
				.As<IFetchAndFillSkillDays>();
			builder.RegisterType<DailyStatisticsAggregator>()
				.SingleInstance()
				.As<IDailyStatisticsAggregator>();
			builder.RegisterType<FutureData>()
				.SingleInstance()
				.As<IFutureData>();
			builder.RegisterType<QuickForecastCreator>()
				.SingleInstance()
				.As<IQuickForecastCreator>();
			builder.RegisterType<QuickForecastEvaluator>()
				.SingleInstance()
				.As<IQuickForecastEvaluator>();
			builder.RegisterType<ForecastVolumeApplier>()
				.SingleInstance()
				.As<IForecastVolumeApplier>();
			builder.RegisterType<HistoricalPeriodProvider>()
				.SingleInstance()
				.As<IHistoricalPeriodProvider>();
			builder.RegisterType<PreForecastWorkload>()
				.SingleInstance()
				.As<IPreForecastWorkload>();
		}
	}
}