using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	/// <summary>
	/// HelperClass for calculating resources
	/// </summary>
	public class ResourceOptimizationHelperExtended : IResourceOptimizationHelperExtended
	{
		private readonly IResourceOptimizationHelper _basicHelper;
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;

		public ResourceOptimizationHelperExtended(IResourceOptimizationHelper basicHelper, Func<ISchedulerStateHolder> stateHolder, Func<IPersonSkillProvider> personSkillProvider)
		{
			_basicHelper = basicHelper;
			_stateHolder = stateHolder;
			_personSkillProvider = personSkillProvider;
		}

		public void ResourceCalculateAllDays(IBackgroundWorkerWrapper backgroundWorker, bool useOccupancyAdjustment)
		{
			var stateHolder = _stateHolder();
			if (!stateHolder.SchedulingResultState.Skills.Any()) return;

			var period = new DateOnlyPeriod(stateHolder.RequestedPeriod.DateOnlyPeriod.StartDate.AddDays(-10), stateHolder.RequestedPeriod.DateOnlyPeriod.EndDate.AddDays(2));
			var extractor = new ScheduleProjectionExtractor(_personSkillProvider(), stateHolder.SchedulingResultState.Skills.Min(s => s.DefaultResolution));
			var resources = extractor.CreateRelevantProjectionList(stateHolder.Schedules, period.ToDateTimePeriod(stateHolder.TimeZoneInfo));
			backgroundWorker.ReportProgress(1);
			using (new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(resources))
			{
				resourceCalculateDays(backgroundWorker, useOccupancyAdjustment, stateHolder.ConsiderShortBreaks,
					stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection());
			}
		}

		private void prepareAndCalculateDate(DateOnly date, bool useOccupancyAdjustment, bool considerShortBreaks)
		{
			using (PerformanceOutput.ForOperation("PrepareAndCalculateDate " + date.ToShortDateString(CultureInfo.CurrentCulture)))
			{
				_basicHelper.ResourceCalculateDate(date, useOccupancyAdjustment, considerShortBreaks);
			}
		}

		public void ResourceCalculateMarkedDays(IBackgroundWorkerWrapper backgroundWorker, bool considerShortBreaks, bool useOccupancyAdjustment)
		{
			var stateHolder = _stateHolder();
			if (!stateHolder.DaysToRecalculate.Any()) return;
			if (!stateHolder.SchedulingResultState.Skills.Any()) return;

			var period = new DateOnlyPeriod(stateHolder.DaysToRecalculate.Min().AddDays(-1), stateHolder.DaysToRecalculate.Max());
			var extractor = new ScheduleProjectionExtractor(_personSkillProvider(), stateHolder.SchedulingResultState.Skills.Min(s => s.DefaultResolution));
			var resources = extractor.CreateRelevantProjectionList(stateHolder.Schedules, period.ToDateTimePeriod(stateHolder.TimeZoneInfo));
			using (new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(resources))
			{
				resourceCalculateDays(backgroundWorker, useOccupancyAdjustment, considerShortBreaks,
					stateHolder.DaysToRecalculate.ToList());
				stateHolder.ClearDaysToRecalculate();
			}
		}

		private void resourceCalculateDays(IBackgroundWorkerWrapper backgroundWorker, bool useOccupancyAdjustment, bool considerShortBreaks, ICollection<DateOnly> datesList)
		{
			if (datesList.Count == 0)
				return;

			var cancel = false;
			foreach (var date in datesList)
			{
				prepareAndCalculateDate(date, useOccupancyAdjustment, considerShortBreaks);

				if (backgroundWorker != null)
				{
					var progress = new ResourceOptimizerProgressEventArgs(0, 0, string.Empty, ()=>cancel=true);

					backgroundWorker.ReportProgress(1, progress);

					if (cancel || backgroundWorker.CancellationPending)
					{
						return;
					}
				}
			}
		}
	}
}