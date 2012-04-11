using Autofac;
using Rhino.ServiceBus.Internal;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class SerializationContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<DateOnlyPeriodSerializer>().As<ICustomElementSerializer>();
			builder.RegisterType<LargeGuidCollectionSerializer>().As<ICustomElementSerializer>();
            builder.RegisterType<LargeForecastsRowCollectionSerializer>().As<ICustomElementSerializer>();
		}
    }
}