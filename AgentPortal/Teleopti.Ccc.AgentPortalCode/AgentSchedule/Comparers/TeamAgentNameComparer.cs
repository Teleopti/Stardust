using System;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule.Comparers
{
    public class TeamAgentNameComparer : IComparer<VisualProjection>
    {
        public int Compare(VisualProjection x, VisualProjection y)
        {
            int result = 0;

            if (x.AgentName == null && y.AgentName == null)
            {
            }
            else if (x.AgentName == null)
            {
                result = -1;
            }
            else if (y.AgentName == null)
            {
                result = 1;
            }
            else
            {
                result = string.Compare(x.AgentName, y.AgentName, StringComparison.CurrentCulture);
            }

            return result;
        }
    }
}
