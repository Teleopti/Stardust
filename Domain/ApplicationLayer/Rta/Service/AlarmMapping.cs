using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AlarmMapping
	{
		public Guid BusinessUnitId { get; set; }
		public Guid PlatformTypeId { get; set; }
		public string StateCode { get; set; }
		public Guid? ActivityId { get; set; }

		public Guid RuleId { get; set; }
		public string AlarmName { get; set; }
		public Adherence? Adherence { get; set; }
		public int? StaffingEffect { get; set; }
		public int DisplayColor { get; set; }
		public long ThresholdTime { get; set; }
		public bool IsAlarm { get; set; }
	}
}