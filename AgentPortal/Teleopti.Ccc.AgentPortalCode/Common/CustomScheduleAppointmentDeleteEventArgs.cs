using System;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;

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
