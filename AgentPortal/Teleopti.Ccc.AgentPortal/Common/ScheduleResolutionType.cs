#region Imports

using System;

#endregion

namespace Teleopti.Ccc.AgentPortal.Common
{

    /// <summary>
    /// Represents a Resolution types that can set in Schedule control.
    /// </summary>
    [Serializable]
    public enum ScheduleResolutionType
    {
        /// <summary>
        /// No of intervals in hour is  one
        /// </summary>
        SixtyMinutes=1,

        /// <summary>
        /// No of intervals in hour is  two
        /// </summary>
        ThirtyMinutes=2,

        /// <summary>
        /// No of intervals in hour is  four
        /// </summary>
        FifteenMinutes=4,

        /// <summary>
        /// No of intervals in hour is  six
        /// </summary>
        TenMinutes=6,

        /// <summary>
        /// No of intervals in hour is  ten
        /// </summary>
        SixMinutes=10,

        /// <summary>
        /// No of intervals in hour is  twelve
        /// </summary>
        FiveMinutes=12
    }

}
