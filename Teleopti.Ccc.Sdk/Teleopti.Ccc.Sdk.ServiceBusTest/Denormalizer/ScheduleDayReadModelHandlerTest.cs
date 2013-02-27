using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class ScheduleDayReadModelHandlerTest
	{
		private MockRepository _mocks;
		private ISignificantChangeChecker _significantChangeChecker;
		private INotificationSender _notificationSender;
		private ScheduleDayReadModelHandler _target;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IPersonRepository _personRepository;
		private ISmsLinkChecker _smsLinkChecker;
		private INotificationSenderFactory _notificationSenderFactory;
		private IScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_personRepository = _mocks.StrictMock<IPersonRepository>();
			_significantChangeChecker = _mocks.StrictMock<ISignificantChangeChecker>();
			_smsLinkChecker = _mocks.StrictMock<ISmsLinkChecker>();
			_notificationSenderFactory = _mocks.StrictMock<INotificationSenderFactory>();
			_notificationSender = _mocks.StrictMock<INotificationSender>();
			_scheduleDayReadModelsCreator = _mocks.StrictMock<IScheduleDayReadModelsCreator>();
			_scheduleDayReadModelRepository = _mocks.StrictMock<IScheduleDayReadModelRepository>();

			_target = new ScheduleDayReadModelHandler(_unitOfWorkFactory, _personRepository, _significantChangeChecker, 
				_smsLinkChecker, _notificationSenderFactory,_scheduleDayReadModelsCreator, _scheduleDayReadModelRepository);

			DefinedLicenseDataFactory.LicenseActivator = new LicenseActivator("", DateTime.Today.AddDays(100), 1000, 1000,
			                                                                  LicenseType.Agent, new Percent(.10), null, null);

			_person = PersonFactory.CreatePerson();
			_person.SetId(Guid.NewGuid());
		}

		[Test]
		public void ShouldSkipOutIfNotDefaultScenario()
		{
			DefinedLicenseDataFactory.LicenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccSmsLink);
			
			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(2));
			var uow = _mocks.StrictMock<IUnitOfWork>();

			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(uow.Dispose);

			_mocks.ReplayAll();
			_target.Consume(new DenormalizedSchedule
			{
				PersonId = _person.Id.GetValueOrDefault(),
				StartDateTime = period.StartDateTime,
				EndDateTime = period.EndDateTime,
				IsDefaultScenario = false
			});
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldSkipOutIfNoLicense()
		{
			var period = new DateTimePeriod(new DateTime(2012, 12, 1, 10, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 1, 17, 0, 0, DateTimeKind.Utc));
			var dateOnlyPeriod = period.ToDateOnlyPeriod(_person.PermissionInformation.DefaultTimeZone());
			var uow = _mocks.StrictMock<IUnitOfWork>();
			var model = new ScheduleDayReadModel();
			var message = new DenormalizedSchedule
			              	{
								Date = dateOnlyPeriod.StartDate,
			              		PersonId = _person.Id.GetValueOrDefault(),
			              		StartDateTime = period.StartDateTime,
			              		EndDateTime = period.EndDateTime,
								IsDefaultScenario = true
			              	};

			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(_personRepository.Get(_person.Id.GetValueOrDefault())).Return(_person);
			Expect.Call(_scheduleDayReadModelsCreator.GetReadModels(message)).Return(model);
			Expect.Call(() => _scheduleDayReadModelRepository.ClearPeriodForPerson(dateOnlyPeriod, _person.Id.GetValueOrDefault()));
			Expect.Call(() => _scheduleDayReadModelRepository.SaveReadModel(model));
			Expect.Call(uow.Dispose);
			Expect.Call(uow.PersistAll());

			_mocks.ReplayAll();
			_target.Consume(message);
			_mocks.VerifyAll();
		}
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ändrats"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Sdk.ServiceBus.SMS.INotificationSender.SendNotification(System.String,System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldCheckSignificantChangeAndSendIfTrue()
		{
			DefinedLicenseDataFactory.LicenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccSmsLink);
			
			var mess = new NotificationMessage {Subject = "ändrats!"};
			var period = new DateTimePeriod(new DateTime(2012, 12, 1, 10, 0, 0, DateTimeKind.Utc),
			                                new DateTime(2012, 12, 1, 17, 0, 0, DateTimeKind.Utc));
			var dateOnlyPeriod = period.ToDateOnlyPeriod(_person.PermissionInformation.DefaultTimeZone());
			var uow = _mocks.StrictMock<IUnitOfWork>();
			var model = new ScheduleDayReadModel();
			var message = new DenormalizedSchedule
			              	{
								Date = dateOnlyPeriod.StartDate,
			              		PersonId = _person.Id.GetValueOrDefault(),
			              		StartDateTime = period.StartDateTime,
			              		EndDateTime = period.EndDateTime,
								IsDefaultScenario = true
			              	};

			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(_personRepository.Get(_person.Id.GetValueOrDefault())).Return(_person);
			Expect.Call(_scheduleDayReadModelsCreator.GetReadModels(message)).Return(model);
			Expect.Call(_significantChangeChecker.SignificantChangeNotificationMessage(dateOnlyPeriod.StartDate, _person, model)).Return(mess);
			Expect.Call(_smsLinkChecker.SmsMobileNumber(_person)).Return("124578");
			Expect.Call(_notificationSenderFactory.GetSender()).Return(_notificationSender);
			Expect.Call(() => _notificationSender.SendNotification(mess, "124578"));
			Expect.Call(() =>_scheduleDayReadModelRepository.ClearPeriodForPerson(dateOnlyPeriod, _person.Id.GetValueOrDefault()));
			Expect.Call(() => _scheduleDayReadModelRepository.SaveReadModel(model));
			Expect.Call(uow.Dispose);
			Expect.Call(uow.PersistAll());

			_mocks.ReplayAll();
			_target.Consume(message);
			_mocks.VerifyAll();
		}
	}
}