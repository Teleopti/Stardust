using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[DomainTest]
	public class EventHandlersModuleScopeTest
	{
		public IResolve Resolve;

		[Test]
		public void ShouldDefaultToSingleton()
		{
			var eventHandler1 = Resolve.Resolve(typeof(IHandleEvent<ScheduleChangedEvent>));
			object eventHandler2;
			using (var scope = Resolve.NewScope())
			{
				eventHandler2 = scope.Resolve(typeof(IHandleEvent<ScheduleChangedEvent>));
			}
			eventHandler1.Should().Be.SameInstanceAs(eventHandler2);
		}

		[Test]
		public void ShouldUseLifetimeScopeForOptimizationWasOrdered()
		{
			var eventHandler1 = Resolve.Resolve(typeof(IHandleEvent<OptimizationWasOrdered>));
			object eventHandler2;
			using (var scope = Resolve.NewScope())
			{
				eventHandler2 = scope.Resolve(typeof(IHandleEvent<OptimizationWasOrdered>));
			}
			eventHandler1.Should().Not.Be.SameInstanceAs(eventHandler2);
		}

		[Test]
		public void ShouldUseLifetimeScopeForOptimizationWasOrderedWhenResolvingEnumerable()
		{
			var eventHandler1 = (IEnumerable<IHandleEvent<OptimizationWasOrdered>>)Resolve.Resolve(typeof(IEnumerable<IHandleEvent<OptimizationWasOrdered>>));
			IEnumerable<IHandleEvent<OptimizationWasOrdered>> eventHandler2;
			using (var scope = Resolve.NewScope())
			{
				eventHandler2 = (IEnumerable<IHandleEvent<OptimizationWasOrdered>>) scope.Resolve(typeof(IEnumerable<IHandleEvent<OptimizationWasOrdered>>));
			}
			eventHandler1.Should().Not.Have.SameValuesAs(eventHandler2);
		}
	}
}