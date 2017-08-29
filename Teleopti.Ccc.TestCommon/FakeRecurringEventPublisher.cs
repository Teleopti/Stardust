using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeRecurringEventPublisher : IRecurringEventPublisher
	{

		public class PublishingInfo
		{
			public string Tenant;
			public IEvent Event;
			public DateTime CreatedAt;
			public bool Hourly;
			public bool Minutely;
			public bool Daily;
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

		public void PublishDaily(IEvent @event)
		{
			var dataSource = _dataSource.Current();
			var tenant = dataSource?.DataSourceName;
			var job = _publishings.SingleOrDefault(x => (tenant == null || x.Tenant == tenant) && @event.GetType() == x.Event.GetType());
			if (job != null)
				return;
			_publishings.Add(new PublishingInfo
			{
				Tenant = tenant,
				Event = @event,
				CreatedAt = _now.UtcDateTime(),
				Daily = true
			});
		}

		public void PublishHourly(IEvent @event)
		{
			var dataSource = _dataSource.Current();
			var tenant = dataSource?.DataSourceName;
			var job =
				_publishings.SingleOrDefault(x => (tenant == null || x.Tenant == tenant) && @event.GetType() == x.Event.GetType());
			if (job != null)
				return;
			_publishings.Add(new PublishingInfo
			{
				Tenant = tenant,
				Event = @event,
				CreatedAt = _now.UtcDateTime(),
				Hourly = true
			});
		}

		public void PublishMinutely(IEvent @event)
		{
			var tenant = _dataSource.Current().DataSourceName;
			var job = _publishings.SingleOrDefault(x => x.Tenant == tenant && @event.GetType() == x.Event.GetType());
			if (job != null)
				return;
			_publishings.Add(new PublishingInfo
			{
				Tenant = tenant,
				Event = @event,
				CreatedAt = _now.UtcDateTime(),
				Minutely = true
			});
		}

		public void StopPublishingAll()
		{
			_publishings.Clear();
		}

		public void StopPublishingForTenantsExcept(IEnumerable<string> excludedTenants)
		{
			_publishings.Where(x => !excludedTenants.Contains(x.Tenant))
				.ToArray()
				.ForEach(job => _publishings.Remove(job));
		}

		public void Clear()
		{
			_publishings.Clear();
		}
	}
}