using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Hangfire;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IHangfireEventClient
	{
		void Enqueue(HangfireEventJob job);
		//void Enqueue(string displayName, string tenant, string queueName, int attempts, string eventType, string serializedEvent, string handlerType);
		void AddOrUpdateHourly(HangfireEventJob job);
		void AddOrUpdateMinutely(HangfireEventJob job);
		void AddOrUpdateDaily(HangfireEventJob job);
		void RemoveIfExists(string id);
		IEnumerable<string> GetRecurringJobIds();
	}
}