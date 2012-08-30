using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Notification
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
		public void ShouldReturnFalseIfPeriodAfterWithinFourteenDays()
		{
			var date = DateTime.Now.Date;
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(20)), new DateOnly(date.AddDays(30)) );

			Assert.That(_target.SignificantChangeMessages(period, _person).Subject, Is.EqualTo(""));
		}

		[Test]
		public void ShouldReturnFalseIfPeriodBeforeWithinFourteenDays()
		{
			var date = DateTime.Now.Date;
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(-20)), new DateOnly(date.AddDays(-1)));

			Assert.That(_target.SignificantChangeMessages(period, _person).Subject, Is.Empty);
		}

		[Test]
		public void ShouldReturnTrueIfPeriodEndsWithinFourteenDays()
		{
			var date = DateTime.Now.Date;
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(-20)), new DateOnly(date.AddDays(1)));

			Assert.That(_target.SignificantChangeMessages(period, _person).Subject, Is.Not.Empty);
		}

		[Test]
		public void ShouldReturnTrueIfPeriodStartsWithinFourteenDays()
		{
			var date = DateTime.Now.Date;
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(5)), new DateOnly(date.AddDays(20)));

			Assert.That(_target.SignificantChangeMessages(period, _person).Subject, Is.Not.Empty);
		}
	}

}