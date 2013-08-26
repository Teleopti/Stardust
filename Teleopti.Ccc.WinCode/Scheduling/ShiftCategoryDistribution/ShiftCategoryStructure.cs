using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public class ShiftCategoryStructure
    {
        public IPerson PersonValue { get; set; }
        public DateOnly DateOnlyValue { get; set; }
        public IShiftCategory ShiftCategory { get; set; }

        public ShiftCategoryStructure(IScheduleDay scheduleDay)
        {

            var personAssignment = scheduleDay.PersonAssignment();
            if (personAssignment != null && personAssignment.ShiftCategory != null)
            {
                populateFeilds(personAssignment.ShiftCategory, scheduleDay.DateOnlyAsPeriod.DateOnly,
                                   scheduleDay.Person);
            }
            
        }

        public ShiftCategoryStructure(IShiftCategory shiftCategory,DateOnly dateOnly, IPerson person  )
        {
            populateFeilds(shiftCategory, dateOnly, person);
        }

        private void populateFeilds(IShiftCategory shiftCategory, DateOnly dateOnly, IPerson person)
        {
            ShiftCategory = shiftCategory;
            DateOnlyValue = dateOnly;
            PersonValue = person;
        }
        
    }
}