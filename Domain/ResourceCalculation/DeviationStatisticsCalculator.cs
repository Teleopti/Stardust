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
		private readonly double[] _relativeStatisticsValues;

		public DeviationStatisticsCalculator(IEnumerable<DeviationStatisticData> statisticData)
        {
            if (statisticData == null)
                throw new ArgumentNullException(nameof(statisticData));

			_relativeStatisticsValues = statisticData.Select(r => r.RelativeDeviation).Where(d => !double.IsNaN(d)).ToArray();
        }

		/// <summary>
        /// Gets the standard deviation.
        /// </summary>
        /// <value>The standard deviation.</value>
        public double RelativeStandardDeviation => Calculation.Variances.StandardDeviation(_relativeStatisticsValues);
	}
}