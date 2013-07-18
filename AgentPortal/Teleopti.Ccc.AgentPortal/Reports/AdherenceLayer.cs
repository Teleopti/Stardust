using System;
using System.Collections.Generic;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Reports
{
    public class AdherenceLayer
    {
        public AdherenceLayer(TimePeriod period, double adherence, DateTime calendarDate, DateTime shiftBelongsToDate)
        {
            Period = period;
            Adherence = adherence;
        	CalendarDate = calendarDate;
        	ShiftBelongsToDate = shiftBelongsToDate;
        }

        public DateTime CalendarDate { get; set; }

        public DateTime ShiftBelongsToDate { get; set; }

        public double Adherence { get; set; }

        public TimePeriod Period { get; set; }
    }
}
