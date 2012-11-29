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
			double result = _target.PeriodValue(skillIntervalData, 15, false, false);
			Assert.AreEqual(4, result);
		}
	}
}