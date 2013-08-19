using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryInfo
{
    public class ShiftCategoryStructure
    {

        public ShiftCategoryStructure(IScheduleDay scheduleDay)
        {
            ShiftCategoryValue = scheduleDay.PersonAssignment().ShiftCategory;
            DateOnlyValue = scheduleDay.DateOnlyAsPeriod.DateOnly;
            PersonValue = scheduleDay.Person;
        }

        public IPerson PersonValue { get; set; }

        public DateOnly DateOnlyValue { get; set; }

        public IShiftCategory ShiftCategoryValue { get; set; }

    }
}