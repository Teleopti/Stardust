using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class EventsPublisherModuleTest
	{
		[Test]
		public void ShouldResolveSyncEventPublisherForBehaviorTest()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new ConfigReader()) { BehaviorTestServer = true}, new FalseToggleManager())));
			var container = builder.Build();

			container.Resolve<IEventPublisher>().Should().Be.OfType<MultiEventPublisherServiceBusAsSync>();
		}
		
		[Test]
		public void ShouldResolvePopulatingEventPublisher()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());
			builder.RegisterType<FakeServiceBusSender>().As<IServiceBusSender>().SingleInstance();
			var container = builder.Build();

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
			using (var container = buildContainer())
			{
				container.Resolve<IEventPublisher>().Should().Be.OfType<MultiEventPublisher>();
			}
		}

		private static ILifetimeScope buildContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());
			builder.RegisterInstance(MockRepository.GenerateMock<IServiceBusSender>()).As<IServiceBusSender>();
			return builder.Build();
		}

	}
}
