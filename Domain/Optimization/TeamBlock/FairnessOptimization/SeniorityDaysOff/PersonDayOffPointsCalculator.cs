using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface IPersonDayOffPointsCalculator
    {
		double CalculateDaysOffSeniorityValue(IScheduleRange scheduleRange, DateOnlyPeriod visiblePeriod, ISeniorityWorkDayRanks seniorityWorkDayRanks);
    }

    public class PersonDayOffPointsCalculator : IPersonDayOffPointsCalculator
    {
        private readonly IWeekDayPoints _weekDayPoints;

        public PersonDayOffPointsCalculator(IWeekDayPoints weekDayPoints)
        {
            _weekDayPoints = weekDayPoints;
        }

        public double CalculateDaysOffSeniorityValue(IScheduleRange scheduleRange , DateOnlyPeriod visiblePeriod, ISeniorityWorkDayRanks seniorityWorkDayRanks)
        {
			var weekDayPoints = _weekDayPoints.GetWeekDaysPoints(seniorityWorkDayRanks);
            double totalPoints = 0;
            foreach (var dateOnly in visiblePeriod.DayCollection() )
            {
                var scheduleDay = scheduleRange.ScheduledDay(dateOnly);
                
                if (scheduleDay.SignificantPart() != SchedulePartView.DayOff)
                    continue;

                totalPoints += weekDayPoints[dateOnly.DayOfWeek];
            }

            return totalPoints;
        }
    }
}
