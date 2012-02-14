namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// An object of this types extends the default IWorkShiftExtender with another period with segment.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 2011-05-24
    /// </remarks>
    public interface IActivityNormalExtender : IWorkShiftExtender
    {
        /// <summary>
        /// Gets or sets the activity position with segment.
        /// </summary>
        /// <value>The activity position with segment.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-27
        /// </remarks>
        TimePeriodWithSegment ActivityPositionWithSegment { get; set; }
    }
}