using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsHourlyAvailabilityRepository
	{
		void Delete(int personId, int dateId, int scenarioId);
		void AddOrUpdate(AnalyticsHourlyAvailability analyticsHourlyAvailability);
	}
}