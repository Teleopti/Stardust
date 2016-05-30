using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStateReadModel
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid? TeamId { get; set; }
		public Guid? SiteId { get; set; }
		public DateTime ReceivedTime { get; set; }

		public string Activity { get; set; }
		public string NextActivity { get; set; }
		public DateTime? NextActivityStartTime { get; set; }

		public string StateCode { get; set; }
		public string StateName { get; set; }
		public DateTime? StateStartTime { get; set; }
		
		public string RuleName { get; set; }
		public DateTime? RuleStartTime { get; set; }
		public int? RuleColor { get; set; }
		public double? StaffingEffect { get; set; }

		public bool IsRuleAlarm { get; set; }
		public DateTime? AlarmStartTime { get; set; }
		public int? AlarmColor { get; set; }
		
		public IEnumerable<ChoppedLayer> Shift{ get; set; }

		public override string ToString()
		{
			return string.Format(
				"PersonId: {0}, " +
				"StateCode: {1} " +
				"StateName: {2}, " +
				"Activity: {3}, " +
				"StateStartTime: {4}, " +
				"NextActivity: {5}, " +
				"NextActivityStartTime: {6}, " +
				"RuleName: {7}, " +
				"RuleStartTime: {8}, " +
				"IsRuleAlarm: {9}, " +
				"AlarmStartTime: {10}",
				PersonId,
				StateCode, 
				StateName, 
				Activity, 
				StateStartTime, 
				NextActivity, 
				NextActivityStartTime,
				RuleName, 
				RuleStartTime,
				IsRuleAlarm, 
				AlarmStartTime
				);
		}

	}

	public class ChoppedLayer
	{
		public string Color { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
	}
}