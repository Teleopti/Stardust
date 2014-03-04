using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff
{
    public interface IValidNumberOfDayOffInAWeekSpecification
    {
        bool IsSatisfied(IScheduleMatrixPro matrix, DateOnlyPeriod period, int noOfDayOff);
    }

    public class ValidNumberOfDayOffInAWeekSpecification : IValidNumberOfDayOffInAWeekSpecification
    {
        public bool IsSatisfied(IScheduleMatrixPro matrix, DateOnlyPeriod period, int noOfDayOff)
        {
            var dayOffCount = 0;
            foreach (var dateTime in period.DayCollection())
            {
                var scheduleDay = matrix.GetScheduleDayByKey(dateTime ).DaySchedulePart();
                if (scheduleDay != null)
                {
                    var schedulePart = scheduleDay.SignificantPart();
                    if(schedulePart == SchedulePartView.ContractDayOff || schedulePart == SchedulePartView.DayOff)
                        dayOffCount++;
                }
            }
            if (dayOffCount >= noOfDayOff)
                return true;
            return false;
        }
    }
}