using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class DayIntervalDataCalculatorTest
    {
        private IDayIntervalDataCalculator _target;
        private IMedianCalculatorForDays _medianCalculatorForDays;
        private ITwoDaysIntervalGenerator _twoDayIntervalCalculator;
        private IMedianCalculatorForSkillInterval _medianCalculatorForSkillInterval;
        private IIntervalDataCalculator _intervalDataCalculator;

        [SetUp]
        public void Setup()
        {
            _intervalDataCalculator = new IntervalDataMedianCalculator();
            _medianCalculatorForSkillInterval = new MedianCalculatorForSkillInterval(_intervalDataCalculator);
            _medianCalculatorForDays = new MedianCalculatorForDays(_medianCalculatorForSkillInterval);
            _twoDayIntervalCalculator=new TwoDaysIntervalGenerator();
            _target = new DayIntervalDataCalculator(_medianCalculatorForDays,_twoDayIntervalCalculator);
        }

        [Test]
        public void ShouldReturnNull()
        {
            var result = _target.Calculate(15, null);
            Assert.IsNull(result);
        }

        [Test]
        public void ShouldHaveCorrectIntervals()
        {
            IDictionary<DateOnly, IList<ISkillIntervalData>> list = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
            var result = _target.Calculate(15, list);
            Assert.AreEqual(result.Count(), 0);
        }

        [Test]
        public void VerifyCalculationOvernightShiftForSingleDay()
        {

            var skillIntervalData0 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 01, 22, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 01, 23, 0, 0, DateTimeKind.Utc)), 3, 3, 0, null, null);

            var skillIntervalData1 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 01, 23, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc)), 4, 4, 0, null, null);
            var skillIntervalData2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 02, 1, 0, 0, DateTimeKind.Utc)), 5, 5, 0, null, null);
            var skillIntervalData3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 02, 1, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 02, 2, 0, 0, DateTimeKind.Utc)), 6, 6, 0, null, null);
            IDictionary<DateOnly, IList<ISkillIntervalData>> list = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
            list.Add(new DateOnly(2013, 10, 01), new List<ISkillIntervalData> { skillIntervalData0, skillIntervalData1, skillIntervalData2, skillIntervalData3 });
            list.Add(new DateOnly(2013, 10, 02), new List<ISkillIntervalData> { skillIntervalData2, skillIntervalData3 });


            var result = _target.Calculate(60, list);
            Assert.AreEqual(result.Count, 6);
            Assert.AreEqual(result[new TimeSpan(0, 22, 0, 0)].ForecastedDemand, 3);
            Assert.AreEqual(result[new TimeSpan(0, 23, 0, 0)].ForecastedDemand, 4);
            Assert.AreEqual(result[new TimeSpan(1, 0, 0, 0)].ForecastedDemand, 5);
            Assert.AreEqual(result[new TimeSpan(1, 1, 0, 0)].ForecastedDemand, 6);
            Assert.AreEqual(result[new TimeSpan(0, 0, 0, 0)].ForecastedDemand, 5);
            Assert.AreEqual(result[new TimeSpan(0, 1, 0, 0)].ForecastedDemand, 6);

        }

        [Test]
        public void VerifyCalculationOvernightShiftForTwoDays()
        {

            var skillIntervalData0 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 01, 22, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 01, 23, 0, 0, DateTimeKind.Utc)), 3, 3, 0, null, null);

            var skillIntervalData1 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 01, 23, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc)), 4, 4, 0, null, null);
            var skillIntervalData2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 02, 1, 0, 0, DateTimeKind.Utc)), 5, 5, 0, null, null);
            var skillIntervalData3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 02, 1, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 02, 2, 0, 0, DateTimeKind.Utc)), 6, 6, 0, null, null);
            var skillIntervalData4 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 02, 22, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 02, 23, 0, 0, DateTimeKind.Utc)), 7, 7, 0, null, null);
            var skillIntervalData5 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 02, 23, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 03, 0, 0, 0, DateTimeKind.Utc)), 2, 2, 0, null, null);
            IDictionary<DateOnly, IList<ISkillIntervalData>> list = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
            list.Add(new DateOnly(2013, 10, 01), new List<ISkillIntervalData> { skillIntervalData0, skillIntervalData1, skillIntervalData2, skillIntervalData3, skillIntervalData4, skillIntervalData5 });
            list.Add(new DateOnly(2013, 10, 02), new List<ISkillIntervalData> { skillIntervalData2, skillIntervalData3, skillIntervalData4, skillIntervalData5 });


            var result = _target.Calculate(60, list);
            Assert.AreEqual(result.Count, 8);
            Assert.AreEqual(result[new TimeSpan(0, 22, 0, 0)].ForecastedDemand, 5);
            Assert.AreEqual(result[new TimeSpan(0, 23, 0, 0)].ForecastedDemand, 3);
            Assert.AreEqual(result[new TimeSpan(1, 0, 0, 0)].ForecastedDemand, 5);
            Assert.AreEqual(result[new TimeSpan(1, 1, 0, 0)].ForecastedDemand, 6);
            Assert.AreEqual(result[new TimeSpan(1, 22, 0, 0)].ForecastedDemand, 7);
            Assert.AreEqual(result[new TimeSpan(1, 23, 0, 0)].ForecastedDemand, 2);
            Assert.AreEqual(result[new TimeSpan(0, 0, 0, 0)].ForecastedDemand, 5);
            Assert.AreEqual(result[new TimeSpan(0, 1, 0, 0)].ForecastedDemand, 6);

        }

        [Test]
        public void VerifyCalculationDayShiftForTwoDays()
        {

            var skillIntervalData0 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 01, 15, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 01, 16, 0, 0, DateTimeKind.Utc)), 4, 4, 0, null, null);

            var skillIntervalData1 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 01, 16, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 01, 17, 0, 0, DateTimeKind.Utc)), 2, 2, 0, null, null);

            var skillIntervalData2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 01, 17, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 01, 18, 0, 0, DateTimeKind.Utc)), 10, 10, 0, null, null);
            var skillIntervalData3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 01, 18, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 01, 19, 0, 0, DateTimeKind.Utc)), 19, 19, 0, null, null);
            var skillIntervalData4 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 02, 16, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 02, 17, 0, 0, DateTimeKind.Utc)), 6, 6, 0, null, null);
            var skillIntervalData5 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 02, 17, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 02, 18, 0, 0, DateTimeKind.Utc)), 8, 8, 0, null, null);
            var skillIntervalData6 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 02, 18, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 02, 19, 0, 0, DateTimeKind.Utc)), 11, 11, 0, null, null);
            IDictionary<DateOnly, IList<ISkillIntervalData>> list = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
            list.Add(new DateOnly(2013, 10, 01), new List<ISkillIntervalData> { skillIntervalData0, skillIntervalData1, skillIntervalData2, skillIntervalData3, skillIntervalData6, skillIntervalData4, skillIntervalData5 });
            list.Add(new DateOnly(2013, 10, 02), new List<ISkillIntervalData> { skillIntervalData6, skillIntervalData4, skillIntervalData5 });


            var result = _target.Calculate(60, list);
            Assert.AreEqual(result.Count, 7);
            Assert.AreEqual(result[new TimeSpan(0, 15, 0, 0)].ForecastedDemand, 4);
            Assert.AreEqual(result[new TimeSpan(0, 16, 0, 0)].ForecastedDemand, 4);
            Assert.AreEqual(result[new TimeSpan(0, 17, 0, 0)].ForecastedDemand, 9);
            Assert.AreEqual(result[new TimeSpan(0, 18, 0, 0)].ForecastedDemand, 15);
            Assert.AreEqual(result[new TimeSpan(1, 16, 0, 0)].ForecastedDemand, 6);
            Assert.AreEqual(result[new TimeSpan(1, 17, 0, 0)].ForecastedDemand, 8);
            Assert.AreEqual(result[new TimeSpan(1, 18, 0, 0)].ForecastedDemand, 11);

        }

    }
}
