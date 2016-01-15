using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class CannotPublishToHangfire : IHangfireEventClient
	{
		public void Enqueue(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			throw new System.NotImplementedException();
		}

		public void AddOrUpdateHourly(string displayName, string id, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			throw new System.NotImplementedException();
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