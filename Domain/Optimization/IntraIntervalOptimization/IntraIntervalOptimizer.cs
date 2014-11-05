using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
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
		private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;
		private readonly ISkillStaffPeriodEvaluator _skillStaffPeriodEvaluator;
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private readonly IIntraIntervalIssueCalculator _intraIntervalIssueCalculator;
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		private ResourceOptimizerProgressEventArgs _progressEvent;
		private bool _cancelMe;


		public IntraIntervalOptimizer(ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory,
			ITeamBlockScheduler teamBlockScheduler, IShiftProjectionCacheManager shiftProjectionCacheManager,
			ISkillStaffPeriodEvaluator skillStaffPeriodEvaluator,
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService,
			IIntraIntervalIssueCalculator intraIntervalIssueCalculator)
		{
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockScheduler = teamBlockScheduler;
			_shiftProjectionCacheManager = shiftProjectionCacheManager;
			_skillStaffPeriodEvaluator = skillStaffPeriodEvaluator;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
			_intraIntervalIssueCalculator = intraIntervalIssueCalculator;
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
			var notBetter = true;
			var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
			var progressCounter = 0;
			_progressEvent = null;

			var teamInfo = _teamInfoFactory.CreateTeamInfo(person, dateOnly, allScheduleMatrixPros);
			var teamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinderTypeForAdvanceScheduling, true);
			var totalScheduleRange = schedulingResultStateHolder.Schedules[person];
			var intervalIssuesAfter = intervalIssuesBefore;

			rollbackService.ClearModificationCollection();
			while (notBetter)
			{
				if (_cancelMe || (_progressEvent != null && _progressEvent.UserCancel)) break;

				if ((progressCounter % 10) == 0)
					OnReportProgress(string.Concat("(", progressCounter, "/", intervalIssuesAfter.IssuesOnDay.Count, ")"));

				progressCounter++;

				var daySchedule = totalScheduleRange.ScheduledDay(dateOnly);
				var shiftProjectionCache = _shiftProjectionCacheManager.ShiftProjectionCacheFromShift(daySchedule.GetEditorShift(), dateOnly, timeZoneInfo);
				schedulingOptions.AddNotAllowedShiftProjectionCache(shiftProjectionCache);

				_deleteAndResourceCalculateService.DeleteWithResourceCalculation(new List<IScheduleDay> { daySchedule }, rollbackService, true);
				var success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlock, dateOnly, schedulingOptions, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, new ShiftNudgeDirective());

				if (!success)
				{
					rollbackService.Rollback();
					resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null);
				}

				intervalIssuesAfter = _intraIntervalIssueCalculator.CalculateIssues(schedulingResultStateHolder, skill, dateOnly);
				
				if (!success) break;
				if (!checkDayAfter && intervalIssuesAfter.IssuesOnDay.Count == 0) break;
				if (checkDayAfter && intervalIssuesAfter.IssuesOnDayAfter.Count == 0) break;
				
				notBetter = _skillStaffPeriodEvaluator.ResultIsWorse(intervalIssuesBefore.IssuesOnDay, intervalIssuesAfter.IssuesOnDay);
				notBetter = notBetter || _skillStaffPeriodEvaluator.ResultIsWorse(intervalIssuesBefore.IssuesOnDayAfter, intervalIssuesAfter.IssuesOnDayAfter);

				if (!notBetter)
				{
					var today = _skillStaffPeriodEvaluator.ResultIsBetter(intervalIssuesBefore.IssuesOnDay, intervalIssuesAfter.IssuesOnDay);
					var dayAfter = _skillStaffPeriodEvaluator.ResultIsBetter(intervalIssuesBefore.IssuesOnDayAfter, intervalIssuesAfter.IssuesOnDayAfter);

					if (!checkDayAfter) notBetter = !today;
					if (checkDayAfter) notBetter = !dayAfter;
				}	
			}

			return intervalIssuesAfter;
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
