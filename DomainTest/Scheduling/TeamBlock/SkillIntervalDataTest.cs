using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
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
		public void ShouldCalculateRelativeDifference()
		{
			_target = new SkillIntervalData(_dtp, 10, 5, 3, null, null);
			Assert.AreEqual(-0.5, _target.RelativeDifference());

			_target = new SkillIntervalData(_dtp, 10, -5, 3, null, null);
			Assert.AreEqual(0.5, _target.RelativeDifference());
		}

		[Test]
		public void ShouldCalculateBoostedRelativeDifferenceWhenMinHeadsNotFilfilled()
		{
			_target = new SkillIntervalData(_dtp, 10, 10, 1, 1, null);
			Assert.AreEqual(-1, _target.RelativeDifferenceMinStaffBoosted());

			_target = new SkillIntervalData(_dtp, 10, 10, 0, 1, null);
			Assert.AreEqual(-10001, _target.RelativeDifferenceMinStaffBoosted());

			_target = new SkillIntervalData(_dtp, 10, 10, 0, 2, null);
			Assert.AreEqual(-20001, _target.RelativeDifferenceMinStaffBoosted());
		}

		[Test]
		public void ShouldCalculateBoostedRelativeDifferenceWhenMaxHeadsIsBroken()
		{
			_target = new SkillIntervalData(_dtp, 10, -5, 5, null, 5);
			Assert.AreEqual(0.5, _target.RelativeDifferenceMaxStaffBoosted());

			_target = new SkillIntervalData(_dtp, 10, -5, 5, null, 4);
			Assert.AreEqual(10000.5, _target.RelativeDifferenceMaxStaffBoosted());

			_target = new SkillIntervalData(_dtp, 10, -5, 5, null, 3);
			Assert.AreEqual(20000.5, _target.RelativeDifferenceMaxStaffBoosted());
		}

		[Test]
		public void ShouldCalculateBoostedRelativeDifferenceWithMinAndMaxHeads()
		{
			_target = new SkillIntervalData(_dtp, 10, 0, 5, null, null);
			Assert.AreEqual(0, _target.RelativeDifferenceBoosted());

			_target = new SkillIntervalData(_dtp, 10, 0, 5, 5, 5);
			Assert.AreEqual(0, _target.RelativeDifferenceBoosted());

			_target = new SkillIntervalData(_dtp, 10, 0, 5, 4, 4);
			Assert.AreEqual(10000, _target.RelativeDifferenceBoosted());

			_target = new SkillIntervalData(_dtp, 10, 0, 5, 6, 6);
			Assert.AreEqual(-10000, _target.RelativeDifferenceBoosted());

			_target = new SkillIntervalData(_dtp, 10, 0, 5, 6, 4);
			Assert.AreEqual(0, _target.RelativeDifferenceBoosted());
		}
	}
}