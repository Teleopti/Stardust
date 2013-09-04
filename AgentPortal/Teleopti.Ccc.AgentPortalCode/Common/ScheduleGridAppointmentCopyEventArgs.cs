using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Syncfusion.Schedule;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public enum CopyPasteAction
    {
        Copy,
        Paste,
        Cut
    }

    /// <summary>
    /// ScheduleGridAppointmentCopy event args
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 11/24/2008
    /// </remarks>
    public class ScheduleGridAppointmentCopyEventArgs : EventArgs
    {
        private readonly Collection<ICustomScheduleAppointment> _appointmentList;
        private readonly Collection<DateTime> _datesToPaste;
        private readonly CopyPasteAction _action;

        public ScheduleGridAppointmentCopyEventArgs(Collection<ICustomScheduleAppointment> appointmentList,
                                                    Collection<DateTime> datesToPaste,
                                                    CopyPasteAction copyPasteAction)
        {
            _appointmentList = appointmentList;
            _datesToPaste = datesToPaste;
            _action = copyPasteAction;
        }

        /// <summary>
        /// Gets the appointments.
        /// </summary>
        /// <value>The appointments.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/24/2008
        /// </remarks>
        public ReadOnlyCollection<ICustomScheduleAppointment> ScheduleAppointments
        {
            get
            {
                List<ICustomScheduleAppointment> temp = new List<ICustomScheduleAppointment>(_appointmentList);
                temp.Sort(ScheduleAppointmentSort);
                return new ReadOnlyCollection<ICustomScheduleAppointment>(temp);
            }
        }

        /// <summary>
        /// Gets the dates to paste.
        /// </summary>
        /// <value>The dates to paste.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/26/2008
        /// </remarks>
        public ReadOnlyCollection<DateTime> DatesToPaste
        {
            get
            {
                List<DateTime> temp = new List<DateTime>(_datesToPaste);
                temp.Sort();
                return new ReadOnlyCollection<DateTime>(temp);
            }
        }

        /// <summary>
        /// Gets the copy paste action.
        /// </summary>
        /// <value>The copy paste action.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/26/2008
        /// </remarks>
        public CopyPasteAction CopyPasteAction
        {
            get { return _action; }
        }

        /// <summary>
        /// Schedules the appointment sort.
        /// </summary>
        /// <param name="app1">The app1.</param>
        /// <param name="app2">The app2.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/26/2008
        /// </remarks>
        private static int ScheduleAppointmentSort(ICustomScheduleAppointment app1, ICustomScheduleAppointment app2)
        {
            return app1.StartTime.Date.CompareTo(app2.StartTime.Date);
        }
    }
}
