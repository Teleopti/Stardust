﻿using Autofac;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;
using Teleopti.Ccc.Domain.Forecasting.Export.Web;
using Teleopti.Ccc.Domain.Forecasting.Models;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ForecasterModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ForecastingTargetMerger>()
				.SingleInstance()
				.As<IForecastingTargetMerger>();
			builder.RegisterType<ForecastMethodProvider>()
				.SingleInstance()
				.As<IForecastMethodProvider>();
			builder.RegisterType<LinearRegressionTrendCalculator>()
				.SingleInstance()
				.As<ILinearTrendCalculator>();
			builder.RegisterType<IntradayForecaster>()
				.SingleInstance();
			builder.RegisterType<OutlierRemover>()
				.SingleInstance();
			builder.RegisterType<ForecastWorkloadEvaluator>()
				.SingleInstance()
				.As<IForecastWorkloadEvaluator>();
			builder.RegisterType<ForecasterWorkload>()
				.SingleInstance();
			builder.RegisterType<ForecastViewModelCreator>()
				.SingleInstance();
			builder.RegisterType<HistoricalData>()
				.SingleInstance()
				.As<IHistoricalData>();
			builder.RegisterType<LoadStatistics>()
				.SingleInstance()
				.As<ILoadStatistics>();
			builder.RegisterType<FetchAndFillSkillDays>()
				.SingleInstance()
				.As<IFetchAndFillSkillDays>();
			builder.RegisterType<DailyStatisticsProvider>()
				.SingleInstance()
				.As<IDailyStatisticsProvider>();
			builder.RegisterType<FutureData>()
				.SingleInstance()
				.As<IFutureData>();
			builder.RegisterType<ForecastVolumeApplier>()
				.SingleInstance()
				.As<IForecastVolumeApplier>();
			builder.RegisterType<HistoricalPeriodProvider>()
				.SingleInstance()
				.As<IHistoricalPeriodProvider>();
			builder.RegisterType<ForecastExportModelCreator>()
				.SingleInstance();
			builder.RegisterType<ForecastDayModelMapper>()
				.SingleInstance();
			builder.RegisterType<QueueStatisticsViewModelFactory>()
				.SingleInstance()
				.As<IQueueStatisticsViewModelFactory>();
		}
	}
}