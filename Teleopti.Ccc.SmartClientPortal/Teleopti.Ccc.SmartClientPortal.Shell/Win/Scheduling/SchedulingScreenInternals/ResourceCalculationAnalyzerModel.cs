using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
	public class ResourceCalculationAnalyzerModel
	{
		private readonly ISchedulerStateHolder _stateHolder;
		private readonly ILifetimeScope _container;
		private readonly IResourceOptimizationHelperExtended _optimizationHelperExtended;
		private readonly DateOnly _selectedDate;
		private readonly TimeSpan? _selectedTime;

		public ResourceCalculationAnalyzerModel(ISchedulerStateHolder stateHolder, ILifetimeScope container, IResourceOptimizationHelperExtended optimizationHelperExtended, DateOnly selectedDate, TimeSpan? selectedTime)
		{
			_stateHolder = stateHolder;
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

			foreach (var dateOnly in period.DayCollection())
			{
				_stateHolder.MarkDateToBeRecalculated(dateOnly);
			}

			using (_container.Resolve<SharedResourceContextOldSchedulingScreenBehaviorWithoutShoveling>().MakeSureExists(period))
			{
				_optimizationHelperExtended.ResourceCalculateMarkedDays(new NoSchedulingProgress(),
					_stateHolder.ConsiderShortBreaks, true);
			}

			worker.ReportProgress(4, "Analyzing Step 2...");

			skillStaffperiods =
				_stateHolder.SchedulingResultState.SkillStaffPeriodHolder.IntersectingSkillStaffPeriodList(skills,
					new DateTimePeriod(utcDateTime, utcDateTime));
			foreach (var skillStaffperiod in skillStaffperiods)
			{
				var modelResult = result[skillStaffperiod.SkillDay.Skill];
				modelResult.PrimaryPercent = new Percent(skillStaffperiod.RelativeDifferenceForDisplayOnly);
				modelResult.PrimaryResources = skillStaffperiod.CalculatedResource;
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

			worker.ReportProgress(5, "Analyzing Step 3...");

			skillStaffperiods =
				_stateHolder.SchedulingResultState.SkillStaffPeriodHolder.IntersectingSkillStaffPeriodList(skills,
					new DateTimePeriod(utcDateTime, utcDateTime));
			foreach (var skillStaffperiod in skillStaffperiods)
			{
				var modelResult = result[skillStaffperiod.SkillDay.Skill];
				modelResult.ShoveledPercent = new Percent(skillStaffperiod.RelativeDifferenceForDisplayOnly);
				modelResult.ShoveledResources = skillStaffperiod.CalculatedResource;
			}

			return result;
		}

		public class ResourceCalculationAnalyzerModelResult
		{
			public Percent ShoveledPercent { get; set; }
			public double ShoveledResources { get; set; }
			public Percent PrimaryPercent { get; set; }
			public double PrimaryResources { get; set; }
		}
	}

	
}
