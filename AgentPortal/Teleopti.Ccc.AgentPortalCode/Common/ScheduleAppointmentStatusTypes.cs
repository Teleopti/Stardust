#region Imports

using System;

#endregion

namespace Teleopti.Ccc.AgentPortalCode.Common
{

    /// <summary>
    /// Represents a action that can be perform against a schedule item in schedule .
    /// </summary>
    [Serializable]
    [Flags]
    public enum ScheduleAppointmentStatusTypes
    {
        /// <summary>
        /// AItem has not changed
        /// </summary>
        Unchanged=1,

        /// <summary>
        /// New Item
        /// </summary>
        New=2,

        /// <summary>
        /// Item has updated
        /// </summary>
        Updated=4,

        /// <summary>
        /// Item is  deleted
        /// </summary>
        Deleted=8
    }

}
