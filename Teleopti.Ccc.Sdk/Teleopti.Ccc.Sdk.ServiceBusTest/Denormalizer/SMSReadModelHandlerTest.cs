using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.Sdk.ServiceBus.SMS;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms"), TestFixture]
	public class SmsReadModelHandlerTest
	{
		private MockRepository _mocks;
		private ISignificantChangeChecker _significantChangeChecker;
		private ISmsSender _smsSender;
		private SmsReadModelHandler _target;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IScenarioRepository _scenarioRepository;
		private IPersonRepository _personRepository;
		private ISmsLinkChecker _smsLinkChecker;
		private ISmsSenderFactory _smsSenderFactory;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_scenarioRepository = _mocks.StrictMock<IScenarioRepository>();
			_personRepository = _mocks.StrictMock<IPersonRepository>();
			_significantChangeChecker = _mocks.StrictMock<ISignificantChangeChecker>();
			_smsLinkChecker = _mocks.StrictMock<ISmsLinkChecker>();
			_smsSenderFactory = _mocks.StrictMock<ISmsSenderFactory>();
			_smsSender = _mocks.StrictMock<ISmsSender>();
			_target = new SmsReadModelHandler(_unitOfWorkFactory, _scenarioRepository, _personRepository, _significantChangeChecker, _smsLinkChecker, _smsSenderFactory);
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
			//var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(DateTime.UtcNow.Date), new DateOnly(DateTime.UtcNow.Date.AddDays(1)));
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
			_mocks.ReplayAll();
			_target.Consume(new DenormalizeScheduleProjection
			{
				ScenarioId = Guid.NewGuid(),
				PersonId = Guid.NewGuid(),
				StartDateTime = DateTime.UtcNow.Date,
				EndDateTime = DateTime.UtcNow.Date
			});
			_mocks.VerifyAll();
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldCheckSignificantChangeAndSendIfTrue()
		{
			DefinedLicenseDataFactory.LicenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccSmsLink);
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			scenario.SetId(Guid.NewGuid());
			scenario.DefaultScenario = true;

			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(2));
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(DateTime.UtcNow.Date), new DateOnly(DateTime.UtcNow.Date.AddDays(1)));
			var uow = _mocks.StrictMock<IUnitOfWork>();

			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(_scenarioRepository.Get(scenario.Id.GetValueOrDefault())).Return(scenario);
			Expect.Call(_personRepository.Get(person.Id.GetValueOrDefault())).Return(person);
			Expect.Call(_significantChangeChecker.SignificantChangeMessages(dateOnlyPeriod, person)).Return(new List<string>{"ändrats!"});
			Expect.Call(_smsLinkChecker.SmsMobileNumber(person)).Return("124578");
			Expect.Call(_smsSenderFactory.Sender).Return(_smsSender);
			Expect.Call(() => _smsSender.SendSms("ändrats!", "124578"));
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