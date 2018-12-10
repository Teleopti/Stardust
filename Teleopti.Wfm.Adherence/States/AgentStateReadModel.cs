using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.States
{
	public class AgentStateReadModel
	{
		public Guid PersonId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string EmploymentNumber { get; set; }
		public Guid? BusinessUnitId { get; set; }

		public Guid? SiteId { get; set; }
		public string SiteName { get; set; }

		public Guid? TeamId { get; set; }
		public string TeamName { get; set; }

		public DateTime ReceivedTime { get; set; }

		public string Activity { get; set; }
		public string NextActivity { get; set; }
		public DateTime? NextActivityStartTime { get; set; }

		public string StateName { get; set; }
		public Guid? StateGroupId { get; set; }
		public DateTime? StateStartTime { get; set; }

		public string RuleName { get; set; }
		public DateTime? RuleStartTime { get; set; }
		public int? RuleColor { get; set; }
		public double? StaffingEffect { get; set; }

		public DateTime? OutOfAdherenceStartTime { get; set; }

		public bool IsRuleAlarm { get; set; }
		public DateTime? AlarmStartTime { get; set; }
		public int? AlarmColor { get; set; }

		public IEnumerable<AgentStateActivityReadModel> Shift { get; set; }

		public IEnumerable<AgentStateOutOfAdherenceReadModel> OutOfAdherences { get; set; }

		public override string ToString()
		{
			return $"PersonId: {PersonId}, " +
				   $"StateName: {StateName}, " +
				   $"Activity: {Activity}, " +
				   $"StateStartTime: {StateStartTime}, " +
				   $"NextActivity: {NextActivity}, " +
				   $"NextActivityStartTime: {NextActivityStartTime}, " +
				   $"RuleName: {RuleName}, " +
				   $"RuleStartTime: {RuleStartTime}, "
				   + $"IsRuleAlarm: {IsRuleAlarm}, " +
				   $"AlarmStartTime: {AlarmStartTime}";
		}
	}

	public class AgentStateActivityReadModel
	{
		public int Color { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string Name { get; set; }
	}

	public class AgentStateOutOfAdherenceReadModel
	{
		public DateTime StartTime { get; set; }
		public DateTime? EndTime { get; set; }
	}
}