using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeRecurringEventPublisher : IRecurringEventPublisher
	{

		public class PublishingInfo
		{
			public string Tenant;
			public IEvent Event;
			public DateTime CreatedAt;
		}

		private readonly IList<PublishingInfo> _publishings = new List<PublishingInfo>();
		private readonly ICurrentDataSource _dataSource;
		private readonly INow _now;

		public bool HasPublishing { get { return _publishings.Any(); } }
		public IEvent Event { get { return _publishings.First().Event; } }
		public IEnumerable<string> Tenants { get { return _publishings.Select(x => x.Tenant).ToArray(); } }
		public IEnumerable<PublishingInfo> Publishings { get { return _publishings; } }

		public FakeRecurringEventPublisher(ICurrentDataSource dataSource, INow now)
		{
			_dataSource = dataSource;
			_now = now;
		}

		public void PublishHourly(IEvent @event)
		{
			var tenant = _dataSource.Current().DataSourceName;
			var job = _publishings.SingleOrDefault(x => x.Tenant == tenant && @event.GetType() == x.Event.GetType());
			if (job != null)
				return;
			_publishings.Add(new PublishingInfo
			{
				Tenant = tenant,
				Event = @event,
				CreatedAt = _now.UtcDateTime()
			});
		}

		public void StopPublishingForCurrentTenant()
		{
			var tenant = _dataSource.Current().DataSourceName;
			var job = _publishings.SingleOrDefault(x => x.Tenant == tenant);
			if (job != null)
				_publishings.Remove(job);
		}

		public void StopPublishingAll()
		{
			_publishings.Clear();
		}

		public IEnumerable<string> TenantsWithRecurringJobs()
		{
			return _publishings
				.Select(x => x.Tenant)
				.Distinct()
				.ToArray();
		}

		public void Clear()
		{
			_publishings.Clear();
		}
	}
}