using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Rta.Service
{
	public class Mapping
	{
		public Guid BusinessUnitId { get; set; }
		
		public string StateCode { get; set; }
		public Guid? StateGroupId { get; set; }
		public string StateGroupName { get; set; }
		public bool IsLoggedOut { get; set; } = true;

		public Guid? ActivityId { get; set; }

		public Guid? RuleId { get; set; }
		public string RuleName { get; set; }
		public Adherence? Adherence { get; set; }
		public double? StaffingEffect { get; set; }
		public int DisplayColor { get; set; }

		public bool IsAlarm { get; set; }
		public int ThresholdTime { get; set; }
		public int AlarmColor { get; set; }
	}
}