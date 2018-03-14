using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
{
	public class HistoricalChangeModel
	{
		[RemoveMeWithToggle(Toggles.RTA_RemoveApprovedOOA_47721)]
		public Guid PersonId { get; set; }
		[RemoveMeWithToggle(Toggles.RTA_RemoveApprovedOOA_47721)]
		public DateOnly? BelongsToDate { get; set; }
		public DateTime Timestamp { get; set; }

		public string StateName { get; set; }
		[RemoveMeWithToggle(Toggles.RTA_RemoveApprovedOOA_47721)]
		public Guid? StateGroupId { get; set; }

		public string ActivityName { get; set; }
		public int? ActivityColor { get; set; }

		public string RuleName { get; set; }
		public int? RuleColor { get; set; }

		public HistoricalChangeAdherence? Adherence { get; set; }
	}
}