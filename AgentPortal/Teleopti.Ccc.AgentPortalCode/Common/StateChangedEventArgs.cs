using System;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public class StateChangedEventArgs : EventArgs
    {
        public StateChangedEventArgs(ScheduleAppointmentStatusTypes statusType, Dto item)
        {
            ScheduleAppointmentStatus = statusType;
            ScheduleItem = item;
        }

        public ScheduleAppointmentStatusTypes ScheduleAppointmentStatus { get; set; }

        public Dto ScheduleItem { get; set; }
    }
}
