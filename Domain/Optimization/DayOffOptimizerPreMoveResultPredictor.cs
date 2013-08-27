﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDayOffOptimizerPreMoveResultPredictor
	{
		double PredictedValue(IScheduleMatrixPro matrix, ILockableBitArray workingBitArray,
								ILockableBitArray originalBitArray, IDaysOffPreferences daysOffPreferences);

		double CurrentValue(IScheduleMatrixPro matrix);
	}

	public class DayOffOptimizerPreMoveResultPredictor : IDayOffOptimizerPreMoveResultPredictor
	{
		private readonly IDailySkillForecastAndScheduledValueCalculator _dailySkillForecastAndScheduledValueCalculator;
		private readonly IPopulationStatisticsCalculator _populationStatisticsCalculator;
		private readonly IDeviationStatisticData _deviationStatisticData;

		public DayOffOptimizerPreMoveResultPredictor(IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator,
			IPopulationStatisticsCalculator populationStatisticsCalculator, IDeviationStatisticData deviationStatisticData)

		{
			_dailySkillForecastAndScheduledValueCalculator = dailySkillForecastAndScheduledValueCalculator;
			_populationStatisticsCalculator = populationStatisticsCalculator;
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

		private double calculateValue(IDictionary<DateOnly, IForecastScheduleValuePair> rawDataDic)
		{
			foreach (var forecastScheduleValuePair in rawDataDic.Values)
			{
				if(forecastScheduleValuePair.ForecastValue > 0)
				{
					double diff = _deviationStatisticData.CalculateRelativeDeviation(forecastScheduleValuePair.ForecastValue,
					                                                                 forecastScheduleValuePair.ScheduleValue);
					_populationStatisticsCalculator.AddItem(diff);
				}
			}
			_populationStatisticsCalculator.Analyze();
			return _populationStatisticsCalculator.StandardDeviation;
		}

		private static void modifyRawData(ILockableBitArray workingBitArray, IScheduleMatrixPro matrix,
		                             ILockableBitArray originalBitArray, IDaysOffPreferences daysOffPreferences,
									IDictionary<DateOnly, IForecastScheduleValuePair> rawDataDic, TimeSpan averageWorkTime)
		{
			int bitArrayToMatrixOffset = 0;
			if (!daysOffPreferences.ConsiderWeekBefore)
				bitArrayToMatrixOffset = 7;

			for (int i = 0; i < workingBitArray.Count; i++)
			{
				IScheduleDayPro scheduleDayPro =
						matrix.OuterWeeksPeriodDays[i + bitArrayToMatrixOffset];
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
}