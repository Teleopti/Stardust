namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Service class for task time distribution
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-08-26
    /// </remarks>
    public interface ITaskTimeDistributionService
    {
        /// <summary>
        /// Gets the type of the distribution.
        /// </summary>
        /// <value>The type of the distribution.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-26
        /// </remarks>
        DistributionType DistributionType { get; }
    }
}