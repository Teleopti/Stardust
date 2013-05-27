using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ITeamBlockMaxSeatChecker
    {
        bool CheckMaxSeat(DateOnly dateOnly);
    }

    public class TeamBlockMaxSeatChecker : ITeamBlockMaxSeatChecker
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly ISchedulingOptions _schdulingOption;

        public TeamBlockMaxSeatChecker(ISchedulingResultStateHolder schedulingResultStateHolder, ISchedulingOptions schdulingOption)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _schdulingOption = schdulingOption;
        }

        public bool  CheckMaxSeat(DateOnly dateOnly )
        {
            var skillDays = _schedulingResultStateHolder.SkillDays;

            if (_schdulingOption.UseMaxSeats && _schdulingOption.DoNotBreakMaxSeats )
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