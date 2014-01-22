using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{

    public interface IWeekDayInfo
    {
        ITeamBlockInfo TeamBlockInfo { get; }
        int Points { get; }
    }
    public class WeekDayInfo : IWeekDayInfo
    {
        public WeekDayInfo(ITeamBlockInfo teamBlockInfo, int points)
        {
            TeamBlockInfo = teamBlockInfo;
            Points = points;
        }

        public ITeamBlockInfo TeamBlockInfo { get; private set; }
        public int Points { get; private set; }
    }
}
