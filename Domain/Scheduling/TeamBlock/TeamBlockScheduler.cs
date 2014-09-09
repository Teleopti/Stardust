﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		bool ScheduleTeamBlockDay(ITeamBlockInfo teamBlockInfo, 
								DateOnly datePointer, 
								ISchedulingOptions schedulingOptions, 
								ISchedulePartModifyAndRollbackService rollbackService, 
								IResourceCalculateDelayer resourceCalculateDelayer,
								ISchedulingResultStateHolder schedulingResultStateHolder,
								ShiftNudgeDirective shiftNudgeDirective);

		void OnDayScheduled(object sender, SchedulingServiceBaseEventArgs e);
	}

	public class TeamBlockScheduler : ITeamBlockScheduler
	{
		private readonly ITeamBlockSingleDayScheduler _singleDayScheduler;
		private readonly ITeamBlockRoleModelSelector _roleModelSelector;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly bool _isMaxSeatToggleEnabled;
		private bool _cancelMe;

		public TeamBlockScheduler(ITeamBlockSingleDayScheduler singleDayScheduler,
		                          ITeamBlockRoleModelSelector roleModelSelector,
									ITeamBlockClearer teamBlockClearer, 
									ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
									//remove this - instead use two different impl of (a smaller interface of) ITeamBlockScheduler
									bool isMaxSeatToggleEnabled)
		{
			_singleDayScheduler = singleDayScheduler;
			_roleModelSelector = roleModelSelector;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_isMaxSeatToggleEnabled = isMaxSeatToggleEnabled;
		}

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public bool ScheduleTeamBlockDay(ITeamBlockInfo teamBlockInfo, 
			DateOnly datePointer,
			ISchedulingOptions schedulingOptions,
			ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			ShiftNudgeDirective shiftNudgeDirective)

		{
			_cancelMe = false;
			var teamInfo = teamBlockInfo.TeamInfo;
			var selectedTeamMembers = teamInfo.GroupMembers.Intersect(teamInfo.UnLockedMembers()).ToList();
			if (selectedTeamMembers.IsEmpty())
				return true;
			//var isMaxSeatToggleEnabled = _toggleManager.IsEnabled(Toggles.Scheduler_TeamBlockAdhereWithMaxSeatRule_23419);
			IShiftProjectionCache roleModelShift = _roleModelSelector.Select(teamBlockInfo, datePointer, selectedTeamMembers.First(),
				schedulingOptions, shiftNudgeDirective.EffectiveRestriction, _isMaxSeatToggleEnabled);

			if (roleModelShift == null)
			{
				OnDayScheduledFailed();
				return false;
			}

			var selectedBlockDays = teamBlockInfo.BlockInfo.UnLockedDates();
			bool success = tryScheduleBlock(teamBlockInfo, schedulingOptions, selectedBlockDays,
				roleModelShift, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, shiftNudgeDirective, _isMaxSeatToggleEnabled);

			if (!success && _teamBlockSchedulingOptions.IsBlockWithSameShiftCategoryInvolved(schedulingOptions))
			{
				schedulingOptions.NotAllowedShiftCategories.Clear();
				while (roleModelShift != null && !success)
				{
						if(_cancelMe)
							break;

					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
					schedulingOptions.NotAllowedShiftCategories.Add(roleModelShift.TheMainShift.ShiftCategory);
					roleModelShift = _roleModelSelector.Select(teamBlockInfo, datePointer, selectedTeamMembers.First(),
						schedulingOptions, shiftNudgeDirective.EffectiveRestriction, _isMaxSeatToggleEnabled);
					success = tryScheduleBlock(teamBlockInfo, schedulingOptions, selectedBlockDays,
						roleModelShift, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, shiftNudgeDirective, _isMaxSeatToggleEnabled);
				}
				schedulingOptions.NotAllowedShiftCategories.Clear();	
			}

			return success;


		}

		private bool tryScheduleBlock(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IList<DateOnly> selectedBlockDays, IShiftProjectionCache roleModelShift, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder, ShiftNudgeDirective shiftNudgeDirective, bool isMaxSeatToggleEnabled)
		{
			var lastIndex = selectedBlockDays.Count - 1;
			IEffectiveRestriction shiftNudgeRestriction = new EffectiveRestriction();
			if (shiftNudgeDirective.Direction == ShiftNudgeDirective.NudgeDirection.Right)
				shiftNudgeRestriction = shiftNudgeDirective.EffectiveRestriction;

			for (int dayIndex = 0; dayIndex <= lastIndex; dayIndex++)
			{
				var day = selectedBlockDays[dayIndex];
				if (_cancelMe)
					return false;

				if (shiftNudgeDirective.Direction == ShiftNudgeDirective.NudgeDirection.Left && dayIndex == lastIndex)
					shiftNudgeRestriction = shiftNudgeDirective.EffectiveRestriction;
				
				_singleDayScheduler.DayScheduled += OnDayScheduled;
				bool successful = _singleDayScheduler.ScheduleSingleDay(teamBlockInfo, schedulingOptions, day,
					roleModelShift, rollbackService,
					resourceCalculateDelayer, schedulingResultStateHolder, shiftNudgeRestriction,isMaxSeatToggleEnabled );
				_singleDayScheduler.DayScheduled -= OnDayScheduled;

				if(shiftNudgeDirective.Direction == ShiftNudgeDirective.NudgeDirection.Right && dayIndex == 0)
					shiftNudgeRestriction = new EffectiveRestriction();

				if (!successful)
				{
					OnDayScheduledFailed();
					return false;
				}
			}
			
			return true;
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
			var args = new SchedulingServiceFailedEventArgs();
			var temp = DayScheduled;
			if (temp != null)
			{
				temp(this, args);
			}
			_cancelMe = args.Cancel;
		}
	}
}