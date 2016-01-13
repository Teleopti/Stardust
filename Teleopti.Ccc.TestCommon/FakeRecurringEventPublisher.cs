using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeRecurringEventPublisher : IRecurringEventPublisher
	{

		public class JobInfo
		{
			public string Id;
			public string Tenant;
			public IEvent Event;
		}

		private readonly IList<JobInfo> _jobs = new List<JobInfo>();

		public IEnumerable<string> Tenants { get { return _jobs.Select(x => x.Tenant).ToArray(); } }
		public string Tenant { get { return _jobs.First().Tenant; } }
		public IEvent Event { get { return _jobs.First().Event; } }
		public bool PublishingHourly { get { return _jobs.Any(); } }

		public void PublishHourly(string id, string tenant, IEvent @event)
		{
			var job = _jobs.SingleOrDefault(x => x.Id == id);
			if (job == null)
			{
				job = new JobInfo();
				_jobs.Add(job);
			}
			job.Id = id;
			job.Tenant = tenant;
			job.Event = @event;
		}

		public void StopPublishing(string id)
		{
			var job = _jobs.SingleOrDefault(x => x.Id == id);
			if (job != null)
				_jobs.Remove(job);
		}

		public IEnumerable<string> AllPublishings()
		{
			return _jobs.Select(x => x.Id).ToArray();
		}
	}
}