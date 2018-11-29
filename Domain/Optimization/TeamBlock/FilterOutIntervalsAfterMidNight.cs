using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
    public interface IFilterOutIntervalsAfterMidNight
    {
        IList<ISkillStaffPeriod> Filter(IList<ISkillStaffPeriod> skillStaffPeriods, DateOnly currentDate, TimeZoneInfo timezone);
    }

    public class FilterOutIntervalsAfterMidNight : IFilterOutIntervalsAfterMidNight
    {
        public  IList<ISkillStaffPeriod> Filter(IList<ISkillStaffPeriod> skillStaffPeriods, DateOnly currentDate, TimeZoneInfo timezone)
        {
            var finalSkillStaffPeriod = new List<ISkillStaffPeriod>();
            var nextDay = currentDate.AddDays(1).Date;
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