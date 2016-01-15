using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeHangfireEventClient : IHangfireEventClient
	{
		private class jobInfo
		{
			public string Id;
			public string DisplayName;
			public string Tenant;
			public string EventType;
			public string Event;
			public string HandlerType;
		}

		private readonly List<jobInfo> _enqueuedJobs = new List<jobInfo>();
		private readonly List<jobInfo> _recurringJobs = new List<jobInfo>();

		public IEnumerable<string> DisplayNames { get { return _enqueuedJobs.Select(x => x.DisplayName); } }
		public IEnumerable<string> Tenants { get { return _enqueuedJobs.Select(x => x.Tenant); } }
		public IEnumerable<string> EventTypes { get { return _enqueuedJobs.Select(x => x.EventType); } }
		public IEnumerable<string> Events { get { return _enqueuedJobs.Select(x => x.Event); } }
		public IEnumerable<string> HandlerTypes { get { return _enqueuedJobs.Select(x => x.HandlerType); } }

		public string DisplayName { get { return DisplayNames.First(); } }
		public string Tenant { get { return Tenants.First(); } }
		public string EventType { get { return EventTypes.First(); } }
		public string SerializedEvent { get { return Events.First(); } }
		public string HandlerType { get { return HandlerTypes.First(); } }

		public bool WasEnqueued { get { return _enqueuedJobs.Any(); } }

		public void Enqueue(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			_enqueuedJobs.Add(new jobInfo
			{
				DisplayName = displayName,
				Tenant = tenant,
				EventType = eventType,
				Event = serializedEvent,
				HandlerType = handlerType
			});
		}

		public IEnumerable<string> RecurringIds { get { return _recurringJobs.Select(x => x.Id); } }
		public IEnumerable<string> RecurringDisplayNames { get { return _recurringJobs.Select(x => x.DisplayName); } }
		public IEnumerable<string> RecurringTenants { get { return _recurringJobs.Select(x => x.Tenant); } }
		public IEnumerable<string> RecurringEventTypes { get { return _recurringJobs.Select(x => x.EventType); } }
		public IEnumerable<string> RecurringEvents { get { return _recurringJobs.Select(x => x.Event); } }
		public IEnumerable<string> RecurringHandlerTypes { get { return _recurringJobs.Select(x => x.HandlerType); } }

		public bool HasRecurringJobs { get { return _recurringJobs.Any(); } }

		public void AddOrUpdateHourly(string displayName, string id, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			var job = _recurringJobs.SingleOrDefault(x => x.Id == id);
			if (job == null)
			{
				job = new jobInfo();
				_recurringJobs.Add(job);
			}
			job.DisplayName = displayName;
			job.Id = id;
			job.Tenant = tenant;
			job.EventType = eventType;
			job.Event = serializedEvent;
			job.HandlerType = handlerType;
		}

		public IEnumerable<string> GetRecurringJobIds()
		{
			return RecurringIds.Distinct().ToArray();
		}

		public void RemoveIfExists(string id)
		{
			var job = _recurringJobs.SingleOrDefault(x => x.Id == id);
			if (job != null)
				_recurringJobs.Remove(job);
		}
	}
}