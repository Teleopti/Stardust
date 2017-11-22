using System.Collections.Generic;
using System.Linq;
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

		public void Delete(AnalyticsForcastWorkload workload)
		{
			AnalyticsForcastWorkloads.RemoveAll(
				x => x.DateId == workload.DateId
					 && x.IntervalId == workload.IntervalId
					 && x.ScenarioId == workload.ScenarioId
					 && x.WorkloadId == workload.WorkloadId
					 && x.StartTime == workload.StartTime);
		}

		public IList<AnalyticsForcastWorkload> GetForecastWorkloads(int workloadId, int dateId, int scenarioId)
		{
			return AnalyticsForcastWorkloads.Where(x => x.WorkloadId == workloadId && x.DateId == dateId && x.ScenarioId == scenarioId).ToList();
		}


	}
}