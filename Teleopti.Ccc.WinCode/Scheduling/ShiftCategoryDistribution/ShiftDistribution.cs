using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public class ShiftDistribution
    {
        public DateOnly DateOnly { get; set; }
        public IShiftCategory ShiftCategory  { get; set; }
        public int Count { get; set; }

        public ShiftDistribution(DateOnly dateOnly, IShiftCategory shiftCategory, int count)
        {
            DateOnly = dateOnly;
            ShiftCategory = shiftCategory;
            Count = count;
        }
    }
}