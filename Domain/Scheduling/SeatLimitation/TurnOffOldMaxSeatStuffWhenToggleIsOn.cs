using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public class TurnOffOldMaxSeatStuffWhenToggleIsOn : IMaxSeatSkillAggregator, ITeamBlockMaxSeatChecker, IIsMaxSeatsReachedOnSkillStaffPeriodSpecification, IMaxSeatBoostingFactorCalculator
	{
		public HashSet<ISkill> GetAggregatedSkills(IList<IPerson> teamMembers, DateOnlyPeriod dateOnlyPeriod)
		{
			return new HashSet<ISkill>();
		}

		public bool CheckMaxSeat(DateOnly dateOnly, ISchedulingOptions schedulingOption, ITeamInfo teamInfo, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays)
		{
			return true;
		}

		public bool IsSatisfiedByWithEqualCondition(double usedSeats, int maxSeats)
		{
			return false;
		}

		public bool IsSatisfiedByWithoutEqualCondition(double usedSeats, int maxSeats)
		{
			return false;
		}

		public double GetBoostingFactor(double currentSeats, double maxSeats)
		{
			return 1;
		}
	}
}