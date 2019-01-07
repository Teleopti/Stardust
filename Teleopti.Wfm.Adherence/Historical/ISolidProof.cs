using System;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Wfm.Adherence.Historical
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