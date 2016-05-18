using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsOvertimeRepository
	{
		void AddOrUpdate(AnalyticsOvertime analyticsOvertime);
		IList<AnalyticsOvertime> Overtimes();
	}
}