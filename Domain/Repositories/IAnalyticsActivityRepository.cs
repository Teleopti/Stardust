using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsActivityRepository
	{
		IList<IAnalyticsActivity> Activities();
		void AddActivity(IAnalyticsActivity activity);
		void UpdateActivity(IAnalyticsActivity activity);
	}
}