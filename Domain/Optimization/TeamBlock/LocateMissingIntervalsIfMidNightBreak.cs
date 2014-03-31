using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
    public interface ILocateMissingIntervalsIfMidNightBreak
    {
        IList<ISkillStaffPeriod> GetMissingSkillStaffPeriods(DateOnly dateOnly, ISkill skill, TimeZoneInfo timeZone);
    }

    public class LocateMissingIntervalsIfMidNightBreak : ILocateMissingIntervalsIfMidNightBreak
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

        public LocateMissingIntervalsIfMidNightBreak(ISchedulingResultStateHolder schedulingResultStateHolder)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
        }

        public IList<ISkillStaffPeriod> GetMissingSkillStaffPeriods(DateOnly dateOnly, ISkill skill, TimeZoneInfo timeZone)
        {
            var missingSkillStaffPeriod = new List<ISkillStaffPeriod>();
            if (skill.MidnightBreakOffset  > TimeSpan.Zero)
            {
                var previousDay = dateOnly.AddDays(-1);
                //TimeZoneInfo timeZone = TimeZoneGuard.Instance.TimeZone;
                var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(new DateOnly[] { previousDay });
                foreach (var skillDay in skillDays)
                {
                    if (skillDay.Skill != skill) continue;
                    foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
                    {
                        var period = skillStaffPeriod.Period;
                        if (period.StartDateTimeLocal(timeZone) >= dateOnly.Date)
                        {
                            missingSkillStaffPeriod.Add(skillStaffPeriod);
                        }
                    }
                }
                return missingSkillStaffPeriod;
            }
            return missingSkillStaffPeriod;
        }

    }
}