namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Segmented staffing information for skill day interval
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-09-18
    /// </remarks>
    public interface ISkillStaffSegment
    {
        /// <summary>
        /// Gets the forecasted distributed demand.
        /// </summary>
        /// <value>The forecasted distributed demand.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-18
        /// </remarks>
        double ForecastedDistributedDemand { get; }
    }
}