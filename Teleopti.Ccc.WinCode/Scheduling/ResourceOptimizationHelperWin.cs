using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	/// <summary>
	/// HelperClass for calculating resources
	/// </summary>
	/// <remarks>
	/// Created from OptimizerHelper
	/// I think it should be refact. and moved to Domain
	/// </remarks>
	public class ResourceOptimizationHelperWin : ResourceOptimizationHelper, IResourceOptimizationHelperWin
	{
		private readonly ISchedulerStateHolder _stateHolder;
		private readonly IPersonSkillProvider _personSkillProvider = new PersonSkillProvider();
		private ResourceOptimizerProgressEventArgs _progressEvent;

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceOptimizationHelperWin"/> class.
		/// </summary>
		/// <param name="stateHolder">The state holder.</param>
		/// <param name="personSkillProvider"></param>
		/// <param name="intraIntervalFinderService"></param>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2008-05-27
		/// </remarks>
		public ResourceOptimizationHelperWin(ISchedulerStateHolder stateHolder, IPersonSkillProvider personSkillProvider, IIntraIntervalFinderService intraIntervalFinderService)
			: base(stateHolder.SchedulingResultState, new OccupiedSeatCalculator(), new NonBlendSkillCalculator(), personSkillProvider, new PeriodDistributionService(), new CurrentTeleoptiPrincipal(), intraIntervalFinderService)
		{
			_stateHolder = stateHolder;
			_personSkillProvider = personSkillProvider;
		}

		public void ResourceCalculateAllDays(IBackgroundWorkerWrapper backgroundWorker, bool useOccupancyAdjustment)
		{
			_progressEvent = null;
			if (!_stateHolder.SchedulingResultState.Skills.Any()) return;

			var period = new DateOnlyPeriod(_stateHolder.RequestedPeriod.DateOnlyPeriod.StartDate.AddDays(-10), _stateHolder.RequestedPeriod.DateOnlyPeriod.EndDate.AddDays(2));
			var extractor = new ScheduleProjectionExtractor(_personSkillProvider, _stateHolder.SchedulingResultState.Skills.Min(s => s.DefaultResolution));
			var resources = extractor.CreateRelevantProjectionList(_stateHolder.Schedules, period.ToDateTimePeriod(_stateHolder.TimeZoneInfo));
			backgroundWorker.ReportProgress(1);
			using (new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(resources))
			{
				resourceCalculateDays(backgroundWorker, useOccupancyAdjustment, _stateHolder.ConsiderShortBreaks,
					_stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection());
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
			if (!_stateHolder.DaysToRecalculate.Any()) return;
			if (!_stateHolder.SchedulingResultState.Skills.Any()) return;

			var period = new DateOnlyPeriod(_stateHolder.DaysToRecalculate.Min().AddDays(-1), _stateHolder.DaysToRecalculate.Max());
			var extractor = new ScheduleProjectionExtractor(_personSkillProvider, _stateHolder.SchedulingResultState.Skills.Min(s => s.DefaultResolution));
			var resources = extractor.CreateRelevantProjectionList(_stateHolder.Schedules, period.ToDateTimePeriod(_stateHolder.TimeZoneInfo));
			using (new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(resources))
			{
				resourceCalculateDays(backgroundWorker, useOccupancyAdjustment, considerShortBreaks,
					_stateHolder.DaysToRecalculate.ToList());
				_stateHolder.ClearDaysToRecalculate();
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