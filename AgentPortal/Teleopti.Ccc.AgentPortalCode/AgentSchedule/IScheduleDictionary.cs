#region Imports

using System;
using System.Collections.Generic;
using Syncfusion.Schedule;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

#endregion

namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule
{

    /// <summary>
    /// Defines the custom functionality of a  schedule dictionary.
    /// </summary>
    public interface IScheduleDictionary : IDictionary<DateTime, IScheduleItemList>
    {

        void Fill(IList<ICustomScheduleAppointment> scheduleItemCollection);

        void Fill(ICustomScheduleAppointment scheduleItem);

        IScheduleAppointmentList AllScheduleAppointments();

        IScheduleAppointmentList ScheduleAppointments(DateTimePeriodDto period, ScheduleAppointmentTypes scheduleItemType);
        
        IList<ICustomScheduleAppointment> UnsavedAppointments();

        IList<ICustomScheduleAppointment> UnsavedAppointments(ScheduleAppointmentTypes appointmentType, ScheduleAppointmentStatusTypes filterBy);

        void Clear(bool keepUnsavedScheduleItems);

        void RemoveScheduleAppointment(ICustomScheduleAppointment sourceScheduleAppointment);

        IList<ICustomScheduleAppointment> Filter(ScheduleAppointmentStatusTypes appointmentStatusType);


    }

}
