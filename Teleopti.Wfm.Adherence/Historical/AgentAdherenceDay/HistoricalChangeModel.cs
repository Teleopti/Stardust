using System;

namespace Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay
{
	public class HistoricalChangeModel
	{
		public DateTime Timestamp { get; set; }

		public string StateName { get; set; }

		public string ActivityName { get; set; }
		public int? ActivityColor { get; set; }

		public string RuleName { get; set; }
		public int? RuleColor { get; set; }

		public HistoricalChangeAdherence? Adherence { get; set; }

		public int? LateForWorkMinutes { get; set; }
		
		public string Duration { get; set; }
	}
}