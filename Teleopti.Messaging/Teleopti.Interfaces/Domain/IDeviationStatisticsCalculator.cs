namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Contains a deviation statistical calculations. 
    /// </summary>
    public interface IDeviationStatisticsCalculator
    {
        /// <summary>
        /// Adds an item.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="realValue">The real value.</param>
        void AddItem(double expectedValue, double realValue);

        /// <summary>
        /// Gets the absolute deviation average.
        /// </summary>
        /// <value>The absolute deviation average.</value>
        /// <remarks>
        /// That is the average of the absolut deviation values of all IDeviationStatisticData.
        /// </remarks>
        double AbsoluteDeviationAverage { get; }

        /// <summary>
        /// Gets the absolute deviation summa.
        /// </summary>
        /// <value>The absolute deviation summa.</value>
        /// <remarks>
        /// That is the summa of the absolut deviation values of all IDeviationStatisticData.
        /// </remarks>
        double AbsoluteDeviationSumma { get; }

        /// <summary>
        /// Gets the absolute standard deviation.
        /// </summary>
        /// <value>The absolute standard deviation.</value>
        double AbsoluteStandardDeviation { get; }

        /// <summary>
        /// Gets the absolute root mean square.
        /// </summary>
        /// <value>The absolute root mean square.</value>
        double AbsoluteRootMeanSquare { get; }

        /// <summary>
        /// Gets the relative deviation average.
        /// </summary>
        /// <value>The relative deviation average.</value>
        double RelativeDeviationAverage { get; }

        /// <summary>
        /// Gets the relative deviation summa.
        /// </summary>
        /// <value>The relative deviation summa.</value>
        /// <remarks>
        /// That is the summa of the realtive deviation values of all IDeviationStatisticData.
        /// </remarks>
        double RelativeDeviationSumma { get; }

        /// <summary>
        /// Gets the relative root mean square.
        /// </summary>
        /// <value>The relative root mean square.</value>
        double RelativeRootMeanSquare { get; }

        /// <summary>
        /// Gets the relative standard deviation.
        /// </summary>
        /// <value>The standard deviation.</value>
        /// <remarks>
        /// That is the squareroot of the summa of the squares of the relative deviation values of all IDeviationStatisticData.
        /// </remarks>
        double RelativeStandardDeviation { get; }
    }
}