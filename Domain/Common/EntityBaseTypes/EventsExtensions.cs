using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public static class EventsExtensions
	{
		public static IEnumerable<IEvent> KeepLastOfType<T>(this IEnumerable<IEvent> events) where T : IEvent
		{
			return events.Aggregate(new List<IEvent>(), (result, e) =>
			{
				if (e is T && result.OfType<T>().Any())
					result.Remove(result.OfType<T>().Single());
				result.Add(e);
				return result;
			});
		}
	}
}