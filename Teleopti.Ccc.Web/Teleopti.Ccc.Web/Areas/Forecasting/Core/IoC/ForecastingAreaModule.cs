using Autofac;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Models;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core.IoC
{
	public class ForecastingAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ForecastViewModelFactory>().SingleInstance().As<IForecastViewModelFactory>();
			builder.RegisterType<IntradayPatternViewModelFactory>().SingleInstance().As<IIntradayPatternViewModelFactory>();
			builder.RegisterType<WorkloadNameBuilder>().SingleInstance().As<IWorkloadNameBuilder>();
			builder.RegisterType<ForecastProvider>().SingleInstance();
			builder.RegisterType<ForecastDayModelMapper>().SingleInstance();
		}
	}
}