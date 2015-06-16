using Autofac;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core.IoC
{
	public class ForecastingAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ForecastEvaluator>()
				.SingleInstance()
				.As<IForecastEvaluator>();

			builder.RegisterType<ForecastResultViewModelFactory>()
				.SingleInstance()
				.As<IForecastResultViewModelFactory>();
		}
	}
}