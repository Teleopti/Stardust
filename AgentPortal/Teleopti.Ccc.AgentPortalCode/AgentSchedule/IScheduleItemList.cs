#region Imports

using System;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;

#endregion

namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule
{

    /// <summary>
    /// Defines the functionality of a class that holds list of Schedule data objects .
    /// </summary>
    public interface IScheduleItemList 
    {
        IList<ICustomScheduleAppointment> ScheduleItemCollection { get; }

        void AddScheduleItem(ICustomScheduleAppointment scheduleItem);

        ICustomScheduleAppointment FirstScheduleItem();

        ICustomScheduleAppointment GetCurrentScheduleItem(DateTime currentDateTime);

        ICustomScheduleAppointment GetNextActivity(DateTime currentDateTime);

    }

}
