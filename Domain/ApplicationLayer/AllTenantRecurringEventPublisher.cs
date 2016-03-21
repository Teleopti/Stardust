using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class AllTenantRecurringEventPublisher
	{
		private readonly IRecurringEventPublisher _publisher;
		private readonly IAllTenantNames _tenants;
		private readonly IDataSourceScope _dataSourceScope;

		public AllTenantRecurringEventPublisher(
			IRecurringEventPublisher publisher,
			IAllTenantNames tenants,
			IDataSourceScope dataSourceScope)
		{
			_publisher = publisher;
			_tenants = tenants;
			_dataSourceScope = dataSourceScope;
		}

		public void RemoveAllPublishings()
		{
			_publisher.StopPublishingAll();
		}

		public void RemovePublishingsOfRemovedTenants()
		{
			_publisher
				.TenantsWithRecurringJobs()
				.Except(_tenants.Tenants())
				.ForEach(j =>
				{
					using (_dataSourceScope.OnThisThreadUse(new DummyDataSource(j)))
						_publisher.StopPublishingForCurrentTenant();
				});
		}

		public void PublishHourly(IEvent @event)
		{
			_tenants.Tenants().ForEach(t =>
			{
				using (_dataSourceScope.OnThisThreadUse(new DummyDataSource(t)))
					_publisher.PublishHourly(@event);
			});
		}

		public void PublishMinutely(IEvent @event)
		{
			_tenants.Tenants().ForEach(t =>
			{
				using (_dataSourceScope.OnThisThreadUse(new DummyDataSource(t)))
					_publisher.PublishMinutely(@event);
			});
		}

	}
}