using System;
using System.Collections.Generic;
using System.Text;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public class StateChangedEventArgs : EventArgs
    {

        #region Fields - Instance Member

        private ScheduleAppointmentStatusTypes _appointmentStatus;
        private Dto _item;

        #endregion

        #region Properties - Instance Member

        public ScheduleAppointmentStatusTypes ScheduleAppointmentStatus
        {
            get { return _appointmentStatus; }
            set { _appointmentStatus = value;}
        }

        public Dto ScheduleItem
        {
            get { return _item; }
            set { _item = value; }
        }

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - StateChangedEventArgs Members - (Constructors)

        public StateChangedEventArgs(ScheduleAppointmentStatusTypes statusType, Dto item)
        {
            _appointmentStatus = statusType;
            _item = item;
        }

        public StateChangedEventArgs()
        {
        }

        #endregion

        #endregion

    }
}
