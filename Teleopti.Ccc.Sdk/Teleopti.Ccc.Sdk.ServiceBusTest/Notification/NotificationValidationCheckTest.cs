using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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
		private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private INotifier _notifier;

		[SetUp]
		public void Setup()
		{
			_significantChangeChecker = MockRepository.GenerateMock<ISignificantChangeChecker>();
			_notifier = MockRepository.GenerateMock<INotifier>();
			_currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();

			_currentUnitOfWorkFactory = new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal());

			_target = new NotificationValidationCheck(_significantChangeChecker, _notifier, _currentUnitOfWorkFactory);

			DefinedLicenseDataFactory.SetLicenseActivator(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().Name, new LicenseActivator("", DateTime.Today.AddDays(100), 1000, 1000,
																			  LicenseType.Agent, new Percent(.10), null, null));
		}

		private void setValidLicense()
		{
			DefinedLicenseDataFactory.GetLicenseActivator(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().Name).EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccSmsLink);
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
			_notifier.AssertWasNotCalled(x => x.Notify(message, person));
		}


		[Test]
		public void ShouldNotify()
		{
			setValidLicense();
			var date = new DateOnly();
			var person = PersonFactory.CreatePerson();
			person.Email = "john@doe.org";
			var readModel = new ScheduleDayReadModel();
			var message = new NotificationMessage { Subject = "This is the message subject" };

			_significantChangeChecker.Stub(x => x.SignificantChangeNotificationMessage(date, person, readModel)).Return(message);
			
			_target.InitiateNotify(readModel, date, person);

			_notifier.AssertWasCalled(x => x.Notify(message, person));
		}
	}
}
