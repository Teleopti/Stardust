using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public static class ScheduleDayToShiftCategoryMapper
    {
        public static  IList<ShiftCategoryStructure> MapScheduleDay(IList<IScheduleDay> scheduleDays)
        {
            var mappedScheduleDays = new List<ShiftCategoryStructure>();
            foreach (var scheduleDay in scheduleDays)
            {
                var shiftCategoryStructure = new ShiftCategoryStructure(scheduleDay);
                if (shiftCategoryStructure.ShiftCategoryValue != null)
                {
                    mappedScheduleDays.Add(shiftCategoryStructure);
                }
            }
            return mappedScheduleDays;
        }
    }
}