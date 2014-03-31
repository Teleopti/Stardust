using System;
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
		private readonly IAgentRestrictionsNoWorkShiftfFinder _agentRestrictionsNoWorkShiftfFinder;
		private readonly ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;

		public AgentRestrictionsDisplayDataExtractor(ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator, IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, IPeriodScheduledAndRestrictionDaysOff periodScheduledAndRestrictionDaysOff, IAgentRestrictionsNoWorkShiftfFinder agentRestrictionsNoWorkShiftfFinder)
		{
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
			_periodScheduledAndRestrictionDaysOff = periodScheduledAndRestrictionDaysOff;
			_agentRestrictionsNoWorkShiftfFinder = agentRestrictionsNoWorkShiftfFinder;
			_schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void ExtractTo(IAgentDisplayData agentDisplayData, RestrictionSchedulingOptions schedulingOptions)
		{
			var currentContractTime = TimeSpan.Zero;
			var targetTime = _schedulePeriodTargetTimeCalculator.TargetTime(agentDisplayData.Matrix);
			var minMax = _schedulePeriodTargetTimeCalculator.TargetWithTolerance(agentDisplayData.Matrix);
			var currentDayOffs = _periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(agentDisplayData.Matrix, schedulingOptions.UseScheduling, schedulingOptions.UsePreferences, schedulingOptions.UseRotations);
			var noWorkshiftFound = false;
			foreach (var scheduleDayPro in agentDisplayData.Matrix.EffectivePeriodDays)
			{
				var scheduleDay = scheduleDayPro.DaySchedulePart();
				//Thread.Sleep(5);
				var projSvc = scheduleDay.ProjectionService();
				var res = projSvc.CreateProjection();
				if (schedulingOptions.UseScheduling) currentContractTime = currentContractTime.Add(res.ContractTime());

				if (!noWorkshiftFound)
					noWorkshiftFound = _agentRestrictionsNoWorkShiftfFinder.Find(scheduleDay, schedulingOptions);	
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
			agentDisplayData.NoWorkshiftFound = noWorkshiftFound;
		}
	}
}