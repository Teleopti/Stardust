using Autofac;
using NUnit.Framework;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
    [TestFixture]
    public class UpdateGroupingReadModelConsumerResolveTest
    {
        [Test]
        public void ShouldResolveUpdateGroupingReadModelConsumer()
        {
			var builder = new ContainerBuilder();
            builder.RegisterType<UpdateGroupingReadModelConsumer>().As<ConsumerOf<PersonChangedMessage>>();

			builder.RegisterModule(CommonModule.ForTest());
			builder.RegisterModule<ServiceBusCommonModule>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<RequestContainerInstaller>();
			builder.RegisterModule<SchedulingContainerInstaller>();

			using (var container = builder.Build())
			{
                container.Resolve<ConsumerOf<PersonChangedMessage>>().Should().Not.Be.Null();
			}
        }
    }
}
