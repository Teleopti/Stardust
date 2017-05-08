using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	[DomainTest]
	public class EventHandlersModuleTest
	{
		public ResolveEventHandlers Resolver;
		public IResolve Resolve;
		
		[AllTogglesOff]
		[Test]
		public void ShouldResolveAllEventHandlersWhenTogglesDisabled()
		{
			Resolver.ResolveAllJobs(oneOfEachEvent());
		}

		[AllTogglesOn]
		[Test]
		public void ShouldResolveAllEventHandlersWhenTogglesEnabled()
		{
			Resolver.ResolveAllJobs(oneOfEachEvent());
		}

		[Test]
		[Ignore("Reason mandatory for NUnit 3")]
		public void ShouldResolveSameInstanceOfHandlers()
		{
			Resolver.ResolveAllJobs(oneOfEachEvent())
				.ForEach(x =>
				{
					var instance1 = Resolve.Resolve(x.HandlerType);
					var instance2 = Resolve.Resolve(x.HandlerType);
					instance1.Should().Be.SameInstanceAs(instance2);
				});
		}

#pragma warning disable 618
		[Test]
		[AllTogglesOn]
		public void ShouldHaveNoHandlersOnBusWithoutLogOnContextTogglesEnabled()
		{
			var eventsWithoutLogOnContext = oneOfEachEvent()
				.Where(e => !(e is ILogOnContext));
			Resolver.JobsFor<IRunOnServiceBus>(eventsWithoutLogOnContext)
				.Should().Be.Empty();
		}

		[Test]
		[AllTogglesOff]
		public void ShouldHaveNoHandlersOnBusWithoutLogOnContextTogglesDisabled()
		{
			var eventsWithoutLogOnContext = oneOfEachEvent()
				.Where(e => !(e is ILogOnContext));
			Resolver.JobsFor<IRunOnServiceBus>(eventsWithoutLogOnContext)
				.Should().Be.Empty();
		}
#pragma warning restore 618

		private static IEnumerable<IEvent> oneOfEachEvent()
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