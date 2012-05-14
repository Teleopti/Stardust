using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsDisplayDataExtractor
	{
		void ExtractTo(IAgentDisplayData agentDisplayData, ISchedulingOptions schedulingOptions);
	}

	public class AgentRestrictionsDisplayDataExtractor : IAgentRestrictionsDisplayDataExtractor
	{
		private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;

		public AgentRestrictionsDisplayDataExtractor(IWorkShiftMinMaxCalculator workShiftMinMaxCalculator)
		{
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void ExtractTo(IAgentDisplayData agentDisplayData, ISchedulingOptions schedulingOptions)
		{
			_workShiftMinMaxCalculator.ResetCache();
			MinMax<TimeSpan> minMaxTime = _workShiftMinMaxCalculator.PossibleMinMaxTimeForPeriod(agentDisplayData.Matrix,
			                                                                                     schedulingOptions);
			agentDisplayData.MinimumPossibleTime = minMaxTime.Minimum;
			agentDisplayData.MaximumPossibleTime = minMaxTime.Maximum;
		}
	}
}