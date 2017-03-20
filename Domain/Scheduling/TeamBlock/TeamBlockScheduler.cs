using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		bool ScheduleTeamBlockDay(IWorkShiftSelector workShiftSelector,
								ITeamBlockInfo teamBlockInfo, 
								DateOnly datePointer, 
								ISchedulingOptions schedulingOptions, 
								ISchedulePartModifyAndRollbackService rollbackService, 
								IResourceCalculateDelayer resourceCalculateDelayer,
								IEnumerable<ISkillDay> allSkillDays,
								IScheduleDictionary schedules,
								ShiftNudgeDirective shiftNudgeDirective,
								INewBusinessRuleCollection businessRules,
								IGroupPersonSkillAggregator groupPersonSkillAggregator);
	}

	public class TeamBlockScheduler : ITeamBlockScheduler
	{
		private readonly ITeamBlockSingleDayScheduler _singleDayScheduler;
		private readonly TeamBlockRoleModelSelector _roleModelSelector;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

		public TeamBlockScheduler(ITeamBlockSingleDayScheduler singleDayScheduler,
															TeamBlockRoleModelSelector roleModelSelector,
									ITeamBlockClearer teamBlockClearer, 
									ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
									IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			_singleDayScheduler = singleDayScheduler;
			_roleModelSelector = roleModelSelector;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
		}

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public bool ScheduleTeamBlockDay(IWorkShiftSelector workShiftSelector,
			ITeamBlockInfo teamBlockInfo, 
			DateOnly datePointer,
			ISchedulingOptions schedulingOptions,
			ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IEnumerable<ISkillDay> allSkillDays,
			IScheduleDictionary schedules,
			ShiftNudgeDirective shiftNudgeDirective,
			INewBusinessRuleCollection businessRules, IGroupPersonSkillAggregator groupPersonSkillAggregator)

		{
			var teamInfo = teamBlockInfo.TeamInfo;
			var selectedTeamMembers = teamInfo.GroupMembers.Intersect(teamInfo.UnLockedMembers(datePointer)).ToList();
			if (selectedTeamMembers.IsEmpty())
				return true;
			IShiftProjectionCache roleModelShift = _roleModelSelector.Select(schedules, allSkillDays, workShiftSelector, teamBlockInfo, datePointer, selectedTeamMembers.First(),
				schedulingOptions, shiftNudgeDirective.EffectiveRestriction, groupPersonSkillAggregator);

			var cancelMe = false;
			if (roleModelShift == null)
			{
				onDayScheduledFailed(new SchedulingServiceFailedEventArgs(()=>cancelMe=true));
				return false;
			}
			var selectedBlockDays = teamBlockInfo.BlockInfo.UnLockedDates();
			bool success = tryScheduleBlock(workShiftSelector, teamBlockInfo, schedulingOptions, selectedBlockDays,
				roleModelShift, rollbackService, resourceCalculateDelayer, allSkillDays, schedules, shiftNudgeDirective, businessRules, () =>
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
					roleModelShift = _roleModelSelector.Select(schedules, allSkillDays, workShiftSelector, teamBlockInfo, datePointer, selectedTeamMembers.First(),
						schedulingOptions, shiftNudgeDirective.EffectiveRestriction, _groupPersonSkillAggregator);
					success = tryScheduleBlock(workShiftSelector, teamBlockInfo, schedulingOptions, selectedBlockDays,
						roleModelShift, rollbackService, resourceCalculateDelayer, allSkillDays, schedules, shiftNudgeDirective, businessRules, ()=>
						{
							cancelMe = true;
						});
				}

				if (!success)
				{
					rollbackService.RollbackMinimumChecks();

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
				roleModelShift = _roleModelSelector.Select(schedules, allSkillDays, workShiftSelector, teamBlockInfo, datePointer, selectedTeamMembers.First(),schedulingOptions, shiftNudgeDirective.EffectiveRestriction, _groupPersonSkillAggregator);		
				success = tryScheduleBlock(workShiftSelector, teamBlockInfo, schedulingOptions, selectedBlockDays, roleModelShift, rollbackService, resourceCalculateDelayer, allSkillDays, schedules, shiftNudgeDirective, businessRules, ()=>
				{
					cancelMe = true;
				});

				if (!success)
				{
					rollbackService.RollbackMinimumChecks();

					foreach (var selectedBlockDay in selectedBlockDays)
					{
						resourceCalculateDelayer.CalculateIfNeeded(selectedBlockDay, null, false);
					}	
				}
			}

			return success;
		}

		private bool tryScheduleBlock(IWorkShiftSelector workShiftSelector, ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IList<DateOnly> selectedBlockDays, IShiftProjectionCache roleModelShift, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, IEnumerable<ISkillDay> allSkillDays, IScheduleDictionary schedules, ShiftNudgeDirective shiftNudgeDirective, INewBusinessRuleCollection businessRules, Action cancelAction)
		{
			var lastIndex = selectedBlockDays.Count - 1;
			IEffectiveRestriction shiftNudgeRestriction = new EffectiveRestriction();
			if (shiftNudgeDirective.Direction == ShiftNudgeDirective.NudgeDirection.Right)
				shiftNudgeRestriction = shiftNudgeDirective.EffectiveRestriction;

			for (int dayIndex = 0; dayIndex <= lastIndex; dayIndex++)
			{
				var day = selectedBlockDays[dayIndex];

				if (shiftNudgeDirective.Direction == ShiftNudgeDirective.NudgeDirection.Left && dayIndex == lastIndex)
					shiftNudgeRestriction = shiftNudgeDirective.EffectiveRestriction;

				var successful = _singleDayScheduler.ScheduleSingleDay(workShiftSelector, teamBlockInfo, schedulingOptions, day,
					roleModelShift, rollbackService,
					resourceCalculateDelayer, allSkillDays, schedules, shiftNudgeRestriction, businessRules, (e) =>
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