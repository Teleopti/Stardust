using System;

namespace Teleopti.Interfaces.Domain
{
	public class AgentStateReadModel
	{
		public Guid PlatformTypeId { get; set; }
		public string OriginalDataSourceId { get; set; }
		public DateTime? BatchId { get; set; }

		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid? TeamId { get; set; }
		public Guid? SiteId { get; set; }
		public DateTime ReceivedTime { get; set; }

		public Guid? ScheduledId { get; set; }
		public string Scheduled { get; set; }
		public Guid? ScheduledNextId { get; set; }
		public string ScheduledNext { get; set; }
		public DateTime? NextStart { get; set; }

		public string StateCode { get; set; }
		public string StateName { get; set; }
		public Guid? StateId { get; set; }
		public DateTime? StateStartTime { get; set; }
		
		public Guid? RuleId { get; set; }
		public string RuleName { get; set; }
		public DateTime? RuleStartTime { get; set; }
		public int? RuleColor { get; set; }
		public double? StaffingEffect { get; set; }
		public int? Adherence { get; set; }

		public bool IsAlarm { get; set; }
		public DateTime? AlarmStartTime { get; set; }
		public int? AlarmColor { get; set; }

		public override string ToString()
		{
			return string.Format(
				"PersonId: {0}, " +
				"BatchId: {9}, " +
				"StateCode: {1} " +
				"State: {2}, " +
				"Scheduled: {3}, " +
				"StateStartTime: {4}, " +
				"ScheduledNext: {5}, " +
				"NextStart: {6}, " +
				"RuleName: {7}, " +
				"RuleStartTime: {8}, " +
				"IsAlarm: {10}, " +
				"AlarmStartTime: {11}",
				PersonId,
				BatchId,
				StateCode, 
				StateName, 
				Scheduled, 
				StateStartTime, 
				ScheduledNext, 
				NextStart,
				RuleName, 
				RuleStartTime,
				IsAlarm, 
				AlarmStartTime
				);
		}

	}
}