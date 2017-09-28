namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Represents the statictical data in DeviationStatisticsCalculator
    /// </summary>
    public class DeviationStatisticData
    {
		private const double maxRelativeDeviationForDisplay = 9.99d;
		private const double maxRelativeDeviationForCalculations = 99.9d;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviationStatisticData"/> class.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="realValue">The real value.</param>
        public DeviationStatisticData(double expectedValue, double realValue)
        {
            AbsoluteDeviation = calculateAbsoluteDeviation(expectedValue, realValue);

			var relativeDeviation = calculateRelativeDeviation(expectedValue, realValue);
			RelativeDeviation = forCalculations(relativeDeviation);
			RelativeDeviationForDisplay = forDisplay(relativeDeviation);
        }
		
        public double AbsoluteDeviation { get; }

		public double RelativeDeviation { get; }

		public double RelativeDeviationForDisplay { get; }

		/// <summary>
        /// Calculates the absolut deviation.
        /// </summary>
        private static double calculateAbsoluteDeviation(double expectedValue, double realValue)
        {
            return realValue - expectedValue;
        }

        /// <summary>
        /// Calculates the visual relative deviation. That is the value to be displayed!!!
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="realValue">The real value.</param>
        private static double calculateRelativeDeviation(double expectedValue, double realValue)
        {
			if (expectedValue == realValue)
				return 0;
			if (expectedValue == 0 && realValue > 0)
				return double.NaN;
			if (expectedValue > 0 && realValue == 0)
				return -1;
			var relativeDeviation = (realValue - expectedValue) / expectedValue;
			return relativeDeviation;
		}

		private static double forCalculations(double relativeDeviation)
		{
			return relativeDeviation >= maxRelativeDeviationForCalculations ? maxRelativeDeviationForCalculations : relativeDeviation;
		}

		private static double forDisplay(double relativeDeviation)
		{
			return relativeDeviation >= maxRelativeDeviationForDisplay ? double.NaN : relativeDeviation;
		}

        /// <summary>
		/// Calculates the absolut deviation. That is the value calculations should be made.
        /// </summary>
        public static double CalculateRelativeDeviation(double expectedValue, double realValue)
        {
            var relativeDeviation = calculateRelativeDeviation(expectedValue,realValue);
			return forCalculations(relativeDeviation);
        }
    }
}