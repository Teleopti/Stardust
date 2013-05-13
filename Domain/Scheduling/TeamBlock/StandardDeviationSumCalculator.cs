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
		private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
		private readonly IScheduleResultDataExtractorProvider _dataExtractorProvider;

		public StandardDeviationSumCalculator(ILockableBitArrayFactory lockableBitArrayFactory, IScheduleResultDataExtractorProvider dataExtractorProvider)
		{
			_lockableBitArrayFactory = lockableBitArrayFactory;
			_dataExtractorProvider = dataExtractorProvider;
		}


		public double Calculate(DateOnlyPeriod dateOnlyPeriod, IList<IScheduleMatrixPro> scheduleMatrixes,
								IOptimizationPreferences optimizerPreferences, ISchedulingOptions schedulingOptions)
		{
			var standardDeviationData = new StandardDeviationData();
			var sum = 0.0;
			foreach (var matrix in scheduleMatrixes)
			{
				var scheduleResultDataExtractor =
					_dataExtractorProvider.CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(matrix, schedulingOptions);
				var values = scheduleResultDataExtractor.Values();
				var periodDays = matrix.EffectivePeriodDays;
				for (var i = 0; i < periodDays.Count; i++)
				{
					var originalArray = _lockableBitArrayFactory.ConvertFromMatrix(optimizerPreferences.DaysOff.ConsiderWeekBefore,
					                                                               optimizerPreferences.DaysOff.ConsiderWeekAfter,
					                                                               matrix);
					if (originalArray.UnlockedIndexes.Contains(i) && !originalArray.DaysOffBitArray[i])
						if (!standardDeviationData.Data.ContainsKey(periodDays[i].Day))
							standardDeviationData.Add(periodDays[i].Day, values[i]);
				}
			}
			foreach (var visibleDay in dateOnlyPeriod.DayCollection())
			{
				if (!standardDeviationData.Data.ContainsKey(visibleDay)) continue;
				var value = standardDeviationData.Data[visibleDay];
				if (value.HasValue)
					sum += value.Value;
			}
			return sum;
		}
	}
}