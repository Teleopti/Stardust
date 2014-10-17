using System.Collections.Generic;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IIntraIntervalOptimizationService
	{
		void Execute(ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer);
	}

	public class IntraIntervalOptimizationService : IIntraIntervalOptimizationService
	{
		private readonly ISkillDayIntraIntervalIssueExtractor _skillDayIntraIntervalIssueExtractor;
		private readonly IScheduleDayIntraIntervalIssueExtractor _scheduleDayIntraIntervalIssueExtractor;
		private readonly IIntraIntervalOptimizer _intraIntervalOptimizer;

		public IntraIntervalOptimizationService(ISkillDayIntraIntervalIssueExtractor skillDayIntraIntervalIssueExtractor, IScheduleDayIntraIntervalIssueExtractor scheduleDayIntraIntervalIssueExtractor, IIntraIntervalOptimizer intraIntervalOptimizer)
		{
			_skillDayIntraIntervalIssueExtractor = skillDayIntraIntervalIssueExtractor;
			_scheduleDayIntraIntervalIssueExtractor = scheduleDayIntraIntervalIssueExtractor;
			_intraIntervalOptimizer = intraIntervalOptimizer;
		}

		public void Execute(ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer)
		{
			var personHashSet = new HashSet<IPerson>();

			foreach (var selectedSchedule in selectedSchedules)
			{
				personHashSet.Add(selectedSchedule.Person);
			}

			var clonedSchedulingOptions = (SchedulingOptions)schedulingOptions.Clone();
			clonedSchedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay;
			clonedSchedulingOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight{Key = "SingleAgentTeam", Name = Resources.SingleAgentTeam};

			foreach (var skill in schedulingResultStateHolder.Skills)
			{
				if(skill.SkillType.ForecastSource.Equals(ForecastSource.MaxSeatSkill)) continue;
				
				foreach (var dateOnly in selectedPeriod.DayCollection())
				{
					var skillDays = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> {dateOnly});
					var intervalIssuesBefore = _skillDayIntraIntervalIssueExtractor.Extract(skillDays, skill);
					if (intervalIssuesBefore.Count == 0) continue;

					var scheduleDaysWithIssue = _scheduleDayIntraIntervalIssueExtractor.Extract(schedulingResultStateHolder.Schedules, dateOnly, intervalIssuesBefore, skill);

					foreach (var scheduleDay in scheduleDaysWithIssue)
					{
						var person = scheduleDay.Person;

						if (!personHashSet.Contains(person)) continue;

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
		}
	}

	public class IntraIntervalOptimizationServiceToggle29846Off : IIntraIntervalOptimizationService
	{
		public void Execute(ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer)
		{
				
		}
	}
}
