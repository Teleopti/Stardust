using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public class ShiftCategoryStructure
    {
        public IPerson PersonValue { get; set; }
        public DateOnly DateOnlyValue { get; set; }
        public IShiftCategory ShiftCategoryValue { get; set; }

        public ShiftCategoryStructure(IScheduleDay scheduleDay)
        {
            populateFeilds(scheduleDay.PersonAssignment().ShiftCategory, scheduleDay.DateOnlyAsPeriod.DateOnly,
                                   scheduleDay.Person);
        }

        public ShiftCategoryStructure(IShiftCategory shiftCategory,DateOnly dateOnly, IPerson person  )
        {
            populateFeilds(shiftCategory, dateOnly, person);
        }

        private void populateFeilds(IShiftCategory shiftCategory, DateOnly dateOnly, IPerson person)
        {
            ShiftCategoryValue = shiftCategory;
            DateOnlyValue = dateOnly;
            PersonValue = person;
        }
        
    }
}