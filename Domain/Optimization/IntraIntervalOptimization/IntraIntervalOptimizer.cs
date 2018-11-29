using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public class IntraIntervalOptimizer
	{
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly SkillStaffPeriodEvaluator _skillStaffPeriodEvaluator;
		private readonly DeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private readonly IntraIntervalIssueCalculator _intraIntervalIssueCalculator;
		private readonly TeamScheduling _teamScheduling;
		private readonly ShiftProjectionIntraIntervalBestFitCalculator _shiftProjectionIntraIntervalBestFitCalculator;
		private readonly ISkillDayIntraIntervalIssueExtractor _skillDayIntraIntervalIssueExtractor;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly ShiftProjectionCachesForIntraInterval _shiftProjectionCachesForIntraInterval;

		public IntraIntervalOptimizer(ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory,
			SkillStaffPeriodEvaluator skillStaffPeriodEvaluator,
			DeleteAndResourceCalculateService deleteAndResourceCalculateService,
			IntraIntervalIssueCalculator intraIntervalIssueCalculator,
			TeamScheduling teamScheduling,
			ShiftProjectionIntraIntervalBestFitCalculator shiftProjectionIntraIntervalBestFitCalculator,
			ISkillDayIntraIntervalIssueExtractor skillDayIntraIntervalIssueExtractor,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
			ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator,
			Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
			ShiftProjectionCachesForIntraInterval shiftProjectionCachesForIntraInterval)
		{
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_skillStaffPeriodEvaluator = skillStaffPeriodEvaluator;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
			_intraIntervalIssueCalculator = intraIntervalIssueCalculator;
			_teamScheduling = teamScheduling;
			_shiftProjectionIntraIntervalBestFitCalculator = shiftProjectionIntraIntervalBestFitCalculator;
			_skillDayIntraIntervalIssueExtractor = skillDayIntraIntervalIssueExtractor;
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_shiftProjectionCachesForIntraInterval = shiftProjectionCachesForIntraInterval;
		}

		public IntraIntervalIssues Optimize(SchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackService, ISchedulingResultStateHolder schedulingResultStateHolder,
			IPerson person, DateOnly dateOnly, IEnumerable<IScheduleMatrixPro> allScheduleMatrixPros,
			IResourceCalculateDelayer resourceCalculateDelayer, ISkill skill, IntraIntervalIssues intervalIssuesBefore, bool checkDayAfter)
		{
			const double limit = 0.7999;

			var teamInfo = _teamInfoFactory.CreateTeamInfo(schedulingResultStateHolder.LoadedAgents, person, dateOnly, allScheduleMatrixPros);
			var teamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinder());
			if (teamBlock == null)
				return intervalIssuesBefore;
			var totalScheduleRange = schedulingResultStateHolder.Schedules[person];

			rollbackService.ClearModificationCollection();

			var daySchedule = totalScheduleRange.ScheduledDay(dateOnly);
			//New bug introduced here
			var originalMainShift = daySchedule.GetEditorShift();
			_mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(schedulingOptions, optimizationPreferences, originalMainShift, dateOnly);

			_deleteAndResourceCalculateService.DeleteWithResourceCalculation(new[] {daySchedule}, rollbackService, true, true);

			var skillDaysOnDay = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly });
			var skillDaysOnDayAfter = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly.AddDays(1) });

			var allSkillstaffPeriodsOnDay = _skillDayIntraIntervalIssueExtractor.ExtractAll(skillDaysOnDay, skill);
			var allSkillStaffPeriodsOnDayAfter = _skillDayIntraIntervalIssueExtractor.ExtractAll(skillDaysOnDayAfter, skill);

			var listResources = _shiftProjectionCachesForIntraInterval.Execute(teamBlock, person, dateOnly, schedulingOptions, schedulingResultStateHolder);
			var totalSkillStaffPeriods = allSkillstaffPeriodsOnDay.ToList();
			totalSkillStaffPeriods.AddRange(allSkillStaffPeriodsOnDayAfter);
			
			var bestFit = _shiftProjectionIntraIntervalBestFitCalculator.GetShiftProjectionCachesSortedByBestIntraIntervalFit(listResources, totalSkillStaffPeriods, skill, limit);

			if (bestFit == null)
			{
				rollbackService.Rollback();
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, true);
				return intervalIssuesBefore;
			}
		
			var shiftProjectionCacheBestFit = bestFit.ShiftProjection;

			var rules = NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder());
			//TODO: should pass in orginal assignments here to fix same issue as #45540 for shiftswithinday
			_teamScheduling.ExecutePerDayPerPerson(Enumerable.Empty<IPersonAssignment>(), person, dateOnly, teamBlock, 
				shiftProjectionCacheBestFit, rollbackService, rules, schedulingOptions, 
				schedulingResultStateHolder.Schedules, new ResourceCalculationData(schedulingResultStateHolder, schedulingOptions.ConsiderShortBreaks, true),null);

			daySchedule = totalScheduleRange.ScheduledDay(dateOnly);
			if (!daySchedule.IsScheduled())
			{
				rollbackService.Rollback();
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, true);
				return intervalIssuesBefore;
			}

			if (!_teamBlockShiftCategoryLimitationValidator.Validate(teamBlock, null, optimizationPreferences))
			{
				rollbackService.Rollback();
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, true);
				return intervalIssuesBefore;
			}

			var intervalIssuesAfter = _intraIntervalIssueCalculator.CalculateIssues(schedulingResultStateHolder, skill, dateOnly);
			var worse = false;

			if (!checkDayAfter)
			{
				var betterToday = _skillStaffPeriodEvaluator.ResultIsBetter(intervalIssuesBefore.IssuesOnDay, intervalIssuesAfter.IssuesOnDay, limit);
				var worseDayAfter = _skillStaffPeriodEvaluator.ResultIsWorse(intervalIssuesBefore.IssuesOnDayAfter, intervalIssuesAfter.IssuesOnDayAfter, limit);
				worse = !betterToday || worseDayAfter;

			}
			if (checkDayAfter)
			{
				var betterDayAfter = _skillStaffPeriodEvaluator.ResultIsBetter(intervalIssuesBefore.IssuesOnDayAfter, intervalIssuesAfter.IssuesOnDayAfter, limit);
				var worseToday = _skillStaffPeriodEvaluator.ResultIsWorse(intervalIssuesBefore.IssuesOnDay, intervalIssuesAfter.IssuesOnDay, limit);
				worse = !betterDayAfter || worseToday;
			}

			if (worse)
			{
				rollbackService.Rollback();
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, true);
			}

			else
			{
				return intervalIssuesAfter;
			}
			
			return intervalIssuesBefore;
		}
	}
}
