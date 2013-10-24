using System;

namespace Teleopti.Ccc.Rta.Server
{
    public class RtaAlarmLight
    {
        public Guid StateGroupId { get; set; }
        public string StateGroupName { get; set; }
        public Guid ActivityId { get; set; }
        public string Name { get; set; }
        public Guid AlarmTypeId { get; set; }
        public int DisplayColor { get; set; }
        public double StaffingEffect { get; set; }
        public long ThresholdTime { get; set; }
        public Guid BusinessUnit { get; set; }
    }
}