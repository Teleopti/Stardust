using System;
using System.Collections.Generic;
using System.Text;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public class CustomScheduleAppointmentDeleteEventArgs : EventArgs
    {
        private ICustomScheduleAppointment _deleteItem;

        public ICustomScheduleAppointment DeleteItem
        {
            get { return _deleteItem; }
            set { _deleteItem = value; }
        }

        public CustomScheduleAppointmentDeleteEventArgs(ICustomScheduleAppointment deleteItem)
        {
            _deleteItem = deleteItem;
        }
    }
}
