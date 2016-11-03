using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public interface ITeamBlockMaxSeatChecker
    {
		bool CheckMaxSeat(DateOnly dateOnly, ISchedulingOptions schedulingOption, ITeamInfo teamInfo, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays);
    }

    public class TeamBlockMaxSeatChecker : ITeamBlockMaxSeatChecker
    {
        private readonly IUsedSeats _usedSeats;

	    public TeamBlockMaxSeatChecker(IUsedSeats usedSeats)
	    {
		    _usedSeats = usedSeats;
	    }

		public bool CheckMaxSeat(DateOnly dateOnly, ISchedulingOptions schedulingOption, ITeamInfo teamInfo, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays)
        {
			if (schedulingOption.UserOptionMaxSeatsFeature == MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak)
			{
				//TODO now it takes first maxSeatSkill, what if different sites
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

                foreach (var skillPair in skillDays)
                {
                    if (skillPair.Key.SkillType.ForecastSource != ForecastSource.MaxSeatSkill || skillPair.Key != maxSeatSkill) continue;
                    foreach (var skillDay in skillPair.Value.Where(skillDay => skillDay.CurrentDate == dateOnly))
                    {
                        foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
                        {
                            if (_usedSeats.Fetch(skillStaffPeriod) > skillStaffPeriod.Payload.MaxSeats)
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