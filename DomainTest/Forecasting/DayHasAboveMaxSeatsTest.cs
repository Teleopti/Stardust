using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
	[TestFixture]
	public class DayHasAboveMaxSeatsTest
	{
		private DayHasAboveMaxSeats _target;
		private IList<ISkillStaffPeriod> _skillStaffPeriods;

		[SetUp]
		public void Setup()
		{
			_target = new DayHasAboveMaxSeats();

            DateTimePeriod period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 9, 16, 0, 0, 0, DateTimeKind.Utc), 0);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(period, new Task(), ServiceAgreement.DefaultValues());
			_skillStaffPeriods = new List<ISkillStaffPeriod> { skillStaffPeriod };
		}

		[Test]
		public void VerifyIsSatisfiedByCanReturnTrue()
		{
			_skillStaffPeriods[0].Payload.CalculatedUsedSeats = 12;
			_skillStaffPeriods[0].Payload.MaxSeats = 11;
			Assert.IsTrue(_target.IsSatisfiedBy(_skillStaffPeriods));
		}

		[Test]
		public void VerifyIsSatisfiedByCanReturnFalse()
		{
			_skillStaffPeriods[0].Payload.CalculatedUsedSeats = 11;
			_skillStaffPeriods[0].Payload.MaxSeats = 11;
			Assert.IsFalse(_target.IsSatisfiedBy(_skillStaffPeriods));

			_skillStaffPeriods[0].Payload.CalculatedLoggedOn = 8;
			Assert.IsFalse(_target.IsSatisfiedBy(_skillStaffPeriods));
		}

		[Test]
		public void VerifyIsSatisfiedByCanHandleNoValue()
		{
			Assert.IsFalse(_target.IsSatisfiedBy(_skillStaffPeriods));
		}
	}
}