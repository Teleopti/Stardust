using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockScheduler
	{
		bool ScheduleTeamBlockDay(ISchedulingCallback schedulingCallback, 
								IWorkShiftSelector workShiftSelector,
								ITeamBlockInfo teamBlockInfo, 
								DateOnly datePointer, 
								SchedulingOptions schedulingOptions, 
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

		public bool ScheduleTeamBlockDay(ISchedulingCallback schedulingCallback, 
			IWorkShiftSelector workShiftSelector,
			ITeamBlockInfo teamBlockInfo, 
			DateOnly datePointer,
			SchedulingOptions schedulingOptions,
			ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IEnumerable<ISkillDay> allSkillDays,
			IScheduleDictionary schedules,
			ShiftNudgeDirective shiftNudgeDirective,
			INewBusinessRuleCollection businessRules, 
			IGroupPersonSkillAggregator groupPersonSkillAggregator)

		{
			var teamInfo = teamBlockInfo.TeamInfo;
			var selectedTeamMembers = teamInfo.GroupMembers.Intersect(teamInfo.UnLockedMembers(datePointer)).ToList();
			if (selectedTeamMembers.IsEmpty())
				return true;

			ShiftProjectionCache roleModelShift = _roleModelSelector.Select(schedules, allSkillDays, workShiftSelector, teamBlockInfo, datePointer, selectedTeamMembers.First(),
				schedulingOptions, shiftNudgeDirective.EffectiveRestriction, groupPersonSkillAggregator);

			if (roleModelShift == null)
			{
				schedulingCallback.Scheduled(new SchedulingCallbackInfo(null, false));
				return false;
			}
			var selectedBlockDays = teamBlockInfo.BlockInfo.UnLockedDates();
			bool success = tryScheduleBlock(schedulingCallback, workShiftSelector, teamBlockInfo, schedulingOptions, selectedBlockDays,
				roleModelShift, rollbackService, resourceCalculateDelayer, allSkillDays, schedules, shiftNudgeDirective, businessRules);

			if (!success && _teamBlockSchedulingOptions.IsBlockWithSameShiftCategoryInvolved(schedulingOptions))
			{
				schedulingOptions.NotAllowedShiftCategories.Clear();
				while (roleModelShift != null && !success)
				{
					if(schedulingCallback.IsCancelled)
						break;

					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
					schedulingOptions.NotAllowedShiftCategories.Add(roleModelShift.TheMainShift.ShiftCategory);
					roleModelShift = _roleModelSelector.Select(schedules, allSkillDays, workShiftSelector, teamBlockInfo, datePointer, selectedTeamMembers.First(),
						schedulingOptions, shiftNudgeDirective.EffectiveRestriction, _groupPersonSkillAggregator);
					success = tryScheduleBlock(schedulingCallback, workShiftSelector, teamBlockInfo, schedulingOptions, selectedBlockDays,
						roleModelShift, rollbackService, resourceCalculateDelayer, allSkillDays, schedules, shiftNudgeDirective, businessRules);
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
				success = tryScheduleBlock(schedulingCallback, workShiftSelector, teamBlockInfo, schedulingOptions, selectedBlockDays, roleModelShift, rollbackService, resourceCalculateDelayer, allSkillDays, schedules, shiftNudgeDirective, businessRules);

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

		private bool tryScheduleBlock(ISchedulingCallback schedulingCallback, IWorkShiftSelector workShiftSelector, ITeamBlockInfo teamBlockInfo, SchedulingOptions schedulingOptions, IList<DateOnly> selectedBlockDays, ShiftProjectionCache roleModelShift, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, IEnumerable<ISkillDay> allSkillDays, IScheduleDictionary schedules, ShiftNudgeDirective shiftNudgeDirective, INewBusinessRuleCollection businessRules)
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
					resourceCalculateDelayer, allSkillDays, schedules, shiftNudgeRestriction, businessRules, e =>
					{
						schedulingCallback.Scheduled(new SchedulingCallbackInfo(e.SchedulePart, e.IsSuccessful));
						return schedulingCallback.IsCancelled;
					});

				if(shiftNudgeDirective.Direction == ShiftNudgeDirective.NudgeDirection.Right && dayIndex == 0)
					shiftNudgeRestriction = new EffectiveRestriction();

				if (!successful)
				{
					schedulingCallback.Scheduled(new SchedulingCallbackInfo(null, false));
					return false;
				}
			}
			
			return true;
		}
	}
}