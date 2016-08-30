using System;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ITeamBlockMaxSeatChecker
    {
		bool CheckMaxSeat(DateOnly dateOnly, ISchedulingOptions schedulingOption, ITeamInfo teamInfo);
    }

    public class TeamBlockMaxSeatChecker : ITeamBlockMaxSeatChecker
    {
        private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;

        public TeamBlockMaxSeatChecker(Func<ISchedulingResultStateHolder> schedulingResultStateHolder)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
        }

		public bool CheckMaxSeat(DateOnly dateOnly, ISchedulingOptions schedulingOption, ITeamInfo teamInfo)
        {
			if (schedulingOption.UserOptionMaxSeatsFeature == MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak)
			{
				ISkill maxSeatSkill = null;
				foreach (var scheduleMatrixPro in teamInfo.MatrixesForGroupAndDate(dateOnly))
				{
					var personPeriod = scheduleMatrixPro.Person.Period(dateOnly);
					if (personPeriod != null)
					{
						var skillMax = personPeriod.Team.Site.MaxSeatSkill;
						if ( skillMax != null)
						{
							maxSeatSkill = skillMax;
							break;
						}
					}
				}

				var skillDays = _schedulingResultStateHolder().SkillDays;
                foreach (var skillPair in skillDays)
                {
                    if (skillPair.Key.SkillType.ForecastSource != ForecastSource.MaxSeatSkill || skillPair.Key != maxSeatSkill) continue;
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