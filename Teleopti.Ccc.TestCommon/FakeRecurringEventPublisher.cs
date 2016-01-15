using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeRecurringEventPublisher : IRecurringEventPublisher
	{

		private class jobInfo
		{
			public string Tenant;
			public IEvent Event;
		}

		private readonly IList<jobInfo> _jobs = new List<jobInfo>();
		private readonly ICurrentDataSource _dataSource;

		public IEnumerable<string> Tenants { get { return _jobs.Select(x => x.Tenant).ToArray(); } }
		public IEvent Event { get { return _jobs.First().Event; } }
		public bool PublishingHourly { get { return _jobs.Any(); } }

		public FakeRecurringEventPublisher(ICurrentDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		public void PublishHourly(IEvent @event)
		{
			var tenant = _dataSource.Current().DataSourceName;
			var job = _jobs.SingleOrDefault(x => x.Tenant == tenant);
			if (job == null)
			{
				job = new jobInfo();
				_jobs.Add(job);
			}
			job.Tenant = tenant;
			job.Event = @event;
		}

		public void StopPublishingForCurrentTenant()
		{
			var tenant = _dataSource.Current().DataSourceName;
			var job = _jobs.SingleOrDefault(x => x.Tenant == tenant);
			if (job != null)
				_jobs.Remove(job);
		}

		public IEnumerable<string> TenantsWithRecurringJobs()
		{
			return _jobs.Select(x => x.Tenant).ToArray();
		}
	}
}