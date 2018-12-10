namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Interface for periodized items, to make queries easier...
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-05-06
    /// </remarks>
    public interface IPeriodized
    {
        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-06
        /// </remarks>
        DateTimePeriod Period { get; }
    }
}