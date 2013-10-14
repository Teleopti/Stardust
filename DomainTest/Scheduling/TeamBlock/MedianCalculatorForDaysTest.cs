﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class MedianCalculatorForDaysTest
    {
        private IMedianCalculatorForDays _target;

        [Test]
        public void ShouldReturnCorrectCount()
        {
            var today = new DateOnly(2013, 10, 01);
            _target = new MedianCalculatorForDays();
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
            var days = new Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>>();

            var intervalData = new Dictionary<TimeSpan, ISkillIntervalData>();
            intervalData.Add(new TimeSpan(0, 22, 0, 0), skillIntervalData0);
            intervalData.Add(new TimeSpan(0, 23, 0, 0), skillIntervalData1);
            intervalData.Add(new TimeSpan(1, 0, 0, 0), skillIntervalData2);
            intervalData.Add(new TimeSpan(1, 1, 0, 0), skillIntervalData3);
            days.Add(today, intervalData);

            intervalData = new Dictionary<TimeSpan, ISkillIntervalData>();
            intervalData.Add(new TimeSpan(0, 0, 0, 0), skillIntervalData2);
            intervalData.Add(new TimeSpan(0, 1, 0, 0), skillIntervalData3);
            days.Add(today.AddDays(1), intervalData);

            Assert.AreEqual(_target.CalculateMedian(days, 60).Count, 6);
        }

        [Test]
        public void ShouldReturnCorrectDataForSingleDay()
        {
            var today = new DateOnly(2013, 10, 01);
            _target = new MedianCalculatorForDays();
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
            var days = new Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>>();

            var intervalData = new Dictionary<TimeSpan, ISkillIntervalData>();
            intervalData.Add(new TimeSpan(0, 22, 0, 0), skillIntervalData0);
            intervalData.Add(new TimeSpan(0, 23, 0, 0), skillIntervalData1);
            intervalData.Add(new TimeSpan(1, 0, 0, 0), skillIntervalData2);
            intervalData.Add(new TimeSpan(1, 1, 0, 0), skillIntervalData3);
            days.Add(today, intervalData);

            intervalData = new Dictionary<TimeSpan, ISkillIntervalData>();
            intervalData.Add(new TimeSpan(0, 0, 0, 0), skillIntervalData2);
            intervalData.Add(new TimeSpan(0, 1, 0, 0), skillIntervalData3);
            days.Add(today.AddDays(1), intervalData);

            var result = _target.CalculateMedian(days, 60);
            Assert.AreEqual(result[new TimeSpan(0, 22, 0, 0)].ForecastedDemand, 3);
            Assert.AreEqual(result[new TimeSpan(0, 23, 0, 0)].ForecastedDemand, 4);
            Assert.AreEqual(result[new TimeSpan(0, 0, 0, 0)].ForecastedDemand, 5);
            Assert.AreEqual(result[new TimeSpan(0, 1, 0, 0)].ForecastedDemand, 6);
            Assert.AreEqual(result[new TimeSpan(1, 0, 0, 0)].ForecastedDemand, 5);
            Assert.AreEqual(result[new TimeSpan(1, 1, 0, 0)].ForecastedDemand, 6);
        }

        [Test]
        public void ShouldReturnCorrectDataForTwoDays()
        {
            var today = new DateOnly(2013, 10, 01);
            _target = new MedianCalculatorForDays();
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
            var days = new Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>>();

            var intervalData = new Dictionary<TimeSpan, ISkillIntervalData>();
            intervalData.Add(new TimeSpan(0, 22, 0, 0), skillIntervalData0);
            intervalData.Add(new TimeSpan(0, 23, 0, 0), skillIntervalData1);
            intervalData.Add(new TimeSpan(1, 0, 0, 0), skillIntervalData2);
            intervalData.Add(new TimeSpan(1, 23, 0, 0), skillIntervalData3);
            days.Add(today, intervalData);

            intervalData = new Dictionary<TimeSpan, ISkillIntervalData>();
            intervalData.Add(new TimeSpan(0, 0, 0, 0), skillIntervalData2);
            intervalData.Add(new TimeSpan(0, 23, 0, 0), skillIntervalData3);
            intervalData.Add(new TimeSpan(1, 0, 0, 0), skillIntervalData4);
            intervalData.Add(new TimeSpan(1, 1, 0, 0), skillIntervalData5);
            days.Add(today.AddDays(1), intervalData);

            intervalData = new Dictionary<TimeSpan, ISkillIntervalData>();
            intervalData.Add(new TimeSpan(0, 0, 0, 0), skillIntervalData4);
            intervalData.Add(new TimeSpan(0, 1, 0, 0), skillIntervalData5);
            days.Add(today.AddDays(2), intervalData);

            var result = _target.CalculateMedian(days, 60);
            Assert.AreEqual(result.Count, 7);
            Assert.AreEqual(result[new TimeSpan(0, 22, 0, 0)].ForecastedDemand, 2);
            Assert.AreEqual(result[new TimeSpan(0, 23, 0, 0)].ForecastedDemand, 5);
            Assert.AreEqual(result[new TimeSpan(1, 0, 0, 0)].ForecastedDemand, 4.5);
            Assert.AreEqual(result[new TimeSpan(0, 1, 0, 0)].ForecastedDemand, 3);
            Assert.AreEqual(result[new TimeSpan(1, 23, 0, 0)].ForecastedDemand, 6);
            Assert.AreEqual(result[new TimeSpan(0, 0, 0, 0)].ForecastedDemand, 4.5);
            Assert.AreEqual(result[new TimeSpan(0, 1, 0, 0)].ForecastedDemand, 3);


        }
    }


}
