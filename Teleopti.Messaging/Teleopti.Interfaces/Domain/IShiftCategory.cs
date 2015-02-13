using System;
using System.Collections.Generic;
using System.Drawing;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Represents a shift category, a grouping of shifts
    /// </summary>
    public interface IShiftCategory : IAggregateRoot,
                                        IChangeInfo,
                                        IBelongsToBusinessUnit
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        Description Description { get; set; }

        /// <summary>
        /// Gets the color of a ShiftCategory
        /// </summary>
        Color DisplayColor { get; set; }

        /// <summary>
        /// Gets the day of week justice values.
        /// </summary>
        /// <value>The day of week justice values.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-02
        /// </remarks>
        IDictionary<DayOfWeek, int> DayOfWeekJusticeValues { get; }

        /// <summary>
        /// Reinitializes the day of week dictionary.
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-18
        /// </remarks>
        void ReinitializeDayOfWeekDictionary();

        /// <summary>
        /// Returns Max value of all justice values.
        /// </summary>
        /// <returns></returns>
        int MaxOfJusticeValues();

		int? Rank { get; set; }
    }
}
