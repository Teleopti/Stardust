using System;
using System.Collections.Generic;
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
		private IScenarioRepository _scenarioRepository;
		private IPersonRepository _personRepository;
		private ISmsLinkChecker _smsLinkChecker;
		private INotificationSenderFactory _notificationSenderFactory;
		private IScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private IScheduleDayReadModelRepository _scheduleDayReadModelRepository;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_scenarioRepository = _mocks.StrictMock<IScenarioRepository>();
			_personRepository = _mocks.StrictMock<IPersonRepository>();
			_significantChangeChecker = _mocks.StrictMock<ISignificantChangeChecker>();
			_smsLinkChecker = _mocks.StrictMock<ISmsLinkChecker>();
			_notificationSenderFactory = _mocks.StrictMock<INotificationSenderFactory>();
			_notificationSender = _mocks.StrictMock<INotificationSender>();
			_scheduleDayReadModelsCreator = _mocks.StrictMock<IScheduleDayReadModelsCreator>();
			_scheduleDayReadModelRepository = _mocks.StrictMock<IScheduleDayReadModelRepository>();

			_target = new ScheduleDayReadModelHandler(_unitOfWorkFactory, _scenarioRepository, _personRepository, _significantChangeChecker, 
				_smsLinkChecker, _notificationSenderFactory,_scheduleDayReadModelsCreator, _scheduleDayReadModelRepository);

			DefinedLicenseDataFactory.LicenseActivator = new LicenseActivator("", DateTime.Now.AddDays(100), 1000, 1000,
			                                                                  LicenseType.Agent, new Percent(.10), null, null);
			
		}

		[Test]
		public void ShouldSkipOutIfNotDefaultScenario()
		{
			DefinedLicenseDataFactory.LicenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccSmsLink);
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			scenario.SetId(Guid.NewGuid());
			scenario.DefaultScenario = false;

			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(2));
			var uow = _mocks.StrictMock<IUnitOfWork>();

			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(_scenarioRepository.Get(scenario.Id.GetValueOrDefault())).Return(scenario);
			
			Expect.Call(uow.Dispose);

			_mocks.ReplayAll();
			_target.Consume(new DenormalizeScheduleProjection
			{
				ScenarioId = scenario.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				StartDateTime = period.StartDateTime,
				EndDateTime = period.EndDateTime
			});
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldSkipOutIfNoLicense()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			scenario.SetId(Guid.NewGuid());
			scenario.DefaultScenario = true;

			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());
			var period = new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(2));
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(DateTime.UtcNow.Date), new DateOnly(DateTime.UtcNow.Date.AddDays(1)));
			var uow = _mocks.StrictMock<IUnitOfWork>();
			var models = new List<ScheduleDayReadModel>();

			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(_scenarioRepository.Get(scenario.Id.GetValueOrDefault())).Return(scenario);
			Expect.Call(_personRepository.Get(person.Id.GetValueOrDefault())).Return(person);
			Expect.Call(_scheduleDayReadModelsCreator.GetReadModels(scenario, period, person)).Return(models);
			Expect.Call(() => _scheduleDayReadModelRepository.ClearPeriodForPerson(dateOnlyPeriod, person.Id.Value));
			Expect.Call(() => _scheduleDayReadModelRepository.SaveReadModels(models));
			Expect.Call(uow.Dispose);

			_mocks.ReplayAll();
			_target.Consume(new DenormalizeScheduleProjection
			{
				ScenarioId = scenario.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				StartDateTime = period.StartDateTime,
				EndDateTime = period.EndDateTime
			});
			_mocks.VerifyAll();
		}
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ändrats"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Sdk.ServiceBus.SMS.INotificationSender.SendNotification(System.String,System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldCheckSignificantChangeAndSendIfTrue()
		{
			DefinedLicenseDataFactory.LicenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccSmsLink);
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			scenario.SetId(Guid.NewGuid());
			scenario.DefaultScenario = true;

			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());
			var mess = new NotificationMessage {Subject = "ändrats!"};
			var period = new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(2));
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(DateTime.UtcNow.Date), new DateOnly(DateTime.UtcNow.Date.AddDays(1)));
			var uow = _mocks.StrictMock<IUnitOfWork>();
			var models = new List<ScheduleDayReadModel>();

			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(_scenarioRepository.Get(scenario.Id.GetValueOrDefault())).Return(scenario);
			Expect.Call(_personRepository.Get(person.Id.GetValueOrDefault())).Return(person);
			Expect.Call(_scheduleDayReadModelsCreator.GetReadModels(scenario, period, person)).Return(models);
			Expect.Call(_significantChangeChecker.SignificantChangeNotificationMessage(dateOnlyPeriod, person, models)).Return(mess);
			Expect.Call(_smsLinkChecker.SmsMobileNumber(person)).Return("124578");
			Expect.Call(_notificationSenderFactory.GetSender()).Return(_notificationSender);
			Expect.Call(() => _notificationSender.SendNotification(mess, "124578"));
			Expect.Call(() =>_scheduleDayReadModelRepository.ClearPeriodForPerson(dateOnlyPeriod, person.Id.Value));
			Expect.Call(() => _scheduleDayReadModelRepository.SaveReadModels(models));
			Expect.Call(uow.Dispose);

			_mocks.ReplayAll();
			_target.Consume(new DenormalizeScheduleProjection
			{
				ScenarioId = scenario.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				StartDateTime = period.StartDateTime,
				EndDateTime = period.EndDateTime
			});
			_mocks.VerifyAll();
		}
	}

}