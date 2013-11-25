using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ISameShiftCategoryBlockScheduler
	{
		bool Schedule(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly, ISchedulingOptions schedulingOptions,
		                              DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons);

		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		void OnDayScheduled(object sender, SchedulingServiceBaseEventArgs e);
	}
	public class SameShiftCategoryBlockScheduler : ISameShiftCategoryBlockScheduler
	{
		private readonly ITeamBlockRoleModelSelector _roleModelSelector;
		private readonly ITeamBlockSingleDayScheduler _singleDayScheduler;
		private readonly ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ISchedulePartModifyAndRollbackService _rollbackService;
		private bool _cancelMe;
		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public SameShiftCategoryBlockScheduler(ITeamBlockRoleModelSelector roleModelSelector,
											   ITeamBlockSingleDayScheduler singleDayScheduler,
											   ITeamBlockSchedulingCompletionChecker teamBlockSchedulingCompletionChecker,
											   ITeamBlockClearer teamBlockClearer,
											   ISchedulePartModifyAndRollbackService rollbackService)
		{
			_roleModelSelector = roleModelSelector;
			_singleDayScheduler = singleDayScheduler;
			_teamBlockSchedulingCompletionChecker = teamBlockSchedulingCompletionChecker;
			_teamBlockClearer = teamBlockClearer;
			_rollbackService = rollbackService;
		}

		public bool Schedule(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly, ISchedulingOptions schedulingOptions,
									  DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
		{
			var allSelectedDaysAreScheduled = false;
			while (!allSelectedDaysAreScheduled)
			{
				var roleModelShift = _roleModelSelector.Select(teamBlockInfo, dateOnly, selectedPersons.First(),  schedulingOptions);
				if (roleModelShift == null)
				{
					clearBlockedShiftCategories(schedulingOptions);
					return false;
				}
				var shiftCategoryToBeBlocked = roleModelShift.TheWorkShift.ShiftCategory;
				var selectedBlockDays = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().Where(x => selectedPeriod.DayCollection().Contains(x)).ToList();
				foreach (var day in selectedBlockDays)
				{
					if (_cancelMe)
					{
						clearBlockedShiftCategories(schedulingOptions);
						return false;
					}
					_singleDayScheduler.DayScheduled += OnDayScheduled;
					_singleDayScheduler.ScheduleSingleDay(teamBlockInfo, schedulingOptions, selectedPersons, day, roleModelShift, selectedPeriod);
					_singleDayScheduler.DayScheduled -= OnDayScheduled;
				}

				allSelectedDaysAreScheduled = selectedBlockDays.All(x => _teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons( teamBlockInfo, x,selectedPersons));
				if (!allSelectedDaysAreScheduled)
				{
					_teamBlockClearer.ClearTeamBlock(schedulingOptions, _rollbackService, teamBlockInfo);
					blockAShiftCategory(schedulingOptions, shiftCategoryToBeBlocked);
				}
			}
			clearBlockedShiftCategories(schedulingOptions);
			return true;
		}

		private static void clearBlockedShiftCategories(ISchedulingOptions schedulingOptions)
		{
			schedulingOptions.NotAllowedShiftCategories.Clear();
		}

		private static void blockAShiftCategory(ISchedulingOptions schedulingOptions, IShiftCategory shiftCategoryToBeBlocked)
		{
			if (!schedulingOptions.NotAllowedShiftCategories.Contains(shiftCategoryToBeBlocked))
				schedulingOptions.NotAllowedShiftCategories.Add(shiftCategoryToBeBlocked);
		}

		public void OnDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, e);
			}
			_cancelMe = e.Cancel;
		}
	}

}
