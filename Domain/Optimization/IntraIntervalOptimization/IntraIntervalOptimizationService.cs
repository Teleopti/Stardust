using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IIntraIntervalOptimizationService
	{
		void Execute(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer);
		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
	}

	public class IntraIntervalOptimizationService : IIntraIntervalOptimizationService
	{
		private readonly IScheduleDayIntraIntervalIssueExtractor _scheduleDayIntraIntervalIssueExtractor;
		private readonly IIntraIntervalOptimizer _intraIntervalOptimizer;
		private readonly IIntraIntervalIssueCalculator _intraIntervalIssueCalculator;
		private string _progressSkill;
		private string _progressDate;
		private string _progressPerson;
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		private ResourceOptimizerProgressEventArgs _progressEvent;
		private bool _cancelMe;

		public IntraIntervalOptimizationService(IScheduleDayIntraIntervalIssueExtractor scheduleDayIntraIntervalIssueExtractor, IIntraIntervalOptimizer intraIntervalOptimizer, IIntraIntervalIssueCalculator intraIntervalIssueCalculator)
		{
			_scheduleDayIntraIntervalIssueExtractor = scheduleDayIntraIntervalIssueExtractor;
			_intraIntervalOptimizer = intraIntervalOptimizer;
			_intraIntervalIssueCalculator = intraIntervalIssueCalculator;
		}

		public void Execute(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules,
			ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer)
		{
			var personHashSet = new HashSet<IPerson>();
			var cultureInfo = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			_progressEvent = null;
			_cancelMe = false;

			foreach (var selectedSchedule in selectedSchedules)
			{
				personHashSet.Add(selectedSchedule.Person);
			}

			foreach (var skill in schedulingResultStateHolder.Skills)
			{
				_progressSkill = skill.Name;
				if (_cancelMe || (_progressEvent != null && _progressEvent.UserCancel)) break;

				var skillType = skill.SkillType.ForecastSource;
				if(skillType != ForecastSource.InboundTelephony && skillType != ForecastSource.Chat) continue;
				
				foreach (var dateOnly in selectedPeriod.DayCollection())
				{
					_progressDate = dateOnly.ToShortDateString(cultureInfo);
			
					if (_cancelMe || (_progressEvent != null && _progressEvent.UserCancel)) break;

					var intervalIssuesBefore = _intraIntervalIssueCalculator.CalculateIssues(schedulingResultStateHolder, skill, dateOnly);
					var schedules = schedulingResultStateHolder.Schedules;
					var scheduleDaysWithIssue = _scheduleDayIntraIntervalIssueExtractor.Extract(schedules, dateOnly, intervalIssuesBefore.IssuesOnDay, skill);
					var scheduleDaysWithIssueDayAfter = _scheduleDayIntraIntervalIssueExtractor.Extract(schedules, dateOnly, intervalIssuesBefore.IssuesOnDayAfter, skill);

					foreach (var scheduleDay in scheduleDaysWithIssue)
					{
						if (_cancelMe || (_progressEvent != null && _progressEvent.UserCancel)) break;

						var person = scheduleDay.Person;

						if (!personHashSet.Contains(person)) continue;

						_progressPerson = person.Name.ToString();
						schedulingOptions.ClearNotAllowedShiftProjectionCaches();

						var affect = affectIssue(scheduleDay, intervalIssuesBefore.IssuesOnDay);
						if(!affect) continue;
						
						var intervalIssuesAfter = _intraIntervalOptimizer.Optimize(schedulingOptions, optimizationPreferences, rollbackService, schedulingResultStateHolder, person, dateOnly, allScheduleMatrixPros, resourceCalculateDelayer, skill, intervalIssuesBefore, false);

						OnReportProgress(string.Concat("(", intervalIssuesAfter.IssuesOnDay.Count, "/", intervalIssuesAfter.IssuesOnDayAfter.Count, ")"));

						intervalIssuesBefore = intervalIssuesAfter;

						if (intervalIssuesAfter.IssuesOnDay.Count == 0) break;
						
					}

					foreach (var scheduleDay in scheduleDaysWithIssueDayAfter)
					{
						if (_cancelMe || (_progressEvent != null && _progressEvent.UserCancel)) break;

						var person = scheduleDay.Person;

						if (!personHashSet.Contains(person)) continue;

						_progressPerson = person.Name.ToString();
						schedulingOptions.ClearNotAllowedShiftProjectionCaches();

						var affect = affectIssue(scheduleDay, intervalIssuesBefore.IssuesOnDayAfter);
						if (!affect) continue;

						var intervalIssuesAfter = _intraIntervalOptimizer.Optimize(schedulingOptions, optimizationPreferences, rollbackService, schedulingResultStateHolder, person, dateOnly, allScheduleMatrixPros, resourceCalculateDelayer, skill, intervalIssuesBefore, true);

						OnReportProgress(string.Concat("(", intervalIssuesAfter.IssuesOnDay.Count, "/", intervalIssuesAfter.IssuesOnDayAfter.Count, ")"));

						intervalIssuesBefore = intervalIssuesAfter;

						if (intervalIssuesAfter.IssuesOnDayAfter.Count == 0) break;
					}	
				}
			}
		}

		private bool affectIssue(IScheduleDay scheduleDay, IList<ISkillStaffPeriod> issues)
		{
			var affects = false;

			foreach (var skillStaffPeriod in issues)
			{
				if (!scheduleDay.PersonAssignment().Period.Intersect(skillStaffPeriod.Period)) continue;
				affects = true;
				break;
			}

			return affects;
		}

		public void OnReportProgress(string message)
		{
			var handler = ReportProgress;
			if (handler == null) return;
			var progressMessage = string.Concat(_progressSkill, " ", _progressDate, " ", _progressPerson, " ", message);
			var args = new ResourceOptimizerProgressEventArgs(0, 0, progressMessage);

			handler(this, args);

			if (args.Cancel || args.UserCancel)
				_cancelMe = true;

			if (_progressEvent != null && _progressEvent.UserCancel) return;

			_progressEvent = args;
		}
	}

	public class IntraIntervalOptimizationServiceToggle29846Off : IIntraIntervalOptimizationService
	{
		public void Execute(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences,
			DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules, ISchedulingResultStateHolder schedulingResultStateHolder,
			IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer)
		{
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		public void OnReportProgress(string message)
		{
			var handler = ReportProgress;
			if (handler == null) return;
			var args = new ResourceOptimizerProgressEventArgs(0, 0, message);
			handler(this, args);
		}
	}
}
