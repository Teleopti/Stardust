#region Imports

using System;

#endregion

namespace Teleopti.Ccc.AgentPortalCode.Common
{

    /// <summary>
    /// Represents a type of the part once  the multi Day 
    /// Schedule appointment is splitted in to parts.
    /// </summary>
    [Serializable]
    public enum ScheduleAppointmentPartType
    {
        None,

        /// <summary>
        /// Furst Part in Splitted Collection
        /// </summary>
        First ,

        /// <summary>
        /// Middle Part in a spllited Collection
        /// </summary>
        Middle,

        /// <summary>
        /// Last Part in Splitted Collection
        /// </summary>
        Last

    }

}
