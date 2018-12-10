using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizerPreMoveResultPredictor
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;
		private readonly ForecastAndScheduleSumForDay _forecastAndScheduleSumForDay;

		public DayOffOptimizerPreMoveResultPredictor(PersonalSkillsProvider personalSkillsProvider, ForecastAndScheduleSumForDay forecastAndScheduleSumForDay)
		{
			_personalSkillsProvider = personalSkillsProvider;
			_forecastAndScheduleSumForDay = forecastAndScheduleSumForDay;
		}

		public WasReallyBetterResult WasReallyBetter(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences,
			ISchedulingResultStateHolder schedulingResultStateHolder, MovedDaysOff movedDaysOff, PredictorResult previousPredictorResult)
		{
			var rawDataDictionary = createRawDataDictionary(matrix, optimizationPreferences, schedulingResultStateHolder);
			var currPredictorValue = calculateValue(rawDataDictionary);
			return previousPredictorResult.IsBetterThan(currPredictorValue, breaksMinimumAgents(rawDataDictionary, movedDaysOff));
		}

		public PredictorResult IsPredictedBetterThanCurrent(IScheduleMatrixPro matrix, ILockableBitArray workingBitArray,
			ILockableBitArray originalBitArray, IDaysOffPreferences daysOffPreferences,
			int numberOfDayOffsMoved, IOptimizationPreferences optimizationPreferences,
			ISchedulingResultStateHolder schedulingResultStateHolder, MovedDaysOff movedDaysOff)
		{
			var rawDataDictionary = createRawDataDictionary(matrix, optimizationPreferences, schedulingResultStateHolder);
			var currentResult = calculateValue(rawDataDictionary);
			var brokenMinimumAgentsIntervals = breaksMinimumAgents(rawDataDictionary, movedDaysOff);
			
			if (brokenMinimumAgentsIntervals > 0)
			{
				return PredictorResult.CreateDueToMinimumAgents(currentResult, brokenMinimumAgentsIntervals);
			}
	
			var averageWorkTime = TimeSpan.FromTicks(matrix.SchedulePeriod.AverageWorkTimePerDay.Ticks * numberOfDayOffsMoved);
			modifyRawData(workingBitArray, matrix, originalBitArray, daysOffPreferences, rawDataDictionary, averageWorkTime);
			var predictedResult = calculateValue(rawDataDictionary);

			return PredictorResult.Create(currentResult, predictedResult);
		}
		
		private static double breaksMinimumAgents(IDictionary<DateOnly, ForecastScheduleValuePair> rawDataDictionary, MovedDaysOff movedDaysOff)
		{
			return rawDataDictionary.Where(x => movedDaysOff.Contains(x.Key)).Sum(x => x.Value.BrokenMinimumAgents);
		}

		private static double calculateValue(IDictionary<DateOnly, ForecastScheduleValuePair> rawDataDic)
		{
			var values =
				rawDataDic.Values.Where(x => x.ForecastValue > 0)
				          .Select(x => DeviationStatisticData.CalculateRelativeDeviation(x.ForecastValue, x.ScheduleValue));
			return Calculation.Variances.StandardDeviation(values);
		}

		private static void modifyRawData(ILockableBitArray workingBitArray, IScheduleMatrixPro matrix,
		                             ILockableBitArray originalBitArray, IDaysOffPreferences daysOffPreferences,
									IDictionary<DateOnly, ForecastScheduleValuePair> rawDataDic, TimeSpan averageWorkTime)
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

		private IDictionary<DateOnly, ForecastScheduleValuePair> createRawDataDictionary(IScheduleMatrixPro matrix,
			IOptimizationPreferences optimizationPreferences, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var personalSkills = extractSkills(matrix);
			var retDic = new Dictionary<DateOnly, ForecastScheduleValuePair>();
			foreach (DateOnly key in matrix.SchedulePeriod.DateOnlyPeriod.DayCollection())
			{
				var forecastAndSchedule = _forecastAndScheduleSumForDay.Execute(optimizationPreferences, schedulingResultStateHolder, personalSkills, key);
				var value = new ForecastScheduleValuePair { ForecastValue = forecastAndSchedule.ForecastSum, ScheduleValue = forecastAndSchedule.ScheduledSum, BrokenMinimumAgents = forecastAndSchedule.BrokenMinimumAgentsIntervals};
				retDic.Add(key, value);
			}

			return retDic;
		}

		private IEnumerable<ISkill> extractSkills(IScheduleMatrixPro matrix)
		{
			DateOnly firstPeriodDay = matrix.EffectivePeriodDays[0].Day;
			var personalSkills = _personalSkillsProvider.PersonSkillsBasedOnPrimarySkill(matrix.Person.Period(firstPeriodDay));
			return personalSkills.Select(personalSkill => personalSkill.Skill).ToArray();
		}
	}
}