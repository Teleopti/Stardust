using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	[DomainTest]
	public class EventHandlersModuleTest
	{
		public ResolveEventHandlers Resolver;

		[AllTogglesOff]
		[Test]
		public void ShouldResolveAllEventHandlersWhenTogglesDisabled()
		{
			allEvents().ForEach(x =>
			{
				Resolver.ResolveServiceBusHandlersForEvent(x);
				Resolver.ResolveHangfireHandlersForEvent(x);
			});
		}

		[AllTogglesOn]
		[Test]
		public void ShouldResolveAllEventHandlersWhenTogglesEnabled()
		{
			allEvents().ForEach(x =>
			{
				Resolver.ResolveServiceBusHandlersForEvent(x);
				Resolver.ResolveHangfireHandlersForEvent(x);
			});
		}

		[Test]
		public void ShouldResolveSameInstanceOfHandlers()
		{
			allEvents().ForEach(x =>
			{
				var instance1 = Resolver.ResolveServiceBusHandlersForEvent(x)
					.Concat(Resolver.ResolveHangfireHandlersForEvent(x))
					.FirstOrDefault();
				var instance2 = Resolver.ResolveServiceBusHandlersForEvent(x)
					.Concat(Resolver.ResolveHangfireHandlersForEvent(x))
					.FirstOrDefault();
				instance1.Should().Be.SameInstanceAs(instance2);
			});
		}

		private static IEnumerable<IEvent> allEvents()
		{
			var events = (
				from type in typeof (Event).Assembly.GetTypes()
				let isEvent = typeof (IEvent).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract
				where isEvent
				select Activator.CreateInstance(type) as IEvent
				).ToArray();
			return events;
		}
	}
}