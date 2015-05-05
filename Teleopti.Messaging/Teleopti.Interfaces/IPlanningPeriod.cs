using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces
{
	public interface IPlanningPeriod : IAggregateRoot
	{
		DateOnlyPeriod Range { get; }
		void ChangeRange(SchedulePeriodForRangeCalculation schedulePeriodForRangeCalculation);
	}
}