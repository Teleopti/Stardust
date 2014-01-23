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
            ITeamBlockPriorityDefinitionInfoPriorityForWeekDay CalculatePriority(IList<ITeamBlockInfo> teamBlockInfos);
        }

        public class DetermineTeamBlockWeekDayPriority : IDetermineTeamBlockWeekDayPriority
        {
            private readonly ISeniorityExtractor _seniorityExtractor;
            private IWeekDayInfoExtractor _weekDayInfoExtractor;

            public DetermineTeamBlockWeekDayPriority(ISeniorityExtractor seniorityExtractor,  IWeekDayInfoExtractor weekDayInfoExtractor)
            {
                _seniorityExtractor = seniorityExtractor;
                _weekDayInfoExtractor = weekDayInfoExtractor;
            }

            public ITeamBlockPriorityDefinitionInfoPriorityForWeekDay CalculatePriority(IList<ITeamBlockInfo> teamBlockInfos)
            {
                var seniorityInfos = _seniorityExtractor.ExtractSeniority(teamBlockInfos);

                var weekDaysInfo = _weekDayInfoExtractor.ExtractWeekDayInfos(teamBlockInfos);

                var teamBlockPriorityDefinitionInfoPriorityForWeekDay = new TeamBlockPriorityDefinitionInfoPriorityForWeekDay();
                foreach (var teamBlockInfo in teamBlockInfos)
                {
                    var seniorityInfo = seniorityInfos[teamBlockInfo];
                    var weekDayInfo = weekDaysInfo[teamBlockInfo];
                    var teamBlockInfoPriority = new TeamBlockInfoPriorityOnWeekDays(teamBlockInfo, seniorityInfo.Seniority, weekDayInfo.Points);
                    teamBlockPriorityDefinitionInfoPriorityForWeekDay.AddItem(teamBlockInfoPriority, teamBlockInfoPriority.TeamBlockInfo, teamBlockInfoPriority.WeekDayPoints);
                }

                return teamBlockPriorityDefinitionInfoPriorityForWeekDay;
            }
        }
    
}
