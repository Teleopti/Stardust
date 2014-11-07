﻿using System;
using System.Collections.Generic;
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

			var teamInfo = _teamInfoFactory.CreateTeamInfo(person, dateOnly, allScheduleMatrixPros);
			var teamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinderTypeForAdvanceScheduling, true);
			var totalScheduleRange = schedulingResultStateHolder.Schedules[person];
			IIntraIntervalIssues intervalIssuesAfter = new IntraIntervalIssues();

			rollbackService.ClearModificationCollection();
			
			var daySchedule = totalScheduleRange.ScheduledDay(dateOnly);
			_deleteAndResourceCalculateService.DeleteWithResourceCalculation(new List<IScheduleDay> { daySchedule }, rollbackService, true);

			var listResources = _teamBlockScheduler.GetShiftProjectionCaches(teamBlock, person, dateOnly, schedulingOptions, schedulingResultStateHolder);
			var listBestFit = _shiftProjectionIntraIntervalBestFitCalculator.GetShiftProjectionCachesSortedByBestIntraIntervalFit(listResources, checkDayAfter ? intervalIssuesBefore.IssuesOnDayAfter : intervalIssuesBefore.IssuesOnDay, skill);

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

				intervalIssuesAfter = _intraIntervalIssueCalculator.CalculateIssues(schedulingResultStateHolder, skill, dateOnly);
				var notBetter = _skillStaffPeriodEvaluator.ResultIsWorse(intervalIssuesBefore.IssuesOnDay, intervalIssuesAfter.IssuesOnDay);
				notBetter = notBetter || _skillStaffPeriodEvaluator.ResultIsWorse(intervalIssuesBefore.IssuesOnDayAfter, intervalIssuesAfter.IssuesOnDayAfter);

				if (!notBetter)
				{
					var today = _skillStaffPeriodEvaluator.ResultIsBetter(intervalIssuesBefore.IssuesOnDay, intervalIssuesAfter.IssuesOnDay);
					var dayAfter = _skillStaffPeriodEvaluator.ResultIsBetter(intervalIssuesBefore.IssuesOnDayAfter, intervalIssuesAfter.IssuesOnDayAfter);

					if (!checkDayAfter) notBetter = !today;
					if (checkDayAfter) notBetter = !dayAfter;
				}
	
				if (notBetter)
				{
					rollbackService.Rollback();
					resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null);
					reportProgress(checkDayAfter, intervalIssuesBefore, progressCounter);
				}

				else
				{
					reportProgress(checkDayAfter, intervalIssuesAfter, progressCounter);
					break;
				}

				progressCounter++;
			}

			return intervalIssuesAfter;
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
