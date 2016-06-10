using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IHangfireEventClient
	{
		void Enqueue(string displayName, string tenant, string queueName, string eventType, string serializedEvent, string handlerType);
		void AddOrUpdateDaily(string displayName, string id, string tenant, string eventType, string serializedEvent, string handlerType, TimeZoneInfo timeZone);
		void AddOrUpdateHourly(string displayName, string id, string tenant, string eventType, string serializedEvent, string handlerType);
		void AddOrUpdateMinutely(string displayName, string id, string tenant, string eventType, string serializedEvent, string handlerType);
		void RemoveIfExists(string id);
		IEnumerable<string> GetRecurringJobIds();
	}
}