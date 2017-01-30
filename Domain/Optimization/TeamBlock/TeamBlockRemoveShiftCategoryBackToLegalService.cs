using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_ShiftCategoryLimitations_42680)]
	public interface ITeamBlockRemoveShiftCategoryBackToLegalService
	{
		void Execute(ISchedulingOptions schedulingOptions, IScheduleMatrixPro scheduleMatrixPro, ISchedulingResultStateHolder schedulingResultStateHolder, IResourceCalculateDelayer resourceCalculateDelayer, IList<IScheduleMatrixPro> scheduleMaxtrixListPros, IOptimizationPreferences optimizationPreferences, IList<IScheduleMatrixPro> allScheduleMatrixListPros);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_ShiftCategoryLimitations_42680)]
	public class TeamBlockRemoveShiftCategoryBackToLegalService : ITeamBlockRemoveShiftCategoryBackToLegalService
	{
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly ShiftCategoryWeekRemover _shiftCategoryWeekRemover;
		private readonly ShiftCategoryPeriodRemover _shiftCategoryPeriodRemover;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly IShiftCategoryLimitCounter _shiftCategoryLimitCounter;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public TeamBlockRemoveShiftCategoryBackToLegalService(ITeamBlockScheduler teamBlockScheduler, 
			ITeamInfoFactory teamInfoFactory, 
			ITeamBlockInfoFactory teamBlockInfoFactory, 
			ITeamBlockClearer teamBlockClearer,  
			ITeamBlockSchedulingOptions teamBlockSchedulingOptions, 
			ShiftCategoryWeekRemover shiftCategoryWeekRemover, 
			ShiftCategoryPeriodRemover shiftCategoryPeriodRemover, 
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation, 
			IShiftCategoryLimitCounter shiftCategoryLimitCounter, 
			IWorkShiftSelector workShiftSelector, 
			IGroupPersonSkillAggregator groupPersonSkillAggregator, 
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, 
			IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_teamBlockScheduler = teamBlockScheduler;
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_shiftCategoryWeekRemover = shiftCategoryWeekRemover;
			_shiftCategoryPeriodRemover = shiftCategoryPeriodRemover;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_shiftCategoryLimitCounter = shiftCategoryLimitCounter;
			_workShiftSelector = workShiftSelector;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public void Execute(ISchedulingOptions schedulingOptions, IScheduleMatrixPro scheduleMatrixPro, ISchedulingResultStateHolder schedulingResultStateHolder, IResourceCalculateDelayer resourceCalculateDelayer, IList<IScheduleMatrixPro> scheduleMaxtrixListPros, IOptimizationPreferences optimizationPreferences, IList<IScheduleMatrixPro> allScheduleMatrixListPros)
		{
			var shiftNudgeDirective = new ShiftNudgeDirective();
			var rollbackService = new SchedulePartModifyAndRollbackService(schedulingResultStateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
			var removedScheduleDayPros = new List<IScheduleDayPro>();
			var isSingleAgentTeam = _teamBlockSchedulingOptions.IsSingleAgentTeam(schedulingOptions);
			var schedulePeriod = scheduleMatrixPro.SchedulePeriod;
			var person = scheduleMatrixPro.Person;
			
			foreach (var limitation in schedulePeriod.ShiftCategoryLimitationCollection())
			{
				removedScheduleDayPros.Clear();

				if (limitation.Weekly) removedScheduleDayPros.AddRange(_shiftCategoryWeekRemover.Remove(limitation, schedulingOptions, scheduleMatrixPro, optimizationPreferences, _schedulePartModifyAndRollbackService));
				else removedScheduleDayPros.AddRange(_shiftCategoryPeriodRemover.RemoveShiftCategoryOnPeriod(limitation, schedulingOptions, scheduleMatrixPro, optimizationPreferences, _schedulePartModifyAndRollbackService));

				foreach (var removedScheduleDayPro in removedScheduleDayPros)
				{
					rollbackService.ClearModificationCollection();

					if (removedScheduleDayPro.DaySchedulePart().IsScheduled()) continue;

					var dateOnly = removedScheduleDayPro.Day;
					var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
					var teamInfo = _teamInfoFactory.CreateTeamInfo(schedulingResultStateHolder.PersonsInOrganization, person, dateOnlyPeriod, scheduleMaxtrixListPros);
					var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinder(), isSingleAgentTeam);
					if (teamBlockInfo == null) continue;

					schedulingOptions.NotAllowedShiftCategories.Clear();

					foreach (var lim in schedulePeriod.ShiftCategoryLimitationCollection())
					{
						var isOnMax = _shiftCategoryLimitCounter.HaveMaxOfShiftCategory(lim, teamInfo, dateOnly);
						if (isOnMax) schedulingOptions.NotAllowedShiftCategories.Add(lim.ShiftCategory);			
					}

					var allSkillDays = schedulingResultStateHolder.AllSkillDays();
					var success = _teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, teamBlockInfo, dateOnly, schedulingOptions, rollbackService, resourceCalculateDelayer, allSkillDays, schedulingResultStateHolder.Schedules, shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator);
					if (success) continue;

					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
					success = _teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, teamBlockInfo, dateOnly, schedulingOptions, rollbackService, resourceCalculateDelayer, allSkillDays, schedulingResultStateHolder.Schedules, shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator);
					if (success) continue;

					_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				}
			}
		}
	}
}
