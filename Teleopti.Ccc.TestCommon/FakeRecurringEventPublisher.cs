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

		private readonly IList<jobInfo> _publishing = new List<jobInfo>();
		private readonly ICurrentDataSource _dataSource;

		public IEnumerable<string> Tenants { get { return _publishing.Select(x => x.Tenant).ToArray(); } }
		public IEvent Event { get { return _publishing.First().Event; } }
		public bool PublishingAdded { get { return _publishing.Any(); } }

		public FakeRecurringEventPublisher(ICurrentDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		public void PublishHourly(IEvent @event)
		{
			var tenant = _dataSource.Current().DataSourceName;
			var job = _publishing.SingleOrDefault(x => x.Tenant == tenant);
			if (job == null)
			{
				job = new jobInfo();
				_publishing.Add(job);
			}
			job.Tenant = tenant;
			job.Event = @event;
		}

		public void StopPublishingForCurrentTenant()
		{
			var tenant = _dataSource.Current().DataSourceName;
			var job = _publishing.SingleOrDefault(x => x.Tenant == tenant);
			if (job != null)
				_publishing.Remove(job);
		}

		public IEnumerable<string> TenantsWithRecurringJobs()
		{
			return _publishing.Select(x => x.Tenant).ToArray();
		}

		public void Clear()
		{
			_publishing.Clear();
		}
	}
}