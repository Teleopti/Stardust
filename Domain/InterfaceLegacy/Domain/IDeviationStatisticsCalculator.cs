namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Contains a deviation statistical calculations. 
    /// </summary>
    public interface IDeviationStatisticsCalculator
    {
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