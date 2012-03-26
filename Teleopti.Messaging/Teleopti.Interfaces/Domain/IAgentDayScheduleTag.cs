namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// The tag for a specific date
    /// </summary>
    public interface IAgentDayScheduleTag : IExportToAnotherScenario
    {
        /// <summary>
        /// Gets or sets the schedule tag.
        /// </summary>
        /// <value>
        /// The schedule tag.
        /// </value>
        IScheduleTag ScheduleTag { get; set; }

        /// <summary>
        /// Gets the tag date.
        /// </summary>
        DateOnly TagDate { get;}

        /// <summary>
        /// Nones the entity clone.
        /// </summary>
        /// <returns></returns>
        IAgentDayScheduleTag NoneEntityClone();
    }
}