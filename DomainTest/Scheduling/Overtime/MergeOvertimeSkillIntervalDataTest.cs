using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Overtime;


namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class MergeOvertimeSkillIntervalDataTest
    {
        private DateTimePeriod _dtPeriod;
        private IMergeOvertimeSkillIntervalData _target;

        [SetUp]
        public void Setup()
        {
            _dtPeriod = new DateTimePeriod(2014, 02, 23, 0, 2014, 02, 25, 0);
            _target = new MergeOvertimeSkillIntervalData();
        }

        [Test]
        public void TestIfPeriodIsSameAsSource()
        {
            var interval1 = new OvertimeSkillIntervalData(_dtPeriod, 5, 2.8);
            var interval2 = new OvertimeSkillIntervalData(_dtPeriod, 7, 8.8);
            Assert.AreEqual(_target.MergeSkillIntervalData(interval1, interval2).Period, _dtPeriod);
        }
        [Test]
        public void TestForecastedDemandMergedCorrectly()
        {
            var interval1 = new OvertimeSkillIntervalData(_dtPeriod, 5.2, 2.8);
            var interval2 = new OvertimeSkillIntervalData(_dtPeriod, 5.6, 8.8);
            Assert.AreEqual(Math.Round(_target.MergeSkillIntervalData(interval1, interval2).ForecastedDemand, 1), 10.8);
        }
        [Test]
        public void TestCurrentDemandMergedCorrectly()
        {
            var interval1 = new OvertimeSkillIntervalData(_dtPeriod, 5.2, 3.8);
            var interval2 = new OvertimeSkillIntervalData(_dtPeriod, 5.6, 1.8);
            Assert.AreEqual(Math.Round(_target.MergeSkillIntervalData(interval1, interval2).CurrentDemand, 1), 5.6);
        }

        [Test]
        public void TestForecastedDemandHaveDefaultValue()
        {
            var interval1 = new OvertimeSkillIntervalData(_dtPeriod, 0, 3);
            var interval2 = new OvertimeSkillIntervalData(_dtPeriod, 0, 1.8);
            Assert.AreEqual(Math.Round(_target.MergeSkillIntervalData(interval1, interval2).ForecastedDemand, 2), 0.02);
        }
    }


}
