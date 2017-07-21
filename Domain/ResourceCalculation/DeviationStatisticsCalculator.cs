using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Carries out statictical calculations on the difference between two input list of values.
    /// </summary>
    public class DeviationStatisticsCalculator : IDeviationStatisticsCalculator
    {
		private readonly IList<double> _absoluteStatisticsValues = new List<double>();
		private readonly IList<double> _relativeStatisticsValues = new List<double>();

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
                throw new ArgumentNullException(nameof(expectedValues));
            if (realValues == null)
                throw new ArgumentNullException(nameof(realValues));

            IList<double> expectedList = new List<double>(expectedValues);
            IList<double> realList = new List<double>(realValues);

            if (expectedList.Count != realList.Count)
                throw new ArgumentException("The count of the input arrays are not equals.");

            for (var i = 0; i < expectedList.Count; i++)
            {
                AddItem(expectedList[i], realList[i]);
            }
        }

        /// <summary>
        /// Adds an item.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="realValue">The real value.</param>
        public void AddItem(double expectedValue, double realValue)
        {
            var item = new DeviationStatisticData(expectedValue, realValue);

            _absoluteStatisticsValues.Add(item.AbsoluteDeviation);
            if (!item.RelativeDeviation.Equals(double.NaN))
                _relativeStatisticsValues.Add(item.RelativeDeviation);
        }

        /// <summary>
        /// Gets the absolute deviation average.
        /// </summary>
        /// <value>The absolute deviation average.</value>
        public double AbsoluteDeviationAverage
        {
			get { return Calculation.Variances.Average(_absoluteStatisticsValues); }
        }

        /// <summary>
        /// Gets the absolute deviation summa.
        /// </summary>
        /// <value>The absolute deviation summa.</value>
        public double AbsoluteDeviationSumma
        {
            get { return _absoluteStatisticsValues.Sum(); }
        }

        /// <summary>
        /// Gets the absolute standard deviation.
        /// </summary>
        /// <value>The absolute standard deviation.</value>
        public double AbsoluteStandardDeviation
        {
            get { return Calculation.Variances.StandardDeviation(_absoluteStatisticsValues); }
        }

        /// <summary>
        /// Gets the absolute root mean square.
        /// </summary>
        /// <value>The absolute root mean square.</value>
        public double AbsoluteRootMeanSquare
        {
            get { return Calculation.Variances.RMS(_absoluteStatisticsValues); }
        }

        /// <summary>
        /// Gets the relative deviation average.
        /// </summary>
        /// <value>The relative deviation average.</value>
        public double RelativeDeviationAverage
        {
            get { return Calculation.Variances.Average(_relativeStatisticsValues); }
        }

        /// <summary>
        /// Gets the relative deviation summa.
        /// </summary>
        /// <value>The relative deviation summa.</value>
        public double RelativeDeviationSumma
        {
            get { return _relativeStatisticsValues.Sum(); }
        }

        /// <summary>
        /// Gets the relative root mean square.
        /// </summary>
        /// <value>The relative root mean square.</value>
        public double RelativeRootMeanSquare
        {
            get { return Calculation.Variances.RMS(_relativeStatisticsValues); }
        }

        /// <summary>
        /// Gets the standard deviation.
        /// </summary>
        /// <value>The standard deviation.</value>
        public double RelativeStandardDeviation
        {
            get { return Calculation.Variances.StandardDeviation(_relativeStatisticsValues); }
        }
    }
}