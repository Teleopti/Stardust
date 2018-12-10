using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	/// <summary>
	/// HelperClass for calculating resources
	/// </summary>
	public class ResourceOptimizationHelperExtended : IResourceOptimizationHelperExtended
	{
		private readonly IResourceCalculation _basicHelper;
		private readonly Func<ISchedulerStateHolder> _stateHolder;

		public ResourceOptimizationHelperExtended(IResourceCalculation basicHelper, Func<ISchedulerStateHolder> stateHolder)
		{
			_basicHelper = basicHelper;
			_stateHolder = stateHolder;
		}

		public void ResourceCalculateAllDays(ISchedulingProgress backgroundWorker, bool doIntraIntervalCalculation)
		{
			var stateHolder = _stateHolder();
			if (!stateHolder.SchedulingResultState.Skills.Any()) return;
			
			backgroundWorker.ReportProgress(1);

			resourceCalculateDays(backgroundWorker, stateHolder.ConsiderShortBreaks,
				stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection(), doIntraIntervalCalculation);
		}

		private void prepareAndCalculateDate(DateOnly date, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			using (PerformanceOutput.ForOperation("PrepareAndCalculateDate " + date.ToShortDateString(CultureInfo.CurrentCulture)))
			{
				var resCalcData = _stateHolder().SchedulingResultState.ToResourceOptimizationData(considerShortBreaks, doIntraIntervalCalculation);
				_basicHelper.ResourceCalculate(date, resCalcData);
			}
		}

		public void ResourceCalculateMarkedDays(ISchedulingProgress backgroundWorker, bool considerShortBreaks,
			bool doIntraIntervalCalculation)
		{
			var stateHolder = _stateHolder();
			if (!stateHolder.DaysToRecalculate.Any()) return;

			resourceCalculateDays(backgroundWorker, considerShortBreaks,
				stateHolder.DaysToRecalculate.ToList(), doIntraIntervalCalculation);
			stateHolder.ClearDaysToRecalculate();
		}

		private void resourceCalculateDays(ISchedulingProgress backgroundWorker, bool considerShortBreaks, ICollection<DateOnly> datesList, bool doIntraIntervalCalculation)
		{
			if (datesList.Count == 0)
				return;

			var cancel = false;
			foreach (var date in datesList)
			{
				prepareAndCalculateDate(date, considerShortBreaks, doIntraIntervalCalculation);

				if (backgroundWorker != null)
				{
					var progress = new ResourceOptimizerProgressEventArgs(0, 0, string.Empty, 100, ()=>cancel=true);

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