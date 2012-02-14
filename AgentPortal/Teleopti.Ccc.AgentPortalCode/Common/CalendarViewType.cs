#region Imports

using System;

#endregion

namespace Teleopti.Ccc.AgentPortalCode.Common
{

    /// <summary>
    /// Represents a ScheduleType available in Calendar view 
    /// </summary>
    [Serializable]
    public enum CalendarViewType
    {
        /// <summary>
        /// Day Type
        /// </summary>
        Day,
        /// <summary>
        /// Work Week type
        /// </summary>
        Workweek,
        /// <summary>
        /// Month Type
        /// </summary>
        Month,

        /// <summary>
        /// Team View Type
        /// </summary>
        Team

        

    }

}
