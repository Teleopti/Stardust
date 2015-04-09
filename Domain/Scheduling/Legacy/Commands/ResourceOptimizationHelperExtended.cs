using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	/// <summary>
	/// HelperClass for calculating resources
	/// </summary>
	public class ResourceOptimizationHelperExtended : ResourceOptimizationHelper, IResourceOptimizationHelperExtended
	{
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;

		private ResourceOptimizerProgressEventArgs _progressEvent;

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceOptimizationHelperExtended"/> class.
		/// </summary>
		/// <param name="stateHolder">The state holder.</param>
		/// <param name="personSkillProvider"></param>
		/// <param name="intraIntervalFinderService"></param>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2008-05-27
		/// </remarks>
		public ResourceOptimizationHelperExtended(Func<ISchedulerStateHolder> stateHolder, IOccupiedSeatCalculator occupiedSeatCalculator, INonBlendSkillCalculator nonBlendSkillCalculator, Func<IPersonSkillProvider> personSkillProvider, IPeriodDistributionService periodDistributionService, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal, IIntraIntervalFinderService intraIntervalFinderService)
			: base(()=>stateHolder().SchedulingResultState, occupiedSeatCalculator, nonBlendSkillCalculator, personSkillProvider, periodDistributionService, currentTeleoptiPrincipal, intraIntervalFinderService)
		{
			_stateHolder = stateHolder;
			_personSkillProvider = personSkillProvider;
		}

		public void ResourceCalculateAllDays(IBackgroundWorkerWrapper backgroundWorker, bool useOccupancyAdjustment)
		{
			_progressEvent = null;
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
				ResourceCalculateDate(date, useOccupancyAdjustment, considerShortBreaks);
			}
		}

		public void ResourceCalculateMarkedDays(IBackgroundWorkerWrapper backgroundWorker, bool considerShortBreaks, bool useOccupancyAdjustment)
		{
			_progressEvent = null;
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

			foreach (var date in datesList)
			{
				prepareAndCalculateDate(date, useOccupancyAdjustment, considerShortBreaks);

				if (backgroundWorker != null)
				{
					var progress = new ResourceOptimizerProgressEventArgs(0, 0, string.Empty);

					backgroundWorker.ReportProgress(1, progress);

					if (backgroundWorker.CancellationPending)
					{
						return;
					}

					if (_progressEvent != null && _progressEvent.UserCancel) return;
					_progressEvent = progress;
				}
			}
		}
	}
}