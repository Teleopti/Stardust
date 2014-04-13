using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class MedianCalculatorForSkillIntervalTest
    {
        private IMedianCalculatorForSkillInterval _target;
        private IIntervalDataCalculator _intervalDataCalculator;

        [SetUp]
        public void Setup()
        {
            _intervalDataCalculator =new IntervalDataMedianCalculator();
            _target = new MedianCalculatorForSkillInterval(_intervalDataCalculator);
        }

        [Test]
        public void VerifyIntervalDataWithValues()
        {
            ISkillIntervalData skillInterval1 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 14, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 14, 08, 0, 0, DateTimeKind.Utc)), 7, 8, 6, 5, 7);
            ISkillIntervalData skillInterval2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 15, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 15, 08, 0, 0, DateTimeKind.Utc)), 5, 0, 7, 9, 17);
            ISkillIntervalData skillInterval3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 16, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 16, 08, 0, 0, DateTimeKind.Utc)), 4, 2, 1, 6, 12);
            IList<ISkillIntervalData> skillIntervalData = new List<ISkillIntervalData>{skillInterval1,skillInterval2,skillInterval3 };
			var result = _target.CalculateMedian(skillInterval1.Period.StartDateTime, skillIntervalData, 60,
		        new DateOnly(2013, 10, 14));
	        Assert.AreEqual(result.ForecastedDemand, 5);
            Assert.AreEqual(result.CurrentDemand   ,2);
            Assert.AreEqual(result.CurrentHeads    ,6);
            Assert.AreEqual(result.MinimumHeads     ,6);
            Assert.AreEqual(result.MaximumHeads     ,12);
        }

        [Test]
        public void VerifyIntervalDataWithSomeNullValuesInMinimumStaff()
        {
            ISkillIntervalData skillInterval1 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 14, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 14, 08, 0, 0, DateTimeKind.Utc)), 7, 8, 6,5, 7);
            ISkillIntervalData skillInterval2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 15, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 15, 08, 0, 0, DateTimeKind.Utc)), 5, 0, 7, null, 17);
            ISkillIntervalData skillInterval3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 16, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 16, 08, 0, 0, DateTimeKind.Utc)), 4, 2, 1, 6, 12);
            IList<ISkillIntervalData> skillIntervalData = new List<ISkillIntervalData> { skillInterval1, skillInterval2, skillInterval3 };
			var result = _target.CalculateMedian(skillInterval1.Period.StartDateTime, skillIntervalData, 60, new DateOnly(2013, 10, 14));
            Assert.AreEqual(result.ForecastedDemand, 5);
            Assert.AreEqual(result.CurrentDemand, 2);
            Assert.AreEqual(result.CurrentHeads, 6);
            Assert.AreEqual(result.MinimumHeads, 5.5);
            Assert.AreEqual(result.MaximumHeads, 12);
        }

        [Test]
        public void VerifyIntervalDataWithAllNullValuesInMinimumStaff()
        {
            ISkillIntervalData skillInterval1 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 14, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 14, 08, 0, 0, DateTimeKind.Utc)), 7, 8, 6, null, 7);
            ISkillIntervalData skillInterval2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 15, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 15, 08, 0, 0, DateTimeKind.Utc)), 5, 0, 7, null, 17);
            ISkillIntervalData skillInterval3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 16, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 16, 08, 0, 0, DateTimeKind.Utc)), 4, 2, 1, null, 12);
            IList<ISkillIntervalData> skillIntervalData = new List<ISkillIntervalData> { skillInterval1, skillInterval2, skillInterval3 };
			var result = _target.CalculateMedian(skillInterval1.Period.StartDateTime, skillIntervalData, 60, new DateOnly(2013, 10, 14));
            Assert.AreEqual(result.ForecastedDemand, 5);
            Assert.AreEqual(result.CurrentDemand, 2);
            Assert.AreEqual(result.CurrentHeads, 6);
            Assert.AreEqual(result.MinimumHeads, null);
            Assert.AreEqual(result.MaximumHeads, 12);
        }

        [Test]
        public void VerifyIntervalDataWithSomeNullValuesInMaximumStaff()
        {
            ISkillIntervalData skillInterval1 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 14, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 14, 08, 0, 0, DateTimeKind.Utc)), 7, 8, 6, 5, null);
            ISkillIntervalData skillInterval2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 15, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 15, 08, 0, 0, DateTimeKind.Utc)), 5, 0, 7, null, 17);
            ISkillIntervalData skillInterval3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 16, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 16, 08, 0, 0, DateTimeKind.Utc)), 4, 2, 1, 6, 12);
            IList<ISkillIntervalData> skillIntervalData = new List<ISkillIntervalData> { skillInterval1, skillInterval2, skillInterval3 };
			var result = _target.CalculateMedian(skillInterval1.Period.StartDateTime, skillIntervalData, 60, new DateOnly(2013, 10, 14));
            Assert.AreEqual(result.ForecastedDemand, 5);
            Assert.AreEqual(result.CurrentDemand, 2);
            Assert.AreEqual(result.CurrentHeads, 6);
            Assert.AreEqual(result.MinimumHeads, 5.5);
            Assert.AreEqual(result.MaximumHeads, 14.5);
        }

        [Test]
        public void VerifyIntervalDataWithAllNullValuesInMaximumStaff()
        {
            ISkillIntervalData skillInterval1 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 14, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 14, 08, 0, 0, DateTimeKind.Utc)), 7, 8, 6, null, null);
            ISkillIntervalData skillInterval2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 15, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 15, 08, 0, 0, DateTimeKind.Utc)), 5, 0, 7, null, null);
            ISkillIntervalData skillInterval3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 16, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 16, 08, 0, 0, DateTimeKind.Utc)), 4, 2, 1, null, null);
            IList<ISkillIntervalData> skillIntervalData = new List<ISkillIntervalData> { skillInterval1, skillInterval2, skillInterval3 };
			var result = _target.CalculateMedian(skillInterval1.Period.StartDateTime, skillIntervalData, 60, new DateOnly(2013, 10, 14));
            Assert.AreEqual(result.ForecastedDemand, 5);
            Assert.AreEqual(result.CurrentDemand, 2);
            Assert.AreEqual(result.CurrentHeads, 6);
            Assert.AreEqual(result.MinimumHeads, null);
            Assert.AreEqual(result.MaximumHeads, null);
        }
    }

    
}
