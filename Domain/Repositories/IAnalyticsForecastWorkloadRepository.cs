using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsForecastWorkloadRepository
	{
		void AddOrUpdate(AnalyticsForcastWorkload analyticsForcastWorkload);
		void Delete(AnalyticsForcastWorkload workloads);
		IList<AnalyticsForcastWorkload> GetForecastWorkloads(int workloadId, int scenarioId, int startDateId, int endDateId, int startIntervalId, int endIntervalId);
	}
}