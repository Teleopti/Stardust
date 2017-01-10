﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IIntraIntervalOptimizationService
	{
		void Execute(IOptimizationPreferences optimizationPreferences, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer);
		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
	}

	public class IntraIntervalOptimizationService : IIntraIntervalOptimizationService
	{
		private readonly IScheduleDayIntraIntervalIssueExtractor _scheduleDayIntraIntervalIssueExtractor;
		private readonly IIntraIntervalOptimizer _intraIntervalOptimizer;
		private readonly IIntraIntervalIssueCalculator _intraIntervalIssueCalculator;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public IntraIntervalOptimizationService(
			IScheduleDayIntraIntervalIssueExtractor scheduleDayIntraIntervalIssueExtractor,
			IIntraIntervalOptimizer intraIntervalOptimizer, IIntraIntervalIssueCalculator intraIntervalIssueCalculator,
			ISchedulingOptionsCreator schedulingOptionsCreator)
		{
			_scheduleDayIntraIntervalIssueExtractor = scheduleDayIntraIntervalIssueExtractor;
			_intraIntervalOptimizer = intraIntervalOptimizer;
			_intraIntervalIssueCalculator = intraIntervalIssueCalculator;
			_schedulingOptionsCreator = schedulingOptionsCreator;
		}

		public void Execute(IOptimizationPreferences optimizationPreferences, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules,
			ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer)
		{
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			var personHashSet = new HashSet<IPerson>();
			var cultureInfo = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			
			foreach (var selectedSchedule in selectedSchedules)
			{
				personHashSet.Add(selectedSchedule.Person);
			}

			foreach (var skill in schedulingResultStateHolder.Skills)
			{
				var progressSkill = skill.Name;

				var skillType = skill.SkillType.ForecastSource;
				if(skillType != ForecastSource.InboundTelephony && skillType != ForecastSource.Chat) continue;

				var cancel = false;
				foreach (var dateOnly in selectedPeriod.DayCollection())
				{
					var progressDate = dateOnly.ToShortDateString(cultureInfo);
			
					var intervalIssuesBefore = _intraIntervalIssueCalculator.CalculateIssues(schedulingResultStateHolder, skill, dateOnly);
					var schedules = schedulingResultStateHolder.Schedules;
					var scheduleDaysWithIssue = _scheduleDayIntraIntervalIssueExtractor.Extract(schedules, dateOnly, intervalIssuesBefore.IssuesOnDay);
					var scheduleDaysWithIssueDayAfter = _scheduleDayIntraIntervalIssueExtractor.Extract(schedules, dateOnly, intervalIssuesBefore.IssuesOnDayAfter);

					foreach (var scheduleDay in scheduleDaysWithIssue)
					{
						if (cancel) return;
						var person = scheduleDay.Person;

						if (!personHashSet.Contains(person)) continue;

						var progressPerson = person.Name.ToString();
						schedulingOptions.ClearNotAllowedShiftProjectionCaches();

						var affect = affectIssue(scheduleDay, intervalIssuesBefore.IssuesOnDay);
						if(!affect) continue;
						
						var intervalIssuesAfter = _intraIntervalOptimizer.Optimize(schedulingOptions, optimizationPreferences, rollbackService, schedulingResultStateHolder, person, dateOnly, allScheduleMatrixPros, resourceCalculateDelayer, skill, intervalIssuesBefore, false);

						var progressResult = onReportProgress(string.Concat("(", intervalIssuesAfter.IssuesOnDay.Count, "/", intervalIssuesAfter.IssuesOnDayAfter.Count, ")"), progressSkill, progressDate, progressPerson,()=>cancel=true);
						if (cancel || progressResult.ShouldCancel) return;

						intervalIssuesBefore = intervalIssuesAfter;

						if (intervalIssuesAfter.IssuesOnDay.Count == 0) break;
					}

					foreach (var scheduleDay in scheduleDaysWithIssueDayAfter)
					{
						if (cancel) return;
						var person = scheduleDay.Person;

						if (!personHashSet.Contains(person)) continue;

						var progressPerson = person.Name.ToString();
						schedulingOptions.ClearNotAllowedShiftProjectionCaches();

						var affect = affectIssue(scheduleDay, intervalIssuesBefore.IssuesOnDayAfter);
						if (!affect) continue;

						var intervalIssuesAfter = _intraIntervalOptimizer.Optimize(schedulingOptions, optimizationPreferences, rollbackService, schedulingResultStateHolder, person, dateOnly, allScheduleMatrixPros, resourceCalculateDelayer, skill, intervalIssuesBefore, true);

						var progressResult = onReportProgress(string.Concat("(", intervalIssuesAfter.IssuesOnDay.Count, "/", intervalIssuesAfter.IssuesOnDayAfter.Count, ")"), progressSkill, progressDate, progressPerson, () => cancel = true);
						if (progressResult.ShouldCancel) return;

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

		private CancelSignal onReportProgress(string message, string skill, string date, string person, Action cancelAction)
		{
			var handler = ReportProgress;
			if (handler != null)
			{
				var progressMessage = string.Concat(skill, " ", date, " ", person, " ", message);
				var args = new ResourceOptimizerProgressEventArgs(0, 0, progressMessage,cancelAction);

				handler(this, args);

				if (args.Cancel)
					return new CancelSignal {ShouldCancel = true};
			}
			return new CancelSignal();
		}
	}

	public class IntraIntervalOptimizationServiceToggle29846Off : IIntraIntervalOptimizationService
	{
		public void Execute(IOptimizationPreferences optimizationPreferences,
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
			var args = new ResourceOptimizerProgressEventArgs(0, 0, message,()=>{});
			handler(this, args);
		}
	}
}
