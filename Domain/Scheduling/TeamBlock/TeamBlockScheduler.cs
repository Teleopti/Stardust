using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class TeamBlockScheduler
	{
		private readonly TeamBlockSingleDayScheduler _singleDayScheduler;
		private readonly TeamBlockRoleModelSelector _roleModelSelector;
		private readonly TeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly IDaysOffInPeriodValidatorForBlock _daysOffsInPeriodCalculator;

		public TeamBlockScheduler(TeamBlockSingleDayScheduler singleDayScheduler,
									TeamBlockRoleModelSelector roleModelSelector,
									TeamBlockClearer teamBlockClearer, 
									ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
									IGroupPersonSkillAggregator groupPersonSkillAggregator,
			IDaysOffInPeriodValidatorForBlock daysOffsInPeriodCalculator)
		{
			_singleDayScheduler = singleDayScheduler;
			_roleModelSelector = roleModelSelector;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_daysOffsInPeriodCalculator = daysOffsInPeriodCalculator;
		}

		public bool ScheduleTeamBlockDay(IEnumerable<IPersonAssignment> orginalPersonAssignments,
			ISchedulingCallback schedulingCallback, 
			IWorkShiftSelector workShiftSelector,
			ITeamBlockInfo teamBlockInfo, 
			DateOnly datePointer,
			SchedulingOptions schedulingOptions,
			ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays,
			IScheduleDictionary schedules,
			ResourceCalculationData resourceCalculationData,
			ShiftNudgeDirective shiftNudgeDirective,
			INewBusinessRuleCollection businessRules, 
			IGroupPersonSkillAggregator groupPersonSkillAggregator)

		{
			var teamInfo = teamBlockInfo.TeamInfo;
			var selectedTeamMembers = teamInfo.GroupMembers.Intersect(teamInfo.UnLockedMembers(datePointer)).ToList();
			if (selectedTeamMembers.IsEmpty())
				return true;
			var allSkillDays = skillDays.ToSkillDayEnumerable();
			ShiftProjectionCache roleModelShift = _roleModelSelector.Select(schedules, allSkillDays, workShiftSelector, teamBlockInfo, datePointer, selectedTeamMembers.First(),
				schedulingOptions, shiftNudgeDirective.EffectiveRestriction, groupPersonSkillAggregator);

			if (roleModelShift == null)
			{
				schedulingCallback.Scheduled(new SchedulingCallbackInfo(null, false));
				return false;
			}
			var selectedBlockDays = teamBlockInfo.BlockInfo.UnLockedDates();
			bool success = tryScheduleBlock(orginalPersonAssignments, schedulingCallback, workShiftSelector, teamBlockInfo, schedulingOptions, selectedBlockDays, resourceCalculationData,
				roleModelShift, rollbackService, skillDays, schedules, shiftNudgeDirective, businessRules);

			if (!success && _teamBlockSchedulingOptions.IsBlockWithSameShiftCategoryInvolved(schedulingOptions))
			{
				schedulingOptions.NotAllowedShiftCategories.Clear();
				while (roleModelShift != null && !success)
				{
					if(schedulingCallback.IsCancelled)
						break;

					if (isBlockSchedulePeriodsUnschedableDueToMissingDaysOff(teamBlockInfo, schedules, teamInfo))
					{
						success = true;
						break;
					}

					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
					schedulingOptions.NotAllowedShiftCategories.Add(roleModelShift.TheWorkShift.ShiftCategory);
					roleModelShift = _roleModelSelector.Select(schedules, allSkillDays, workShiftSelector, teamBlockInfo, datePointer, selectedTeamMembers.First(),
						schedulingOptions, shiftNudgeDirective.EffectiveRestriction, _groupPersonSkillAggregator);
					success = tryScheduleBlock(orginalPersonAssignments, schedulingCallback, workShiftSelector, teamBlockInfo, schedulingOptions, selectedBlockDays, resourceCalculationData,
						roleModelShift, rollbackService, skillDays, schedules, shiftNudgeDirective, businessRules);
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
				if (isBlockSchedulePeriodsUnschedableDueToMissingDaysOff(teamBlockInfo, schedules, teamInfo))
				{
					return true;
				}

				_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
				roleModelShift = _roleModelSelector.Select(schedules, allSkillDays, workShiftSelector, teamBlockInfo, datePointer, selectedTeamMembers.First(),schedulingOptions, shiftNudgeDirective.EffectiveRestriction, _groupPersonSkillAggregator);
				success = tryScheduleBlock(orginalPersonAssignments, schedulingCallback, workShiftSelector, teamBlockInfo, schedulingOptions, selectedBlockDays, resourceCalculationData, roleModelShift, rollbackService, skillDays, schedules, shiftNudgeDirective, businessRules);

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

		private bool isBlockSchedulePeriodsUnschedableDueToMissingDaysOff(ITeamBlockInfo teamBlockInfo, IScheduleDictionary schedules,
			ITeamInfo teamInfo)
		{
			var involvedMatrixes = teamBlockInfo.TeamInfo.MatrixesForMemberAndPeriod(teamInfo.GroupMembers.First(),
				teamBlockInfo.BlockInfo.BlockPeriod);
			if (involvedMatrixes.Count() > 1)
			{
				foreach (var matrix in involvedMatrixes)
				{
					if (!_daysOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedules, matrix.SchedulePeriod))
						return true;
				}
			}

			return false;
		}

		private bool tryScheduleBlock(IEnumerable<IPersonAssignment> orginalPersonAssignments, ISchedulingCallback schedulingCallback, IWorkShiftSelector workShiftSelector, 
			ITeamBlockInfo teamBlockInfo, SchedulingOptions schedulingOptions, IList<DateOnly> selectedBlockDays, ResourceCalculationData resourceCalculationData,
			ShiftProjectionCache roleModelShift, ISchedulePartModifyAndRollbackService rollbackService, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, IScheduleDictionary schedules, ShiftNudgeDirective shiftNudgeDirective, INewBusinessRuleCollection businessRules)
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

				var successful = _singleDayScheduler.ScheduleSingleDay(orginalPersonAssignments, workShiftSelector, teamBlockInfo, schedulingOptions, day,
					roleModelShift, rollbackService,
					skillDays, schedules, resourceCalculationData, shiftNudgeRestriction, businessRules, e =>
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