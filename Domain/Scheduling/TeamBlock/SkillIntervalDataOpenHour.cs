using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ISkillIntervalDataOpenHour
    {
        TimePeriod GetOpenHours(IList<ISkillIntervalData> skillIntervalDataList);
    }

    public class SkillIntervalDataOpenHour : ISkillIntervalDataOpenHour
    {
        
        public TimePeriod GetOpenHours(IList<ISkillIntervalData> skillIntervalDataList)
        {
            var minDate = (from o in skillIntervalDataList
                           select o.Period.StartDateTime).Min();
            var maxDate = (from o in skillIntervalDataList
                           select o.Period.EndDateTime).Max( );
            if(minDate.Day == maxDate.Day  )
                return new TimePeriod(minDate.TimeOfDay, maxDate.TimeOfDay );
            return new TimePeriod(minDate.TimeOfDay, maxDate.TimeOfDay.Add(TimeSpan.FromDays(1)));
        }
    }
}