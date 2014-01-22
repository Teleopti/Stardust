using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface IWeekDayInfoExtractor
    {
        IDictionary<ITeamBlockInfo, IWeekDayInfo> ExtractWeekDayInfos(IList<ITeamBlockInfo> teamBlockInfos);
    }

    public class WeekDayInfoExtractor : IWeekDayInfoExtractor 
    {
        private readonly IWeekDayPoints _weekDayPoints;

        public WeekDayInfoExtractor(IWeekDayPoints weekDayPoints)
        {
            _weekDayPoints = weekDayPoints;
        }

        public IDictionary<ITeamBlockInfo, IWeekDayInfo> ExtractWeekDayInfos(IList<ITeamBlockInfo> teamBlockInfos)
        {
            var weekDayPoints = _weekDayPoints.GetWeekDaysPoints();
            var result = new Dictionary<ITeamBlockInfo, IWeekDayInfo >();

            foreach (var teamBlockInfo in teamBlockInfos)
            {
                var points = 0;
                var period = teamBlockInfo.BlockInfo.BlockPeriod;

                foreach (var dateOnly in period.DayCollection())
                {
                    int dayPoint = weekDayPoints[dateOnly.DayOfWeek];
                    points += dayPoint;
                }

                var weekDayInfo = new WeekDayInfo(teamBlockInfo, points);
                result.Add(teamBlockInfo, weekDayInfo);
            }

            return result;
        }
    }
}
