using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ITeamBlockMaxSeatChecker
    {
		bool CheckMaxSeat(DateOnly dateOnly, ISchedulingOptions schdulingOption);
    }

    public class TeamBlockMaxSeatChecker : ITeamBlockMaxSeatChecker
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

        public TeamBlockMaxSeatChecker(ISchedulingResultStateHolder schedulingResultStateHolder)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
        }

		public bool CheckMaxSeat(DateOnly dateOnly, ISchedulingOptions schdulingOption)
        {
            var skillDays = _schedulingResultStateHolder.SkillDays;

			if (schdulingOption.UseMaxSeats && schdulingOption.DoNotBreakMaxSeats)
            {
                foreach (var skillPair in skillDays)
                {
                    if (skillPair.Key.SkillType.ForecastSource != ForecastSource.MaxSeatSkill) continue;
                    foreach (var skillDay in skillPair.Value.Where(skillDay => skillDay.CurrentDate == dateOnly))
                    {
                        foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
                        {
                            if (skillStaffPeriod.Payload.CalculatedUsedSeats > skillStaffPeriod.Payload.MaxSeats)
                            {
                                return false ;
                            }

                        }
                    }
                }
            }
            return true;
        }
    }
}