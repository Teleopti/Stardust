using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy
{
	public interface IPlanningPeriod : IAggregateRoot
	{
		DateOnlyPeriod Range { get; }
		int Number { get; }
		SchedulePeriodType PeriodType { get; }
		void ChangeRange(SchedulePeriodForRangeCalculation schedulePeriodForRangeCalculation, bool updateTypeAndNumber = false);
		IPlanningPeriod NextPlanningPeriod(PlanningGroup planningGroup);
		PlanningPeriodState State { get; }
		PlanningGroup PlanningGroup { get; }
		ISet<IJobResult> JobResults { get; }
		void Publish(params IPerson[] people);
		IJobResult GetLastSchedulingJob();
		IJobResult GetLastIntradayOptimizationJob();
		IJobResult GetLastClearScheduleJob();
		void Reset();
	}

	public enum PlanningPeriodState
	{
		New,
		Scheduled,
		Published,
		ScheduleFailed,
		IntradayOptimizationFailed
	}
}