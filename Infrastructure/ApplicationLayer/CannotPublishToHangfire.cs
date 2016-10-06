using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Hangfire;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class CannotPublishToHangfire : IHangfireEventClient
	{
		//public void Enqueue(string displayName, string tenant, string queueName, int attempts, string eventType, string serializedEvent, string handlerType)
		//{
		//	throw new System.NotImplementedException();
		//}

		public void Enqueue(HangfireEventJob job)
		{
			throw new NotImplementedException();
		}

		public void AddOrUpdateHourly(HangfireEventJob job)
		{
			throw new System.NotImplementedException();
		}

		public void AddOrUpdateMinutely(HangfireEventJob job)
		{
			throw new NotImplementedException();
		}

		public void RemoveIfExists(string id)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> GetRecurringJobIds()
		{
			throw new System.NotImplementedException();
		}

	}
}