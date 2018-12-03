using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;


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
                                       new DateTime(2013, 10, 14, 08, 0, 0, DateTimeKind.Utc)), 7, 8, 6, 5, 7); //minmaxboost 0
            ISkillIntervalData skillInterval2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 15, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 15, 08, 0, 0, DateTimeKind.Utc)), 5, 0, 18, 9, 17); //minmaxboost -2, minnaxboostforSTDEV -1
            ISkillIntervalData skillInterval3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 16, 07, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 16, 08, 0, 0, DateTimeKind.Utc)), 4, 2, 1, 6, 12); //minmaxboost 5
            IList<ISkillIntervalData> skillIntervalData = new List<ISkillIntervalData>{skillInterval1,skillInterval2,skillInterval3 };
			var result = _target.CalculateMedian(skillInterval1.Period.StartDateTime.TimeOfDay, skillIntervalData, 60,
		        new DateOnly(2013, 10, 14));
	        Assert.AreEqual(result.ForecastedDemand, 5);
            Assert.AreEqual(result.CurrentDemand   ,2);
			Assert.AreEqual(0, result.CurrentHeads);
			Assert.IsNull(result.MinimumHeads);
			Assert.IsNull(result.MaximumHeads);
			Assert.AreEqual(3, result.MinMaxBoostFactor);
			Assert.AreEqual(4, result.MinMaxBoostFactorForStandardDeviation);
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
			var result = _target.CalculateMedian(skillInterval1.Period.StartDateTime.TimeOfDay, skillIntervalData, 60, new DateOnly(2013, 10, 14));
            Assert.AreEqual(result.ForecastedDemand, 5);
            Assert.AreEqual(result.CurrentDemand, 2);
			Assert.AreEqual(0, result.CurrentHeads);
			Assert.IsNull(result.MinimumHeads);
			Assert.IsNull(result.MaximumHeads);
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
			var result = _target.CalculateMedian(skillInterval1.Period.StartDateTime.TimeOfDay, skillIntervalData, 60, new DateOnly(2013, 10, 14));
            Assert.AreEqual(result.ForecastedDemand, 5);
            Assert.AreEqual(result.CurrentDemand, 2);
            Assert.AreEqual(0, result.CurrentHeads);
            Assert.IsNull(result.MinimumHeads);
			Assert.IsNull(result.MaximumHeads);
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
			var result = _target.CalculateMedian(skillInterval1.Period.StartDateTime.TimeOfDay, skillIntervalData, 60, new DateOnly(2013, 10, 14));
            Assert.AreEqual(result.ForecastedDemand, 5);
            Assert.AreEqual(result.CurrentDemand, 2);
			Assert.AreEqual(0, result.CurrentHeads);
			Assert.IsNull(result.MinimumHeads);
			Assert.IsNull(result.MaximumHeads);
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
			var result = _target.CalculateMedian(skillInterval1.Period.StartDateTime.TimeOfDay, skillIntervalData, 60, new DateOnly(2013, 10, 14));
            Assert.AreEqual(result.ForecastedDemand, 5);
            Assert.AreEqual(result.CurrentDemand, 2);
            Assert.AreEqual(0, result.CurrentHeads);
            Assert.IsNull(result.MinimumHeads);
            Assert.IsNull(result.MaximumHeads);
        }
    }

    
}
