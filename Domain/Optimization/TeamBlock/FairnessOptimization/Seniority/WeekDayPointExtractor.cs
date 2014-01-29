using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface IWeekDayPointExtractor
    {
        IEnumerable<ITeamBlockPoints> ExtractWeekDayInfos(IEnumerable<ITeamBlockInfo> teamBlockInfos);
        double ExtractWeekDayPointForTeamBlock(ITeamBlockInfo roleModelTeamBlock);
    }

    public class WeekDayPointExtractor : IWeekDayPointExtractor 
    {
        private readonly IWeekDayPoints _weekDayPoints;

        public WeekDayPointExtractor(IWeekDayPoints weekDayPoints)
        {
            _weekDayPoints = weekDayPoints;
        }

        public IEnumerable<ITeamBlockPoints> ExtractWeekDayInfos(IEnumerable<ITeamBlockInfo> teamBlockInfos)
        {
            var weekDayPoints = _weekDayPoints.GetWeekDaysPoints();
            var result = new List<ITeamBlockPoints>( );

            foreach (var teamBlockInfo in teamBlockInfos)
            {
                var points = 0;
                var period = teamBlockInfo.BlockInfo.BlockPeriod;

                foreach (var dateOnly in period.DayCollection())
                {
                    var matrixesOnThatDay = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dateOnly);
                    foreach (var matrix in matrixesOnThatDay)
                    {
                        var scheduleDay = matrix.GetScheduleDayByKey(dateOnly);
                        if (scheduleDay != null && scheduleDay.DaySchedulePart()!=null )
                        {
                            var significantPart = scheduleDay.DaySchedulePart().SignificantPart( );
                            if (significantPart == SchedulePartView.DayOff)
                            {
                                int dayPoint = weekDayPoints[dateOnly.DayOfWeek];
                                points += dayPoint;
                            }
                                
                        }
 
                           
                               
                    }
                    
                }

                var weekDayInfo = new TeamBlockPoints(teamBlockInfo, points);
                result.Add(weekDayInfo);
            }

            return result;
        }

        public double ExtractWeekDayPointForTeamBlock(ITeamBlockInfo roleModelTeamBlock)
        {
            throw new NotImplementedException();
        }
    }
}
