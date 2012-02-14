using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
	[TestFixture]
	public class IntervalHasAboveMaxSeatsTest
	{
		private IntervalHasAboveMaxSeats _target;
		private ISkillStaffPeriod _skillStaffPeriod;

		[SetUp]
		public void Setup()
		{
			_target = new IntervalHasAboveMaxSeats();

			_skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
                DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 9, 16, 0, 0, 0, DateTimeKind.Utc), 0),
				new Task(), ServiceAgreement.DefaultValues());
		}

		[Test]
		public void VerifyIsSatisfiedByCanReturnTrue()
		{
			_skillStaffPeriod.Payload.CalculatedUsedSeats = 12;
			_skillStaffPeriod.Payload.MaxSeats = 10;
			Assert.IsTrue(_target.IsSatisfiedBy(_skillStaffPeriod));
		}

		[Test]
		public void VerifyIsSatisfiedByCanReturnFalse()
		{
			_skillStaffPeriod.Payload.CalculatedUsedSeats = 10;
			_skillStaffPeriod.Payload.MaxSeats = 10;
			Assert.IsFalse(_target.IsSatisfiedBy(_skillStaffPeriod));

			_skillStaffPeriod.Payload.CalculatedLoggedOn = 8;
			Assert.IsFalse(_target.IsSatisfiedBy(_skillStaffPeriod));
		}

		[Test]
		public void VerifyIsSatisfiedByCanHandleNoValue()
		{
			Assert.IsFalse(_target.IsSatisfiedBy(_skillStaffPeriod));
		}
	}
}