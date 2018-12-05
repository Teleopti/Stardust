using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Wfm.Adherence.Historical.Infrastructure
{
	public class RtaEventStoreTypeIdMapper
	{
		public string EventTypeId(IEvent @event) => eventTypeId(@event.GetType());
		public string EventTypeId<T>() => typeof(T).GetCustomAttribute<JsonObjectAttribute>().Id;
		public Type TypeForTypeId(string type) => typeForId[type];
		
		private static string eventTypeId(Type type) => type.GetCustomAttribute<JsonObjectAttribute>().Id;

		private static readonly IDictionary<string, Type> typeForId = buildTypeForId();

		private static Dictionary<string, Type> buildTypeForId()
		{
			var example = typeof(PersonStateChangedEvent);
			return example.Assembly
				.GetTypes()
				.Where(x => x.Namespace == example.Namespace)
				.Where(x => x.IsClass)
				.Where(x => typeof(IRtaStoredEvent).IsAssignableFrom(x))
				.ToDictionary(eventTypeId, x => x);
		}
	}
}