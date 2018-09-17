using System;

namespace Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay
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

		public string LateForWork { get; set; }
		public string Duration { get; set; }
	}
}