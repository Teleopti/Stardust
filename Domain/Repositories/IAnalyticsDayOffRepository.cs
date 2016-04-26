using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsDayOffRepository
	{
		void AddOrUpdate(AnalyticsDayOff analyticsDayOff);
		IList<AnalyticsDayOff> DayOffs();
	}
}