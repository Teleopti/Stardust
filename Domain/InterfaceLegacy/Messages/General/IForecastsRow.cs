using System;

namespace Teleopti.Interfaces.Messages.General
{
    /// <summary>
    /// Represents a forecast row
    /// </summary>
    public interface IForecastsRow
    {
        /// <summary>
        /// Skill's name
        /// </summary>
        string SkillName { get; set; }
        /// <summary>
        /// Local start date time
        /// </summary>
        DateTime LocalDateTimeFrom { get; set; }
        /// <summary>
        /// Local end date time
        /// </summary>
        DateTime LocalDateTimeTo { get; set; }
        /// <summary>
        /// Start date time in UTC
        /// </summary>
        DateTime UtcDateTimeFrom { get; set; }
        /// <summary>
        /// End date time in UTC
        /// </summary>
        DateTime UtcDateTimeTo { get; set; }
        /// <summary>
        /// Tasks
        /// </summary>
        double Tasks { get; set; }
        /// <summary>
        /// Task time
        /// </summary>
        double TaskTime { get; set; }
        /// <summary>
        /// After task time
        /// </summary>
        double AfterTaskTime { get; set; }
        /// <summary>
        /// Forecasted agents
        /// </summary>
        double? Agents { get; set; }
        /// <summary>
        /// Shrinkage
        /// </summary>
        double? Shrinkage { get; set; }
    }
}