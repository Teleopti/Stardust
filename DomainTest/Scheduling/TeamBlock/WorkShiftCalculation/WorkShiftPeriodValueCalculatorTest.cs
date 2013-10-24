using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
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
			double result = _target.PeriodValue(skillIntervalData, 15, false, false);
			Assert.AreEqual(12.5, result, 0.01);
		}

		//[Test]
		//public void ShouldCalculateAsBeforeWhenUnderstaffedShouldBoostIntervalWithNearToZeroScheduled()
		//{
		//	ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 13.33, 13.33, 0, null, null);
		//	double result = _target.PeriodValue(skillIntervalData, 15, false, false);
		//	Assert.AreEqual(100028.87, result, 0.01);
		//}

		[Test]
		public void ShouldCalculateAsBeforeWhenUnderstaffedShouldNotBoostIntervalWithNotThatNearToZeroScheduled()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 13.33, 13, 0, null, null);
			double result = _target.PeriodValue(skillIntervalData, 15, false, false);
			Assert.AreEqual(28.13, result, 0.01);
		}

		[Test]
		public void ShouldCalculateAsBeforeWhenOverstaffed()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 13.33, -13.33, 0, null, null);
			double result = _target.PeriodValue(skillIntervalData, 15, false, false);
			Assert.AreEqual(-31.12, result, 0.01);
		}

		[Test]
		public void ShouldHandleZeroForecasted()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 0, -1, 0, null, null);
			double result = _target.PeriodValue(skillIntervalData, 15, false, false);
			Assert.AreEqual(0, result, 0.01);
		}

		[Test]
		public void ShouldHandleZeroCurrentDemand()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, 0, 0, null, null);
			double result = _target.PeriodValue(skillIntervalData, 15, false, false);
			Assert.AreEqual(-1, result, 0.01);
		}

		
		[Test]
		public void ShouldBoostIfUnderMinHeads()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, 5, 0, null, null);
			double result = _target.PeriodValue(skillIntervalData, 15, true, true);
			Assert.AreEqual(9, result, 0.01);

			skillIntervalData = new SkillIntervalData(_period, 15, 5, 9, 11, null);
			result = _target.PeriodValue(skillIntervalData, 15, true, true);
			Assert.AreEqual(200009, result, 0.01);

			skillIntervalData = new SkillIntervalData(_period, 15, 5, 10, 11, null);
			result = _target.PeriodValue(skillIntervalData, 15, true, true);
			Assert.AreEqual(100009, result, 0.01);

			skillIntervalData = new SkillIntervalData(_period, 15, 5, 11, 11, null);
			result = _target.PeriodValue(skillIntervalData, 15, true, true);
			Assert.AreEqual(9, result, 0.01);
		}

		[Test]
		public void ShouldPunishIfOverMaxHeads()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, 5, 10, null, 11);
			double result = _target.PeriodValue(skillIntervalData, 15, true, true);
			Assert.AreEqual(9, result, 0.01);

			skillIntervalData = new SkillIntervalData(_period, 15, 5, 11, null, 11);
			result = _target.PeriodValue(skillIntervalData, 15, true, true);
			Assert.AreEqual(9-100000, result, 0.01);

			skillIntervalData = new SkillIntervalData(_period, 15, 5, 12, null, 11);
			result = _target.PeriodValue(skillIntervalData, 15, true, true);
			Assert.AreEqual(9-200000, result, 0.01);
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
			double result = _target.PeriodValue(skillIntervalData, 15, true, true);
			Assert.AreEqual(9, result, 0.01);
		}
	}
}