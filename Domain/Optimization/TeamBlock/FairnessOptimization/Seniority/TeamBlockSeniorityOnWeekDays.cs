using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface ITeamBlockSeniorityOnWeekDays
        {
            ITeamBlockInfo TeamBlockInfo { get; }
            double Seniority { get; }
            double WeekDayPoints { get; set; }
        }

        public class TeamBlockSeniorityOnWeekDays : ITeamBlockSeniorityOnWeekDays
        {
            public ITeamBlockInfo TeamBlockInfo { get; private set; }
            public double Seniority { get; private set; }
            public double WeekDayPoints { get; set; }

            public TeamBlockSeniorityOnWeekDays(ITeamBlockInfo teamBlockInfo, double seniority, double weekDayPoints)
            {
                TeamBlockInfo = teamBlockInfo;
                Seniority = seniority;
                WeekDayPoints = weekDayPoints;
            }
        }
    
}
