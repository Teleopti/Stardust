
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Gets som important information for a SchedulePart
    /// </summary>
    public interface ISchedulePartExtractor
    {
        /// <summary>
        /// Gets the contained schedule part.
        /// </summary>
        /// <value>The schedule part.</value>
        IScheduleDay SchedulePart { get; }

        /// <summary>
        /// Gets the schedule period.
        /// </summary>
        /// <value>The schedule period.</value>
        IVirtualSchedulePeriod SchedulePeriod { get; }

        /// <summary>
        /// Gets the actual schedule period.
        /// </summary>
        /// <value>The actual schedule period.</value>
        DateOnlyPeriod ActualSchedulePeriod { get; }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>The person.</value>
        IPerson Person { get; }
    }
}