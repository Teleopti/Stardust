namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Daily data holder class for representing a selected schedule day in a selected schedule period.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 2008-08-21
    /// </remarks>
    public interface IScheduleDayPro
    {
        /// <summary>
        /// Gets the day.
        /// </summary>
        /// <value>The day.</value>
        DateOnly Day { get; }

        /// <summary>
        /// Gets the day's schedule part.
        /// </summary>
        /// <value>The day schedule part.</value>
        IScheduleDay DaySchedulePart();
    }
}