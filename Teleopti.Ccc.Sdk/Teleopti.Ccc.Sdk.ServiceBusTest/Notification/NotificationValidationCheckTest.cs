﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Notification
{
	[TestFixture]
	public class NotificationValidationCheckTest
	{
		private NotificationValidationCheck _target;
		private ISignificantChangeChecker _significantChangeChecker;
		private INotificationChecker _notificationChecker;
		private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		[SetUp]
		public void Setup()
		{
			_significantChangeChecker = MockRepository.GenerateMock<ISignificantChangeChecker>();
			_notificationChecker = MockRepository.GenerateMock<INotificationChecker>();
			_currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();

			_currentUnitOfWorkFactory = new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal());

			_target = new NotificationValidationCheck(_significantChangeChecker, _notificationChecker, _currentUnitOfWorkFactory);

			DefinedLicenseDataFactory.SetLicenseActivator(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().Name, new LicenseActivator("", DateTime.Today.AddDays(100), 1000, 1000,
																			  LicenseType.Agent, new Percent(.10), null, null));
		}

		private void setValidLicense()
		{
			DefinedLicenseDataFactory.GetLicenseActivator(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().Name).EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccSmsLink);
		}

		//[Test]
		//public void ShouldNotNotifyWhenNoLicense()
		//{
		//	_target.InitiateNotify(null, new DateOnly(), null);

		//	_significantChangeChecker.AssertWasNotCalled(
		//		x =>
		//			x.SignificantChangeNotificationMessage(Arg<DateOnly>.Is.Anything, Arg<IPerson>.Is.Anything,
		//				Arg<ScheduleDayReadModel>.Is.Anything));
		//}

		//[Test]
		//public void ShouldNotNotifyWhenInvalidChange()
		//{
		//	setValidLicense();
		//	var date = new DateOnly();
		//	var person = PersonFactory.CreatePerson();
		//	var readModel = new ScheduleDayReadModel();
		//	var message = new NotificationMessage {Subject = ""};
		//	_significantChangeChecker.Stub(x => x.SignificantChangeNotificationMessage(date, person, readModel)).Return(message);
		//	_target.InitiateNotify(readModel, date, person);
		//	_notificationChecker.AssertWasNotCalled(x => x.SmsMobileNumber(person));
		//}

		//[Test]
		//public void ShouldNotNotifyBySmsWhenNoMobileNumber()
		//{
		//	setValidLicense();
		//	var date = new DateOnly();
		//	var person = PersonFactory.CreatePerson();
		//	var readModel = new ScheduleDayReadModel();
		//	var message = new NotificationMessage {Subject = "This is the message subject"};
		//	_significantChangeChecker.Stub(x => x.SignificantChangeNotificationMessage(date, person, readModel)).Return(message);
		//	_notificationChecker.Stub(x => x.NotificationType).Return(NotificationType.Sms);
		//	_notificationChecker.Stub(x => x.SmsMobileNumber(person)).Return("");
		//	_target.InitiateNotify(readModel, date, person);
		//	_notificationSenderFactory.AssertWasNotCalled(x => x.GetSender());
		//}

		//[Test]
		//public void ShouldNotifyBySms()
		//{
		//	setValidLicense();
		//	var date = new DateOnly();
		//	var person = PersonFactory.CreatePerson();
		//	var readModel = new ScheduleDayReadModel();
		//	var message = new NotificationMessage { Subject = "This is the message subject" };
		//	var notificationSender = MockRepository.GenerateMock<INotificationSender>();

		//	_significantChangeChecker.Stub(x => x.SignificantChangeNotificationMessage(date, person, readModel)).Return(message);
		//	_notificationChecker.Stub(x => x.NotificationType).Return(NotificationType.Sms);
		//	_notificationChecker.Stub(x => x.SmsMobileNumber(person)).Return("123456789");
		//	_notificationSenderFactory.Stub(x => x.GetSender()).Return(notificationSender);

		//	_target.InitiateNotify(readModel, date, person);
		//	notificationSender.AssertWasCalled(x => x.SendNotification(message, "123456789"));
		//}

		//[Test]
		//public void ShouldNotifyByEmail()
		//{
		//	setValidLicense();
		//	var date = new DateOnly();
		//	var person = PersonFactory.CreatePerson();
		//	person.Email = "me@you.com";
		//	var readModel = new ScheduleDayReadModel();
		//	var message = new NotificationMessage { Subject = "This is the message subject" };

		//	_significantChangeChecker.Stub(x => x.SignificantChangeNotificationMessage(date, person, readModel)).Return(message);
		//	_notificationChecker.Stub(x => x.NotificationType).Return(NotificationType.Email);
		//	_notificationChecker.Stub(x => x.EmailSender).Return("sender");

		//	_target.InitiateNotify(readModel, date, person);

		//	_emailNotifier.AssertWasCalled(x => x.Notify(person.Email, "sender", message));
		//}

		//[Test]
		//public void ShouldNotNotifyByEmailWhenEmailAddressIsMissing()
		//{
		//	setValidLicense();
		//	var date = new DateOnly();
		//	var person = PersonFactory.CreatePerson();
		//	person.Email = "";
		//	var readModel = new ScheduleDayReadModel();
		//	var message = new NotificationMessage { Subject = "This is the message subject" };

		//	_significantChangeChecker.Stub(x => x.SignificantChangeNotificationMessage(date, person, readModel)).Return(message);
		//	_notificationChecker.Stub(x => x.NotificationType).Return(NotificationType.Email);

		//	_target.InitiateNotify(readModel, date, person);

		//	_emailNotifier.AssertWasNotCalled(x => x.Notify(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<NotificationMessage>.Is.Anything));
		//}
	}
}
