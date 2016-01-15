using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;

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
			
			var jobsToRemove =
				_hangfire.RecurringPublishingIds()
					.Except(tenants.Select(x => x.Name));

			jobsToRemove.ForEach(j =>
			{
				_hangfire.StopPublishing(j);
			});

			tenants.ForEach(t =>
			{
				using (_dataSourceScope.OnThisThreadUse(t.DataSource))
					_hangfire.PublishHourly(t.Name, new HourlyTickEvent());
			});

		}
	}
}