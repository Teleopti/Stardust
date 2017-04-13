using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Meetings
{
    /// <summary>
    /// Arguments for modified schedulepart event
    /// </summary>
    public class ModifyMeetingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyMeetingEventArgs"/> class.
        /// </summary>
        /// <param name="meeting">The meeting.</param>
        /// <param name="delete">if set to <c>true</c> [delete].</param>
        public ModifyMeetingEventArgs(IMeeting meeting, bool delete)
        {
            ModifiedMeeting = meeting;
            Delete = delete;
        }

        /// <summary>
        /// Meeting that is modified
        /// </summary>
        /// <value></value>
        public IMeeting ModifiedMeeting { get; private set; }

        /// <summary>
        /// Delete
        /// </summary>
        /// <value></value>
        public bool Delete { get; private set; }
    }
}