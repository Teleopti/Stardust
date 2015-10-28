using Autofac;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core.IoC
{
	public class ForecastingAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ForecastViewModelFactory>().SingleInstance().As<IForecastViewModelFactory>();
			builder.RegisterType<ForecastResultViewModelFactory>().SingleInstance().As<IForecastResultViewModelFactory>();
			builder.RegisterType<IntradayPatternViewModelFactory>().SingleInstance().As<IIntradayPatternViewModelFactory>();
			builder.RegisterType<CampaignPersister>().SingleInstance().As<ICampaignPersister>();
			builder.RegisterType<OverrideTasksPersister>().SingleInstance().As<IOverrideTasksPersister>();
		}
	}
}