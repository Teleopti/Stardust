using Autofac;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core.IoC
{
	public class ForecastingAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ForecastViewModelFactory>()
				.SingleInstance()
				.As<IForecastViewModelFactory>();

			builder.RegisterType<ForecastResultViewModelFactory>()
				.SingleInstance()
				.As<IForecastResultViewModelFactory>();
		}
	}
}