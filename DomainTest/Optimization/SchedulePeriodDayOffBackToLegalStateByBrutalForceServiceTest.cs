﻿using System.Collections;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulePeriodDayOffBackToLegalStateByBrutalForceServiceTest
    {
        private SchedulePeriodDayOffBackToLegalStateByBrutalForceService _target;
        private MockRepository _mockRepository;
        private IScheduleMatrixBitArrayConverter _matrixBitArrayConverter;
        private DayOffPlannerRules _dayOffPlannerRules;
        private CultureInfo _cultureInfo;
        private IScheduleMatrixPro _matrix;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _matrixBitArrayConverter = _mockRepository.StrictMock<IScheduleMatrixBitArrayConverter>();
            _dayOffPlannerRules = new DayOffPlannerRules();
            _cultureInfo = new CultureInfo("se-SE");
            _matrix = _mockRepository.StrictMock<IScheduleMatrixPro>();
            _target = new SchedulePeriodDayOffBackToLegalStateByBrutalForceService(_matrixBitArrayConverter, _dayOffPlannerRules, _cultureInfo);
        }

        [Test]
        public void VerifyBackToLegalStateByBruteForceWithTwoDayOffPerWeekInRules()
        {
            using (_mockRepository.Record())
            {
                mockExpectations();
            }

            setTwoDayOffPerWeekInRules();
            _target = new SchedulePeriodDayOffBackToLegalStateByBrutalForceService(_matrixBitArrayConverter, _dayOffPlannerRules, _cultureInfo);

            Assert.IsNull(_target.Result);
            _target.Execute(_matrix);
            Assert.IsTrue(_target.Result.Value);
            BitArray result = _target.ResultArray;
            const string expectedBitArray = "1100000-1100000-1100000-0000011";
            string resultBitArray = BitArrayHelper.ToWeeklySeparatedString(result);
            Assert.AreEqual(expectedBitArray, resultBitArray);
        }

        [Test]
        public void VerifyBackToLegalStateByBruteForceWithTwoFullWeekendsInRules()
        {
            using (_mockRepository.Record())
            {
                mockExpectations();
            }

            setTwoFullWeekEndsInRules();
            _target = new SchedulePeriodDayOffBackToLegalStateByBrutalForceService(_matrixBitArrayConverter, _dayOffPlannerRules, _cultureInfo);
            
            Assert.IsNull(_target.Result);
            _target.Execute(_matrix);
            Assert.IsTrue(_target.Result.Value);
            
            BitArray result = _target.ResultArray;
            const string expectedBitArray = "1100000-0000011-0000011-0000011";
            string resultBitArray = BitArrayHelper.ToWeeklySeparatedString(result);
            Assert.AreEqual(expectedBitArray, resultBitArray);
        }

        [Test]
        public void VerifyBackToLegalStateByBruteForceWithImpossibleRules()
        {
            using (_mockRepository.Record())
            {
                mockExpectations();
            }

            setImpossibleRules();
            _target = new SchedulePeriodDayOffBackToLegalStateByBrutalForceService(_matrixBitArrayConverter, _dayOffPlannerRules, _cultureInfo);

            Assert.IsNull(_target.Result);
            _target.Execute(_matrix);
            Assert.That(_target.Result.HasValue, Is.True);
            Assert.That(_target.Result.Value, Is.False );

            BitArray result = _target.ResultArray;
            Assert.IsNull(result);

        }

        private void mockExpectations()
        {
            Expect.Call(_matrixBitArrayConverter.OuterWeekPeriodDayOffsBitArray(_matrix))
                .Return(createPeriodBitArrayForTest())
                .Repeat.Any();

            Expect.Call(_matrixBitArrayConverter.OuterWeekPeriodLockedDaysBitArray(_matrix))
                .Return(createLocksBitArrayForTest())
                .Repeat.Any();

            Expect.Call(_matrixBitArrayConverter.PeriodIndexRange(_matrix))
                .Return(new MinMax<int>(7, 20))
                .Repeat.Any();
        }

        private void setTwoDayOffPerWeekInRules()
        {
            _dayOffPlannerRules = new DayOffPlannerRules
                                      {
                                          UseConsecutiveDaysOff = false,
                                          UseConsecutiveWorkdays = false,
                                          UseDaysOffPerWeek = true,
                                          DaysOffPerWeek = new MinMax<int>(2, 2)
                                      };
        }

        private void setTwoFullWeekEndsInRules()
        {
            _dayOffPlannerRules = new DayOffPlannerRules
                                      {
                                          UseConsecutiveDaysOff = false,
                                          UseConsecutiveWorkdays = false,
                                          UseDaysOffPerWeek = false,
                                          UseFreeWeekends = true,
                                          FreeWeekends = new MinMax<int>(2, 2)
                                      };
        }

        private void setImpossibleRules()
        {
            _dayOffPlannerRules = new DayOffPlannerRules
                                      {
                                          UseConsecutiveDaysOff = false,
                                          UseConsecutiveWorkdays = false,
                                          UseDaysOffPerWeek = false,
                                          UseFreeWeekends = true,
                                          FreeWeekends = new MinMax<int>(20, 20)
                                      };
        }

        private static BitArray createPeriodBitArrayForTest()
        {
            var values = new[]
                                {
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,

                                    true,
                                    true,
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    true,
                                    true
                                };
            var result = new BitArray(values);
            return result;
        }

        private static BitArray createLocksBitArrayForTest()
        {
            var values = new[]
                                {
                                    true,
                                    true,
                                    true,
                                    true,
                                    true,
                                    true,
                                    true,

                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,

                                    true,
                                    true,
                                    true,
                                    true,
                                    true,
                                    true,
                                    true
                                };
            var result = new BitArray(values);
            return result;
        }

    }
}
