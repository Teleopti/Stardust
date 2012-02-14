using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Represents the data in the PopulationStatisticsCalculator
    /// </summary>
    public class PopulationStatisticsData : IPopulationStatisticsData
    {
        private readonly double _value;
        private double _deviationFromAverage;
        private double _deviationSquareFromAverage;

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationStatisticsData"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public PopulationStatisticsData(double value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the input value.
        /// </summary>
        /// <value>The value.</value>
        public double Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the deviation from mean.
        /// </summary>
        /// <value>The deviation from mean.</value>
        public double DeviationFromAverage
        {
            get { return _deviationFromAverage; }
        }

        /// <summary>
        /// Gets the deviation square from mean.
        /// </summary>
        /// <value>The deviation square from mean.</value>
        public double DeviationSquareFromAverage
        {
            get { return _deviationSquareFromAverage; }
        }

        /// <summary>
        /// Analizes and calculates the item data.
        /// </summary>
        /// <param name="average">The mean.</param>
        public void Analyze(double average)
        {
            CalculateDeviationFromAverage(average);
            CalculateDeviationSquareFromAverage();
        }

        /// <summary>
        /// Calculates the deviation from average.
        /// </summary>
        /// <param name="average">The mean.</param>
        protected void CalculateDeviationFromAverage(double average)
        {
            _deviationFromAverage = (_value - average);
        }

        /// <summary>
        /// Calculates the deviation square from mean.
        /// </summary>
        protected void CalculateDeviationSquareFromAverage()
        {
            _deviationSquareFromAverage = Math.Pow(_deviationFromAverage, 2d);
        }
    }
}