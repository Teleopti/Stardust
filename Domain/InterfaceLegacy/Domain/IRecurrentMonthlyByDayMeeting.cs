namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Monthly meeting by day
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-10-13
    /// </remarks>
    public interface IRecurrentMonthlyByDayMeeting : IRecurrentMeetingOption
    {
        /// <summary>
        /// Gets or sets the day in month.
        /// </summary>
        /// <value>The day in month.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-13
        /// </remarks>
        int DayInMonth { get; set; }
    }
}