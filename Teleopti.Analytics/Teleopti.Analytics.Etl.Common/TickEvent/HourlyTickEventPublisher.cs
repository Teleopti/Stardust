using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Analytics.Etl.Common.TickEvent
{
	public class HourlyTickEventPublisher
	{
		private readonly IRecurringEventPublisher _hangfire;
		private readonly Tenants _tenants;
		private readonly IDataSourceScope _dataSourceScope;

		public HourlyTickEventPublisher(
			IRecurringEventPublisher hangfire, 
			Tenants tenants,
			IDataSourceScope dataSourceScope)
		{
			_hangfire = hangfire;
			_tenants = tenants;
			_dataSourceScope = dataSourceScope;
		}

		public void Tick()
		{
			var tenants = _tenants.CurrentTenants();
			
			var removedTenants =
				_hangfire.TenantsWithRecurringJobs()
					.Except(tenants.Select(x => x.Name));

			removedTenants.ForEach(j =>
			{
				using (_dataSourceScope.OnThisThreadUse(new DummyDataSource(j)))
					_hangfire.StopPublishingForCurrentTenant();
			});

			tenants.ForEach(t =>
			{
				using (_dataSourceScope.OnThisThreadUse(t.DataSource))
					_hangfire.PublishHourly(new HourlyTickEvent());
			});

		}
	}
}