using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
        public interface IDetermineTeamBlockWeekDayPriority
        {
            ITeamBlockWeekDaySeniorityCalculator CalculatePriority(IList<ITeamBlockInfo> teamBlockInfos);
        }

        public class DetermineTeamBlockWeekDayPriority : IDetermineTeamBlockWeekDayPriority
        {
            private readonly ISeniorityExtractor _seniorityExtractor;
            private IWeekDayPointExtractor _weekDayPointExtractor;

            public DetermineTeamBlockWeekDayPriority(ISeniorityExtractor seniorityExtractor,  IWeekDayPointExtractor weekDayPointExtractor)
            {
                _seniorityExtractor = seniorityExtractor;
                _weekDayPointExtractor = weekDayPointExtractor;
            }

            public ITeamBlockWeekDaySeniorityCalculator CalculatePriority(IList<ITeamBlockInfo> teamBlockInfos)
            {
                var seniorityInfos = _seniorityExtractor.ExtractSeniority(teamBlockInfos);

                var weekDaysInfo = _weekDayPointExtractor.ExtractWeekDayInfos(teamBlockInfos);

                var teamBlockPriorityDefinitionInfoPriorityForWeekDay = new TeamBlockWeekDaySeniorityCalculator();
                foreach (var teamBlockInfo in teamBlockInfos)
                {
                    var seniorityInfo = seniorityInfos[teamBlockInfo];
                    var weekDayInfo = weekDaysInfo[teamBlockInfo];
                    var teamBlockInfoPriority = new TeamBlockSeniorityOnWeekDays(teamBlockInfo, seniorityInfo.Seniority, weekDayInfo.Points);
                    teamBlockPriorityDefinitionInfoPriorityForWeekDay.AddItem(teamBlockInfoPriority, teamBlockInfoPriority.TeamBlockInfo, teamBlockInfoPriority.WeekDayPoints);
                }

                return teamBlockPriorityDefinitionInfoPriorityForWeekDay;
            }
        }
    
}
