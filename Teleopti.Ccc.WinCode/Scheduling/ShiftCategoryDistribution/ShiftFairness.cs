using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public class ShiftFairness
    {
        public IShiftCategory ShiftCategory { get; set; }
        public int MinimumValue { get; set; }
        public int MaximumValue { get; set; }
        public double AverageValue { get; set; }
        public double StandardDeviationValue { get; set; }

        public ShiftFairness(IShiftCategory shiftCategory , int minimumValue, int maximumValue, double averageValue, double standardDeviationValue)
        {
            ShiftCategory = shiftCategory;
            MinimumValue = minimumValue;
            MaximumValue = maximumValue;
            AverageValue = averageValue;
            StandardDeviationValue = standardDeviationValue;
        }
    }
}