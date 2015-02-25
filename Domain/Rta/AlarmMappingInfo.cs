using System;

namespace Teleopti.Ccc.Domain.Rta
{
    public class AlarmMappingInfo
    {
		public Guid BusinessUnit { get; set; }
		public Guid StateGroupId { get; set; }
	    public Guid ActivityId { get; set; }
        public Guid AlarmTypeId { get; set; }
		public string Name { get; set; }
		public int DisplayColor { get; set; }
        public double StaffingEffect { get; set; }
        public long ThresholdTime { get; set; }
    }
}