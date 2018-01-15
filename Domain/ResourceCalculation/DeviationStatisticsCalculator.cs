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
		private readonly List<double> _absoluteStatisticsValues = new List<double>();
		private readonly List<double> _relativeStatisticsValues = new List<double>();

		public DeviationStatisticsCalculator(IEnumerable<DeviationStatisticData> statisticData)
        {
            if (statisticData == null)
                throw new ArgumentNullException(nameof(statisticData));

			_absoluteStatisticsValues.AddRange(statisticData.Select(r => r.AbsoluteDeviation));
			_relativeStatisticsValues.AddRange(statisticData.Select(r => r.RelativeDeviation).Where(d => !double.IsNaN(d)));
        }
		
        /// <summary>
        /// Gets the absolute deviation average.
        /// </summary>
        /// <value>The absolute deviation average.</value>
        public double AbsoluteDeviationAverage => Calculation.Variances.Average(_absoluteStatisticsValues);

		/// <summary>
        /// Gets the absolute deviation summa.
        /// </summary>
        /// <value>The absolute deviation summa.</value>
        public double AbsoluteDeviationSumma => _absoluteStatisticsValues.Sum();

		/// <summary>
        /// Gets the absolute standard deviation.
        /// </summary>
        /// <value>The absolute standard deviation.</value>
        public double AbsoluteStandardDeviation => Calculation.Variances.StandardDeviation(_absoluteStatisticsValues);

		/// <summary>
        /// Gets the absolute root mean square.
        /// </summary>
        /// <value>The absolute root mean square.</value>
        public double AbsoluteRootMeanSquare => Calculation.Variances.RMS(_absoluteStatisticsValues);

		/// <summary>
        /// Gets the relative deviation average.
        /// </summary>
        /// <value>The relative deviation average.</value>
        public double RelativeDeviationAverage => Calculation.Variances.Average(_relativeStatisticsValues);

		/// <summary>
        /// Gets the relative deviation summa.
        /// </summary>
        /// <value>The relative deviation summa.</value>
        public double RelativeDeviationSumma => _relativeStatisticsValues.Sum();

		/// <summary>
        /// Gets the relative root mean square.
        /// </summary>
        /// <value>The relative root mean square.</value>
        public double RelativeRootMeanSquare => Calculation.Variances.RMS(_relativeStatisticsValues);

		/// <summary>
        /// Gets the standard deviation.
        /// </summary>
        /// <value>The standard deviation.</value>
        public double RelativeStandardDeviation => Calculation.Variances.StandardDeviation(_relativeStatisticsValues);
	}
}