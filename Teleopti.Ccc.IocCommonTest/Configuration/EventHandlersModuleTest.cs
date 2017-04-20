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
using Teleopti.Interfaces.Domain;

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
			allEvents().ForEach(x =>
			{
#pragma warning disable 618
				Resolver.HandlerTypesFor<IRunOnServiceBus>(x);
#pragma warning restore 618
				Resolver.HandlerTypesFor<IRunOnHangfire>(x);
				Resolver.HandlerTypesFor<IRunOnStardust>(x);
			});
		}

		[AllTogglesOn]
		[Test]
		public void ShouldResolveAllEventHandlersWhenTogglesEnabled()
		{
			allEvents().ForEach(x =>
			{
#pragma warning disable 618
				Resolver.HandlerTypesFor<IRunOnServiceBus>(x);
#pragma warning restore 618
				Resolver.HandlerTypesFor<IRunOnHangfire>(x);
				Resolver.HandlerTypesFor<IRunOnStardust>(x);
			});
		}

		[Test]
		[Ignore("Reason mandatory for NUnit 3")]
		public void ShouldResolveSameInstanceOfHandlers()
		{
			allEvents().ForEach(x =>
			{
#pragma warning disable 618
				var handlerTypes = Resolver.HandlerTypesFor<IRunOnServiceBus>(x)
#pragma warning restore 618
					.Concat(Resolver.HandlerTypesFor<IRunOnHangfire>(x))
					.Concat(Resolver.HandlerTypesFor<IRunOnStardust>(x))
					.Concat(Resolver.HandlerTypesFor<IRunInSyncInFatClientProcess>(x))
					;
				handlerTypes.ForEach(t =>
				{
					var instance1 = Resolve.Resolve(t);
					var instance2 = Resolve.Resolve(t);
					instance1.Should().Be.SameInstanceAs(instance2);
				});
			});
		}

		[Test]
		[AllTogglesOn]
		public void ShouldHaveNoHandlersOnBusWithoutLogOnContextTogglesEnabled()
		{
			var eventsWithoutLogOnContext = allEvents()
				.Where(e => !(e is ILogOnContext));
			eventsWithoutLogOnContext.ForEach(x =>
			{
#pragma warning disable 618
				Resolver.HandlerTypesFor<IRunOnServiceBus>(x)
#pragma warning restore 618
					.Should().Be.Empty();
			});
		}

		[Test]
		[AllTogglesOff]
		public void ShouldHaveNoHandlersOnBusWithoutLogOnContextTogglesDisabled()
		{
			var eventsWithoutLogOnContext = allEvents()
				.Where(e => !(e is ILogOnContext));
			eventsWithoutLogOnContext.ForEach(x =>
			{
#pragma warning disable 618
				Resolver.HandlerTypesFor<IRunOnServiceBus>(x)
#pragma warning restore 618
					.Should().Be.Empty();
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