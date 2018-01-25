using System;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsActivityRepository
	{
		void AddActivity(AnalyticsActivity activity);
		void UpdateActivity(AnalyticsActivity activity);
		AnalyticsActivity Activity(Guid code);
	}
}