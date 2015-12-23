using System;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class EventHandlersModuleTest
	{
		[Test]
		public void ShouldResolveAllEventHandlersWhenTogglesEnabled()
		{
			testResolveHandlersForAllEvents(new TrueToggleManager());
		}

		[Test]
		public void ShouldResolveAllEventHandlersWhenTogglesDisabled()
		{
			testResolveHandlersForAllEvents(new FalseToggleManager());
		}

		private static void testResolveHandlersForAllEvents(IToggleManager toggleManager)
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest(toggleManager));
			var container = builder.Build();
			var resolver = new ResolveEventHandlers(new AutofacResolve(container));

			var events = (
				from type in typeof (Event).Assembly.GetTypes()
				let isEvent = typeof (IEvent).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract
				where isEvent
				select Activator.CreateInstance(type) as IEvent
				).ToArray();


			events.ForEach(x =>
			{
				var instances = resolver.ResolveHandlersForEvent(x);
				instances.Should().Not.Be.Null();
				if (!instances.Cast<object>().Any())
					return;
				var instance2 = resolver.ResolveHandlersForEvent(x).Cast<object>().First();
				instances.Cast<object>().First().Should().Be.SameInstanceAs(instance2);
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
			container.Resolve<IAnalyticsFactScheduleHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactScheduleDayCountHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsScheduleRepository>().Should().Not.Be.Null();
			
		}
	}
}