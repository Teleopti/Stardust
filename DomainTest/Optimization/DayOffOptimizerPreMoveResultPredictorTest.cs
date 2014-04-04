using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class DayOffOptimizerPreMoveResultPredictorTest
	{
		private MockRepository _mocks;
		private IDayOffOptimizerPreMoveResultPredictor _target;
		private IScheduleMatrixPro _matrix;
		private IDailySkillForecastAndScheduledValueCalculator _dailySkillForecastAndScheduledValueCalculator;
		private IDeviationStatisticData _deviationStatisticData;
		private IVirtualSchedulePeriod _schedulePeriod;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayPro2;
		private IPerson _person;
		private ISkill _skill;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_deviationStatisticData = new DeviationStatisticData();
			_dailySkillForecastAndScheduledValueCalculator = _mocks.StrictMock<IDailySkillForecastAndScheduledValueCalculator>();
			_target = new DayOffOptimizerPreMoveResultPredictor(_dailySkillForecastAndScheduledValueCalculator, _deviationStatisticData);
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			_skill = SkillFactory.CreateSkill("skill");
			_person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue,
			                                                     new List<ISkill> {_skill});
		}

		[Test]
		public void ShouldPredictIfTheMoveWillResultInABetterPeriodResult()
		{
			ILockableBitArray originalArray = new LockableBitArray(2, false, false, null);
			originalArray.Set(0, true);
			ILockableBitArray workingArray = new LockableBitArray(2, false, false, null);
			workingArray.Set(1, true);
			IDaysOffPreferences daysOffPreferences = new DaysOffPreferences();
			ForecastScheduleValuePair forecastScheduleValuePair = new ForecastScheduleValuePair();
			forecastScheduleValuePair.ScheduleValue = 1000;
			forecastScheduleValuePair.ForecastValue = 1000;
			using(_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8));
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 1, 1),
				                                                                      new DateOnly(2012, 1, 2)));
				Expect.Call(_matrix.EffectivePeriodDays).Return(
					new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_scheduleDayPro1, _scheduleDayPro2}));
				Expect.Call(_matrix.OuterWeeksPeriodDays).Return(
					new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {
						_scheduleDayPro1, 
						_scheduleDayPro1, 
						_scheduleDayPro1, 
						_scheduleDayPro1, 
						_scheduleDayPro1, 
						_scheduleDayPro1, 
						_scheduleDayPro1,
						_scheduleDayPro1, _scheduleDayPro2 })).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro1.Day).Return(new DateOnly(2012, 1, 1)).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro2.Day).Return(new DateOnly(2012, 1, 2)).Repeat.AtLeastOnce();
				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_dailySkillForecastAndScheduledValueCalculator.CalculateDailyForecastAndScheduleDataForSkill(_skill, new DateOnly(2012, 1,1))).
					Return(forecastScheduleValuePair);
				Expect.Call(_dailySkillForecastAndScheduledValueCalculator.CalculateDailyForecastAndScheduleDataForSkill(_skill, new DateOnly(2012, 1, 2))).
					Return(forecastScheduleValuePair);
			}

			double predictedNewPeriodValue;
			using(_mocks.Playback())
			{
				predictedNewPeriodValue = _target.PredictedValue(_matrix, workingArray, originalArray, daysOffPreferences);
			}

			Assert.AreEqual(0.48, predictedNewPeriodValue, 0.01);
		}

		[Test]
		public void ShouldReturnCurrentValueWithoutMakingAnyMove()
		{
			ForecastScheduleValuePair forecastScheduleValuePair = new ForecastScheduleValuePair();
			forecastScheduleValuePair.ScheduleValue = 1000;
			forecastScheduleValuePair.ForecastValue = 1000;
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 1, 1),
																					  new DateOnly(2012, 1, 2)));
				Expect.Call(_matrix.EffectivePeriodDays).Return(
					new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro1, _scheduleDayPro2 }));
				Expect.Call(_scheduleDayPro1.Day).Return(new DateOnly(2012, 1, 1)).Repeat.AtLeastOnce();
				//Expect.Call(_scheduleDayPro2.Day).Return(new DateOnly(2012, 1, 2)).Repeat.AtLeastOnce();
				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_dailySkillForecastAndScheduledValueCalculator.CalculateDailyForecastAndScheduleDataForSkill(_skill, new DateOnly(2012, 1, 1))).
					Return(forecastScheduleValuePair);
				Expect.Call(_dailySkillForecastAndScheduledValueCalculator.CalculateDailyForecastAndScheduleDataForSkill(_skill, new DateOnly(2012, 1, 2))).
					Return(forecastScheduleValuePair);
			}

			double predictedNewPeriodValue;

			using (_mocks.Playback())
			{
				predictedNewPeriodValue = _target.CurrentValue(_matrix);
			}

			Assert.AreEqual(0, predictedNewPeriodValue);
		}
	}
}