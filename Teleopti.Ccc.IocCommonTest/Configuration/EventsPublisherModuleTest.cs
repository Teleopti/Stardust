using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class EventsPublisherModuleTest
	{
		[Test]
		public void ShouldResolveServiceBusLocalEventPublisher()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<UnitOfWorkModule>();
			containerBuilder.RegisterModule<AuthenticationModule>();
			containerBuilder.RegisterModule<LocalServiceBusEventsPublisherModule>();
			var container = containerBuilder.Build();
			container.Resolve<IEventsPublisher>().Should().Not.Be.Null();
			container.Resolve<IEventPublisher>().Should().Be.OfType<EventPublisher>();
		}

		[Test]
		public void ShouldResolveLocalInMemoryEventPublisher()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<UnitOfWorkModule>();
			containerBuilder.RegisterModule<AuthenticationModule>();
			containerBuilder.RegisterModule<LocalInMemoryEventsPublisherModule>();
			var container = containerBuilder.Build();
			container.Resolve<IEventsPublisher>().Should().Not.Be.Null();
			container.Resolve<IEventPublisher>().Should().Be.OfType<EventPublisher>();
		}

		[Test]
		public void ShouldResolveServiceBusEventPublisher()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterInstance(MockRepository.GenerateMock<IServiceBusSender>()).As<IServiceBusSender>();
			containerBuilder.RegisterModule<AuthenticationModule>();
			containerBuilder.RegisterModule<UnitOfWorkModule>();
			containerBuilder.RegisterModule<ServiceBusEventsPublisherModule>();
			var container = containerBuilder.Build();
			container.Resolve<IEventsPublisher>().Should().Not.Be.Null();
			container.Resolve<IEventPublisher>().Should().Be.OfType<ServiceBusEventPublisher>();
		}

		[Test]
		public void ShouldResolveServiceBusPublisher()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterInstance(MockRepository.GenerateMock<IServiceBusSender>()).As<IServiceBusSender>();
			containerBuilder.RegisterModule<AuthenticationModule>();
			containerBuilder.RegisterModule<UnitOfWorkModule>();
			containerBuilder.RegisterModule<ServiceBusEventsPublisherModule>();
			var container = containerBuilder.Build();
			container.Resolve<IServiceBusEventPublisher>().Should().Not.Be.Null();
			container.Resolve<IServiceBusEventPublisher>().Should().Be.OfType<ServiceBusEventPublisher>();
		}
	}
}
