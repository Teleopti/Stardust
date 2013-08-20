using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public class ShiftDistribution
    {
        public DateOnly DateOnly { get; set; }
        public string ShiftCategoryName { get; set; }
        public int Count { get; set; }

        public ShiftDistribution(DateOnly dateOnly, string shiftCategory, int count)
        {
            DateOnly = dateOnly;
            ShiftCategoryName = shiftCategory;
            Count = count;
        }
    }
}