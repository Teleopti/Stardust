using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IIntraIntervalOptimizationService
	{
		void Execute(ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer);
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

		public IntraIntervalOptimizationService(IScheduleDayIntraIntervalIssueExtractor scheduleDayIntraIntervalIssueExtractor, IIntraIntervalOptimizer intraIntervalOptimizer, IIntraIntervalIssueCalculator intraIntervalIssueCalculator)
		{
			_scheduleDayIntraIntervalIssueExtractor = scheduleDayIntraIntervalIssueExtractor;
			_intraIntervalOptimizer = intraIntervalOptimizer;
			_intraIntervalIssueCalculator = intraIntervalIssueCalculator;
		}

		public void Execute(ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer)
		{
			var personHashSet = new HashSet<IPerson>();
			var cultureInfo = TeleoptiPrincipal.Current.Regional.Culture;
			_intraIntervalOptimizer.ReportProgress += OnReportProgress;

			foreach (var selectedSchedule in selectedSchedules)
			{
				personHashSet.Add(selectedSchedule.Person);
			}

			foreach (var skill in schedulingResultStateHolder.Skills)
			{
				_progressSkill = skill.Name;
				if (_intraIntervalOptimizer.IsCanceled) break;

				var skillType = skill.SkillType.ForecastSource;
				if(skillType != ForecastSource.InboundTelephony && skillType != ForecastSource.Chat) continue;
				
				foreach (var dateOnly in selectedPeriod.DayCollection())
				{
					_progressDate = dateOnly.ToShortDateString(cultureInfo);
					if (_intraIntervalOptimizer.IsCanceled) break;

					var intervalIssuesBefore = _intraIntervalIssueCalculator.CalculateIssues(schedulingResultStateHolder, skill, dateOnly);
					var schedules = schedulingResultStateHolder.Schedules;
					var scheduleDaysWithIssue = _scheduleDayIntraIntervalIssueExtractor.Extract(schedules, dateOnly, intervalIssuesBefore.IssuesOnDay, skill);
					var scheduleDaysWithIssueDayAfter = _scheduleDayIntraIntervalIssueExtractor.Extract(schedules, dateOnly, intervalIssuesBefore.IssuesOnDayAfter, skill);

					foreach (var scheduleDay in scheduleDaysWithIssue)
					{
						if (_intraIntervalOptimizer.IsCanceled) break;

						var person = scheduleDay.Person;

						if (!personHashSet.Contains(person)) continue;

						_progressPerson = person.Name.ToString();
						schedulingOptions.ClearNotAllowedShiftProjectionCaches();

						var affect = affectIssue(scheduleDay, intervalIssuesBefore.IssuesOnDay);
						if(!affect) continue;
						
						var intervalIssuesAfter = _intraIntervalOptimizer.Optimize(schedulingOptions, rollbackService, schedulingResultStateHolder, person, dateOnly, allScheduleMatrixPros, resourceCalculateDelayer, skill, intervalIssuesBefore, false);
						if (intervalIssuesAfter.IssuesOnDay.Count == 0) break;
						intervalIssuesBefore = intervalIssuesAfter;
					}

					foreach (var scheduleDay in scheduleDaysWithIssueDayAfter)
					{
						if (_intraIntervalOptimizer.IsCanceled) break;

						var person = scheduleDay.Person;

						if (!personHashSet.Contains(person)) continue;

						_progressPerson = person.Name.ToString();
						schedulingOptions.ClearNotAllowedShiftProjectionCaches();

						var affect = affectIssue(scheduleDay, intervalIssuesBefore.IssuesOnDayAfter);
						if(!affect) continue;

						var intervalIssuesAfter = _intraIntervalOptimizer.Optimize(schedulingOptions, rollbackService, schedulingResultStateHolder, person, dateOnly, allScheduleMatrixPros, resourceCalculateDelayer, skill, intervalIssuesBefore, true);
						if (intervalIssuesAfter.IssuesOnDayAfter.Count == 0) break;
						intervalIssuesBefore = intervalIssuesAfter;
					}
				}
			}

			_intraIntervalOptimizer.Reset();
			_intraIntervalOptimizer.ReportProgress -= OnReportProgress;
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

		public void OnReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			var handler = ReportProgress;
			if (handler == null) return;
			var progressMessage = string.Concat(_progressSkill, " ", _progressDate, " ", _progressPerson, " ", e.Message);
			var args = new ResourceOptimizerProgressEventArgs(0, 0, progressMessage);
			handler(this, args);
			e.Cancel = args.Cancel;
			e.UserCancel = args.UserCancel;
		}
	}

	public class IntraIntervalOptimizationServiceToggle29846Off : IIntraIntervalOptimizationService
	{
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		public void Execute(ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer)
		{
				
		}

		public void OnReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			var handler = ReportProgress;
			if (handler == null) return;
			handler(this, e);
		}
	}
}
