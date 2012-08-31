using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Repositories;
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
		private IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private IScheduleDayReadModelComparer _scheduleDayReadModelComparer;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleDayReadModelRepository = _mocks.StrictMock<IScheduleDayReadModelRepository>();
			_scheduleDayReadModelComparer = _mocks.StrictMock<IScheduleDayReadModelComparer>();
			_target = new SignificantChangeChecker(_scheduleDayReadModelRepository, _scheduleDayReadModelComparer);
			_person = _mocks.StrictMock<IPerson>();
		}

		[Test]
		public void ShouldReturnFalseIfPeriodAfterWithinFourteenDays()
		{
			var date = DateTime.Now.Date;
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(20)), new DateOnly(date.AddDays(30)) );

			Assert.That(_target.SignificantChangeNotificationMessage(period, _person, new List<ScheduleDayReadModel>()).Subject, Is.EqualTo(""));
		}

		[Test]
		public void ShouldReturnFalseIfPeriodBeforeWithinFourteenDays()
		{
			var date = DateTime.Now.Date;
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(-20)), new DateOnly(date.AddDays(-1)));

			Assert.That(_target.SignificantChangeNotificationMessage(period, _person, new List<ScheduleDayReadModel>()).Subject, Is.Empty);
		}

		
	}

}