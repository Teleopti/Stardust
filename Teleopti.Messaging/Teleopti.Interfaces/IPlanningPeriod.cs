using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces
{
	public interface IPlanningPeriod : IAggregateRoot
	{
		DateOnlyPeriod Range { get; }
		void ChangeRange(SchedulePeriodForRangeCalculation schedulePeriodForRangeCalculation);
		IPlanningPeriod NextPlanningPeriod();
		PlanningPeriodState State { get; }
		void Scheduled();
		void Publish(params IPerson[] people);
	}

	public enum PlanningPeriodState
	{
		New,
		Scheduled,
		Published
	}
}