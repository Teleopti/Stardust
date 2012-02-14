using System;

namespace Teleopti.Ccc.AgentPortal.Reports.Grid
{
    /// <summary>
    /// Represet the Adapter class for MyScheduleGrid  
    /// </summary>
    /// <remarks>
    /// Created by: Muhamad Risath
    /// Created date: 10/15/2008
    /// </remarks>
    public class MyScheduleGridAdapter
    {
        public DateTime Date { get; set; }

        public string ShortDateTime
        {
            get { return Date.ToShortDateString(); }
        }

        public double? Adherence { get; set; }

        public string MySchedule { get; set; }

        public ScheduleAdherence MyScheduleAdherence { get; set; }
    }
}