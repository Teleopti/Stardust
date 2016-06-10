using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IIndexMaintenanceHangfireEventPublisher
	{
		void PublishDaily(IndexMaintenanceHangfireEvent @event);
	}

	public class IndexMaintenanceHangfireEventPublisher : IIndexMaintenanceHangfireEventPublisher
	{
		private readonly IAllTenantEtlSettings _allTenantEtlSettings;
		private readonly IRecurringEventPublisher _publisher;
		private readonly IDataSourceScope _dataSourceScope;

		public IndexMaintenanceHangfireEventPublisher(IAllTenantEtlSettings allTenantEtlSettings, IRecurringEventPublisher publisher, IDataSourceScope dataSourceScope)
		{
			_allTenantEtlSettings = allTenantEtlSettings;
			_publisher = publisher;
			_dataSourceScope = dataSourceScope;
		}

		public void PublishDaily(IndexMaintenanceHangfireEvent @event)
		{
			_allTenantEtlSettings.All().ForEach(t =>
			{
				if (t.RunIndexMaintenance)
				{
					using (_dataSourceScope.OnThisThreadUse(new DummyDataSource(t.Tenant)))
						_publisher.PublishDaily(@event, t.TimeZone);
				}
			});
		}
	}
}