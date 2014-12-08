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
		public void ShouldResolveEventsPublisher()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<CommonModule>();
			containerBuilder.RegisterInstance(MockRepository.GenerateMock<IServiceBusSender>()).As<IServiceBusSender>();
			var container = containerBuilder.Build();

			container.Resolve<IEventsPublisher>().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveSyncEventPublisher()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<CommonModule>();
			containerBuilder.RegisterModule<SyncEventsPublisherModule>();
			var container = containerBuilder.Build();

			container.Resolve<ISyncEventPublisher>().Should().Not.Be.Null();
			container.Resolve<IEventPublisher>().Should().Be.OfType<SyncEventPublisher>();
		}

		[Test]
		public void ShouldResolveServiceBusEventPublisher()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<CommonModule>();
			containerBuilder.RegisterInstance(MockRepository.GenerateMock<IServiceBusSender>()).As<IServiceBusSender>();
			var container = containerBuilder.Build();

			container.Resolve<IServiceBusEventPublisher>().Should().Not.Be.Null();
			container.Resolve<IEventPublisher>().Should().Be.OfType<ServiceBusEventPublisher>();
		}

		[Test]
		public void ShouldResolvePopulatingEventPublisher()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<CommonModule>();
			containerBuilder.RegisterModule<SyncEventsPublisherModule>();
			var container = containerBuilder.Build();

			container.Resolve<IEventPopulatingPublisher>().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveServiceBusSender()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<CommonModule>();
			containerBuilder.RegisterInstance(MockRepository.GenerateMock<IServiceBusSender>()).As<IServiceBusSender>();
			var container = containerBuilder.Build();

			container.Resolve<IMessagePopulatingServiceBusSender>().Should().Not.Be.Null();
		}
	}
}
