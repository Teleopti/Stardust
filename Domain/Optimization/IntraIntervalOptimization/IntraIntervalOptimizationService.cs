using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public class IntraIntervalOptimizationService
	{
		private readonly ScheduleDayIntraIntervalIssueExtractor _scheduleDayIntraIntervalIssueExtractor;
		private readonly IntraIntervalOptimizer _intraIntervalOptimizer;
		private readonly IntraIntervalIssueCalculator _intraIntervalIssueCalculator;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly IUserCulture _userCulture;
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public IntraIntervalOptimizationService(
			ScheduleDayIntraIntervalIssueExtractor scheduleDayIntraIntervalIssueExtractor,
			IntraIntervalOptimizer intraIntervalOptimizer, IntraIntervalIssueCalculator intraIntervalIssueCalculator,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			IUserCulture userCulture)
		{
			_scheduleDayIntraIntervalIssueExtractor = scheduleDayIntraIntervalIssueExtractor;
			_intraIntervalOptimizer = intraIntervalOptimizer;
			_intraIntervalIssueCalculator = intraIntervalIssueCalculator;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_userCulture = userCulture;
		}

		public void Execute(IOptimizationPreferences optimizationPreferences, DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedAgents,
			ISchedulingResultStateHolder schedulingResultStateHolder, IEnumerable<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer)
		{
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			var cultureInfo = _userCulture.GetCulture();
			var agentsHash = selectedAgents.ToHashSet();

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

						if (!agentsHash.Contains(person)) continue;

						var progressPerson = person.Name.ToString();

						var affect = affectIssue(scheduleDay, intervalIssuesBefore.IssuesOnDay);
						if(!affect) continue;
						
						var intervalIssuesAfter = _intraIntervalOptimizer.Optimize(schedulingOptions, optimizationPreferences, rollbackService, schedulingResultStateHolder, person, dateOnly, allScheduleMatrixPros, resourceCalculateDelayer, skill, intervalIssuesBefore, false);

						var progressResult = onReportProgress(string.Concat("(", intervalIssuesAfter.IssuesOnDay.Count, "/", intervalIssuesAfter.IssuesOnDayAfter.Count, ")"), progressSkill, progressDate, progressPerson,()=>cancel=true, optimizationPreferences.Advanced.RefreshScreenInterval);
						if (cancel || progressResult.ShouldCancel) return;

						intervalIssuesBefore = intervalIssuesAfter;

						if (intervalIssuesAfter.IssuesOnDay.Count == 0) break;
					}

					foreach (var scheduleDay in scheduleDaysWithIssueDayAfter)
					{
						if (cancel) return;
						var person = scheduleDay.Person;

						if (!agentsHash.Contains(person)) continue;

						var progressPerson = person.Name.ToString();

						var affect = affectIssue(scheduleDay, intervalIssuesBefore.IssuesOnDayAfter);
						if (!affect) continue;

						var intervalIssuesAfter = _intraIntervalOptimizer.Optimize(schedulingOptions, optimizationPreferences, rollbackService, schedulingResultStateHolder, person, dateOnly, allScheduleMatrixPros, resourceCalculateDelayer, skill, intervalIssuesBefore, true);

						var progressResult = onReportProgress(string.Concat("(", intervalIssuesAfter.IssuesOnDay.Count, "/", intervalIssuesAfter.IssuesOnDayAfter.Count, ")"), progressSkill, progressDate, progressPerson, () => cancel = true, optimizationPreferences.Advanced.RefreshScreenInterval);
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

		private CancelSignal onReportProgress(string message, string skill, string date, string person, Action cancelAction, int screenRefreshRate)
		{
			var handler = ReportProgress;
			if (handler != null)
			{
				var progressMessage = string.Concat(skill, " ", date, " ", person, " ", message);
				var args = new ResourceOptimizerProgressEventArgs(0, 0, progressMessage, screenRefreshRate, cancelAction);

				handler(this, args);

				if (args.Cancel)
					return new CancelSignal {ShouldCancel = true};
			}
			return new CancelSignal();
		}
	}
}
