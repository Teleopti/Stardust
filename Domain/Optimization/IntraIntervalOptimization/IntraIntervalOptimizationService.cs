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
		private readonly ISkillDayIntraIntervalIssueExtractor _skillDayIntraIntervalIssueExtractor;
		private readonly IScheduleDayIntraIntervalIssueExtractor _scheduleDayIntraIntervalIssueExtractor;
		private readonly IIntraIntervalOptimizer _intraIntervalOptimizer;
		private string _progressSkill;
		private string _progressDate;
		private string _progressPerson;
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public IntraIntervalOptimizationService(ISkillDayIntraIntervalIssueExtractor skillDayIntraIntervalIssueExtractor, IScheduleDayIntraIntervalIssueExtractor scheduleDayIntraIntervalIssueExtractor, IIntraIntervalOptimizer intraIntervalOptimizer)
		{
			_skillDayIntraIntervalIssueExtractor = skillDayIntraIntervalIssueExtractor;
			_scheduleDayIntraIntervalIssueExtractor = scheduleDayIntraIntervalIssueExtractor;
			_intraIntervalOptimizer = intraIntervalOptimizer;	
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

				if(skill.SkillType.ForecastSource.Equals(ForecastSource.MaxSeatSkill)) continue;
				
				foreach (var dateOnly in selectedPeriod.DayCollection())
				{
					_progressDate = dateOnly.ToShortDateString(cultureInfo);
					if (_intraIntervalOptimizer.IsCanceled) break;

					var skillDays = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> {dateOnly});
					var intervalIssuesBefore = _skillDayIntraIntervalIssueExtractor.Extract(skillDays, skill);
					if (intervalIssuesBefore.Count == 0) continue;

					var scheduleDaysWithIssue = _scheduleDayIntraIntervalIssueExtractor.Extract(schedulingResultStateHolder.Schedules, dateOnly, intervalIssuesBefore, skill);

					foreach (var scheduleDay in scheduleDaysWithIssue)
					{
						if (_intraIntervalOptimizer.IsCanceled) break;

						var person = scheduleDay.Person;

						if (!personHashSet.Contains(person)) continue;

						_progressPerson = person.Name.ToString();
						schedulingOptions.ClearNotAllowedShiftProjectionCaches();
						var intervalIssuesAfter = _intraIntervalOptimizer.Optimize(schedulingOptions, rollbackService, schedulingResultStateHolder, person, dateOnly, allScheduleMatrixPros, resourceCalculateDelayer, skill, intervalIssuesBefore);

						intervalIssuesBefore.Clear();
						if (intervalIssuesAfter.Count == 0) break;

						foreach (var skillStaffPeriod in intervalIssuesAfter)
						{
							intervalIssuesBefore.Add((ISkillStaffPeriod) skillStaffPeriod.NoneEntityClone());
						}
					}
				}
			}

			_intraIntervalOptimizer.Reset();
			_intraIntervalOptimizer.ReportProgress -= OnReportProgress;
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
