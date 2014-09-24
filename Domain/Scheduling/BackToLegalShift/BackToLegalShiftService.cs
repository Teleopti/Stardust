using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.BackToLegalShift
{
	public interface IBackToLegalShiftService
	{
		void Execute(IList<ITeamBlockInfo> selectedTeamBlocks, ISchedulingOptions schedulingOptions,
			ISchedulingResultStateHolder schedulingResultStateHolder, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, bool isMaxSeatToggleEnabled);

		event EventHandler<BackToLegalShiftArgs> Progress;
	}

	public class BackToLegalShiftService : IBackToLegalShiftService
	{
		private readonly IBackToLegalShiftWorker _backToLegalShiftWorker;
		private readonly IFirstShiftInTeamBlockFinder _firstShiftInTeamBlockFinder;
		private readonly ILegalShiftDecider _legalShiftDecider;
		private readonly IWorkShiftFinderResultHolder _workShiftFinderResultHolder;
		private CancelEventArgs _cancelEvent;
		private bool _cancelMe;

		public BackToLegalShiftService(IBackToLegalShiftWorker backToLegalShiftWorker, IFirstShiftInTeamBlockFinder firstShiftInTeamBlockFinder, ILegalShiftDecider legalShiftDecider, IWorkShiftFinderResultHolder workShiftFinderResultHolder)
		{
			_backToLegalShiftWorker = backToLegalShiftWorker;
			_firstShiftInTeamBlockFinder = firstShiftInTeamBlockFinder;
			_legalShiftDecider = legalShiftDecider;
			_workShiftFinderResultHolder = workShiftFinderResultHolder;
		}

		public event EventHandler<BackToLegalShiftArgs> Progress;

		public void Execute(IList<ITeamBlockInfo> selectedTeamBlocks, ISchedulingOptions schedulingOptions,
			ISchedulingResultStateHolder schedulingResultStateHolder, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, bool isMaxSeatToggleEnabled)
		{
			//single block, single team
			int processedBlocks = 0;
			_workShiftFinderResultHolder.Clear();
			_workShiftFinderResultHolder.AlwaysShowTroubleshoot = true;
			foreach (var selectedTeamBlock in selectedTeamBlocks.GetRandom(selectedTeamBlocks.Count, true))
			{
				if (_cancelMe)
					return;

				isSingleTeamSingleDay(selectedTeamBlock);

				var person = selectedTeamBlock.TeamInfo.GroupMembers.First();
				var date = selectedTeamBlock.BlockInfo.BlockPeriod.StartDate;
				var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
				var ruleSetBag = person.Period(date).RuleSetBag;
				processedBlocks++;
				var roleModel = _firstShiftInTeamBlockFinder.FindFirst(selectedTeamBlock, person, date, schedulingResultStateHolder);
				if(roleModel == null)
				{
					OnProgress(selectedTeamBlocks.Count, processedBlocks);
					continue;
				}

				if (_legalShiftDecider.IsLegalShift(date, timeZoneInfo, ruleSetBag, roleModel))
				{
					OnProgress(selectedTeamBlocks.Count, processedBlocks);
					continue;
				}

				var success = _backToLegalShiftWorker.ReSchedule(selectedTeamBlock, schedulingOptions, roleModel, rollbackService,
					resourceCalculateDelayer, schedulingResultStateHolder, isMaxSeatToggleEnabled);

				if (!success)
				{
					var workShiftFinderResult = new WorkShiftFinderResult(person, date);
					workShiftFinderResult.AddFilterResults(new WorkShiftFilterResult(Resources.CouldNotFindAnyValidShiftInShiftBag, 0, 0));
					_workShiftFinderResultHolder.AddResults(new List<IWorkShiftFinderResult> { workShiftFinderResult }, DateTime.Now);
				}
				
				OnProgress(selectedTeamBlocks.Count, processedBlocks);
			}
		}

		protected void OnProgress(int totalBlocks, int processedBlocks )
		{
			var tmp = Progress;
			if (tmp != null)
			{
				var args = new BackToLegalShiftArgs {TotalBlocks = totalBlocks, ProcessedBlocks = processedBlocks};
				tmp(this, args);

				if (_cancelEvent != null && _cancelEvent.Cancel)
					_cancelMe = true;

				_cancelEvent = args;

				if (_cancelEvent != null && _cancelEvent.Cancel)
					_cancelMe = true;
			}
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