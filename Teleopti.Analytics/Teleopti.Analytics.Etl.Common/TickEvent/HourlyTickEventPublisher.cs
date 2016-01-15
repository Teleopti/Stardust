using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Analytics.Etl.Common.TickEvent
{
	public class HourlyTickEventPublisher
	{
		private readonly IRecurringEventPublisher _hangfire;
		private readonly Tenants _tenants;

		public HourlyTickEventPublisher(IRecurringEventPublisher hangfire, Tenants tenants)
		{
			_hangfire = hangfire;
			_tenants = tenants;
		}

		public void Tick()
		{
			var tenants = _tenants
				.CurrentTenants()
				.Select(x => x.Name);

			var jobsToRemove =
				_hangfire.RecurringPublishingIds()
					.Except(tenants);

			jobsToRemove.ForEach(j =>
			{
				_hangfire.StopPublishing(j);
			});

			tenants.ForEach(t =>
			{
				_hangfire.PublishHourly(t, t, new HourlyTickEvent());
			});

		}
	}
}