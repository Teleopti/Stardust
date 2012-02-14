using System;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    /// <summary>
    /// CustomScheduleAppointmentCopyPasteEventArgs
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 11/24/2008
    /// </remarks>
    public class CustomScheduleAppointmentCopyPasteEventArgs : EventArgs
    {
        private readonly ScheduleAppointmentTypes _scheduleAppointmentType;
        private readonly IList<ICustomScheduleAppointment> _scheduleAppointmentList;
        private readonly IList<DateTime> _datesToPaste;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomScheduleAppointmentCopyPasteEventArgs"/> class.
        /// </summary>
        /// <param name="scheduleAppointmentType">Type of the schedule item.</param>
        /// <param name="scheduleAppointmentList">The schedule appointment list.</param>
        /// <param name="datesToPaste">The dates to paste.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/26/2008
        /// </remarks>
        public CustomScheduleAppointmentCopyPasteEventArgs(ScheduleAppointmentTypes scheduleAppointmentType, 
                                                       IList<ICustomScheduleAppointment> scheduleAppointmentList,
                                                       IList<DateTime> datesToPaste)
        {
            _scheduleAppointmentType = scheduleAppointmentType;
            _scheduleAppointmentList = scheduleAppointmentList;
            _datesToPaste = datesToPaste;
        }

        /// <summary>
        /// Gets the type of the item.
        /// </summary>
        /// <value>The type of the item.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/26/2008
        /// </remarks>
        public ScheduleAppointmentTypes AppointmentType
        {
            get { return _scheduleAppointmentType; }
        }

        /// <summary>
        /// Gets the schedule appointments.
        /// </summary>
        /// <value>The schedule appointments.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/26/2008
        /// </remarks>
        public IList<ICustomScheduleAppointment> ScheduleAppointments
        {
            get { return _scheduleAppointmentList; }
        }

        /// <summary>
        /// Gets the dates to paste.
        /// </summary>
        /// <value>The dates to paste.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/26/2008
        /// </remarks>
        public IList<DateTime> DatesToPaste
        {
            get { return _datesToPaste; }
        }
    }
}
