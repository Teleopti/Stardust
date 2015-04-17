using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// What kind of schedule are visible? 
    /// The published one? The preference one? Or a combination?
    /// </summary>
    [Flags]
    public enum ScheduleVisibleReasons
    {
        /// <summary>
        /// Published schedule data
        /// </summary>
        Published = 1,
        /// <summary>
        /// Preferenced schedule data
        /// </summary>
        Preference = 2,

        /// <summary>
        /// Any
        /// </summary>
        Any = Published | Preference 
    }
}
