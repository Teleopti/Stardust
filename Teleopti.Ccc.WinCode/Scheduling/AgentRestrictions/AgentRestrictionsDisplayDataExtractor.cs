using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsDisplayDataExtractor
	{
		void ExtractTo(IAgentDisplayData agentDisplayData, ISchedulingOptions schedulingOptions, bool useSchedules);
	}

	public class AgentRestrictionsDisplayDataExtractor : IAgentRestrictionsDisplayDataExtractor
	{
		private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private readonly IPeriodScheduledAndRestrictionDaysOff _periodScheduledAndRestrictionDaysOff;
		private readonly IRestrictionExtractor _restrictionExtractor;

		public AgentRestrictionsDisplayDataExtractor(IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, IPeriodScheduledAndRestrictionDaysOff periodScheduledAndRestrictionDaysOff, IRestrictionExtractor restrictionExtractor)
		{
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
			_periodScheduledAndRestrictionDaysOff = periodScheduledAndRestrictionDaysOff;
			_restrictionExtractor = restrictionExtractor;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void ExtractTo(IAgentDisplayData agentDisplayData, ISchedulingOptions schedulingOptions, bool useSchedules)
		{
			_workShiftMinMaxCalculator.ResetCache();
			MinMax<TimeSpan> minMaxTime = _workShiftMinMaxCalculator.PossibleMinMaxTimeForPeriod(agentDisplayData.Matrix,
			                                                                                     schedulingOptions);
			agentDisplayData.MinimumPossibleTime = minMaxTime.Minimum;
			agentDisplayData.MaximumPossibleTime = minMaxTime.Maximum;
			agentDisplayData.ScheduledAndRestrictionDaysOff =
				_periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(_restrictionExtractor, agentDisplayData.Matrix, useSchedules,
				                                                        schedulingOptions.UsePreferences,
				                                                        schedulingOptions.UseRotations);
		}
	}
}