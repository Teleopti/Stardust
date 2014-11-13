using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IIntraIntervalOptimizer
	{
		IIntraIntervalIssues Optimize(ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingResultStateHolder schedulingResultStateHolder, IPerson person, DateOnly dateOnly, IList<IScheduleMatrixPro> allScheduleMatrixPros, IResourceCalculateDelayer resourceCalculateDelayer, ISkill skill, IIntraIntervalIssues intervalIssuesBefore, bool checkDayAfter);
		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		bool IsCanceled { get; }
		void Reset();
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
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		private ResourceOptimizerProgressEventArgs _progressEvent;
		private bool _cancelMe;


		public IntraIntervalOptimizer(ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory,
			ITeamBlockScheduler teamBlockScheduler,
			ISkillStaffPeriodEvaluator skillStaffPeriodEvaluator,
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService,
			IIntraIntervalIssueCalculator intraIntervalIssueCalculator,
			ITeamScheduling  teamScheduling,
			IShiftProjectionIntraIntervalBestFitCalculator shiftProjectionIntraIntervalBestFitCalculator,
			ISkillDayIntraIntervalIssueExtractor skillDayIntraIntervalIssueExtractor)
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
		}

		public void Reset()
		{
			_progressEvent = null;
			_cancelMe = false;
		}

		public bool IsCanceled
		{
			get { return _cancelMe || (_progressEvent != null && _progressEvent.UserCancel); }
		}

		
		public IIntraIntervalIssues Optimize(ISchedulingOptions schedulingOptions,
			ISchedulePartModifyAndRollbackService rollbackService, ISchedulingResultStateHolder schedulingResultStateHolder,
			IPerson person, DateOnly dateOnly, IList<IScheduleMatrixPro> allScheduleMatrixPros,
			IResourceCalculateDelayer resourceCalculateDelayer, ISkill skill, IIntraIntervalIssues intervalIssuesBefore, bool checkDayAfter)
		{
			_progressEvent = null;
			var progressCounter = 0;
			var limit = 0.7999;

			//if (_cancelMe || (_progressEvent != null && _progressEvent.UserCancel)) return intervalIssuesBefore;

			var teamInfo = _teamInfoFactory.CreateTeamInfo(person, dateOnly, allScheduleMatrixPros);
			var teamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinderTypeForAdvanceScheduling, true);
			var totalScheduleRange = schedulingResultStateHolder.Schedules[person];

			rollbackService.ClearModificationCollection();
			
			var daySchedule = totalScheduleRange.ScheduledDay(dateOnly);
			_deleteAndResourceCalculateService.DeleteWithResourceCalculation(new List<IScheduleDay> { daySchedule }, rollbackService, true);

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
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null);
				reportProgress(checkDayAfter, intervalIssuesBefore, progressCounter);
				return intervalIssuesBefore;
			}
		
			var shiftProjectionCacheBestFit = bestFit.ShiftProjection;
			_teamScheduling.ExecutePerDayPerPerson(person, dateOnly, teamBlock, shiftProjectionCacheBestFit, rollbackService, resourceCalculateDelayer);

			daySchedule = totalScheduleRange.ScheduledDay(dateOnly);
			if (!daySchedule.IsScheduled())
			{
				rollbackService.Rollback();
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null);
				reportProgress(checkDayAfter, intervalIssuesBefore, progressCounter);
				return intervalIssuesBefore;
			}

			var intervalIssuesAfter = _intraIntervalIssueCalculator.CalculateIssues(schedulingResultStateHolder, skill, dateOnly);
			var worse = false;

			if (!checkDayAfter)
			{
				var betterToday = _skillStaffPeriodEvaluator.ResultIsBetter(intervalIssuesBefore.IssuesOnDay, intervalIssuesAfter.IssuesOnDay, limit);
				var worseDayAfter = _skillStaffPeriodEvaluator.ResultIsWorseX(intervalIssuesBefore.IssuesOnDayAfter, intervalIssuesAfter.IssuesOnDayAfter, limit);
				worse = !betterToday || worseDayAfter;

			}
			if (checkDayAfter)
			{
				var betterDayAfter = _skillStaffPeriodEvaluator.ResultIsBetter(intervalIssuesBefore.IssuesOnDayAfter, intervalIssuesAfter.IssuesOnDayAfter, limit);
				var worseToday = _skillStaffPeriodEvaluator.ResultIsWorseX(intervalIssuesBefore.IssuesOnDay, intervalIssuesAfter.IssuesOnDay, limit);
				worse = !betterDayAfter || worseToday;
			}

			if (worse)
			{
				rollbackService.Rollback();
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null);
				reportProgress(checkDayAfter, intervalIssuesBefore, progressCounter);
			}

			else
			{
				reportProgress(checkDayAfter, intervalIssuesAfter, progressCounter);
				return intervalIssuesAfter;
			}
			
			return intervalIssuesBefore;
		}

		private void reportProgress(bool checkDayAfter, IIntraIntervalIssues intervalIssues, int progressCounter)
		{
			var issueCounter = checkDayAfter ? intervalIssues.IssuesOnDayAfter.Count : intervalIssues.IssuesOnDay.Count;
			if ((progressCounter % 10) == 0) OnReportProgress(string.Concat("(", progressCounter + 1, "/", issueCounter, ")"));	
		}

		public void OnReportProgress(string message)
		{
			var handler = ReportProgress;
			if (handler == null) return;
			var args = new ResourceOptimizerProgressEventArgs(0, 0, message);
			handler(this, args);

			if (args.Cancel || args.UserCancel)
				_cancelMe = true;
				
			if (_progressEvent != null && _progressEvent.UserCancel) return;

			_progressEvent = args;
		}
	}
}
