using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IBlockPeriodFinderBetweenDayOff
    {
        DateOnlyPeriod? GetBlockPeriod(IScheduleMatrixPro scheduleMatrixPro, DateOnly dateOnly);
    }

    public class BlockPeriodFinderBetweenDayOff : IBlockPeriodFinderBetweenDayOff
    {
        public DateOnlyPeriod? GetBlockPeriod(IScheduleMatrixPro scheduleMatrixPro, DateOnly providedDateOnly)
        {
            //move to left side to get the starting date
            DateOnlyPeriod? blockPeriod = null;
            DateOnly startDate = providedDateOnly;
            var scheduleDayProTemp = scheduleMatrixPro.GetScheduleDayByKey(startDate);
            while (scheduleDayProTemp != null && !isDayOff(scheduleDayProTemp.DaySchedulePart()))
            {
                startDate = startDate.AddDays(-1);
                scheduleDayProTemp = scheduleMatrixPro.GetScheduleDayByKey(startDate);
            }
            startDate = startDate.AddDays(1);

            foreach (var dateOnly in scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.DayCollection())
            {
                if (startDate >= dateOnly) continue;

                scheduleDayProTemp = scheduleMatrixPro.GetScheduleDayByKey(dateOnly);
                if (scheduleDayProTemp == null) continue;

                if (isDayOff(scheduleDayProTemp.DaySchedulePart()))
                {
                    blockPeriod = new DateOnlyPeriod(startDate, dateOnly.AddDays(-1));
                    break;
                }
                if (dateOnly == scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.EndDate)
                {
                    var outerDateOnly = dateOnly.AddDays(1);
                    var outerScheduleDayPro = scheduleMatrixPro.GetScheduleDayByKey(outerDateOnly);

                    while (outerScheduleDayPro != null && !isDayOff(outerScheduleDayPro.DaySchedulePart()))
                    {
                        outerDateOnly = outerDateOnly.AddDays(1);
                        outerScheduleDayPro = scheduleMatrixPro.GetScheduleDayByKey(outerDateOnly);
                    }
                    outerDateOnly =  outerDateOnly.AddDays(-1);
                    blockPeriod = new DateOnlyPeriod(startDate, outerDateOnly);
                    break;
                }


            }
            return blockPeriod;
        }

        //Absence can not be a block breaker when using teams
        private static bool isDayOff(IScheduleDay scheduleDay)
        {
            var significantPart = scheduleDay.SignificantPart();
            if (significantPart == SchedulePartView.DayOff ||
                significantPart == SchedulePartView.ContractDayOff)
            {
                return true;
            }
            return false;
        }
    }
}