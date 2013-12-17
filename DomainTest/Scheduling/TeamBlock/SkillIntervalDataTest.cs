using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
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
            var skillIntervalData = new SkillIntervalData(_dtp, 3.5, 3, 1, 2,4 );
            Assert.AreEqual(1,skillIntervalData.MinMaxBoostFactor);
        }

        [Test]
        public void CalculateBoostedValueTestB()
        {
            var skillIntervalData = new SkillIntervalData(_dtp, 3.5, 3, 4, 2, 4);
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
            var skillIntervalData = new SkillIntervalData(_dtp, 3.5, 3, 1, 2, 2);
            Assert.AreEqual(1, skillIntervalData.MinMaxBoostFactor);
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
            Assert.AreEqual( skillIntervalData.RelativeDifference, -0.17,0.01 );
        }

        [Test]
        public void CalculateAbsoluteDifference()
        {
            var skillIntervalData = new SkillIntervalData(_dtp, 3.63, 2, 3, 0, 0);
            Assert.AreEqual(skillIntervalData.AbsoluteDifference, -0.63,0.01);
        }
	}
}