using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;

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
			var container = builder.Build();

			container.Resolve<IEventPopulatingPublisher>().Should().Not.Be.Null();
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
			return builder.Build();
		}
	}
}
