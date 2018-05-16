using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizerPreMoveResultPredictor
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;
		private readonly ForecastAndScheduleSumForDay _forecastAndScheduleSumForDay;

		public static double REMOVEMELATER = -123;

		public DayOffOptimizerPreMoveResultPredictor(PersonalSkillsProvider personalSkillsProvider, ForecastAndScheduleSumForDay forecastAndScheduleSumForDay)
		{
			_personalSkillsProvider = personalSkillsProvider;
			_forecastAndScheduleSumForDay = forecastAndScheduleSumForDay;
		}
		

		public bool WasReallyBetter(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences,
			ISchedulingResultStateHolder schedulingResultStateHolder, MovedDaysOff movedDaysOff, PredictorResult previousPredictorResult)
		{
			var currPredictorValue = currentValue(matrix, optimizationPreferences, schedulingResultStateHolder, movedDaysOff);

			return currPredictorValue < previousPredictorResult.CurrentValue || 
				   currPredictorValue == REMOVEMELATER; //should be removed probably
		}

		public PredictorResult IsPredictedBetterThanCurrent(IScheduleMatrixPro matrix, ILockableBitArray workingBitArray,
			ILockableBitArray originalBitArray, IDaysOffPreferences daysOffPreferences,
			int numberOfDayOffsMoved, IOptimizationPreferences optimizationPreferences,
			ISchedulingResultStateHolder schedulingResultStateHolder, MovedDaysOff movedDaysOff)
		{
			var rawDataDictionary = createRawDataDictionary(matrix, optimizationPreferences, schedulingResultStateHolder);
			if (rawDataDictionary.Any(x => movedDaysOff.Contains(x.Key) && x.Value.BreaksMinimumAgents))
			{
				return new PredictorResult
				{
					IsBetter = true,
					CurrentValue = REMOVEMELATER
				};
			}
		
			double currentResult = calculateValue(rawDataDictionary);

			TimeSpan averageWorkTime = TimeSpan.FromTicks(matrix.SchedulePeriod.AverageWorkTimePerDay.Ticks * numberOfDayOffsMoved);
			modifyRawData(workingBitArray, matrix, originalBitArray, daysOffPreferences, rawDataDictionary, averageWorkTime);
			double predictedResult = calculateValue(rawDataDictionary);

			return new PredictorResult
			{
				IsBetter = predictedResult < currentResult,
				CurrentValue = currentResult
			};
		}
		
		private double currentValue(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences,
			ISchedulingResultStateHolder schedulingResultStateHolder, MovedDaysOff movedDaysOff)
		{
			var rawDataDictionary = createRawDataDictionary(matrix, optimizationPreferences, schedulingResultStateHolder);
			if (rawDataDictionary.Any(x => movedDaysOff.Contains(x.Key) && x.Value.BreaksMinimumAgents))
			{
				return REMOVEMELATER;
			}
		
			return calculateValue(rawDataDictionary);
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
				var value = new ForecastScheduleValuePair { ForecastValue = forecastAndSchedule.ForecastSum, ScheduleValue = forecastAndSchedule.ScheduledSum, BreaksMinimumAgents = forecastAndSchedule.BreaksMinimumAgents};
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

	
	
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MinimumAgents_75339)]
	public class DayOffOptimizerPreMoveResultPredictorOLD
	{
		private readonly IDailySkillForecastAndScheduledValueCalculator _dailySkillForecastAndScheduledValueCalculator;
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public DayOffOptimizerPreMoveResultPredictorOLD(IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator, 
																		PersonalSkillsProvider personalSkillsProvider)

		{
			_dailySkillForecastAndScheduledValueCalculator = dailySkillForecastAndScheduledValueCalculator;
			_personalSkillsProvider = personalSkillsProvider;
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

		private double calculateValue(IDictionary<DateOnly, ForecastScheduleValuePair> rawDataDic)
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

		private IDictionary<DateOnly, ForecastScheduleValuePair> createRawDataDictionary(IVirtualSchedulePeriod schedulePeriod, IEnumerable<ISkill> personalSkills)
		{
			var retDic = new Dictionary<DateOnly, ForecastScheduleValuePair>();
			foreach (DateOnly key in schedulePeriod.DateOnlyPeriod.DayCollection())
			{
				double dailyForecast = 0;
				double dailyScheduled = 0;
				foreach (ISkill skill in personalSkills)
				{
					var forecastScheduleValuePairForSkill = _dailySkillForecastAndScheduledValueCalculator.CalculateDailyForecastAndScheduleDataForSkill(skill, key);
					dailyForecast += forecastScheduleValuePairForSkill.ForecastValue;
					dailyScheduled += forecastScheduleValuePairForSkill.ScheduleValue;
				}

				var value = new ForecastScheduleValuePair { ForecastValue = dailyForecast, ScheduleValue = dailyScheduled };
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