using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftCalculation
{
    [TestFixture]
    public class WorkShiftPeriodValueCalculatorTest
    {
        private IWorkShiftPeriodValueCalculator _target;
        private DateTimePeriod _period;

        [SetUp]
        public void Setup()
        {
            _target = new WorkShiftPeriodValueCalculator();
            DateTime start = new DateTime(2009, 02, 02, 8, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2009, 02, 02, 8, 30, 0, DateTimeKind.Utc);

            _period = new DateTimePeriod(start, end);
        }

       
        [Test]
        public void ShouldBoostIfUnderMinHeads()
        {
            ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, 5, 0, null, null);
            double rawResult = _target.PeriodValue(skillIntervalData, 30, true, true);
			Assert.AreEqual(18, rawResult, 0.01);

            skillIntervalData = new SkillIntervalData(_period, 15, 5, 9, 11, null);
            double result = _target.PeriodValue(skillIntervalData, 30, true, true);
            Assert.AreEqual(200000 + rawResult, result, 0.01);

            skillIntervalData = new SkillIntervalData(_period, 15, 5, 10, 11, null);
            result = _target.PeriodValue(skillIntervalData, 30, true, true);
            Assert.AreEqual(100000 + rawResult, result, 0.01);

            skillIntervalData = new SkillIntervalData(_period, 15, 5, 11, 11, null);
            result = _target.PeriodValue(skillIntervalData, 30, true, true);
            Assert.AreEqual(rawResult, result, 0.01);
        }

        [Test]
        public void ShouldPunishIfOverMaxHeads()
        {
            ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, 5, 10, null, 11);
            double rawResult = _target.PeriodValue(skillIntervalData, 30, true, true);
			Assert.AreEqual(18, rawResult, 0.01);

            skillIntervalData = new SkillIntervalData(_period, 15, 5, 11, null, 11);
            double result = _target.PeriodValue(skillIntervalData, 30, true, true);
            Assert.AreEqual(rawResult - 100000, result, 0.01);

            skillIntervalData = new SkillIntervalData(_period, 15, 5, 12, null, 11);
            result = _target.PeriodValue(skillIntervalData, 30, true, true);
            Assert.AreEqual(rawResult - 200000, result, 0.01);
        }

        [Test]
        public void ShouldPunishAndBoostIfNeeded()
        {
            ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, 5, 11, 13, 11);
            double result = _target.PeriodValue(skillIntervalData, 30, true, true);
			Assert.AreEqual(18 + 100000, result, 0.01);
        }

        [Test]
        public void ShouldHandleOptionUseBothMinMaxButNoValues()
        {
            ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, 5, 10, null, null);
            double result = _target.PeriodValue(skillIntervalData, 30, true, true);
			Assert.AreEqual(18, result, 0.01);
        }
    }
}