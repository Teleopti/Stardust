using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class EventHandlersModuleTest
	{
		[Test]
		public void ShouldResolveAllEventHandlersWhenTogglesEnabled()
		{
			testResolveAllEventHandlers(new TrueToggleManager());
		}

		[Test]
		public void ShouldResolveAllEventHandlersWhenTogglesDisabled()
		{
			testResolveAllEventHandlers(new FalseToggleManager());
		}

		private static void testResolveAllEventHandlers(IToggleManager toggleManager)
		{
			var handlers = (
				from type in typeof(IHandleEvent<>).Assembly.GetTypes()
				where type.EnabledByToggle(toggleManager)
				let handlerInterfaces =
					from i in type.GetInterfaces()
					let isHandlerInterface = i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleEvent<>)
					where isHandlerInterface
					select i
				where handlerInterfaces.Any()
				select new
				{
					type,
					handlerInterfaces = handlerInterfaces.ToArray()
				}
				).ToArray();

			var builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest(toggleManager));
			var container = builder.Build();

			handlers.Should().Have.Count.GreaterThan(10);
			handlers.ForEach(x =>
			{
				var instance1 = container.Resolve(x.handlerInterfaces.First());
				var instance2 = container.Resolve(x.handlerInterfaces.First());
				instance1.Should().Not.Be.Null();
				instance1.Should().Be.SameInstanceAs(instance2);
			});
		}



		[Test]
		public void ShouldRegisterAnalytics()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());
			var container = builder.Build();
			
			container.Resolve<IIntervalLengthFetcher>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactScheduleTimeHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactScheduleDateHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactSchedulePersonHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsScheduleRepository>().Should().Not.Be.Null();
			
		}
	}
}