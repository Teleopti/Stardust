using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
    public interface IFilterOutIntervalsAfterMidNight
    {
        IList<ISkillStaffPeriod> Filter(IList<ISkillStaffPeriod> skillStaffPeriods, DateOnly currentDate);
    }

    public class FilterOutIntervalsAfterMidNight : IFilterOutIntervalsAfterMidNight
    {
        public  IList<ISkillStaffPeriod> Filter(IList<ISkillStaffPeriod> skillStaffPeriods, DateOnly currentDate)
        {
            var finalSkillStaffPeriod = new List<ISkillStaffPeriod>();
            var nextDay = currentDate.AddDays(1).Date;
            TimeZoneInfo timezone = TimeZoneGuard.Instance.TimeZone;
            foreach (var skillStaffPeriod in skillStaffPeriods)
            {
                
                if (skillStaffPeriod.Period.StartDateTimeLocal(timezone) < nextDay)
                {
                    finalSkillStaffPeriod.Add(skillStaffPeriod);
                }
            }
            return finalSkillStaffPeriod;
        }
    }
}