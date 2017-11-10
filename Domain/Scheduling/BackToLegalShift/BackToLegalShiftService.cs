using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Scheduling.BackToLegalShift
{
	public class BackToLegalShiftService : IBackToLegalShiftService
	{
		//copied from BackToLegalShiftService
		
		private readonly IBackToLegalShiftWorker _backToLegalShiftWorker;
		private readonly IFirstShiftInTeamBlockFinder _firstShiftInTeamBlockFinder;
		private readonly ILegalShiftDecider _legalShiftDecider;
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly CascadingResourceCalculationContextFactory _cascadingResourceCalculationContextFactory;

		public BackToLegalShiftService(IBackToLegalShiftWorker backToLegalShiftWorker,
			IFirstShiftInTeamBlockFinder firstShiftInTeamBlockFinder, ILegalShiftDecider legalShiftDecider,
			IDayOffsInPeriodCalculator dayOffsInPeriodCalculator,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			CascadingResourceCalculationContextFactory cascadingResourceCalculationContextFactory)
		{
			_backToLegalShiftWorker = backToLegalShiftWorker;
			_firstShiftInTeamBlockFinder = firstShiftInTeamBlockFinder;
			_legalShiftDecider = legalShiftDecider;
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_cascadingResourceCalculationContextFactory = cascadingResourceCalculationContextFactory;
		}

		public event EventHandler<BackToLegalShiftArgs> Progress;

		public void Execute(IList<ITeamBlockInfo> selectedTeamBlocks, SchedulingOptions schedulingOptions,
			ISchedulingResultStateHolder schedulingResultStateHolder, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer)
		{
			//single block, single team
			int processedBlocks = 0;
			foreach (var selectedTeamBlock in selectedTeamBlocks.GetRandom(selectedTeamBlocks.Count, true))
			{
				using (_cascadingResourceCalculationContextFactory.Create(schedulingResultStateHolder.Schedules,
					schedulingResultStateHolder.Skills, false, selectedTeamBlock.BlockInfo.BlockPeriod))
				{
					isSingleTeamSingleDay(selectedTeamBlock);
	
					var person = selectedTeamBlock.TeamInfo.GroupMembers.First();
					var date = selectedTeamBlock.BlockInfo.BlockPeriod.StartDate;
					var ruleSetBag = person.Period(date).RuleSetBag;
	
					processedBlocks++;
					var roleModel = _firstShiftInTeamBlockFinder.FindFirst(selectedTeamBlock, person, date, schedulingResultStateHolder.Schedules);
					CancelSignal progressResult;
					if(roleModel == null)
					{
						progressResult = onProgress(selectedTeamBlocks.Count, processedBlocks);
						if (progressResult.ShouldCancel) return;
						continue;
					}
	
					var scheduleDay = schedulingResultStateHolder.Schedules[person].ScheduledDay(date);
	
					if (_legalShiftDecider.IsLegalShift(ruleSetBag, roleModel, scheduleDay))
					{
						progressResult = onProgress(selectedTeamBlocks.Count, processedBlocks);
						if (progressResult.ShouldCancel) return;
						continue;
					}
	
					var success = _dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulingResultStateHolder.Schedules, person.VirtualSchedulePeriod(date), out int _, out IList<IScheduleDay> _);
	
					if (!success)
					{
						continue;
					}
	
					var scheduleMatrixPro = selectedTeamBlock.MatrixesForGroupAndBlock().First();
					var originalDay = scheduleMatrixPro.GetScheduleDayByKey(date).DaySchedulePart();
					var overTimeActivities = originalDay.PersonAssignment(true).OvertimeActivities();
	
					success = _backToLegalShiftWorker.ReSchedule(selectedTeamBlock, schedulingOptions, roleModel, rollbackService,
						resourceCalculateDelayer, schedulingResultStateHolder);
	
					if (success)
					{
						if (overTimeActivities.Any())
						{
							var scheduledDay = schedulingResultStateHolder.Schedules[person].ScheduledDay(date);
	
							foreach (var overtimeShiftLayer in overTimeActivities)
							{
								scheduledDay.CreateAndAddOvertime(overtimeShiftLayer.Payload, overtimeShiftLayer.Period,
									overtimeShiftLayer.DefinitionSet);
							}
							schedulingResultStateHolder.Schedules.Modify(scheduledDay, _scheduleDayChangeCallback);
							resourceCalculateDelayer.CalculateIfNeeded(date, null, false);
						}
					}
	
					progressResult = onProgress(selectedTeamBlocks.Count, processedBlocks);
					if (progressResult.ShouldCancel) return;	
				}
			}
		}

		private CancelSignal onProgress(int totalBlocks, int processedBlocks)
		{
			var handler = Progress;
			if (handler != null)
			{
				var args = new BackToLegalShiftArgs {TotalBlocks = totalBlocks, ProcessedBlocks = processedBlocks};
				handler(this, args);

				if (args.Cancel)
					return new CancelSignal{ShouldCancel = true};
			}
			return new CancelSignal();
		}

		private void isSingleTeamSingleDay(ITeamBlockInfo teamBlockInfo)
		{
			if(teamBlockInfo.TeamInfo.GroupMembers.Count() > 1 || teamBlockInfo.BlockInfo.BlockPeriod.DayCount()>1)
				throw new ArgumentException("Only single team, single block allowed");
		}
	}
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680)]
	public interface IBackToLegalShiftService
	{
		void Execute(IList<ITeamBlockInfo> selectedTeamBlocks, SchedulingOptions schedulingOptions,
			ISchedulingResultStateHolder schedulingResultStateHolder, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer);

		event EventHandler<BackToLegalShiftArgs> Progress;
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680)]
	public class BackToLegalShiftServiceOLD : IBackToLegalShiftService
	{
		private readonly IBackToLegalShiftWorker _backToLegalShiftWorker;
		private readonly IFirstShiftInTeamBlockFinder _firstShiftInTeamBlockFinder;
		private readonly ILegalShiftDecider _legalShiftDecider;
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public BackToLegalShiftServiceOLD(IBackToLegalShiftWorker backToLegalShiftWorker,
			IFirstShiftInTeamBlockFinder firstShiftInTeamBlockFinder, ILegalShiftDecider legalShiftDecider,
			IDayOffsInPeriodCalculator dayOffsInPeriodCalculator,
			IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_backToLegalShiftWorker = backToLegalShiftWorker;
			_firstShiftInTeamBlockFinder = firstShiftInTeamBlockFinder;
			_legalShiftDecider = legalShiftDecider;
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public event EventHandler<BackToLegalShiftArgs> Progress;

		public void Execute(IList<ITeamBlockInfo> selectedTeamBlocks, SchedulingOptions schedulingOptions,
			ISchedulingResultStateHolder schedulingResultStateHolder, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer)
		{
			//single block, single team
			int processedBlocks = 0;
			foreach (var selectedTeamBlock in selectedTeamBlocks.GetRandom(selectedTeamBlocks.Count, true))
			{
				isSingleTeamSingleDay(selectedTeamBlock);

				var person = selectedTeamBlock.TeamInfo.GroupMembers.First();
				var date = selectedTeamBlock.BlockInfo.BlockPeriod.StartDate;
				var ruleSetBag = person.Period(date).RuleSetBag;

				processedBlocks++;
				var roleModel = _firstShiftInTeamBlockFinder.FindFirst(selectedTeamBlock, person, date, schedulingResultStateHolder.Schedules);
				CancelSignal progressResult;
				if(roleModel == null)
				{
					progressResult = onProgress(selectedTeamBlocks.Count, processedBlocks);
					if (progressResult.ShouldCancel) return;
					continue;
				}

				var scheduleDay = schedulingResultStateHolder.Schedules[person].ScheduledDay(date);

				if (_legalShiftDecider.IsLegalShift(ruleSetBag, roleModel, scheduleDay))
				{
					progressResult = onProgress(selectedTeamBlocks.Count, processedBlocks);
					if (progressResult.ShouldCancel) return;
					continue;
				}

				var success = _dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulingResultStateHolder.Schedules, person.VirtualSchedulePeriod(date), out int _, out IList<IScheduleDay> _);

				if (!success)
				{
					continue;
				}

				var scheduleMatrixPro = selectedTeamBlock.MatrixesForGroupAndBlock().First();
				var originalDay = scheduleMatrixPro.GetScheduleDayByKey(date).DaySchedulePart();
				var overTimeActivities = originalDay.PersonAssignment(true).OvertimeActivities();

				success = _backToLegalShiftWorker.ReSchedule(selectedTeamBlock, schedulingOptions, roleModel, rollbackService,
					resourceCalculateDelayer, schedulingResultStateHolder);

				if (success)
				{
					if (overTimeActivities.Any())
					{
						var scheduledDay = schedulingResultStateHolder.Schedules[person].ScheduledDay(date);

						foreach (var overtimeShiftLayer in overTimeActivities)
						{
							scheduledDay.CreateAndAddOvertime(overtimeShiftLayer.Payload, overtimeShiftLayer.Period,
								overtimeShiftLayer.DefinitionSet);
						}
						schedulingResultStateHolder.Schedules.Modify(scheduledDay, _scheduleDayChangeCallback);
						resourceCalculateDelayer.CalculateIfNeeded(date, null, false);
					}
				}

				progressResult = onProgress(selectedTeamBlocks.Count, processedBlocks);
				if (progressResult.ShouldCancel) return;
			}
		}

		private CancelSignal onProgress(int totalBlocks, int processedBlocks)
		{
			var handler = Progress;
			if (handler != null)
			{
				var args = new BackToLegalShiftArgs {TotalBlocks = totalBlocks, ProcessedBlocks = processedBlocks};
				handler(this, args);

				if (args.Cancel)
					return new CancelSignal{ShouldCancel = true};
			}
			return new CancelSignal();
		}

		private void isSingleTeamSingleDay(ITeamBlockInfo teamBlockInfo)
		{
			if(teamBlockInfo.TeamInfo.GroupMembers.Count() > 1 || teamBlockInfo.BlockInfo.BlockPeriod.DayCount()>1)
				throw new ArgumentException("Only single team, single block allowed");
		}
	}

	public class BackToLegalShiftArgs : CancelEventArgs
	{
		public int TotalBlocks { get; set; }
		public int ProcessedBlocks { get; set; }
	}
}