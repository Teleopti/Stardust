using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class SkillIntervalDataSkillFactorApplierTest
	{
		private ISkillIntervalDataSkillFactorApplier _target;
		private ISkillIntervalData _skillIntervalData;
		private DateTimePeriod _dp;
		private ISkill _skill;

		[SetUp]
		public void Setup()
		{
			_target = new SkillIntervalDataSkillFactorApplier(new SkillPriorityProvider());
			_dp = new DateTimePeriod(2013, 1, 23, 2013, 1, 24);
			_skill = SkillFactory.CreateSkill("hej");
			((ISkillPriority)_skill).OverstaffingFactor = new Percent(0.5);

		}

		[Test]
		public void ShouldIncreaseUnderstaffingIfDoNotUnderStaff()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 14, 10, 0, null, null);
			((ISkillPriority)_skill).OverstaffingFactor = new Percent(0);
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.AreEqual(10, result.CurrentDemand);
		}

		[Test]
		public void ShouldLeaveOverstaffingIfDoNotUnderStaff()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 10, -10, 0, null, null);
			((ISkillPriority)_skill).OverstaffingFactor = new Percent(0);
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.AreEqual(result.ForecastedDemand, _skillIntervalData.ForecastedDemand);
			Assert.AreEqual(-10, result.CurrentDemand);
		}

		[Test]
		public void ShouldIncreaseOverstaffingIfDoNotOverstaff()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 10, -10, 0, null, null);
			((ISkillPriority)_skill).OverstaffingFactor = new Percent(1);
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.AreEqual(-10, result.CurrentDemand);
		}

		[Test]
		public void ShouldLeaveUnderstaffingIfDoNotOverstaff()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 10, 10, 0, null, null);
			((ISkillPriority)_skill).OverstaffingFactor = new Percent(1);
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.AreEqual(0, result.CurrentDemand);
		}

        [Test]
        public void ShouldReturnNullIfSkillIntervalDataIsNull()
        {
            Assert.IsNull(_target.ApplyFactors(null, _skill)) ;
        }

        [Test]
        public void ShouldReturnNullIfSkillIsNull()
        {
            Assert.IsNull(_target.ApplyFactors(_skillIntervalData , null));
        }

		[Test]
		public void UnderStaffedNormalPriority()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 14, 10, 0, null, null);
			((ISkillPriority)_skill).Priority = 4;
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.AreEqual(14, result.ForecastedDemand);
			Assert.AreEqual(10, result.CurrentDemand);
		}

		[Test]
		public void UnderStaffedHighPriority()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 14, 10, 0, null, null);
			((ISkillPriority)_skill).Priority = 5;
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.AreEqual(14, result.ForecastedDemand);
			Assert.AreEqual(40, result.CurrentDemand);
		}

		[Test]
		public void UnderStaffedLowPriority()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 14, 10, 0, null, null);
			((ISkillPriority)_skill).Priority = 3;
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.AreEqual(14, result.ForecastedDemand);
			Assert.AreEqual(6.4, result.CurrentDemand);
		}

		[Test]
		public void OverStaffedNormalPriority()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 14, -16, 0, null, null);
			((ISkillPriority)_skill).Priority = 4;
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.AreEqual(14, result.ForecastedDemand);
			Assert.AreEqual(-16, result.CurrentDemand);
		}

		[Test]
		public void OverStaffedHighPriority()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 14, -16, 0, null, null);
			((ISkillPriority)_skill).Priority = 5;
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.AreEqual(14, result.ForecastedDemand);
			Assert.AreEqual(-64, result.CurrentDemand);
		}

		[Test]
		public void OverStaffedLowPriority()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 14, -16, 0, null, null);
			((ISkillPriority)_skill).Priority = 3;
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.AreEqual(14, result.ForecastedDemand);
			Assert.AreEqual(-10.24, result.CurrentDemand);
		}
	}
}