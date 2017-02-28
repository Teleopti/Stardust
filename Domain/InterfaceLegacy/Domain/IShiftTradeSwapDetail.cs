namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Swap details for a shift trade
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-09-01
    /// </remarks>
    public interface IShiftTradeSwapDetail : IAggregateEntity, ISwapDetail
    {
        /// <summary>
        /// Gets or sets the checksum from.
        /// </summary>
        /// <value>The checksum from.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-01
        /// </remarks>
        long ChecksumFrom { get; set; }

        /// <summary>
        /// Gets or sets the checksum to.
        /// </summary>
        /// <value>The checksum to.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-01
        /// </remarks>
        long ChecksumTo { get; set; }

        /// <summary>
        /// Gets or sets the schedule part from.
        /// This is extra information that's used to send the schedule information back via SDK
        /// when working with requests.
        /// </summary>
        /// <value>The schedule part from.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-01
        /// </remarks>
        IScheduleDay SchedulePartFrom { get; set; }

        /// <summary>
        /// Gets or sets the schedule part to.
        /// This is extra information that's used to send the schedule information back via SDK
        /// when working with requests.
        /// </summary>
        /// <value>The schedule part to.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-01
        /// </remarks>
        IScheduleDay SchedulePartTo { get; set; }
    }
}