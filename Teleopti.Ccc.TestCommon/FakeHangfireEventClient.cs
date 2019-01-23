using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Hangfire;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeHangfireEventClient : IHangfireEventClient
	{
		private readonly IJsonEventSerializer _serializer;

		public FakeHangfireEventClient(IJsonEventSerializer serializer)
		{
			_serializer = serializer;
		}

		public class JobInfo
		{
			public string Id;
			public string DisplayName;
			public string Tenant;
			public string EventType;
			public string Event;
			public string HandlerTypeName;
			public bool Daily;
			public bool Hourly;
			public bool Minutely;
		}

		private readonly System.Collections.Concurrent.ConcurrentBag<JobInfo> _enqueuedJobs = new System.Collections.Concurrent.ConcurrentBag<JobInfo>();
		private readonly List<JobInfo> _recurringJobs = new List<JobInfo>();

		public IEnumerable<string> DisplayNames { get { return _enqueuedJobs.Select(x => x.DisplayName); } }
		public IEnumerable<string> Tenants { get { return _enqueuedJobs.Select(x => x.Tenant); } }
		public IEnumerable<string> EventTypes { get { return _enqueuedJobs.Select(x => x.EventType); } }
		public IEnumerable<string> Events { get { return _enqueuedJobs.Select(x => x.Event); } }
		public IEnumerable<string> HandlerTypeNames { get { return _enqueuedJobs.Select(x => x.HandlerTypeName); } }

		public string DisplayName => DisplayNames.First();
		public string Tenant => Tenants.First();
		public string EventType => EventTypes.First();
		public string SerializedEvent => Events.First();
		public string HandlerTypeName => HandlerTypeNames.First();

		public bool WasEnqueued => _enqueuedJobs.Any();

		public void Enqueue(HangfireEventJob job)
		{
			_enqueuedJobs.Add(new JobInfo
			{
				DisplayName = job.DisplayName,
				Tenant = job.Tenant,
				EventType = EventTypeName(job),
				Event = _serializer.SerializeEvent(job.Event),
				HandlerTypeName = job.HandlerTypeName
			});
		}

		public IEnumerable<JobInfo> Recurring => _recurringJobs;
		public IEnumerable<string> RecurringIds { get { return _recurringJobs.Select(x => x.Id); } }
		public IEnumerable<string> RecurringDisplayNames { get { return _recurringJobs.Select(x => x.DisplayName); } }
		public IEnumerable<string> RecurringTenants { get { return _recurringJobs.Select(x => x.Tenant); } }
		public IEnumerable<string> RecurringEventTypes { get { return _recurringJobs.Select(x => x.EventType); } }
		public IEnumerable<string> RecurringEvents { get { return _recurringJobs.Select(x => x.Event); } }
		public IEnumerable<string> RecurringHandlerTypeNames { get { return _recurringJobs.Select(x => x.HandlerTypeName); } }

		public bool HasRecurringJobs => _recurringJobs.Any();

		public void AddOrUpdateHourly(HangfireEventJob job)
		{
			var j = recurring(job.DisplayName, job.RecurringId(), job.Tenant, EventTypeName(job), _serializer.SerializeEvent(job.Event), job.HandlerTypeName);
			j.Hourly = true;
		}

		public void AddOrUpdateMinutely(HangfireEventJob job)
		{
			var j = recurring(job.DisplayName, job.RecurringId(), job.Tenant, EventTypeName(job), _serializer.SerializeEvent(job.Event), job.HandlerTypeName);
			j.Minutely = true;
		}

		public void AddOrUpdateDaily(HangfireEventJob job)
		{
			var j = recurring(job.DisplayName, job.RecurringId(), job.Tenant, EventTypeName(job), _serializer.SerializeEvent(job.Event), job.HandlerTypeName);
			j.Daily = true;
		}

		public string EventTypeName(HangfireEventJob j)
		{
			if (j.Event == null)
				return null;
			var eventType = j.Event.GetType();
			return $"{eventType.FullName}, {eventType.Assembly.GetName().Name}";
		}

		private JobInfo recurring(string displayName, string id, string tenant, string eventType, string serializedEvent, string handlerTypeName)
		{
			var job = _recurringJobs.SingleOrDefault(x => x.Id == id);
			if (job == null)
			{
				job = new JobInfo();
				_recurringJobs.Add(job);
			}
			job.DisplayName = displayName;
			job.Id = id;
			job.Tenant = tenant;
			job.EventType = eventType;
			job.Event = serializedEvent;
			job.HandlerTypeName = handlerTypeName;
			return job;
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