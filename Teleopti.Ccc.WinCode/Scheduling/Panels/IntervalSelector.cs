using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Panels
{
    public class IntervalSelector
    {
        private Dictionary<TimeSpan, TimeSpan> _mappedTimeSpans = new Dictionary<TimeSpan, TimeSpan>()
                                   {
                                       {TimeSpan.FromHours(3),TimeSpan.FromMinutes(5)},
                                       {TimeSpan.FromHours(36),TimeSpan.FromHours(1)},
                                       {TimeSpan.FromHours(108),TimeSpan.FromHours(3)},
                                       {TimeSpan.FromHours(216),TimeSpan.FromHours(6)},
                                       {TimeSpan.FromDays(36),TimeSpan.FromDays(1)},
                                       {TimeSpan.MaxValue,TimeSpan.FromDays(7)}
                                 };
        
        public TimeSpan SuggestedTimeSpan(DateTimePeriod period)
        {
            return _mappedTimeSpans[_mappedTimeSpans.Keys.Where(t => period.ElapsedTime() <= t).Min()];
        }
    }
}
