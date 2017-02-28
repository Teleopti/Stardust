namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Carries out statictical calculations on the input list of values (as population).
    /// </summary>
    /// <remarks>
    /// Created date: 2009-01-21
    /// </remarks>
    public interface IPopulationStatisticsCalculatedValues
    {
        /// <summary>
        /// Gets the standard deviation. Also known as STDEV.
        /// </summary>
        /// <value>The standard population deviation.</value>
        /// <remarks>
        /// That is the squareroot of the average of the summa of squares of each deviation between the value and the average. [SQRT(SUM(SQR(x-AVG))/N)]
        /// </remarks>
        double StandardDeviation { get; }

        /// <summary>
        /// Gets the population deviation from zero. Also known as Root Mean Square deviation or RMS.
        /// </summary>
        /// <value>The population deviation from zero.</value>
        /// <remarks>
        /// That is the squareroot of the average of the summa of squares of each value. [SQRT(SUM(SQR(x))/N)]
        /// </remarks>
        double RootMeanSquare { get; }
    }
}