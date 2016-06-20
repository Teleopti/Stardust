using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsForecastWorkloadRepository : IAnalyticsForecastWorkloadRepository
	{
		public List<AnalyticsForcastWorkload> AnalyticsForcastWorkloads = new List<AnalyticsForcastWorkload>();

		public void AddOrUpdate(AnalyticsForcastWorkload analyticsForcastWorkload)
		{
			AnalyticsForcastWorkloads.RemoveAll(
				x =>
					x.WorkloadId == analyticsForcastWorkload.WorkloadId 
					&& x.ScenarioId == analyticsForcastWorkload.ScenarioId 
					&& x.DateId == analyticsForcastWorkload.DateId 
					&& new DateOnly(x.StartTime) == new DateOnly(analyticsForcastWorkload.StartTime) 
					&& x.IntervalId == analyticsForcastWorkload.IntervalId);
			AnalyticsForcastWorkloads.Add(analyticsForcastWorkload);
		}
	}
}