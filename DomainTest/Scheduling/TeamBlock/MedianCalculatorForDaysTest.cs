using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class MedianCalculatorForDaysTest
    {
        private MedianCalculatorForDays _target;
        private IMedianCalculatorForSkillInterval _medianCalculatorForSkillInterval;
        private IIntervalDataCalculator _intervalDataCalculator;


        [SetUp]
        public void Setup()
        {
            _intervalDataCalculator = new IntervalDataMedianCalculator();
            _medianCalculatorForSkillInterval = new MedianCalculatorForSkillInterval(_intervalDataCalculator);
        }

        [Test]
        public void ShouldReturnCorrectCount()
        {
            var today = new DateOnly(2013, 10, 01);
            _target = new MedianCalculatorForDays(_medianCalculatorForSkillInterval);
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
            var days = new Dictionary<DateOnly, Dictionary<DateTime, ISkillIntervalData>>();

            var intervalData = new Dictionary<DateTime, ISkillIntervalData>();
			intervalData.Add(skillIntervalData0.Period.StartDateTime, skillIntervalData0);
			intervalData.Add(skillIntervalData1.Period.StartDateTime, skillIntervalData1);
			intervalData.Add(skillIntervalData2.Period.StartDateTime, skillIntervalData2);
			intervalData.Add(skillIntervalData3.Period.StartDateTime, skillIntervalData3);
            days.Add(today, intervalData);

			intervalData = new Dictionary<DateTime, ISkillIntervalData>();
			intervalData.Add(skillIntervalData2.Period.StartDateTime, skillIntervalData2);
			intervalData.Add(skillIntervalData3.Period.StartDateTime, skillIntervalData3);
            days.Add(today.AddDays(1), intervalData);

            Assert.AreEqual(_target.CalculateMedian(days, 60, today).Count, 6);
        }

        [Test]
        public void ShouldReturnCorrectDataForSingleDay()
        {
            var today = new DateOnly(2013, 10, 01);
            _target = new MedianCalculatorForDays(_medianCalculatorForSkillInterval);
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
            var days = new Dictionary<DateOnly, Dictionary<DateTime, ISkillIntervalData>>();

            var intervalData = new Dictionary<DateTime, ISkillIntervalData>();
			intervalData.Add(skillIntervalData0.Period.StartDateTime, skillIntervalData0);
			intervalData.Add(skillIntervalData1.Period.StartDateTime, skillIntervalData1);
			intervalData.Add(skillIntervalData2.Period.StartDateTime, skillIntervalData2);
			intervalData.Add(skillIntervalData3.Period.StartDateTime, skillIntervalData3);
            days.Add(today, intervalData);

            intervalData = new Dictionary<DateTime, ISkillIntervalData>();
			intervalData.Add(skillIntervalData2.Period.StartDateTime, skillIntervalData2);
			intervalData.Add(skillIntervalData3.Period.StartDateTime, skillIntervalData3);
            days.Add(today.AddDays(1), intervalData);

            var result = _target.CalculateMedian(days, 60, today);
			Assert.AreEqual(result[skillIntervalData0.Period.StartDateTime].ForecastedDemand, 3);
			Assert.AreEqual(result[skillIntervalData1.Period.StartDateTime].ForecastedDemand, 4);
			Assert.AreEqual(result[skillIntervalData2.Period.StartDateTime].ForecastedDemand, 5);
			Assert.AreEqual(result[skillIntervalData3.Period.StartDateTime].ForecastedDemand, 6);
			Assert.AreEqual(result[skillIntervalData2.Period.StartDateTime].ForecastedDemand, 5);
			Assert.AreEqual(result[skillIntervalData3.Period.StartDateTime].ForecastedDemand, 6);
        }

        [Test]
        public void ShouldReturnCorrectDataForTwoDays()
        {
            var today = new DateOnly(2013, 10, 01);
            _target = new MedianCalculatorForDays(_medianCalculatorForSkillInterval);
            var skillIntervalData0 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 01, 22, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 01, 23, 0, 0, DateTimeKind.Utc)), 2, 2, 0, null, null);

            var skillIntervalData1 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 01, 23, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc)), 4, 4, 0, null, null);
            var skillIntervalData2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 02, 1, 0, 0, DateTimeKind.Utc)), 7, 7, 0, null, null);
            var skillIntervalData3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 02, 23, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 03, 0, 0, 0, DateTimeKind.Utc)), 6, 6, 0, null, null);
            var skillIntervalData4 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 03, 0, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 03, 1, 0, 0, DateTimeKind.Utc)), 2, 2, 0, null, null);

            var skillIntervalData5 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 03, 1, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 03, 2, 0, 0, DateTimeKind.Utc)), 3, 3, 0, null, null);
            var days = new Dictionary<DateOnly, Dictionary<DateTime, ISkillIntervalData>>();

            var intervalData = new Dictionary<DateTime, ISkillIntervalData>();
			intervalData.Add(skillIntervalData0.Period.StartDateTime, skillIntervalData0);
			intervalData.Add(skillIntervalData1.Period.StartDateTime, skillIntervalData1);
			intervalData.Add(skillIntervalData2.Period.StartDateTime, skillIntervalData2);
			intervalData.Add(skillIntervalData3.Period.StartDateTime, skillIntervalData3);
            days.Add(today, intervalData);

            intervalData = new Dictionary<DateTime, ISkillIntervalData>();
			intervalData.Add(skillIntervalData2.Period.StartDateTime, skillIntervalData2);
			intervalData.Add(skillIntervalData3.Period.StartDateTime, skillIntervalData3);
			intervalData.Add(skillIntervalData4.Period.StartDateTime, skillIntervalData4);
			intervalData.Add(skillIntervalData5.Period.StartDateTime, skillIntervalData5);
            days.Add(today.AddDays(1), intervalData);

            intervalData = new Dictionary<DateTime, ISkillIntervalData>();
			intervalData.Add(skillIntervalData4.Period.StartDateTime, skillIntervalData4);
			intervalData.Add(skillIntervalData5.Period.StartDateTime, skillIntervalData5);
            days.Add(today.AddDays(2), intervalData);

            var result = _target.CalculateMedian(days, 60, today);
			Assert.AreEqual(result[skillIntervalData0.Period.StartDateTime].ForecastedDemand, 2);
			Assert.AreEqual(result[skillIntervalData1.Period.StartDateTime].ForecastedDemand, 5);
            Assert.AreEqual(result[skillIntervalData2.Period.StartDateTime].ForecastedDemand, 4.5);
			Assert.AreEqual(result[skillIntervalData3.Period.StartDateTime].ForecastedDemand, 6);



        }
    }


}
