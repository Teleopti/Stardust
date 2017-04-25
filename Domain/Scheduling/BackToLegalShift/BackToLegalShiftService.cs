using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Scheduling.BackToLegalShift
{
	public interface IBackToLegalShiftService
	{
		void Execute(IList<ITeamBlockInfo> selectedTeamBlocks, SchedulingOptions schedulingOptions,
			ISchedulingResultStateHolder schedulingResultStateHolder, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer);

		event EventHandler<BackToLegalShiftArgs> Progress;
	}

	public class BackToLegalShiftService : IBackToLegalShiftService
	{
		private readonly IBackToLegalShiftWorker _backToLegalShiftWorker;
		private readonly IFirstShiftInTeamBlockFinder _firstShiftInTeamBlockFinder;
		private readonly ILegalShiftDecider _legalShiftDecider;
		private readonly IWorkShiftFinderResultHolder _workShiftFinderResultHolder;
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public BackToLegalShiftService(IBackToLegalShiftWorker backToLegalShiftWorker,
			IFirstShiftInTeamBlockFinder firstShiftInTeamBlockFinder, ILegalShiftDecider legalShiftDecider,
			IWorkShiftFinderResultHolder workShiftFinderResultHolder, IDayOffsInPeriodCalculator dayOffsInPeriodCalculator,
			IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_backToLegalShiftWorker = backToLegalShiftWorker;
			_firstShiftInTeamBlockFinder = firstShiftInTeamBlockFinder;
			_legalShiftDecider = legalShiftDecider;
			_workShiftFinderResultHolder = workShiftFinderResultHolder;
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
			_workShiftFinderResultHolder.Clear();
			_workShiftFinderResultHolder.AlwaysShowTroubleshoot = true;
			var workShiftFinderResultList = new List<IWorkShiftFinderResult>();
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

				int targetDaysOff;
				IList<IScheduleDay> daysOffNow;
				var success = _dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(person.VirtualSchedulePeriod(date),
					out targetDaysOff, out daysOffNow);

				if (!success)
				{
					var workShiftFinderResult = new WorkShiftFinderResult(person, date);
					workShiftFinderResult.AddFilterResults(new WorkShiftFilterResult(string.Format(CultureInfo.InvariantCulture, Resources.WrongNumberOfDayOffsInSchedulePeriod, targetDaysOff, daysOffNow.Count), 0, 0));
					workShiftFinderResultList.Add(workShiftFinderResult);
					continue;
				}

				var scheduleMatrixPro = selectedTeamBlock.MatrixesForGroupAndBlock().First();
				var originalDay = scheduleMatrixPro.GetScheduleDayByKey(date).DaySchedulePart();
				var overTimeActivities = originalDay.PersonAssignment(true).OvertimeActivities();

				success = _backToLegalShiftWorker.ReSchedule(selectedTeamBlock, schedulingOptions, roleModel, rollbackService,
					resourceCalculateDelayer, schedulingResultStateHolder);

				if (!success)
				{
					var workShiftFinderResult = new WorkShiftFinderResult(person, date);
					workShiftFinderResult.AddFilterResults(new WorkShiftFilterResult(Resources.CouldNotFindAnyValidShiftInShiftBag, 0, 0));
					workShiftFinderResultList.Add(workShiftFinderResult);
				}
				else
				{
					if (overTimeActivities.Any())
					{
						var scheduledDay = schedulingResultStateHolder.Schedules[person].ScheduledDay(date);

						foreach (var overtimeShiftLayer in overTimeActivities)
						{
							scheduledDay.CreateAndAddOvertime(overtimeShiftLayer.Payload, overtimeShiftLayer.Period, overtimeShiftLayer.DefinitionSet);
						}
						schedulingResultStateHolder.Schedules.Modify(scheduledDay, _scheduleDayChangeCallback);
						resourceCalculateDelayer.CalculateIfNeeded(date, null, false);
					}
				}

				progressResult = onProgress(selectedTeamBlocks.Count, processedBlocks);
				if (progressResult.ShouldCancel) return;
			}
			_workShiftFinderResultHolder.AddResults(workShiftFinderResultList, DateTime.Now);
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