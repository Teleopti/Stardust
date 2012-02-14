#region Imports

using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Schedule;
using Teleopti.Ccc.AgentPortalCode.Common;

#endregion

namespace Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider
{

    public interface ICustomScheduleAppointment : IScheduleAppointment
    {
        Color DisplayColor { get; set; }

        ScheduleAppointmentStatusTypes Status { get; set; }

        ScheduleAppointmentTypes AppointmentType{ get; set;}

        bool AllowCopy{ get; set;}

        bool AllowOpen { get; set; }

        bool AllowDelete { get; set; }

        bool AllowMultipleDaySplit { get; set; }

        bool IsSplit { get; set; }

        ScheduleAppointmentPartType SplitPartType { get; set; }

        //string DisplayItem(string timeFormat, CultureInfo culture);
    
    }

}
