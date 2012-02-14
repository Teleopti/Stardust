using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Carries out statictical calculations on the difference between two input list of values.
    /// </summary>
    public class DeviationStatisticsCalculator : IDeviationStatisticsCalculator
    {
        private readonly PopulationStatisticsCalculator _absoluteDeviationStatisticsCalculator = new PopulationStatisticsCalculator();
        private readonly PopulationStatisticsCalculator _relativeDeviationStatisticsCalculator = new PopulationStatisticsCalculator();

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviationStatisticsCalculator"/> class.
        /// </summary>
        public DeviationStatisticsCalculator()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviationStatisticsCalculator"/> class.
        /// </summary>
        /// <param name="expectedValues">The expected values.</param>
        /// <param name="realValues">The real values.</param>
        public DeviationStatisticsCalculator(IEnumerable<double> expectedValues, IEnumerable<double> realValues)
        {
            if (expectedValues == null)
                throw new ArgumentNullException("expectedValues");
            if (realValues == null)
                throw new ArgumentNullException("realValues");

            IList<double> expectedList = new List<double>(expectedValues);
            IList<double> realList = new List<double>(realValues);

            if (expectedList.Count != realList.Count)
                throw new ArgumentException("The count of the input arrays are not equals.");

            for (int i = 0; i < expectedList.Count; i++)
            {
                AddItem(expectedList[i], realList[i]);
            }
            AnalyzeData();
        }

        /// <summary>
        /// Adds an item.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="realValue">The real value.</param>
        public void AddItem(double expectedValue, double realValue)
        {
            DeviationStatisticData item = new DeviationStatisticData(expectedValue, realValue);
            //_items.Add(item);
            AbsoluteStatisticsCalculator.AddItem(item.AbsoluteDeviation);
            if (!item.RelativeDeviation.Equals(double.NaN))
                RelativeStatisticsCalculator.AddItem(item.RelativeDeviation);
        }

        /// <summary>
        /// Analizes the input data and calculates statistic data.
        /// </summary>
        public void AnalyzeData()
        {
            AbsoluteStatisticsCalculator.Analyze();
            RelativeStatisticsCalculator.Analyze();
        }

        /// <summary>
        /// Gets the absolute deviation average.
        /// </summary>
        /// <value>The absolute deviation average.</value>
        public double AbsoluteDeviationAverage
        {
            get { return AbsoluteStatisticsCalculator.Average; }
        }

        /// <summary>
        /// Gets the absolute deviation summa.
        /// </summary>
        /// <value>The absolute deviation summa.</value>
        public double AbsoluteDeviationSumma
        {
            get { return AbsoluteStatisticsCalculator.Summa; }
        }

        /// <summary>
        /// Gets the absolute standard deviation.
        /// </summary>
        /// <value>The absolute standard deviation.</value>
        public double AbsoluteStandardDeviation
        {
            get { return AbsoluteStatisticsCalculator.StandardDeviation; }
        }

        /// <summary>
        /// Gets the absolute root mean square.
        /// </summary>
        /// <value>The absolute root mean square.</value>
        public double AbsoluteRootMeanSquare
        {
            get { return AbsoluteStatisticsCalculator.RootMeanSquare; }
        }

        /// <summary>
        /// Gets the relative deviation average.
        /// </summary>
        /// <value>The relative deviation average.</value>
        public double RelativeDeviationAverage
        {
            get { return RelativeStatisticsCalculator.Average; }
        }

        /// <summary>
        /// Gets the relative deviation summa.
        /// </summary>
        /// <value>The relative deviation summa.</value>
        public double RelativeDeviationSumma
        {
            get { return RelativeStatisticsCalculator.Summa; }
        }

        /// <summary>
        /// Gets the relative root mean square.
        /// </summary>
        /// <value>The relative root mean square.</value>
        public double RelativeRootMeanSquare
        {
            get { return RelativeStatisticsCalculator.RootMeanSquare; }
        }

        /// <summary>
        /// Gets the standard deviation.
        /// </summary>
        /// <value>The standard deviation.</value>
        public double RelativeStandardDeviation
        {
            get { return RelativeStatisticsCalculator.StandardDeviation; }
        }

        /// <summary>
        /// Gets the inner absolute deviation statistics calculator.
        /// </summary>
        /// <value>The absolute deviation statistics calculator.</value>
        public PopulationStatisticsCalculator AbsoluteStatisticsCalculator
        {
            get { return _absoluteDeviationStatisticsCalculator; }
        }

        /// <summary>
        /// Gets the inner relative deviation statistics calculator.
        /// </summary>
        /// <value>The relative deviation statistics calculator.</value>
        public PopulationStatisticsCalculator RelativeStatisticsCalculator
        {
            get { return _relativeDeviationStatisticsCalculator; }
        }

    }
}