namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IIsMaxSeatsReachedOnSkillStaffPeriodSpecification
	{
		bool IsSatisfiedByWithEqualCondition(double usedSeats, int maxSeats);
		bool IsSatisfiedByWithoutEqualCondition(double usedSeats, int maxSeats);
	}

	public class IsMaxSeatsReachedOnSkillStaffPeriodSpecification : IIsMaxSeatsReachedOnSkillStaffPeriodSpecification
	{

		public bool IsSatisfiedByWithEqualCondition(double usedSeats, int maxSeats)
		{
			if (usedSeats < maxSeats)
				return false;
			return true;
		}

		public bool IsSatisfiedByWithoutEqualCondition(double usedSeats, int maxSeats)
		{
			if (usedSeats > maxSeats)
				return true;
			return false;
		}
	}
}