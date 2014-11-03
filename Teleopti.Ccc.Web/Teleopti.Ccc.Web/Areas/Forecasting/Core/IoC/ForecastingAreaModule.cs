﻿using Autofac;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core.IoC
{
	public class ForecastingAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<OneYearHistoryForecastPeriodCalculator>()
				.SingleInstance()
				.As<IForecastHistoricalPeriodCalculator>();
		}
	}
}