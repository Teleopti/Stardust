using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDayOffOptimizerPreMoveResultPredictor
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		double PredictedValue(IScheduleMatrixPro matrix, ILockableBitArray workingBitArray,
			ILockableBitArray originalBitArray, IDaysOffPreferences daysOffPreferences);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		double CurrentValue(IScheduleMatrixPro matrix);

		PredictorResult IsPredictedBetterThanCurrent(IScheduleMatrixPro matrix, ILockableBitArray workingBitArray,
			ILockableBitArray originalBitArray, IDaysOffPreferences daysOffPreferences);
	}

	public class DayOffOptimizerPreMoveResultPredictor : IDayOffOptimizerPreMoveResultPredictor
	{
		private readonly IDailySkillForecastAndScheduledValueCalculator _dailySkillForecastAndScheduledValueCalculator;
		private readonly IDeviationStatisticData _deviationStatisticData;

		public DayOffOptimizerPreMoveResultPredictor(IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator, IDeviationStatisticData deviationStatisticData)

		{
			_dailySkillForecastAndScheduledValueCalculator = dailySkillForecastAndScheduledValueCalculator;
			_deviationStatisticData = deviationStatisticData;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public double PredictedValue(IScheduleMatrixPro matrix, ILockableBitArray workingBitArray,
		                             ILockableBitArray originalBitArray, IDaysOffPreferences daysOffPreferences)
		{
			IVirtualSchedulePeriod schedulePeriod = matrix.SchedulePeriod;
			IEnumerable<ISkill> personalSkills = extractSkills(matrix);
			var rawDataDictionary = createRawDataDictionary(schedulePeriod, personalSkills);
			TimeSpan averageWorkTime = schedulePeriod.AverageWorkTimePerDay;
			modifyRawData(workingBitArray, matrix, originalBitArray, daysOffPreferences, rawDataDictionary, averageWorkTime);
			double result = calculateValue(rawDataDictionary);

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public double CurrentValue(IScheduleMatrixPro matrix)
		{
			IVirtualSchedulePeriod schedulePeriod = matrix.SchedulePeriod;
			IEnumerable<ISkill> personalSkills = extractSkills(matrix);
			var rawDataDictionary = createRawDataDictionary(schedulePeriod, personalSkills);
			double result = calculateValue(rawDataDictionary);

			return result;
		}

		public PredictorResult IsPredictedBetterThanCurrent(IScheduleMatrixPro matrix, ILockableBitArray workingBitArray,
									 ILockableBitArray originalBitArray, IDaysOffPreferences daysOffPreferences)
		{
			IVirtualSchedulePeriod schedulePeriod = matrix.SchedulePeriod;
			IEnumerable<ISkill> personalSkills = extractSkills(matrix);
			var rawDataDictionary = createRawDataDictionary(schedulePeriod, personalSkills);
			double currentResult = calculateValue(rawDataDictionary);

			TimeSpan averageWorkTime = schedulePeriod.AverageWorkTimePerDay;
			modifyRawData(workingBitArray, matrix, originalBitArray, daysOffPreferences, rawDataDictionary, averageWorkTime);
			double predictedResult = calculateValue(rawDataDictionary);

			return new PredictorResult { CurrentValue = currentResult, IsBetter = predictedResult < currentResult };
		}

		private double calculateValue(IDictionary<DateOnly, IForecastScheduleValuePair> rawDataDic)
		{
			var values =
				rawDataDic.Values.Where(x => x.ForecastValue > 0)
				          .Select(x => _deviationStatisticData.CalculateRelativeDeviation(x.ForecastValue, x.ScheduleValue));
			return Calculation.Variances.StandardDeviation(values);
		}

		private static void modifyRawData(ILockableBitArray workingBitArray, IScheduleMatrixPro matrix,
		                             ILockableBitArray originalBitArray, IDaysOffPreferences daysOffPreferences,
									IDictionary<DateOnly, IForecastScheduleValuePair> rawDataDic, TimeSpan averageWorkTime)
		{
			int bitArrayToMatrixOffset = 0;
			if (!daysOffPreferences.ConsiderWeekBefore)
				bitArrayToMatrixOffset = 7;

			var outerWeeksPeriodDays = matrix.OuterWeeksPeriodDays;
			for (int i = 0; i < workingBitArray.Count; i++)
			{
				IScheduleDayPro scheduleDayPro =
						outerWeeksPeriodDays[i + bitArrayToMatrixOffset];
				DateOnly date = scheduleDayPro.Day;
				if (workingBitArray[i] && !originalBitArray[i])
				{
					//remove avg wt from raw values
					rawDataDic[date].ScheduleValue = rawDataDic[date].ScheduleValue - averageWorkTime.TotalMinutes;
				}
				if (!workingBitArray[i] && originalBitArray[i])
				{
					//add avg wt to raw values
					rawDataDic[date].ScheduleValue = rawDataDic[date].ScheduleValue + averageWorkTime.TotalMinutes;
				}
			}
		}

		private IDictionary<DateOnly, IForecastScheduleValuePair> createRawDataDictionary(IVirtualSchedulePeriod schedulePeriod, IEnumerable<ISkill> personalSkills)
		{
			var retDic = new Dictionary<DateOnly, IForecastScheduleValuePair>();
			foreach (DateOnly key in schedulePeriod.DateOnlyPeriod.DayCollection())
			{
				double dailyForecast = 0;
				double dailyScheduled = 0;
				foreach (ISkill skill in personalSkills)
				{
					IForecastScheduleValuePair forecastScheduleValuePairForSkill = _dailySkillForecastAndScheduledValueCalculator.CalculateDailyForecastAndScheduleDataForSkill(skill, key);
					dailyForecast += forecastScheduleValuePairForSkill.ForecastValue;
					dailyScheduled += forecastScheduleValuePairForSkill.ScheduleValue;
				}

				var value = new ForecastScheduleValuePair { ForecastValue = dailyForecast, ScheduleValue = dailyScheduled };
				retDic.Add(key, value);
			}

			return retDic;
		}

		private static IEnumerable<ISkill> extractSkills(IScheduleMatrixPro matrix)
		{
			DateOnly firstPeriodDay = matrix.EffectivePeriodDays[0].Day;
			var personalSkills = matrix.Person.Period(firstPeriodDay).PersonSkillCollection;
			return personalSkills.Select(personalSkill => personalSkill.Skill).ToList();
		}
	}

	public class PredictorResult
	{
		public bool IsBetter { get; set; }
		public double CurrentValue { get; set; }
	}
}