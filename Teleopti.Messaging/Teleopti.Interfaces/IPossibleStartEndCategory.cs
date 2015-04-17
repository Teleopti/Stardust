using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPossibleStartEndCategory
    {
        /// <summary>
        /// 
        /// </summary>
        TimeSpan StartTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
		TimeSpan EndTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        IList<DateTimePeriod> ActivityPeriods { get; set; }
        /// <summary>
        /// 
        /// </summary>
        IShiftCategory ShiftCategory { get; set; }
    }
}
