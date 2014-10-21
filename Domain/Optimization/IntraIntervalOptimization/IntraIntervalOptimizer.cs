﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IIntraIntervalOptimizer
	{
		IList<ISkillStaffPeriod> Optimize(ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingResultStateHolder schedulingResultStateHolder, IPerson person, DateOnly dateOnly, IList<IScheduleMatrixPro> allScheduleMatrixPros, IResourceCalculateDelayer resourceCalculateDelayer, ISkill skill, IList<ISkillStaffPeriod> intervalIssuesBefore);
		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		bool IsCanceled { get; }
		void Reset();
	}

	public class IntraIntervalOptimizer : IIntraIntervalOptimizer
	{
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;
		private readonly ISkillDayIntraIntervalIssueExtractor _skillDayIntraIntervalIssueExtractor;
		private readonly ISkillStaffPeriodEvaluator _skillStaffPeriodEvaluator;
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		private ResourceOptimizerProgressEventArgs _progressEvent;


		public IntraIntervalOptimizer(ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory, ITeamBlockClearer teamBlockClearer, ITeamBlockScheduler teamBlockScheduler, IShiftProjectionCacheManager shiftProjectionCacheManager, ISkillDayIntraIntervalIssueExtractor skillDayIntraIntervalIssueExtractor, ISkillStaffPeriodEvaluator skillStaffPeriodEvaluator)
		{
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockScheduler = teamBlockScheduler;
			_shiftProjectionCacheManager = shiftProjectionCacheManager;
			_skillDayIntraIntervalIssueExtractor = skillDayIntraIntervalIssueExtractor;
			_skillStaffPeriodEvaluator = skillStaffPeriodEvaluator;
		}

		public void Reset()
		{
			_progressEvent = null;
		}

		public bool IsCanceled
		{
			get { return _progressEvent != null && _progressEvent.UserCancel; }
		}

		public IList<ISkillStaffPeriod> Optimize(ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingResultStateHolder schedulingResultStateHolder, IPerson person, DateOnly dateOnly, IList<IScheduleMatrixPro> allScheduleMatrixPros, IResourceCalculateDelayer resourceCalculateDelayer, ISkill skill, IList<ISkillStaffPeriod> intervalIssuesBefore)
		{
			IList<ISkillStaffPeriod> intervalIssuesAfter = new List<ISkillStaffPeriod>();
			var notBetter = true;
			var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
			var progressCounter = 0;
			_progressEvent = null;

			rollbackService.ClearModificationCollection();
			while (notBetter)
			{
				if (_progressEvent != null && _progressEvent.UserCancel) break;
				
				intervalIssuesAfter.Clear();
				var totalScheduleRange = schedulingResultStateHolder.Schedules[person];
				var daySchedule = totalScheduleRange.ScheduledDay(dateOnly);

				var shiftProjectionCache = _shiftProjectionCacheManager.ShiftProjectionCacheFromShift(daySchedule.GetEditorShift(), dateOnly, timeZoneInfo);
				schedulingOptions.AddNotAllowedShiftProjectionCache(shiftProjectionCache);

				var teamInfo = _teamInfoFactory.CreateTeamInfo(person, dateOnly, allScheduleMatrixPros);
				var teamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinderTypeForAdvanceScheduling, true);

				_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlock);
				var success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlock, dateOnly, schedulingOptions, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, new ShiftNudgeDirective());

				if (!success)
				{
					rollbackService.Rollback();
					resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null);
				}

				var skillDays = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly });
				intervalIssuesAfter = _skillDayIntraIntervalIssueExtractor.Extract(skillDays, skill);

				if (!success) break;

				if (intervalIssuesAfter.Count == 0)
				{
					break;
				}

				notBetter = !_skillStaffPeriodEvaluator.ResultIsBetter(intervalIssuesBefore, intervalIssuesAfter);

				if((progressCounter % 10) == 0)
					OnReportProgress(UserTexts.Resources.IntraIntervalOptimization + " " + person.Name.FirstName + " " + person.Name.LastName + dateOnly.Date.ToShortDateString());

				progressCounter++;
			}

			return intervalIssuesAfter;
		}

		public void OnReportProgress(string message)
		{
			var handler = ReportProgress;
			if (handler == null) return;
			var args = new ResourceOptimizerProgressEventArgs(0, 0, message);
			handler(this, args);
				
			if (_progressEvent != null && _progressEvent.UserCancel) return;

			_progressEvent = args;
		}
	}
}
