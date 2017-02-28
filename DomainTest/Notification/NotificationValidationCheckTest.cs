﻿using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Notification
{
	[TestFixture]
	public class NotificationValidationCheckTest
	{
		private NotificationValidationCheck _target;
		private ISignificantChangeChecker _significantChangeChecker;
		private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private FakeNotifier _notifier;

		[SetUp]
		public void Setup()
		{
			_significantChangeChecker = MockRepository.GenerateMock<ISignificantChangeChecker>();
			_notifier = new FakeNotifier();
			_currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_currentUnitOfWorkFactory.Stub(x => x.Current())
				.Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.Name).Return("for test");

			_target = new NotificationValidationCheck(_significantChangeChecker, _notifier, _currentUnitOfWorkFactory);

			DefinedLicenseDataFactory.SetLicenseActivator(_currentUnitOfWorkFactory.Current().Name, new LicenseActivator("Test", DateTime.Today.AddDays(100), false, 1000, 1000,
																			  LicenseType.Agent, new Percent(.10), null, null, "8"));
		}

		private void setValidLicense()
		{
			DefinedLicenseDataFactory.GetLicenseActivator(_currentUnitOfWorkFactory.Current().Name).EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccSmsLink);
		}

		[Test]
		public void ShouldNotNotifyWhenNoLicense()
		{
			_target.InitiateNotify(null, new DateOnly(), null);

			_significantChangeChecker.AssertWasNotCalled(
				x =>
					x.SignificantChangeNotificationMessage(Arg<DateOnly>.Is.Anything, Arg<IPerson>.Is.Anything,
						Arg<ScheduleDayReadModel>.Is.Anything));
		}

		[Test]
		public void ShouldNotNotifyWhenInvalidChange()
		{
			setValidLicense();
			var date = new DateOnly();
			var person = PersonFactory.CreatePerson();
			var readModel = new ScheduleDayReadModel();
			var message = new NotificationMessage { Subject = "" };
			_significantChangeChecker.Stub(x => x.SignificantChangeNotificationMessage(date, person, readModel)).Return(message);
			_target.InitiateNotify(readModel, date, person);
			_notifier.SentMessages.Should().Be.Empty();
		}


		[Test]
		public void ShouldNotifyAndSetCustomerName()
		{
			setValidLicense();
			var date = new DateOnly();
			var person = PersonFactory.CreatePerson();
			person.Email = "john@doe.org";
			var readModel = new ScheduleDayReadModel();
			var message = new NotificationMessage { Subject = "This is the message subject" };

			_significantChangeChecker.Stub(x => x.SignificantChangeNotificationMessage(date, person, readModel)).Return(message);
			
			_target.InitiateNotify(readModel, date, person);

			_notifier.SentMessages.Should().Not.Be.Empty();
			var sentMessage = _notifier.SentMessages.Single();
			sentMessage.Message.CustomerName.Should().Be.EqualTo("Test");
			sentMessage.Message.Subject.Should().Be.EqualTo(message.Subject);
			sentMessage.Person.Should().Be.EqualTo(person);
		}
	}
}
