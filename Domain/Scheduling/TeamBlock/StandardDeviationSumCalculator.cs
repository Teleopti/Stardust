using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IStandardDeviationSumCalculator
	{
		double Calculate(DateOnlyPeriod dateOnlyPeriod, IList<IScheduleMatrixPro> scheduleMatrixes,
										 IOptimizationPreferences optimizerPreferences, ISchedulingOptions schedulingOptions);
	}
	public class StandardDeviationSumCalculator : IStandardDeviationSumCalculator
	{
        private readonly IRelativeDailyValueCalculatorForTeamBlock _dataExtractorProvider;

        public StandardDeviationSumCalculator(IRelativeDailyValueCalculatorForTeamBlock dataExtractorProvider)
		{
			_dataExtractorProvider = dataExtractorProvider;
		}


		public double Calculate(DateOnlyPeriod dateOnlyPeriod, IList<IScheduleMatrixPro> scheduleMatrixes,
								IOptimizationPreferences optimizerPreferences, ISchedulingOptions schedulingOptions)
		{
			var periodIntervalData = new PeriodIntervalData();
			var sum = 0.0;
			foreach (var matrix in scheduleMatrixes)
			{
                var values = _dataExtractorProvider.Values(matrix, optimizerPreferences.Advanced);
				var periodDays = matrix.EffectivePeriodDays;
				for (var i = 0; i < periodDays.Count; i++)
				{
					if (!periodIntervalData.Data.ContainsKey(periodDays[i].Day))
							periodIntervalData.Add(periodDays[i].Day, values[i]);
				}
			}
			foreach (var visibleDay in dateOnlyPeriod.DayCollection())
			{
				if (!periodIntervalData.Data.ContainsKey(visibleDay)) continue;
				var value = periodIntervalData.Data[visibleDay];
				if (value.HasValue)
					sum += value.Value;
			}
			return sum;
		}
	}
}