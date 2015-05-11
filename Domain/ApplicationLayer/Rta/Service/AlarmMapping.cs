using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AlarmMapping
	{
		public AlarmMapping()
		{
			Adherence = AdherenceState.Unknown;
		}

		public Guid BusinessUnitId { get; set; }
		public Guid PlatformTypeId { get; set; }
		public string StateCode { get; set; }
		public Guid? ActivityId { get; set; }

		public Guid AlarmTypeId { get; set; }
		public string AlarmName { get; set; }
		public AdherenceState Adherence { get; set; }
		public int StaffingEffect { get; set; }
		public int DisplayColor { get; set; }
		public long ThresholdTime { get; set; }
	}
}