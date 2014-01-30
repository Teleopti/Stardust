using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface IWeekDayPointCalculatorForTeamBlock
    {
        double CalculateDaysOffSeniorityValue(ITeamBlockInfo teamBlockInfo, IDictionary<DayOfWeek, int> weekDayPoints);
    }
    public class WeekDayPointCalculatorForTeamBlock:IWeekDayPointCalculatorForTeamBlock 
    {
        
        public double CalculateDaysOffSeniorityValue(ITeamBlockInfo teamBlockInfo, IDictionary<DayOfWeek, int> weekDayPoints)
        {
			double totalPoints = 0;
			foreach (var matrix in teamBlockInfo.MatrixesForGroupAndBlock())
			{
				foreach (var scheduleDayPro in matrix.EffectivePeriodDays)
				{
					if (scheduleDayPro.DaySchedulePart().SignificantPart() != SchedulePartView.DayOff)
						continue;

					var dayOfWeek = scheduleDayPro.Day.DayOfWeek;
					totalPoints += weekDayPoints[dayOfWeek];
				}
			}

			return totalPoints;
        }
    }
}
