using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class EventsPublisherModuleTest
	{
		[Test]
		public void ShouldResolveLocalInMemoryEventPublisher()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<CommonModule>();
			containerBuilder.RegisterModule<LocalInMemoryEventsPublisherModule>();
			var container = containerBuilder.Build();
			container.Resolve<IEventsPublisher>().Should().Not.Be.Null();
			container.Resolve<IEventPopulatingPublisher>().Should().Be.OfType<EventPopulatingPublisher>();
		}

		[Test]
		public void ShouldResolveServiceBusEventPublisher()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterInstance(MockRepository.GenerateMock<IServiceBusSender>()).As<IServiceBusSender>();
			containerBuilder.RegisterModule<CommonModule>();
			containerBuilder.RegisterModule<ServiceBusEventsPublisherModule>();
			var container = containerBuilder.Build();
			container.Resolve<IEventsPublisher>().Should().Not.Be.Null();
			container.Resolve<IEventPopulatingPublisher>().Should().Be.OfType<ServiceBusEventPopulatingPublisher>();
		}

		[Test]
		public void ShouldResolveServiceBusPublisher()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterInstance(MockRepository.GenerateMock<IServiceBusSender>()).As<IServiceBusSender>();
			containerBuilder.RegisterModule<CommonModule>();
			containerBuilder.RegisterModule<ServiceBusEventsPublisherModule>();
			var container = containerBuilder.Build();
			container.Resolve<IServiceBusEventPopulatingPublisher>().Should().Not.Be.Null();
			container.Resolve<IServiceBusEventPopulatingPublisher>().Should().Be.OfType<ServiceBusEventPopulatingPublisher>();
		}
	}
}
