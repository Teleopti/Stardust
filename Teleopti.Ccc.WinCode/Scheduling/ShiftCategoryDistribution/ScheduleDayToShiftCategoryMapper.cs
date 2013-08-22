using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public static class ScheduleDayToShiftCategoryMapper
    {
        public static  IList<ShiftCategoryStructure> MapScheduleDay(IList<IScheduleDay> scheduleDays)
        {
            return scheduleDays.Select(scheduleDay => new ShiftCategoryStructure(scheduleDay)).ToList();
        }
    }
}