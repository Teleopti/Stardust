
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Can extract data out of schedulerange and scheduledictionaries
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-05-19
    /// </remarks>
    public interface IScheduleExtractor
    {
        /// <summary>
        /// Sets the schedule part.
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-26
        /// </remarks>
        void AddSchedulePart(IScheduleDay schedulePart);
    }
}