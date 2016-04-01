﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

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
				Resolver.HandlerTypesFor<IRunOnServiceBus>(x);
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
				Resolver.HandlerTypesFor<IRunOnServiceBus>(x);
				Resolver.HandlerTypesFor<IRunOnHangfire>(x);
				Resolver.HandlerTypesFor<IRunOnStardust>(x);
			});
		}

		[Test]
		public void ShouldResolveSameInstanceOfHandlers()
		{
			allEvents().ForEach(x =>
			{
				var handlerTypes = Resolver.HandlerTypesFor<IRunOnServiceBus>(x)
					.Concat(Resolver.HandlerTypesFor<IRunOnHangfire>(x))
					.Concat(Resolver.HandlerTypesFor<IRunOnStardust>(x))
					.Concat(Resolver.HandlerTypesFor<IRunInProcess>(x))
					;
				handlerTypes.ForEach(t =>
				{
					var instance1 = Resolver.HandlerFor(t);
					var instance2 = Resolver.HandlerFor(t);
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
				Resolver.HandlerTypesFor<IRunOnServiceBus>(x)
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
				Resolver.HandlerTypesFor<IRunOnServiceBus>(x)
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