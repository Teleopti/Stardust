using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
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
		public void ShouldCalculateAsBeforeWhenUnderStaffed()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 10.80, -10.80, 0, null, null);
			double result = _target.PeriodValue(skillIntervalData, 15, false, false, 0.49, 1);
			Assert.AreEqual(13.91, result, 0.01);
		}

		[Test]
		public void ShouldCalculateAsBeforeWhenUnderStaffed1()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 13.33, -13.33, 0, null, null);
			double result = _target.PeriodValue(skillIntervalData, 15, false, false, 0.5, 1);
			Assert.AreEqual(13.87, result, 0.01);
		}

		[Test]
		public void ShouldCalculateAsBeforeWhenOverStaffed()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 13.33, 13.33, 0, null, null);
			double result = _target.PeriodValue(skillIntervalData, 15, false, false, 0.5, 1);
			Assert.AreEqual(-16.12, result, 0.01);
		}

		[Test]
		public void ShouldHandleZeroForecasted()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 0, 1, 0, null, null);
			double result = _target.PeriodValue(skillIntervalData, 15, false, false, 0.5, 1);
			Assert.AreEqual(0, result, 0.01);
		}

		[Test]
		public void ShouldHandleZeroCurrentDemand()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, 0, 0, null, null);
			double result = _target.PeriodValue(skillIntervalData, 15, false, false, 0.5, 1);
			Assert.AreEqual(0, result, 0.01);
		}

		[Test]
		public void ShouldAdjustForPriority()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, -5, 0, null, null);
			double result = _target.PeriodValue(skillIntervalData, 15, false, false, 0.5, 1);
			Assert.AreEqual(4, result, 0.01);
			result = _target.PeriodValue(skillIntervalData, 15, false, false, 0.5, 0.5);
			Assert.AreEqual(1.5, result, 0.01);
			result = _target.PeriodValue(skillIntervalData, 15, false, false, 0.5, 1.5);
			Assert.AreEqual(6.5, result, 0.01);
		}

		[Test]
		public void ShouldAdjustForOverStaffFactor()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, -5, 0, null, null);
			double result = _target.PeriodValue(skillIntervalData, 15, false, false, 0.5, 1);
			Assert.AreEqual(4, result, 0.01);
			result = _target.PeriodValue(skillIntervalData, 15, false, false, 0.1, 1);
			Assert.AreEqual(8, result, 0.01);
			result = _target.PeriodValue(skillIntervalData, 15, false, false, 0.9, 1);
			Assert.AreEqual(0.5, result, 0.01);
		}

		[Test]
		public void ShouldBoostIfUnderMinHeads()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, -5, 0, null, null);
			double result = _target.PeriodValue(skillIntervalData, 15, true, true, 0.5, 1);
			Assert.AreEqual(4, result, 0.01);

			skillIntervalData = new SkillIntervalData(_period, 15, -5, 9, 11, null);
			result = _target.PeriodValue(skillIntervalData, 15, true, true, 0.5, 1);
			Assert.AreEqual(200004, result, 0.01);

			skillIntervalData = new SkillIntervalData(_period, 15, -5, 10, 11, null);
			result = _target.PeriodValue(skillIntervalData, 15, true, true, 0.5, 1);
			Assert.AreEqual(100004, result, 0.01);

			skillIntervalData = new SkillIntervalData(_period, 15, -5, 11, 11, null);
			result = _target.PeriodValue(skillIntervalData, 15, true, true, 0.5, 1);
			Assert.AreEqual(4, result, 0.01);
		}

		[Test]
		public void ShouldPunishIfOverMaxHeads()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, -5, 10, null, 11);
			double result = _target.PeriodValue(skillIntervalData, 15, true, true, 0.5, 1);
			Assert.AreEqual(4, result, 0.01);

			skillIntervalData = new SkillIntervalData(_period, 15, -5, 11, null, 11);
			result = _target.PeriodValue(skillIntervalData, 15, true, true, 0.5, 1);
			Assert.AreEqual(4-100000, result, 0.01);

			skillIntervalData = new SkillIntervalData(_period, 15, -5, 12, null, 11);
			result = _target.PeriodValue(skillIntervalData, 15, true, true, 0.5, 1);
			Assert.AreEqual(4-200000, result, 0.01);
		}

		[Test]
		public void ShouldHandleZeroAdditionalResource()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, -5, 10, null, 11);
			double result = _target.PeriodValue(skillIntervalData, 0, true, true, 0.5, 1);
			Assert.AreEqual(0, result, 0.01);
		}

		[Test]
		public void ShouldHandleOptionUseBothMinMaxButNoValues()
		{
			ISkillIntervalData skillIntervalData = new SkillIntervalData(_period, 15, -5, 10, null, null);
			double result = _target.PeriodValue(skillIntervalData, 15, true, true, 0.5, 1);
			Assert.AreEqual(4, result, 0.01);
		}
	}
}