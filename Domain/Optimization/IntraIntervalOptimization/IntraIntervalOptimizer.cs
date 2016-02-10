using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IIntraIntervalOptimizer
	{
		IIntraIntervalIssues Optimize(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingResultStateHolder schedulingResultStateHolder, IPerson person, DateOnly dateOnly, IList<IScheduleMatrixPro> allScheduleMatrixPros, IResourceCalculateDelayer resourceCalculateDelayer, ISkill skill, IIntraIntervalIssues intervalIssuesBefore, bool checkDayAfter);
	}

	public class IntraIntervalOptimizer : IIntraIntervalOptimizer
	{
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ISkillStaffPeriodEvaluator _skillStaffPeriodEvaluator;
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private readonly IIntraIntervalIssueCalculator _intraIntervalIssueCalculator;
		private readonly ITeamScheduling _teamScheduling;
		private readonly IShiftProjectionIntraIntervalBestFitCalculator _shiftProjectionIntraIntervalBestFitCalculator;
		private readonly ISkillDayIntraIntervalIssueExtractor _skillDayIntraIntervalIssueExtractor;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;

		public IntraIntervalOptimizer(ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory,
			ITeamBlockScheduler teamBlockScheduler,
			ISkillStaffPeriodEvaluator skillStaffPeriodEvaluator,
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService,
			IIntraIntervalIssueCalculator intraIntervalIssueCalculator,
			ITeamScheduling  teamScheduling,
			IShiftProjectionIntraIntervalBestFitCalculator shiftProjectionIntraIntervalBestFitCalculator,
			ISkillDayIntraIntervalIssueExtractor skillDayIntraIntervalIssueExtractor,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
			ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator)
		{
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockScheduler = teamBlockScheduler;
			_skillStaffPeriodEvaluator = skillStaffPeriodEvaluator;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
			_intraIntervalIssueCalculator = intraIntervalIssueCalculator;
			_teamScheduling = teamScheduling;
			_shiftProjectionIntraIntervalBestFitCalculator = shiftProjectionIntraIntervalBestFitCalculator;
			_skillDayIntraIntervalIssueExtractor = skillDayIntraIntervalIssueExtractor;
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
		}

		public IIntraIntervalIssues Optimize(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackService, ISchedulingResultStateHolder schedulingResultStateHolder,
			IPerson person, DateOnly dateOnly, IList<IScheduleMatrixPro> allScheduleMatrixPros,
			IResourceCalculateDelayer resourceCalculateDelayer, ISkill skill, IIntraIntervalIssues intervalIssuesBefore, bool checkDayAfter)
		{
			const double limit = 0.7999;

			var teamInfo = _teamInfoFactory.CreateTeamInfo(person, dateOnly, allScheduleMatrixPros);
			var teamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinderTypeForAdvanceScheduling, true);
			if (teamBlock == null)
				return intervalIssuesBefore;
			var totalScheduleRange = schedulingResultStateHolder.Schedules[person];

			rollbackService.ClearModificationCollection();

			var daySchedule = totalScheduleRange.ScheduledDay(dateOnly);
			//New bug introduced here
			var originalMainShift = daySchedule.GetEditorShift();
			_mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(schedulingOptions, optimizationPreferences, originalMainShift, dateOnly);

			_deleteAndResourceCalculateService.DeleteWithResourceCalculation(daySchedule, rollbackService, true, true);

			var skillDaysOnDay = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly });
			var skillDaysOnDayAfter = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly.AddDays(1) });

			var allSkillstaffPeriodsOnDay = _skillDayIntraIntervalIssueExtractor.ExtractAll(skillDaysOnDay, skill);
			var allSkillStaffPeriodsOnDayAfter = _skillDayIntraIntervalIssueExtractor.ExtractAll(skillDaysOnDayAfter, skill);

			var listResources = _teamBlockScheduler.GetShiftProjectionCaches(teamBlock, person, dateOnly, schedulingOptions, schedulingResultStateHolder);
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

			_teamScheduling.ExecutePerDayPerPerson(person, dateOnly, teamBlock, shiftProjectionCacheBestFit, rollbackService, resourceCalculateDelayer, true);

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
