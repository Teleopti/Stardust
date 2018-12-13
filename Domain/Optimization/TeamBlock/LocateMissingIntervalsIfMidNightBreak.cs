using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
    public interface ILocateMissingIntervalsIfMidNightBreak
    {
        IList<ISkillStaffPeriod> GetMissingSkillStaffPeriods(DateOnly dateOnly, ISkill skill, TimeZoneInfo timeZone);
    }

    public class LocateMissingIntervalsIfMidNightBreak : ILocateMissingIntervalsIfMidNightBreak
    {
        private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;

        public LocateMissingIntervalsIfMidNightBreak(Func<ISchedulingResultStateHolder> schedulingResultStateHolder)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
        }

        public IList<ISkillStaffPeriod> GetMissingSkillStaffPeriods(DateOnly dateOnly, ISkill skill, TimeZoneInfo timeZone)
        {
            var missingSkillStaffPeriod = new List<ISkillStaffPeriod>();
            if (skill.MidnightBreakOffset  > TimeSpan.Zero)
            {
                var previousDay = dateOnly.AddDays(-1);
                var skillDays = _schedulingResultStateHolder().SkillDaysOnDateOnly(new[] { previousDay });
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