using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.ScheduleThreading
{
    internal class TimeInfoObject
    {

        public int Time { get; set; }
        public int AbsenceTime { get; set; }
        public int ActivityTime { get; set; }
    }
}