namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IIsMaxSeatsReachedOnSkillStaffPeriodSpecification
	{
		bool IsSatisfiedBy(double usedSeats, int maxSeats);
	}

	public class IsMaxSeatsReachedOnSkillStaffPeriodSpecification : IIsMaxSeatsReachedOnSkillStaffPeriodSpecification
	{

		public bool IsSatisfiedBy(double usedSeats, int maxSeats)
		{
			if (usedSeats < maxSeats)
				return false;
			return true;
		}
	}
}