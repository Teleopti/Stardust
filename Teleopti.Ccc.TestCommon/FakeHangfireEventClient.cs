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
			public string HandlerType;
			public bool Daily;
			public bool Hourly;
			public bool Minutely;
		}

		private readonly List<JobInfo> _enqueuedJobs = new List<JobInfo>();
		private readonly List<JobInfo> _recurringJobs = new List<JobInfo>();

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

		public void Enqueue(Infrastructure.Hangfire.HangfireEventJob job)
		{
			_enqueuedJobs.Add(new JobInfo
			{
				DisplayName = job.DisplayName,
				Tenant = job.Tenant,
				EventType = job.EventTypeName(),
				Event = _serializer.SerializeEvent(job.Events.First()),
				HandlerType = job.HandlerTypeName
			});
		}

		public void Enqueue(string displayName, string tenant, string queueName, int attempts, string eventType, string serializedEvent, string handlerType)
		{
			_enqueuedJobs.Add(new JobInfo
			{
				DisplayName = displayName,
				Tenant = tenant,
				EventType = eventType,
				Event = serializedEvent,
				HandlerType = handlerType
			});
		}

		public IEnumerable<JobInfo> Recurring { get { return _recurringJobs; } }
		public IEnumerable<string> RecurringIds { get { return _recurringJobs.Select(x => x.Id); } }
		public IEnumerable<string> RecurringDisplayNames { get { return _recurringJobs.Select(x => x.DisplayName); } }
		public IEnumerable<string> RecurringTenants { get { return _recurringJobs.Select(x => x.Tenant); } }
		public IEnumerable<string> RecurringEventTypes { get { return _recurringJobs.Select(x => x.EventType); } }
		public IEnumerable<string> RecurringEvents { get { return _recurringJobs.Select(x => x.Event); } }
		public IEnumerable<string> RecurringHandlerTypes { get { return _recurringJobs.Select(x => x.HandlerType); } }

		public bool HasRecurringJobs { get { return _recurringJobs.Any(); } }

		public void AddOrUpdateHourly(HangfireEventJob job2)
		{
			var job = recurring(job2.DisplayName, job2.RecurringId(), job2.Tenant, job2.EventTypeName(), _serializer.SerializeEvent(job2.Events.First()), job2.HandlerTypeName);
			job.Hourly = true;
		}

		public void AddOrUpdateMinutely(HangfireEventJob job2)
		{
			var job = recurring(job2.DisplayName, job2.RecurringId(), job2.Tenant, job2.EventTypeName(), _serializer.SerializeEvent(job2.Events.First()), job2.HandlerTypeName);
			job.Minutely = true;
		}

		public void AddOrUpdateDaily(HangfireEventJob job)
		{
			var j = recurring(job.DisplayName, job.RecurringId(), job.Tenant, job.EventTypeName(), _serializer.SerializeEvent(job.Events.First()), job.HandlerTypeName);
			j.Daily = true;
		}

		private JobInfo recurring(string displayName, string id, string tenant, string eventType, string serializedEvent,
			string handlerType)
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
			job.HandlerType = handlerType;
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