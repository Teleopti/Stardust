using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;
using Teleopti.Interfaces.Domain;

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
        public void ShouldReturnZeroIfIntervalDataIsNull()
        {
            ISkillIntervalData skillIntervalData = null;
            double result = _target.PeriodValue(skillIntervalData, 15, false, false);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void ShouldCalculateAsBeforeWhenUnderstaffed()
        {
            ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 10.80, 5, 0, null, null);
            double result = _target.PeriodValue(skillIntervalData, 30, false, false);
			Assert.AreEqual(25, result, 0.01);
        }

        [Test]
        public void ShouldCalculateAsBeforeWhenOverstaffed()
        {
            ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 13.33, -13.33, 0, null, null);
            double result = _target.PeriodValue(skillIntervalData, 30, false, false);
			Assert.AreEqual(-62.25, result, 0.01);
        }

        [Test]
        public void ShouldHandleZeroForecasted()
        {
            ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 0, -1, 0, null, null);
            double result = _target.PeriodValue(skillIntervalData, 30, false, false);
			Assert.AreEqual(-9000, result, 0.01);
        }

        [Test]
        public void ShouldHandleZeroCurrentDemand()
        {
            ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, 0, 0, null, null);
            double result = _target.PeriodValue(skillIntervalData, 30, false, false);
			Assert.AreEqual(0, result, 0.01);
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
        public void ShouldHandleZeroAdditionalResource()
        {
            ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, 5, 10, null, 11);
            double result = _target.PeriodValue(skillIntervalData, 0, true, true);
            Assert.AreEqual(0, result, 0.01);
        }

        [Test]
        public void ShouldHandleOptionUseBothMinMaxButNoValues()
        {
            ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, 5, 10, null, null);
            double result = _target.PeriodValue(skillIntervalData, 30, true, true);
			Assert.AreEqual(18, result, 0.01);
        }

        [Test]
        public void PeriodWithHigherDiffShouldHaveHigherValueIfForecastIsSame()
        {
            var skillIntervalDataLow = new SkillIntervalData(_period, 15, 4, 10, null, null);
            var skillIntervalDataHigh = new SkillIntervalData(_period, 15, 5, 10, null, null);
            double resultLow = _target.PeriodValue(skillIntervalDataLow, 30, true, true);
            double resultHigh = _target.PeriodValue(skillIntervalDataHigh, 30, true, true);
            Assert.That(resultHigh > resultLow);
        }

        [Test]
        public void PeriodWithLowerForecastShouldHaveHigherValueIfDiffIsSame()
        {
			var skillIntervalDataHigh = new SkillIntervalData(_period, 16, 11, 10, null, null);
			var skillIntervalDataLow = new SkillIntervalData(_period, 15, 10, 10, null, null);
            double resultLow = _target.PeriodValue(skillIntervalDataLow, 30, true, true);
            double resultHigh = _target.PeriodValue(skillIntervalDataHigh, 30, true, true);
            Assert.That(resultHigh > resultLow);
        }

        [Test]
        public void PeriodWithFiveMinutesCoveredByShiftShouldBeLowerThanPeriodCoveredWithTenMinutes()
        {
            var skillIntervalData = new SkillIntervalData(_period, 15, 10, 10, null, null);
            double resultLow = _target.PeriodValue(skillIntervalData, 5, true, true);
            double resultHigh = _target.PeriodValue(skillIntervalData, 10, true, true);
            Assert.That(resultHigh > resultLow);
        }

        [Test]
		public void PeriodValueShouldBePositiveIfUnderstaffed()
        {
            var skillIntervalData = new SkillIntervalData(_period, 5, 4, 1, null, null);
            double result = _target.PeriodValue(skillIntervalData, 30, true, true);
			Assert.AreEqual(42, result, 0.01);
        }

        [Test]
		public void PeriodValueShouldBeNegativeIfOverstaffed()
        {
            var skillIntervalData = new SkillIntervalData(_period, 5, -16, 21, null, null);
            double result = _target.PeriodValue(skillIntervalData, 30, true, true);
			Assert.AreEqual(-198, result, 0.01);
		}

		[Test]
		public void ShouldBeMoreImportantToScheduleAHigherForecastThanALowerWhenDiffPercentIsSame()
		{
			var skillIntervalDataHigh = new SkillIntervalData(_period, 60, 60, 0, null, null);
			var skillIntervalDataLow = new SkillIntervalData(_period, 20, 20, 0, null, null);
			double resultLow = _target.PeriodValue(skillIntervalDataLow, 30, true, true);
			double resultHigh = _target.PeriodValue(skillIntervalDataHigh, 30, true, true);
			Assert.That(resultHigh > resultLow);
        }
    }
}