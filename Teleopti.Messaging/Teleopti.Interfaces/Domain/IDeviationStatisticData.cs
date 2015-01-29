namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Contains the absolute and relative deviation data.
    /// </summary>
    public interface IDeviationStatisticData
    {
        /// <summary>
        /// Gets the absolut deviation.
        /// </summary>
        /// <value>The absolut deviation.</value>
        /// <remarks>
        /// That is the absolut difference between the ExpectedValue and the RealValue.
        /// </remarks>
        double AbsoluteDeviation { get; }

        /// <summary>
        /// Gets the relative deviation.
        /// </summary>
        /// <value>The relative deviation.</value>
        /// <remarks>
        /// That is the relative difference between the ExpectedValue and the RealValue.
        /// </remarks>
        double RelativeDeviation { get; }

        /// <summary>
        /// Gets the visual relative deviation to be displayed.
        /// </summary>
        /// <value>The visual relative deviation.</value>
        double RelativeDeviationForDisplay { get; }

		/// <summary>
		/// Calculates the relative deviation, overriding the values used when creating this class.
		/// </summary>
		/// <param name="expectedValue">The expected value.</param>
		/// <param name="realValue">The real value.</param>
		/// <returns></returns>
    	double CalculateRelativeDeviation(double expectedValue, double realValue);

    }
}