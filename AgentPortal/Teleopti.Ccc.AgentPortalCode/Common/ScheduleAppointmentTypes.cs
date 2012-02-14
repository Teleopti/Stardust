#region Imports

using System;

#endregion

namespace Teleopti.Ccc.AgentPortalCode.Common
{

    /// <summary>
    /// Represents a types of schedule item that visualizing in schedule controls .
    /// </summary>
    [Flags]
    [Serializable]
    public enum ScheduleAppointmentTypes
    {
        /// <summary>
        /// Activiteis assign to  person , basically his  shift
        /// </summary>
        Activity =1,

        /// <summary>
        /// Absence assign to person
        /// </summary>
        Absence=2,

        /// <summary>
        /// Person Day offs
        /// </summary>
        DayOff=4,

        /// <summary>
        /// Person requests
        /// </summary>
        Request=8,

        /// <summary>
        /// Preference Restrictions set by person
        /// </summary>
        PreferenceRestriction=16,

        /// <summary>
        /// Student Availability PreferenceRestriction
        /// </summary>
        StudentAvailability=32,

        /// <summary>
        /// Person Meeting
        /// </summary>
        Meeting=64,

        /// <summary>
        /// Personal Shift
        /// </summary>
        PersonalShift=128,


        /// <summary>
        /// Public Note on agent day
        /// </summary>
        PublicNote=256
    }

}