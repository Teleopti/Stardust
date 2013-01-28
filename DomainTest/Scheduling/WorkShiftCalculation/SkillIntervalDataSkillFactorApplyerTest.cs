using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
	[TestFixture]
	public class SkillIntervalDataSkillFactorApplyerTest
	{
		private ISkillIntervalDataSkillFactorApplyer _target;
		private ISkillIntervalData _skillIntervalData;
		private DateTimePeriod _dp;
		private ISkill _skill;

		[SetUp]
		public void Setup()
		{
			_target = new SkillIntervalDataSkillFactorApplyer();
			_dp = new DateTimePeriod(2013, 1, 23, 2013, 1, 23);
			_skill = SkillFactory.CreateSkill("hej");

		}

		[Test]
		public void ShouldIncreaseUnderstaffingIfHighPriority()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 10, 10, 0, null, null);
			_skill.Priority = 7;
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.IsTrue(result.ForecastedDemand > _skillIntervalData.ForecastedDemand);
			Assert.IsTrue(result.CurrentDemand > _skillIntervalData.CurrentDemand);
			Assert.AreEqual(2560, result.CurrentDemand);
		}

		[Test]
		public void ShouldIncreaseOverstaffingIfHighPriority()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 10, -10, 0, null, null);
			_skill.Priority = 7;
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.IsTrue(result.ForecastedDemand < _skillIntervalData.ForecastedDemand);
			Assert.IsTrue(result.CurrentDemand < _skillIntervalData.CurrentDemand);
			Assert.AreEqual(-2560, result.CurrentDemand);
		}

		[Test]
		public void ShouldDecreaseUnderstaffingIfLowPriority()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 10, 10, 0, null, null);
			_skill.Priority = 1;
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.IsTrue(result.ForecastedDemand < _skillIntervalData.ForecastedDemand);
			Assert.IsTrue(result.CurrentDemand < _skillIntervalData.CurrentDemand);
			Assert.AreEqual(10*0.16, result.CurrentDemand);
		}

		[Test]
		public void ShouldDecreaseOverstaffingIfLowPriority()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 10, -10, 0, null, null);
			_skill.Priority = 1;
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.IsTrue(result.ForecastedDemand > _skillIntervalData.ForecastedDemand);
			Assert.IsTrue(result.CurrentDemand > _skillIntervalData.CurrentDemand);
			Assert.AreEqual(-10 * 0.16, result.CurrentDemand);
		}

		[Test]
		public void ShouldIncreaseUnderstaffingIfDoNotUnderstaff()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 10, 10, 0, null, null);
			_skill.OverstaffingFactor = new Percent(0);
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.IsTrue(result.ForecastedDemand > _skillIntervalData.ForecastedDemand);
			Assert.IsTrue(result.CurrentDemand > _skillIntervalData.CurrentDemand);
			Assert.AreEqual(20, result.CurrentDemand);
		}

		[Test]
		public void ShouldLeaveOverstaffingIfDoNotUnderstaff()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 10, -10, 0, null, null);
			_skill.OverstaffingFactor = new Percent(0);
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.AreEqual(result.ForecastedDemand, _skillIntervalData.ForecastedDemand);
			Assert.AreEqual(result.CurrentDemand, _skillIntervalData.CurrentDemand);
		}

		[Test]
		public void ShouldIncreaseOverstaffingIfDoNotOverstaff()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 10, -10, 0, null, null);
			_skill.OverstaffingFactor = new Percent(1);
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.IsTrue(result.ForecastedDemand < _skillIntervalData.ForecastedDemand);
			Assert.IsTrue(result.CurrentDemand < _skillIntervalData.CurrentDemand);
			Assert.AreEqual(-20, result.CurrentDemand);
		}

		[Test]
		public void ShouldLeaveUnderstaffingIfDoNotOverstaff()
		{
			_skillIntervalData = new SkillIntervalData(_dp, 10, 10, 0, null, null);
			_skill.OverstaffingFactor = new Percent(1);
			ISkillIntervalData result = _target.ApplyFactors(_skillIntervalData, _skill);
			Assert.AreEqual(result.ForecastedDemand, _skillIntervalData.ForecastedDemand);
			Assert.AreEqual(result.CurrentDemand, _skillIntervalData.CurrentDemand);
		}
	}
}