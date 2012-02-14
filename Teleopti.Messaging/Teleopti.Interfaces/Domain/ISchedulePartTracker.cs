
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Keeps track of the changed SchedulePart interface
    /// </summary>
    public interface ISchedulePartTracker
    {
        /// <summary>
        /// Gets or sets the original part.
        /// </summary>
        /// <value>The original part.</value>
        IScheduleDay OriginalPart { get; set; }

        /// <summary>
        /// Gets or sets the changed part.
        /// </summary>
        /// <value>The changed part.</value>
        IScheduleDay ChangedPart { get; set; }
    }
}