using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.ServiceBus.SMS;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Sms
{
	[TestFixture]
	public class SignificantChangeCheckerTest
	{
		private MockRepository _mocks;
		private SignificantChangeChecker _target;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new SignificantChangeChecker();
			_person = _mocks.StrictMock<IPerson>();
		}

		[Test]
		public void ShouldReturnFalseIfPeriodAfterWithingFourteenDays()
		{
			var date = DateTime.Now.Date;
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(20)), new DateOnly(date.AddDays(30)) );

			Assert.That(_target.IsSignificantChange(period, _person), Is.False);
		}

		[Test]
		public void ShouldReturnFalseIfPeriodBeforeWithingFourteenDays()
		{
			var date = DateTime.Now.Date;
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(-20)), new DateOnly(date.AddDays(-1)));

			Assert.That(_target.IsSignificantChange(period, _person), Is.False);
		}

		[Test]
		public void ShouldReturnTrueIfPeriodEndsWithingFourteenDays()
		{
			var date = DateTime.Now.Date;
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(-20)), new DateOnly(date.AddDays(1)));

			Assert.That(_target.IsSignificantChange(period, _person), Is.True);
		}

		[Test]
		public void ShouldReturnTrueIfPeriodStartsWithingFourteenDays()
		{
			var date = DateTime.Now.Date;
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(5)), new DateOnly(date.AddDays(20)));

			Assert.That(_target.IsSignificantChange(period, _person), Is.True);
		}
	}

}