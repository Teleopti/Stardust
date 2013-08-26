using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public class ShiftCategoryPerAgent
    {
        public IPerson Person { get; set; }
        public IShiftCategory ShiftCategory { get; set; }
        public int Count { get; set; }

        public ShiftCategoryPerAgent(IPerson person, IShiftCategory shiftCategory, int count)
        {
            Person = person;
            ShiftCategory = shiftCategory;
            Count = count;
        }
    }
}