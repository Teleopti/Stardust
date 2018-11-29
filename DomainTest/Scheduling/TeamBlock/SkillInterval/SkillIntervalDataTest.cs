using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.SkillInterval
{
	[TestFixture]
	public class SkillIntervalDataTest
	{
		private ISkillIntervalData _target;
		private DateTimePeriod _dtp;

		[SetUp]
		public void Setup()
		{
			_dtp = new DateTimePeriod(2012, 11, 28, 2012, 11, 28);
			_target = new SkillIntervalData(_dtp, 3.5, 1.5, 3, 2, null);
		}

		[Test]
		public void ShouldContainCurrentDemand()
		{
			Assert.AreEqual(1.5, _target.CurrentDemand);
		}

		[Test]
		public void CouldContainMinMaxHeads()
		{
			Assert.IsNull(_target.MaximumHeads);
			Assert.AreEqual(2, _target.MinimumHeads.Value);
		}

		[Test]
		public void ShouldContainCurrentHeads()
		{
			Assert.AreEqual(3, _target.CurrentHeads);
		}

		[Test]
		public void ShouldContainPeriod()
		{
			Assert.AreEqual(_dtp, _target.Period);
		}

		[Test]
		public void ShouldContainForecastedDemand()
		{
			Assert.AreEqual(3.5, _target.ForecastedDemand);
		}

        [Test]
        public void CalculateBoostedValueTestA()
        {
            var skillIntervalData = new SkillIntervalData(_dtp, 3.5, 3, 1, 2, 4);
            Assert.AreEqual(1, skillIntervalData.MinMaxBoostFactor);
        }

        [Test]
        public void CalculateBoostedValueTestB()
        {
            var skillIntervalData = new SkillIntervalData(_dtp, 3.5, 3, 4, 4, 4);
            Assert.AreEqual(-1, skillIntervalData.MinMaxBoostFactor);
        }

        [Test]
        public void CalculateBoostedValueTestC()
        {
            var skillIntervalData = new SkillIntervalData(_dtp, 3.5, 3, 4, 6, 1);
            Assert.AreEqual(-2, skillIntervalData.MinMaxBoostFactor);
        }

        [Test]
        public void CalculateBoostedValueTestD()
        {
            var skillIntervalData = new SkillIntervalData(_dtp, 3.5, 3, 1, 1, 2);
            Assert.AreEqual(0, skillIntervalData.MinMaxBoostFactor);
        }

		[Test]
		public void CalculateBoostedForStandardDevValueTestA()
		{
			var skillIntervalData = new SkillIntervalData(_dtp, 3.5, 3, 1, 2, 4);
			Assert.AreEqual(1, skillIntervalData.MinMaxBoostFactorForStandardDeviation);
		}

		[Test]
		public void CalculateBoostedForStandardDevValueTestB()
		{
			var skillIntervalData = new SkillIntervalData(_dtp, 3.5, 3, 4, 4, 4);
			Assert.AreEqual(0, skillIntervalData.MinMaxBoostFactorForStandardDeviation);
		}

		[Test]
		public void CalculateBoostedForStandardDevValueTestC()
		{
			var skillIntervalData = new SkillIntervalData(_dtp, 3.5, 3, 4, 6, 1);
			Assert.AreEqual(-1, skillIntervalData.MinMaxBoostFactorForStandardDeviation);
		}

		[Test]
		public void CalculateBoostedForStandardDevValueTestD()
		{
			var skillIntervalData = new SkillIntervalData(_dtp, 3.5, 3, 1, 1, 2);
			Assert.AreEqual(0, skillIntervalData.MinMaxBoostFactor);
		}

        [Test]
        public void CalculateBoostedValueWithNullMinimum()
        {
            var skillIntervalData = new SkillIntervalData(_dtp, 3.5, 3, 6, null, 5);
            Assert.AreEqual(-2, skillIntervalData.MinMaxBoostFactor);
        }

        [Test]
        public void CalculateBoostedValueWithNullMaximum()
        {
            var skillIntervalData = new SkillIntervalData(_dtp, 3.5, 3, 6, 5, null);
            Assert.AreEqual(0, skillIntervalData.MinMaxBoostFactor);
        }

        [Test]
        public void CalculateBoostedValueWithAllNull()
        {
            var skillIntervalData = new SkillIntervalData(_dtp, 3.5, 3, 3, null, null);
            Assert.AreEqual(0, skillIntervalData.MinMaxBoostFactor);
        }

		[Test]
		public void ZeroMinOrMaxStaffMeansNull()
		{
			var skillIntervalData = new SkillIntervalData(_dtp, 3.5, 3, 3, 0, 0);
			Assert.IsNull(skillIntervalData.MaximumHeads);
			Assert.IsNull(skillIntervalData.MinimumHeads);
		}

        [Test]
        public void CalculateRelativeDifference()
        {
            var skillIntervalData = new SkillIntervalData(_dtp, 3.63, 2, 3, 0, 0);
            Assert.AreEqual(-0.551, Math.Round(skillIntervalData.RelativeDifference(), 3));
        }

        [Test]
        public void CalculateAbsoluteDifference()
        {
            var skillIntervalData = new SkillIntervalData(_dtp, 3.63, 2, 3, 0, 0);
            Assert.AreEqual(-2, skillIntervalData.AbsoluteDifference,2);
        }

		[Test]
		public void ShouldCalculateRelativeDifference()
		{
			_target = new SkillIntervalData(_dtp, 10, 5, 3, null, null);
			Assert.AreEqual(-0.5, _target.RelativeDifference());

			_target = new SkillIntervalData(_dtp, 10, -5, 3, null, null);
			Assert.AreEqual(0.5, _target.RelativeDifference());
		}

	
		[Test]
		public void ShouldCalculateBoostedRelativeDifferenceWithMinAndMaxHeads()
		{
			_target = new SkillIntervalData(_dtp, 10, 0, 5, null, null);
			Assert.AreEqual(0, _target.RelativeDifferenceBoosted());

			_target = new SkillIntervalData(_dtp, 10, 0, 5, 5, 5);
			Assert.AreEqual(0, _target.RelativeDifferenceBoosted());

			_target = new SkillIntervalData(_dtp, 10, 0, 5, 4, 4);
			Assert.AreEqual(-10000, _target.RelativeDifferenceBoosted());

			_target = new SkillIntervalData(_dtp, 10, 0, 5, 6, 6);
			Assert.AreEqual(10000, _target.RelativeDifferenceBoosted());

			_target = new SkillIntervalData(_dtp, 10, 0, 5, 6, 4);
			Assert.AreEqual(0, _target.RelativeDifferenceBoosted());
		}
	}
}