namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
    public class ShiftFairness
    {
        public string ShiftCategoryName { get; set; }
        public int MinimumValue { get; set; }
        public int MaximumValue { get; set; }
        public double AverageValue { get; set; }
        public double StandardDeviationValue { get; set; }

        public ShiftFairness(string name, int minimumValue, int maximumValue, double averageValue, double standardDeviationValue)
        {
            ShiftCategoryName = name;
            MinimumValue = minimumValue;
            MaximumValue = maximumValue;
            AverageValue = averageValue;
            StandardDeviationValue = standardDeviationValue;
        }
    }
}