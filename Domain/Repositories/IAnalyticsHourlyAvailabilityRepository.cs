using System;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsHourlyAvailabilityRepository
	{
		void Delete(Guid personCode, int dateId, int scenarioId);
		void AddOrUpdate(AnalyticsHourlyAvailability analyticsHourlyAvailability);
		void UpdateUnlinkedPersonids(int[] personPeriodIds);
	}
}