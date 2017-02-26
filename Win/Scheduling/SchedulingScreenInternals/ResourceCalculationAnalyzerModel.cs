using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
	public class ResourceCalculationAnalyzerModel
	{
		private readonly ISchedulerStateHolder _stateHolder;
		private readonly IUndoRedoContainer _undoRedoContainer;
		private readonly ILifetimeScope _container;
		private readonly IResourceOptimizationHelperExtended _optimizationHelperExtended;
		private DateOnly _selectedDate;
		private TimeSpan? _selectedTime;

		public ResourceCalculationAnalyzerModel(ISchedulerStateHolder stateHolder, IUndoRedoContainer undoRedoContainer, ILifetimeScope container, IResourceOptimizationHelperExtended optimizationHelperExtended, DateOnly selectedDate, TimeSpan? selectedTime)
		{
			_stateHolder = stateHolder;
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
			return _stateHolder.RequestedPeriod.DateOnlyPeriod;
		}

		public IDictionary<ISkill, ResourceCalculationAnalyzerModelResult> AnalyzeLastChange(DateTime localDateTime, BackgroundWorker worker)
		{
			if (_undoRedoContainer.InUndoRedo)
				return null;

			if (!_undoRedoContainer.CanUndo())
				return null;

			worker.ReportProgress(1, "Analyzing Step 1...");
			var utcDateTime = TimeZoneHelper.ConvertToUtc(localDateTime, _stateHolder.TimeZoneInfo).AddTicks(1);
			var result = new Dictionary<ISkill, ResourceCalculationAnalyzerModelResult>();
			var skills =
				_stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.Keys.Where(
					s => s.SkillType.ForecastSource != ForecastSource.MaxSeatSkill);

			var period = new DateOnlyPeriod(new DateOnly(localDateTime.Date), new DateOnly(localDateTime.Date));
			IList<ISkillStaffPeriod> skillStaffperiods;
			skillStaffperiods =
				_stateHolder.SchedulingResultState.SkillStaffPeriodHolder.IntersectingSkillStaffPeriodList(skills,
					new DateTimePeriod(utcDateTime, utcDateTime));

			foreach (var skillStaffperiod in skillStaffperiods)
			{
				result.Add(skillStaffperiod.SkillDay.Skill, new ResourceCalculationAnalyzerModelResult());
			}

			_undoRedoContainer.Undo();

			using (_container.Resolve<SharedResourceContextOldSchedulingScreenBehaviorWithoutShoveling>().MakeSureExists(period))
			{
				_optimizationHelperExtended.ResourceCalculateMarkedDays(new NoSchedulingProgress(),
					_stateHolder.ConsiderShortBreaks, true);
			}

			worker.ReportProgress(2, "Analyzing Step 2...");

			skillStaffperiods =
				_stateHolder.SchedulingResultState.SkillStaffPeriodHolder.IntersectingSkillStaffPeriodList(skills,
					new DateTimePeriod(utcDateTime, utcDateTime));
			foreach (var skillStaffperiod in skillStaffperiods)
			{
				var modelResult = result[skillStaffperiod.SkillDay.Skill];
				modelResult.PrimaryPercentBefore = new Percent(skillStaffperiod.RelativeDifferenceForDisplayOnly);
				modelResult.PrimaryResourcesBefore = skillStaffperiod.CalculatedResource;
			}

			foreach (var dateOnly in period.DayCollection())
			{
				_stateHolder.MarkDateToBeRecalculated(dateOnly);
			}

			using (_container.Resolve<SharedResourceContextOldSchedulingScreenBehavior>().MakeSureExists(period))
			{
				_optimizationHelperExtended.ResourceCalculateMarkedDays(new NoSchedulingProgress(),
					_stateHolder.ConsiderShortBreaks, true);
			}

			worker.ReportProgress(3, "Analyzing Step 3...");

			skillStaffperiods =
				_stateHolder.SchedulingResultState.SkillStaffPeriodHolder.IntersectingSkillStaffPeriodList(skills,
					new DateTimePeriod(utcDateTime, utcDateTime));
			foreach (var skillStaffperiod in skillStaffperiods)
			{
				var modelResult = result[skillStaffperiod.SkillDay.Skill];
				modelResult.ShoveledPercentBefore = new Percent(skillStaffperiod.RelativeDifferenceForDisplayOnly);
				modelResult.ShoveledResourcesBefore = skillStaffperiod.CalculatedResource;
			}

			_undoRedoContainer.Redo();


			//after
			using (_container.Resolve<SharedResourceContextOldSchedulingScreenBehaviorWithoutShoveling>().MakeSureExists(period))
			{
				_optimizationHelperExtended.ResourceCalculateMarkedDays(new NoSchedulingProgress(),
					_stateHolder.ConsiderShortBreaks, true);
			}

			worker.ReportProgress(4, "Analyzing Step 4...");

			skillStaffperiods =
				_stateHolder.SchedulingResultState.SkillStaffPeriodHolder.IntersectingSkillStaffPeriodList(skills,
					new DateTimePeriod(utcDateTime, utcDateTime));
			foreach (var skillStaffperiod in skillStaffperiods)
			{
				var modelResult = result[skillStaffperiod.SkillDay.Skill];
				modelResult.PrimaryPercentAfter = new Percent(skillStaffperiod.RelativeDifferenceForDisplayOnly);
				modelResult.PrimaryResourcesAfter = skillStaffperiod.CalculatedResource;
			}

			foreach (var dateOnly in period.DayCollection())
			{
				_stateHolder.MarkDateToBeRecalculated(dateOnly);
			}

			using (_container.Resolve<SharedResourceContextOldSchedulingScreenBehavior>().MakeSureExists(period))
			{
				_optimizationHelperExtended.ResourceCalculateMarkedDays(new NoSchedulingProgress(),
					_stateHolder.ConsiderShortBreaks, true);
			}

			worker.ReportProgress(5, "Analyzing Step 5...");

			skillStaffperiods =
				_stateHolder.SchedulingResultState.SkillStaffPeriodHolder.IntersectingSkillStaffPeriodList(skills,
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
