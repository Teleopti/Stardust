using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
	public class ResourceCalculationAnalyzerModel
	{
		private readonly SchedulingScreen _scheduler;
		private readonly IUndoRedoContainer _undoRedoContainer;
		private readonly ILifetimeScope _container;
		private readonly IResourceOptimizationHelperExtended _optimizationHelperExtended;
		private DateOnly _selectedDate;
		private TimeSpan? _selectedTime;

		public ResourceCalculationAnalyzerModel(SchedulingScreen scheduler, IUndoRedoContainer undoRedoContainer, ILifetimeScope container, IResourceOptimizationHelperExtended optimizationHelperExtended, DateOnly selectedDate, TimeSpan? selectedTime)
		{
			_scheduler = scheduler;
			_undoRedoContainer = undoRedoContainer;
			_container = container;
			_optimizationHelperExtended = optimizationHelperExtended;
			_selectedDate = selectedDate;
			_selectedTime = selectedTime;
		}

		public DateOnly SelectedDate
		{
			get { return _selectedDate; }
		}

		public TimeSpan? SelectedTime
		{
			get { return _selectedTime; }
		}

		public DateOnlyPeriod Period()
		{
			return _scheduler.SchedulerState.RequestedPeriod.DateOnlyPeriod;
		}

		public IDictionary<ISkill, ResourceCalculationAnalyzerModelResult> AnalyzeLastChange(DateTime localDateTime)
		{
			if(_undoRedoContainer.InUndoRedo)
				return null;

			if (!_undoRedoContainer.CanUndo())
				return null;

			var utcDateTime = TimeZoneHelper.ConvertToUtc(localDateTime, _scheduler.SchedulerState.TimeZoneInfo).AddTicks(1);
			var result = new Dictionary<ISkill, ResourceCalculationAnalyzerModelResult>();
			var skills =
				_scheduler.SchedulerState.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.Keys.Where(
					s => s.SkillType.ForecastSource != ForecastSource.MaxSeatSkill);
			
			_undoRedoContainer.Undo();
			var period = new DateOnlyPeriod(_scheduler.SchedulerState.DaysToRecalculate.Min(),
				_scheduler.SchedulerState.DaysToRecalculate.Max());
			var skillStaffperiods = _scheduler.SchedulerState.SchedulingResultState.SkillStaffPeriodHolder.IntersectingSkillStaffPeriodList(skills,
					new DateTimePeriod(utcDateTime, utcDateTime));

			foreach (var skillStaffperiod in skillStaffperiods)
			{
				result.Add(skillStaffperiod.SkillDay.Skill, new ResourceCalculationAnalyzerModelResult());
			}

			using (_container.Resolve<SharedResourceContextOldSchedulingScreenBehaviorWithoutShoveling>().MakeSureExists(period))
			{
				_optimizationHelperExtended.ResourceCalculateMarkedDays(new NoSchedulingProgress(), _scheduler.SchedulerState.ConsiderShortBreaks, true);
			}

			skillStaffperiods =
				_scheduler.SchedulerState.SchedulingResultState.SkillStaffPeriodHolder.IntersectingSkillStaffPeriodList(skills,
					new DateTimePeriod(utcDateTime, utcDateTime));
			foreach (var skillStaffperiod in skillStaffperiods)
			{
				var modelResult = result[skillStaffperiod.SkillDay.Skill];
				modelResult.PrimaryPercentBefore = new Percent(skillStaffperiod.RelativeDifferenceForDisplayOnly);
				modelResult.PrimaryResourcesBefore = skillStaffperiod.CalculatedResource;
			}

			foreach (var dateOnly in period.DayCollection())
			{
				_scheduler.SchedulerState.MarkDateToBeRecalculated(dateOnly);
			}

			using (_container.Resolve<SharedResourceContextOldSchedulingScreenBehavior>().MakeSureExists(period))
			{
				_optimizationHelperExtended.ResourceCalculateMarkedDays(new NoSchedulingProgress(), _scheduler.SchedulerState.ConsiderShortBreaks, true);
			}

			skillStaffperiods =
				_scheduler.SchedulerState.SchedulingResultState.SkillStaffPeriodHolder.IntersectingSkillStaffPeriodList(skills,
					new DateTimePeriod(utcDateTime, utcDateTime));
			foreach (var skillStaffperiod in skillStaffperiods)
			{
				var modelResult = result[skillStaffperiod.SkillDay.Skill];
				modelResult.ShoveledPercentBefore = new Percent(skillStaffperiod.RelativeDifferenceForDisplayOnly);
				modelResult.ShoveledResourcesBefore = skillStaffperiod.CalculatedResource;
			}

			_undoRedoContainer.Redo();

			using (_container.Resolve<SharedResourceContextOldSchedulingScreenBehaviorWithoutShoveling>().MakeSureExists(period))
			{
				_optimizationHelperExtended.ResourceCalculateMarkedDays(new NoSchedulingProgress(), _scheduler.SchedulerState.ConsiderShortBreaks, true);
			}
			//after
			skillStaffperiods =
				_scheduler.SchedulerState.SchedulingResultState.SkillStaffPeriodHolder.IntersectingSkillStaffPeriodList(skills,
					new DateTimePeriod(utcDateTime, utcDateTime));
			foreach (var skillStaffperiod in skillStaffperiods)
			{
				var modelResult = result[skillStaffperiod.SkillDay.Skill];
				modelResult.PrimaryPercentAfter = new Percent(skillStaffperiod.RelativeDifferenceForDisplayOnly);
				modelResult.PrimaryResourcesAfter = skillStaffperiod.CalculatedResource;
			}

			foreach (var dateOnly in period.DayCollection())
			{
				_scheduler.SchedulerState.MarkDateToBeRecalculated(dateOnly);
			}

			using (_container.Resolve<SharedResourceContextOldSchedulingScreenBehavior>().MakeSureExists(period))
			{
				_optimizationHelperExtended.ResourceCalculateMarkedDays(new NoSchedulingProgress(), _scheduler.SchedulerState.ConsiderShortBreaks, true);
			}
			//after
			skillStaffperiods =
				_scheduler.SchedulerState.SchedulingResultState.SkillStaffPeriodHolder.IntersectingSkillStaffPeriodList(skills,
					new DateTimePeriod(utcDateTime, utcDateTime));
			foreach (var skillStaffperiod in skillStaffperiods)
			{
				var modelResult = result[skillStaffperiod.SkillDay.Skill];
				modelResult.ShoveledPercentAfter = new Percent(skillStaffperiod.RelativeDifferenceForDisplayOnly);
				modelResult.ShoveledResourcesAfter = skillStaffperiod.CalculatedResource;
			}

			return result;
		}

		public class ResourceCalculationAnalyzerModelResult
		{
			public Percent ShoveledPercentBefore { get; set; }
			public Percent ShoveledPercentAfter { get; set; }
			public double ShoveledResourcesBefore { get; set; }
			public double ShoveledResourcesAfter { get; set; }
			public Percent PrimaryPercentBefore { get; set; }
			public Percent PrimaryPercentAfter { get; set; }
			public double PrimaryResourcesBefore { get; set; }
			public double PrimaryResourcesAfter { get; set; }
		}
	}

	
}
