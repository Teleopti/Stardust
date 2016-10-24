﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
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
								IEnumerable<ISkillDay> allSkillDays,
								ShiftNudgeDirective shiftNudgeDirective,
								INewBusinessRuleCollection businessRules);
	}

	public class TeamBlockScheduler : ITeamBlockScheduler
	{
		private readonly ITeamBlockSingleDayScheduler _singleDayScheduler;
		private readonly ITeamBlockRoleModelSelector _roleModelSelector;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;

		public TeamBlockScheduler(ITeamBlockSingleDayScheduler singleDayScheduler,
		                          ITeamBlockRoleModelSelector roleModelSelector,
									ITeamBlockClearer teamBlockClearer, 
									ITeamBlockSchedulingOptions teamBlockSchedulingOptions)
		{
			_singleDayScheduler = singleDayScheduler;
			_roleModelSelector = roleModelSelector;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
		}

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public bool ScheduleTeamBlockDay(ITeamBlockInfo teamBlockInfo, 
			DateOnly datePointer,
			ISchedulingOptions schedulingOptions,
			ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IEnumerable<ISkillDay> allSkillDays,
			ShiftNudgeDirective shiftNudgeDirective,
			INewBusinessRuleCollection businessRules)

		{
			var teamInfo = teamBlockInfo.TeamInfo;
			var selectedTeamMembers = teamInfo.GroupMembers.Intersect(teamInfo.UnLockedMembers(datePointer)).ToList();
			if (selectedTeamMembers.IsEmpty())
				return true;
			IShiftProjectionCache roleModelShift = _roleModelSelector.Select(teamBlockInfo, datePointer, selectedTeamMembers.First(),
				schedulingOptions, shiftNudgeDirective.EffectiveRestriction);

			var cancelMe = false;
			if (roleModelShift == null)
			{
				onDayScheduledFailed(new SchedulingServiceFailedEventArgs(()=>cancelMe=true));
				return false;
			}
			var selectedBlockDays = teamBlockInfo.BlockInfo.UnLockedDates();
			bool success = tryScheduleBlock(teamBlockInfo, schedulingOptions, selectedBlockDays,
				roleModelShift, rollbackService, resourceCalculateDelayer, allSkillDays, shiftNudgeDirective, businessRules, () =>
				{
					cancelMe = true;
				});

			if (!success && _teamBlockSchedulingOptions.IsBlockWithSameShiftCategoryInvolved(schedulingOptions))
			{
				schedulingOptions.NotAllowedShiftCategories.Clear();
				while (roleModelShift != null && !success)
				{
					if(cancelMe)
						break;

					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
					schedulingOptions.NotAllowedShiftCategories.Add(roleModelShift.TheMainShift.ShiftCategory);
					roleModelShift = _roleModelSelector.Select(teamBlockInfo, datePointer, selectedTeamMembers.First(),
						schedulingOptions, shiftNudgeDirective.EffectiveRestriction);
					success = tryScheduleBlock(teamBlockInfo, schedulingOptions, selectedBlockDays,
						roleModelShift, rollbackService, resourceCalculateDelayer, allSkillDays, shiftNudgeDirective, businessRules, ()=>
						{
							cancelMe = true;
						});
				}

				if (!success)
				{
					rollbackService.Rollback();

					foreach (var selectedBlockDay in selectedBlockDays)
					{
						resourceCalculateDelayer.CalculateIfNeeded(selectedBlockDay, null, false);
					}		
				}
		
				schedulingOptions.NotAllowedShiftCategories.Clear();	
			}

			if (!success && _teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(schedulingOptions))
			{
				_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
				roleModelShift = _roleModelSelector.Select(teamBlockInfo, datePointer, selectedTeamMembers.First(),schedulingOptions, shiftNudgeDirective.EffectiveRestriction);		
				success = tryScheduleBlock(teamBlockInfo, schedulingOptions, selectedBlockDays, roleModelShift, rollbackService, resourceCalculateDelayer, allSkillDays, shiftNudgeDirective, businessRules, ()=>
				{
					cancelMe = true;
				});

				if (!success)
				{
					rollbackService.Rollback();

					foreach (var selectedBlockDay in selectedBlockDays)
					{
						resourceCalculateDelayer.CalculateIfNeeded(selectedBlockDay, null, false);
					}	
				}
			}

			return success;
		}

		private bool tryScheduleBlock(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IList<DateOnly> selectedBlockDays, IShiftProjectionCache roleModelShift, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, IEnumerable<ISkillDay> allSkillDays, ShiftNudgeDirective shiftNudgeDirective, INewBusinessRuleCollection businessRules, Action cancelAction)
		{
			var lastIndex = selectedBlockDays.Count - 1;
			IEffectiveRestriction shiftNudgeRestriction = new EffectiveRestriction();
			if (shiftNudgeDirective.Direction == ShiftNudgeDirective.NudgeDirection.Right)
				shiftNudgeRestriction = shiftNudgeDirective.EffectiveRestriction;

			var cancelMe = false;
			for (int dayIndex = 0; dayIndex <= lastIndex; dayIndex++)
			{
				var day = selectedBlockDays[dayIndex];
				if (cancelMe)
					return false;

				if (shiftNudgeDirective.Direction == ShiftNudgeDirective.NudgeDirection.Left && dayIndex == lastIndex)
					shiftNudgeRestriction = shiftNudgeDirective.EffectiveRestriction;

				var successful = _singleDayScheduler.ScheduleSingleDay(teamBlockInfo, schedulingOptions, day,
					roleModelShift, rollbackService,
					resourceCalculateDelayer, allSkillDays, shiftNudgeRestriction, businessRules, (e) =>
					{
						if (DayScheduled != null)
						{
							DayScheduled(this, e);
							if (e.Cancel) return true;
						}
						return false;
					});

				if(shiftNudgeDirective.Direction == ShiftNudgeDirective.NudgeDirection.Right && dayIndex == 0)
					shiftNudgeRestriction = new EffectiveRestriction();

				if (!successful)
				{
					var progressResult = onDayScheduledFailed(new SchedulingServiceFailedEventArgs(cancelAction));
					if (progressResult.ShouldCancel) cancelAction();
					return false;
				}
			}
			
			return true;
		}

		private CancelSignal onDayScheduledFailed(SchedulingServiceBaseEventArgs args)
		{
			var handler = DayScheduled;
			if (handler != null)
			{
				handler(this, args);
				if (args.Cancel) return new CancelSignal {ShouldCancel = true};
			}
			return new CancelSignal();
		}
	}
}