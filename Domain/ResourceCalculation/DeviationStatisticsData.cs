using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
        private const double MAX_RELATIVE_DEVIATION_FACTOR = 9.99d;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviationStatisticData"/> class.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="realValue">The real value.</param>
        public DeviationStatisticData(double expectedValue, double realValue)
        {
            _absoluteDeviation = CalculateAbsoluteDeviation(expectedValue, realValue);
            _relativeDeviation = CalculateRelativeDeviation(expectedValue, realValue);
            _relativeDeviationForDisplay = CalculateRelativeDeviationForDisplay(expectedValue, realValue);
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
        private static double CalculateAbsoluteDeviation(double expectedValue, double realValue)
        {
            return (realValue - expectedValue);
        }

        /// <summary>
        /// Calculates the visual relative deviation. That is the value that is to be displayed.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="realValue">The real value.</param>
        private static double CalculateRelativeDeviationForDisplay(double expectedValue, double realValue)
        {
            double result = CalculateRelativeDeviation(expectedValue, realValue);
            //if (expectedValue >= 0 && expectedValue < 1)
            //{
            //    if (result == MAX_RELATIVE_DEVIATION_FACTOR)
            //        return double.NaN;
            //}

            return result >= MAX_RELATIVE_DEVIATION_FACTOR ? double.NaN : result;
        }

        /// <summary>
        /// Calculates the absolut deviation.
        /// </summary>
        private static double CalculateRelativeDeviation(double expectedValue, double realValue)
        {
            if (expectedValue == realValue)
                return 0;
            if (expectedValue == 0 && realValue > 0)
                return double.NaN;
            if (expectedValue > 0 && realValue == 0)
                return -1;
            if (expectedValue >= 0 && expectedValue < 1)
                return Math.Min((realValue - expectedValue) / expectedValue, MAX_RELATIVE_DEVIATION_FACTOR);
            return (realValue - expectedValue) / expectedValue;
        }

        ///// <summary>
        ///// Calculates the absolut deviation.
        ///// </summary>
        //private void CalculateRelativeDeviation(double expectedValue, double realValue)
        //{
        //    //InParameter.ValueMustBePositive("expectedValue", expectedValue);
        //    //InParameter.ValueMustBePositive("realValue", realValue);
        //    if (expectedValue == realValue)
        //        _relativeDeviation = 0;
        //    else if (expectedValue > realValue)
        //        _relativeDeviation = (realValue - expectedValue) / realValue;
        //    else if (expectedValue < realValue)
        //        _relativeDeviation = (realValue - expectedValue) / expectedValue;
        //    _relativeDeviation = Math.Min(_relativeDeviation, 1000d);
        //    _relativeDeviation = Math.Max(_relativeDeviation, -1000d);
        //}
    }
}