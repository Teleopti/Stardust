using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Toggle;
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
			using (var container = buildContainer())
			{
				container.Resolve<IMessagePopulatingServiceBusSender>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveHangfireOrBusPublisherIfToggleEnabled()
		{
			using (var container = buildContainer(Toggles.RTA_HangfireEventProcessing_31593, true))
			{
				container.Resolve<IEventPublisher>().Should().Be.OfType<HangfireOrBusEventPublisher>();
			}
		}

		private ILifetimeScope buildContainer(Toggles toggle, bool value)
		{
			var toggleManager = MockRepository.GenerateStub<IToggleManager>();
			toggleManager.Stub(x => x.IsEnabled(toggle)).Return(value);
			return buildContainer(toggleManager);
		}

		private ILifetimeScope buildContainer(IToggleManager toggleManager)
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(), toggleManager)));
			builder.RegisterInstance(MockRepository.GenerateMock<IServiceBusSender>()).As<IServiceBusSender>();
			return builder.Build();
		}

		private static ILifetimeScope buildContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule<CommonModule>();
			builder.RegisterInstance(MockRepository.GenerateMock<IServiceBusSender>()).As<IServiceBusSender>();
			return builder.Build();
		}

	}
}
