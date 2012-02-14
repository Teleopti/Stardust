#region Imports

using System;

#endregion

namespace Teleopti.Ccc.AgentPortalCode.Common
{

    /// <summary>
    /// Represents a Available color themes to represent activities in scheduler.
    /// </summary>
    [Serializable]
    public enum ScheduleAppointmentColorTheme
    {
        /// <summary>
        /// Show the Schedule Items in Default blue color theme
        /// </summary>
        SystemColor,

        /// <summary>
        /// Show the Schedule Items based on the Colors defined in system
        /// </summary>
        DefaultColor
    }

}
