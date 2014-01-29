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
            ITeamBlockWeekDaySeniorityCalculator PerformTeamBlockCalculation(IList<ITeamBlockInfo> teamBlockInfos);
        }

        public class DetermineTeamBlockWeekDayPriority : IDetermineTeamBlockWeekDayPriority
        {
            private readonly ISeniorityExtractor _seniorityExtractor;
            private readonly IWeekDayPointExtractor _weekDayPointExtractor;

            public DetermineTeamBlockWeekDayPriority(ISeniorityExtractor seniorityExtractor,  IWeekDayPointExtractor weekDayPointExtractor)
            {
                _seniorityExtractor = seniorityExtractor;
                _weekDayPointExtractor = weekDayPointExtractor;
            }

            public ITeamBlockWeekDaySeniorityCalculator PerformTeamBlockCalculation(IList<ITeamBlockInfo> teamBlockInfos)
            {
                var seniorityInfos = _seniorityExtractor.ExtractSeniority(teamBlockInfos).ToList();

                var weekDaysInfo = _weekDayPointExtractor.ExtractWeekDayInfos(teamBlockInfos).ToList();

                var teamBlockPriorityDefinitionInfoPriorityForWeekDay = new TeamBlockWeekDaySeniorityCalculator();
                foreach (var teamBlockInfo in teamBlockInfos)
                {
                    var seniorityPoints = getTeamBlockPoints(teamBlockInfo,seniorityInfos);
                    var weekDayPoints = getTeamBlockPoints(teamBlockInfo, weekDaysInfo);
                    var teamBlockInfoPriority = new TeamBlockSeniorityOnWeekDays(teamBlockInfo, seniorityPoints, weekDayPoints);
                    teamBlockPriorityDefinitionInfoPriorityForWeekDay.AddItem(teamBlockInfoPriority, teamBlockInfo, teamBlockInfoPriority.WeekDayPoints);
                }

                return teamBlockPriorityDefinitionInfoPriorityForWeekDay;
            }

           private double getTeamBlockPoints(ITeamBlockInfo teamBlock, IEnumerable<ITeamBlockPoints> teamBlockPointList)
            {
                var extractedTeamBlock = teamBlockPointList.FirstOrDefault(s => s.TeamBlockInfo.Equals(teamBlock));
                if (extractedTeamBlock != null)
                    return extractedTeamBlock.Points;
                return -1;
            }
        }
    
}
