using System;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events
{
	public interface ISolidProof
	{
		DateTime Timestamp { get; set; }
		string StateName { get; set; }
		string ActivityName { get; set; }
		int? ActivityColor { get; set; }
		string RuleName { get; set; }
		int? RuleColor { get; set; }
		EventAdherence? Adherence { get; set; }
	}
}