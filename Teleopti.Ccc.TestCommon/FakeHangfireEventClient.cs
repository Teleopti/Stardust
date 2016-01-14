using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeHangfireEventClient : IHangfireEventClient
	{
		public IList<string> DisplayNames = new List<string>();
		public IList<string> Tenants = new List<string>();
		public IList<string> EventTypes = new List<string>();
		public IList<string> Events = new List<string>();
		public IList<string> HandlerTypes = new List<string>();

		public string DisplayName { get { return DisplayNames.First(); } }
		public string Tenant { get { return Tenants.First(); } }
		public string EventType { get { return EventTypes.First(); } }
		public string SerializedEvent { get { return Events.First(); } }
		public string HandlerType { get { return HandlerTypes.First(); } }

		public bool WasEnqueued { get { return Events.Any(); } }
		
		public void Enqueue(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			DisplayNames.Add(displayName);
			Tenants.Add(tenant);
			EventTypes.Add(eventType);
			Events.Add(serializedEvent);
			HandlerTypes.Add(handlerType);
		}

		public IList<string> RecurringIds = new List<string>();
		public IList<string> RecurringDisplayNames = new List<string>();
		public IList<string> RecurringTenants = new List<string>();
		public IList<string> RecurringEventTypes = new List<string>();
		public IList<string> RecurringEvents = new List<string>();
		public IList<string> RecurringHandlerTypes = new List<string>();

		public bool WasAdded { get { return RecurringEvents.Any(); } }

		public void AddOrUpdate(string displayName, string id, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			RecurringDisplayNames.Add(displayName);
			RecurringIds.Add(id);
			RecurringTenants.Add(tenant);
			RecurringEventTypes.Add(eventType);
			RecurringEvents.Add(serializedEvent);
			RecurringHandlerTypes.Add(handlerType);
		}
	}
}