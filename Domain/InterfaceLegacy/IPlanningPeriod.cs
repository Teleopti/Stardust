using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy
{
	public interface IPlanningPeriod : IAggregateRoot
	{
		DateOnlyPeriod Range { get; }
		int Number { get; }
		SchedulePeriodType PeriodType { get; }
		void ChangeRange(SchedulePeriodForRangeCalculation schedulePeriodForRangeCalculation, bool updateTypeAndNumber = false);
		IPlanningPeriod NextPlanningPeriod(IPlanningGroup planningGroup);
		PlanningPeriodState State { get; }
		IPlanningGroup PlanningGroup { get; }
		ISet<IJobResult> JobResults { get; }
		void Scheduled();
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