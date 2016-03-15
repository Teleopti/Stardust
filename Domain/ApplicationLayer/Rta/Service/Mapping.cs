using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class Mapping
	{
		public Guid BusinessUnitId { get; set; }
		public Guid PlatformTypeId { get; set; }

		public string StateCode { get; set; }
		public Guid StateGroupId { get; set; }
		public string StateGroupName { get; set; }
		public bool IsLogOutState { get; set; }

		public Guid? ActivityId { get; set; }

		public Guid RuleId { get; set; }
		public string RuleName { get; set; }
		public Adherence? Adherence { get; set; }
		public int? StaffingEffect { get; set; }
		public int DisplayColor { get; set; }

		public bool IsAlarm { get; set; }
		public long ThresholdTime { get; set; }
		public int AlarmColor { get; set; }
		
	}
}