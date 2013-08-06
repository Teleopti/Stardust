using System;
using System.Collections.Generic;
using Syncfusion.Schedule;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule
{
    public interface IScheduleDictionary : IDictionary<DateTime, IScheduleItemList>
    {
        void Fill(IList<ICustomScheduleAppointment> scheduleItemCollection);

        IScheduleAppointmentList AllScheduleAppointments();

        IScheduleAppointmentList ScheduleAppointments(DateTimePeriodDto period, ScheduleAppointmentTypes scheduleItemType);
        
        void Clear(bool keepUnsavedScheduleItems);

        IList<ICustomScheduleAppointment> Filter(ScheduleAppointmentStatusTypes appointmentStatusType);
    }
}
