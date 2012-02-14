using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// To detemine the time format
    /// </summary>
    public enum TimeFormatsType
    {
        /// <summary>
        /// Format hh:mm eg 29:23
        /// </summary>
        HoursMinutes,
        /// <summary>
        /// Format hh:mm:ss eg 12:09:40
        /// </summary>
        HoursMinutesSeconds
    }
}
