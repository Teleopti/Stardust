using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.ApplicationLayer;
using Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
	[TestFixture]
	public class EventsConsumerResolveTest
	{
		[Test]
		public void ShouldResolveEventsConsumer()
		{
			var bus = MockRepository.GenerateMock<IServiceBus>();
			var builder = new ContainerBuilder();
			builder.RegisterType<EventsConsumer>().As<ConsumerOf<IEvent>>();
			
			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<AuthenticationModule>();
			builder.RegisterModule<UnitOfWorkModule>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule<EventHandlersModule>();
			builder.RegisterModule<ServiceBusEventsPublisherModule>();

			var client = MockRepository.GenerateMock<ISignalRClient>();
			builder.Register(x => client).As<ISignalRClient>();
			builder.Register(x => bus).As<IServiceBus>();
			builder.RegisterType<NoJsonSerializer>().As<IJsonSerializer>();

			using (var container = builder.Build())
			{
				container.Resolve<ConsumerOf<IEvent>>().Should().Not.Be.Null();
			}
		}
	}
}