using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsOvertimeRepository
	{
		void AddOrUpdate(AnalyticsOvertime analyticsOvertime);
	}
}