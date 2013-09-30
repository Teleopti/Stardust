using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Holds the user defined preferences for operation optimization of activites 
    /// </summary>
    public interface IOptimizerActivitiesPreferences: ICloneable
    {
        /// <summary>
        /// Get/set keep shiftcategory
        /// </summary>
        bool KeepShiftCategory { get; set; }
        /// <summary>
        /// Get/set keep start time
        /// </summary>
        bool KeepStartTime { get; set; }
        /// <summary>
        /// Get/set keep end time
        /// </summary>
        bool KeepEndTime { get; set; }
        /// <summary>
        /// Get/set keep between
        /// </summary>
        TimePeriod? AllowAlterBetween { get; set; }
        /// <summary>
        /// Get list with activities not to move
        /// </summary>
        IList<IActivity> DoNotMoveActivities { get; }
        /// <summary>
        /// Get available activities
        /// </summary>
        IList<IActivity> Activities { get; }

	    IActivity DoNotAlterLengthOfActivity { get; set; }

	    /// <summary>
        /// Set all activities
        /// </summary>
        /// <param name="activities"></param>
        void SetActivities(IList<IActivity> activities);
        /// <summary>
        /// Set all activities not to move
        /// </summary>
        /// <param name="activities"></param>
        void SetDoNotMoveActivities(IList<IActivity> activities);

        /// <summary>
        /// Get the utc period from a specific local date and the time period.
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
        /// <param name="timeZoneInfo">The timezone info.</param>
        /// <returns></returns>
        DateTimePeriod? UtcPeriodFromDateAndTimePeriod(DateOnly dateOnly, TimeZoneInfo timeZoneInfo);
    }
}
