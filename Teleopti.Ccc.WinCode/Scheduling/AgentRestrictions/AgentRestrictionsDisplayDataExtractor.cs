﻿using System;
using System.Threading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsDisplayDataExtractor
	{
		void ExtractTo(IAgentDisplayData agentDisplayData, RestrictionSchedulingOptions schedulingOptions);
	}

	public class AgentRestrictionsDisplayDataExtractor : IAgentRestrictionsDisplayDataExtractor
	{
		private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private readonly IPeriodScheduledAndRestrictionDaysOff _periodScheduledAndRestrictionDaysOff;
		private readonly ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;

		public AgentRestrictionsDisplayDataExtractor(ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator, IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, IPeriodScheduledAndRestrictionDaysOff periodScheduledAndRestrictionDaysOff)
		{
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
			_periodScheduledAndRestrictionDaysOff = periodScheduledAndRestrictionDaysOff;
			_schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void ExtractTo(IAgentDisplayData agentDisplayData, RestrictionSchedulingOptions schedulingOptions)
		{
			var currentContractTime = TimeSpan.Zero;
			var targetTime = _schedulePeriodTargetTimeCalculator.TargetTime(agentDisplayData.Matrix);
			var minMax = _schedulePeriodTargetTimeCalculator.TargetWithTolerance(agentDisplayData.Matrix);
			var currentDayOffs = _periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(agentDisplayData.Matrix, schedulingOptions.UseScheduling, schedulingOptions.UsePreferences, schedulingOptions.UseRotations);

			foreach (var scheduleDayPro in agentDisplayData.Matrix.EffectivePeriodDays)
			{
				//Thread.Sleep(5);
				var projSvc = scheduleDayPro.DaySchedulePart().ProjectionService();
				var res = projSvc.CreateProjection();

				if (schedulingOptions.UseScheduling) currentContractTime = currentContractTime.Add(res.ContractTime());
			}

			agentDisplayData.ContractCurrentTime = currentContractTime;
			agentDisplayData.ContractTargetTime = targetTime;
			agentDisplayData.CurrentDaysOff = currentDayOffs;
			agentDisplayData.MinMaxTime = minMax;

			_workShiftMinMaxCalculator.ResetCache();
			var minMaxTime = _workShiftMinMaxCalculator.PossibleMinMaxTimeForPeriod(agentDisplayData.Matrix, schedulingOptions);
			agentDisplayData.MinimumPossibleTime = minMaxTime.Minimum;
			agentDisplayData.MaximumPossibleTime = minMaxTime.Maximum;
			agentDisplayData.ScheduledAndRestrictionDaysOff = _periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(agentDisplayData.Matrix, schedulingOptions.UseScheduling , schedulingOptions.UsePreferences, schedulingOptions.UseRotations);
		}
	}
}