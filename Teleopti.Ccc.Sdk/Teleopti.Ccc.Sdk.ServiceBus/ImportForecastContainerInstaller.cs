using Autofac;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class ImportForecastContainerInstaller : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ForecastsFileContentProvider>().As<IForecastsFileContentProvider>();
            builder.RegisterType<ForecastsAnalyzeQuery>().As<IForecastsAnalyzeQuery>();
            builder.RegisterType<ForecastsRowExtractor>().As<IForecastsRowExtractor>();
        }
    }
}
