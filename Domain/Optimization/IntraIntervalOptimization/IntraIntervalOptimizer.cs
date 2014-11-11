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
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		private ResourceOptimizerProgressEventArgs _progressEvent;
		private bool _cancelMe;


		public IntraIntervalOptimizer(ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory,
			ITeamBlockScheduler teamBlockScheduler,
			ISkillStaffPeriodEvaluator skillStaffPeriodEvaluator,
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService,
			IIntraIntervalIssueCalculator intraIntervalIssueCalculator,
			ITeamScheduling  teamScheduling,
			IShiftProjectionIntraIntervalBestFitCalculator shiftProjectionIntraIntervalBestFitCalculator)
		{
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockScheduler = teamBlockScheduler;
			_skillStaffPeriodEvaluator = skillStaffPeriodEvaluator;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
			_intraIntervalIssueCalculator = intraIntervalIssueCalculator;
			_teamScheduling = teamScheduling;
			_shiftProjectionIntraIntervalBestFitCalculator = shiftProjectionIntraIntervalBestFitCalculator;
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
			var limit = 0.8;

			var teamInfo = _teamInfoFactory.CreateTeamInfo(person, dateOnly, allScheduleMatrixPros);
			var teamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinderTypeForAdvanceScheduling, true);
			var totalScheduleRange = schedulingResultStateHolder.Schedules[person];

			rollbackService.ClearModificationCollection();
			
			var daySchedule = totalScheduleRange.ScheduledDay(dateOnly);
			_deleteAndResourceCalculateService.DeleteWithResourceCalculation(new List<IScheduleDay> { daySchedule }, rollbackService, true);

			intervalIssuesBefore = _intraIntervalIssueCalculator.CalculateIssues(schedulingResultStateHolder, skill, dateOnly);

			var skillDaysOnDay = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly });
			var skillDaysOnDayAfter = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly.AddDays(1) });

			var allSkillstaffPeriodsOnDay = Extract(skillDaysOnDay, skill);
			var allSkillStaffPeriodsOnDayAfter = Extract(skillDaysOnDayAfter, skill);

			var listResources = _teamBlockScheduler.GetShiftProjectionCaches(teamBlock, person, dateOnly, schedulingOptions, schedulingResultStateHolder);
			//var listBestFit = _shiftProjectionIntraIntervalBestFitCalculator.GetShiftProjectionCachesSortedByBestIntraIntervalFit(listResources, checkDayAfter ? intervalIssuesBefore.IssuesOnDayAfter : intervalIssuesBefore.IssuesOnDay, skill);

			var totalSkillStaffPeriods = allSkillstaffPeriodsOnDay.ToList();
			totalSkillStaffPeriods.AddRange(allSkillStaffPeriodsOnDayAfter);


			//var listBestFit = _shiftProjectionIntraIntervalBestFitCalculator.GetShiftProjectionCachesSortedByBestIntraIntervalFit(listResources, checkDayAfter ? allSkillStaffPeriodsOnDayAfter : allSkillstaffPeriodsOnDay, skill, limit);
			var listBestFit = _shiftProjectionIntraIntervalBestFitCalculator.GetShiftProjectionCachesSortedByBestIntraIntervalFit(listResources, totalSkillStaffPeriods, skill, limit);

			if (listBestFit.Count == 0)
			{
				rollbackService.Rollback();
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null);
				reportProgress(checkDayAfter, intervalIssuesBefore, progressCounter);
				return intervalIssuesBefore;
			}

			foreach (var workShiftCalculationResultHolder in listBestFit)
			{
				if (_cancelMe || (_progressEvent != null && _progressEvent.UserCancel)) break;
					
				var shiftProjectionCacheBestFit = workShiftCalculationResultHolder.ShiftProjection;
				_teamScheduling.ExecutePerDayPerPerson(person, dateOnly, teamBlock, shiftProjectionCacheBestFit, rollbackService, resourceCalculateDelayer);

				//TODO DETECT IF NOT ABLE TO SCHEDULE
				daySchedule = totalScheduleRange.ScheduledDay(dateOnly);
				if (!daySchedule.IsScheduled())
				{
					reportProgress(checkDayAfter, intervalIssuesBefore, progressCounter);
					continue;
				}

				var intervalIssuesAfter = _intraIntervalIssueCalculator.CalculateIssues(schedulingResultStateHolder, skill, dateOnly);
				//var worse = _skillStaffPeriodEvaluator.ResultIsWorse(intervalIssuesBefore.IssuesOnDay, intervalIssuesAfter.IssuesOnDay);
				//worse = worse || _skillStaffPeriodEvaluator.ResultIsWorse(intervalIssuesBefore.IssuesOnDayAfter, intervalIssuesAfter.IssuesOnDayAfter);

				var worse = false;

				//if (!checkDayAfter)
				//	worse = !_skillStaffPeriodEvaluator.ResultIsBetter(intervalIssuesBefore.IssuesOnDay, intervalIssuesAfter.IssuesOnDay, limit);
				//if (checkDayAfter)
				//	worse = !_skillStaffPeriodEvaluator.ResultIsBetter(intervalIssuesBefore.IssuesOnDayAfter, intervalIssuesAfter.IssuesOnDayAfter, limit);


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

				progressCounter++;
			}

			return intervalIssuesBefore;
		}

		private IList<ISkillStaffPeriod> Extract(IList<ISkillDay> skillDays, ISkill skill)
		{
			var result = new List<ISkillStaffPeriod>();

			foreach (var skillDay in skillDays)
			{
				if (skillDay.Skill != skill) continue;

				foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
				{
					//if (skillStaffPeriod.HasIntraIntervalIssue)
					//{
						result.Add((ISkillStaffPeriod)skillStaffPeriod.NoneEntityClone());
					//}
				}
			}

			return result;
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
