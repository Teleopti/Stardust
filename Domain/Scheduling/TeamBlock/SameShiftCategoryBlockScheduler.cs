using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ISameShiftCategoryBlockScheduler
	{
		bool Schedule(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly, ISchedulingOptions schedulingOptions,
									  DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, 
										ISchedulePartModifyAndRollbackService rollbackService,
										IResourceCalculateDelayer resourceCalculateDelayer,
										ISchedulingResultStateHolder schedulingResultStateHolder);

		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		void OnDayScheduled(object sender, SchedulingServiceBaseEventArgs e);
	}
	public class SameShiftCategoryBlockScheduler : ISameShiftCategoryBlockScheduler
	{
		private readonly ITeamBlockRoleModelSelector _roleModelSelector;
		private readonly ITeamBlockSingleDayScheduler _singleDayScheduler;
		private readonly ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private bool _cancelMe;
		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public SameShiftCategoryBlockScheduler(ITeamBlockRoleModelSelector roleModelSelector,
											   ITeamBlockSingleDayScheduler singleDayScheduler,
											   ITeamBlockSchedulingCompletionChecker teamBlockSchedulingCompletionChecker,
											   ITeamBlockClearer teamBlockClearer)
		{
			_roleModelSelector = roleModelSelector;
			_singleDayScheduler = singleDayScheduler;
			_teamBlockSchedulingCompletionChecker = teamBlockSchedulingCompletionChecker;
			_teamBlockClearer = teamBlockClearer;
		}

		public bool Schedule(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly, ISchedulingOptions schedulingOptions,
									  DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, 
								ISchedulePartModifyAndRollbackService rollbackService,
								IResourceCalculateDelayer resourceCalculateDelayer,
								ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_cancelMe = false;
			var allSelectedDaysAreScheduled = false;
			var selectedTeamMembers = selectedPersons.Intersect(teamBlockInfo.TeamInfo.GroupMembers).ToList();
			if (selectedTeamMembers.IsEmpty()) return true;
			while (!allSelectedDaysAreScheduled)
			{
				var roleModelShift = _roleModelSelector.Select(teamBlockInfo, dateOnly, selectedTeamMembers.First(), schedulingOptions);
				if (roleModelShift == null)
				{
					clearBlockedShiftCategories(schedulingOptions);
					OnDayScheduledFailed();
					return false;
				}
				var shiftCategoryToBeBlocked = roleModelShift.TheWorkShift.ShiftCategory;
				var selectedBlockDays = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().Where(x => selectedPeriod.DayCollection().Contains(x)).ToList();
				foreach (var day in selectedBlockDays)
				{
					if (_cancelMe)
					{
						clearBlockedShiftCategories(schedulingOptions);
						OnDayScheduledFailed();
						return false;
					}
					_singleDayScheduler.DayScheduled += OnDayScheduled;
					_singleDayScheduler.ScheduleSingleDay(teamBlockInfo, schedulingOptions, selectedPersons, day, roleModelShift,
					                                      selectedPeriod, rollbackService, resourceCalculateDelayer,
					                                      schedulingResultStateHolder);
					_singleDayScheduler.DayScheduled -= OnDayScheduled;
				}

				allSelectedDaysAreScheduled = selectedBlockDays.All(x => _teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons( teamBlockInfo, x,selectedPersons));
				if (!allSelectedDaysAreScheduled)
				{
					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
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

		public void OnDayScheduledFailed()
		{
			var temp = DayScheduled;
			if (temp != null)
			{
				temp(this, new SchedulingServiceFailedEventArgs());
			}
		}
	}

}
