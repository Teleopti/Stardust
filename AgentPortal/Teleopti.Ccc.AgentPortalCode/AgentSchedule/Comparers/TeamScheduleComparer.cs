using System;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule.Comparers
{
    public class TeamScheduleComparer : IComparer<VisualProjection>
    {
        public int Compare(VisualProjection x, VisualProjection y)
        {
            return TimeSpan.Compare(x.ScheduleStartTime, y.ScheduleStartTime);
        }
    }
}
