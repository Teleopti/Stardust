using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Messages.General
{
    /// <summary>
    /// Represents a row from csv file
    /// </summary>
    public interface IForecastsFileRow
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
        int Tasks { get; set; }
        /// <summary>
        /// Task time
        /// </summary>
        int TaskTime { get; set; }
        /// <summary>
        /// After task time
        /// </summary>
        int AfterTaskTime { get; set; }
        /// <summary>
        /// Forecasted agents
        /// </summary>
        double? Agents { get; set; }
    }
}