using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Represents the statictical data in DeviationStatisticsCalculator
    /// </summary>
    public class DeviationStatisticData : IDeviationStatisticData
    {
        private readonly double _absoluteDeviation;
        private readonly double _relativeDeviation;
        private readonly double _relativeDeviationForDisplay;
        private const double maxRelativeDeviationFactor = 9.99d;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviationStatisticData"/> class.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="realValue">The real value.</param>
        public DeviationStatisticData(double expectedValue, double realValue)
        {
            _absoluteDeviation = calculateAbsoluteDeviation(expectedValue, realValue);
            _relativeDeviation = CalculateRelativeDeviation(expectedValue, realValue);
            _relativeDeviationForDisplay = calculateRelativeDeviationForDisplay(expectedValue, realValue);
        }

		public DeviationStatisticData()
		{
			_absoluteDeviation = calculateAbsoluteDeviation(0, 0);
			_relativeDeviation = CalculateRelativeDeviation(0, 0);
			_relativeDeviationForDisplay = calculateRelativeDeviationForDisplay(0, 0);
		}

        public double AbsoluteDeviation
        {
            get { return _absoluteDeviation; }
        }

        public double RelativeDeviation
        {
            get { return _relativeDeviation; }
        }

        public double RelativeDeviationForDisplayOnly
        {
            get { return _relativeDeviationForDisplay; }
        }

        /// <summary>
        /// Calculates the absolut deviation.
        /// </summary>
        private static double calculateAbsoluteDeviation(double expectedValue, double realValue)
        {
            return (realValue - expectedValue);
        }

        /// <summary>
        /// Calculates the visual relative deviation. That is the value that is to be displayed.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="realValue">The real value.</param>
        private double calculateRelativeDeviationForDisplay(double expectedValue, double realValue)
        {
            double result = CalculateRelativeDeviation(expectedValue, realValue);
            return result >= maxRelativeDeviationFactor ? double.NaN : result;
        }

        /// <summary>
        /// Calculates the absolut deviation.
        /// </summary>
        public double CalculateRelativeDeviation(double expectedValue, double realValue)
        {
            if (expectedValue == realValue)
                return 0;
            if (expectedValue == 0 && realValue > 0)
                return double.NaN;
            if (expectedValue > 0 && realValue == 0)
                return -1;
            if (expectedValue >= 0 && expectedValue < 1)
                return Math.Min((realValue - expectedValue) / expectedValue, maxRelativeDeviationFactor);
            return (realValue - expectedValue) / expectedValue;
        }
    }
}