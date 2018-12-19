using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public static class EventHandlerLocations
	{
		private static IEnumerable<Assembly> assembliesFromTypes()
		{
			yield return typeof(IHandleEvent<>).Assembly;
			yield return typeof(Person).Assembly;
			yield return typeof(Rta).Assembly;
		}

		public static IEnumerable<Assembly> Assemblies()
		{
			return assembliesFromTypes()
				.Distinct();
		}

		public static IEnumerable<IEvent> OneOfEachEvent()
		{
			var events = (
				from assembly in Assemblies()
				from type in assembly.GetTypes()
				let isEvent = typeof(IEvent).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract
				where isEvent
				select Activator.CreateInstance(type) as IEvent
			).ToArray();
			return events;
		}
	}
}