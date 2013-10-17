﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class AddDayOffCommandHandlerTest
    {
        private IDayOffTemplateRepository _dayOffRepository;
        private IScheduleRepository _scheduleRepository;
        private IPersonRepository _personRepository;
        private IScenarioRepository _scenarioRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ISaveSchedulePartService _saveSchedulePartService;
        private IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
        private MockRepository _mock;
        private AddDayOffCommandHandler _target;
        private IPerson _person;
        private IScenario _scenario;
        private static DateOnly _startDate = new DateOnly(2012, 1, 1);
		private readonly DateOnlyDto _dateOnydto = new DateOnlyDto { DateTime = _startDate };
        private static DateOnlyPeriod _dateOnlyPeriod = new DateOnlyPeriod(_startDate, _startDate.AddDays(1));
        private DateTimePeriod _period;
        private DayOffTemplate _dayOff;
        private SchedulePartFactoryForDomain _schedulePartFactory;
        private AddDayOffCommandDto _addAbsenceCommandDto;
    	private IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private IScheduleTagAssembler _scheduleTagAssembler;

        [SetUp]
        public  void Setup()
        {
            _mock = new MockRepository();
            _dayOffRepository = _mock.StrictMock<IDayOffTemplateRepository>();
            _scheduleRepository = _mock.StrictMock<IScheduleRepository>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _scenarioRepository = _mock.StrictMock<IScenarioRepository>();
            _currentUnitOfWorkFactory = _mock.DynamicMock<ICurrentUnitOfWorkFactory>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _saveSchedulePartService = _mock.StrictMock<ISaveSchedulePartService>();
            _messageBrokerEnablerFactory = _mock.DynamicMock<IMessageBrokerEnablerFactory>();
    		_businessRulesForPersonalAccountUpdate = _mock.DynamicMock<IBusinessRulesForPersonalAccountUpdate>();
			_scheduleTagAssembler = _mock.DynamicMock<IScheduleTagAssembler>();

            _person = PersonFactory.CreatePerson();
            _person.SetId(Guid.NewGuid());

            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _period = _dateOnlyPeriod.ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone());

            _dayOff = DayOffFactory.CreateDayOff();
            _dayOff.SetId(Guid.NewGuid());

			_target = new AddDayOffCommandHandler(_dayOffRepository, _scheduleRepository, _personRepository, _scenarioRepository, _currentUnitOfWorkFactory, _saveSchedulePartService, _messageBrokerEnablerFactory, _businessRulesForPersonalAccountUpdate, _scheduleTagAssembler);
            _schedulePartFactory = new SchedulePartFactoryForDomain(_person, _scenario, _period, SkillFactory.CreateSkill("Test Skill"));

            _addAbsenceCommandDto = new AddDayOffCommandDto
            {
                PersonId = _person.Id.GetValueOrDefault(),
                Date = _dateOnydto,
                DayOffInfoId = _dayOff.Id.GetValueOrDefault(),
                ScheduleTagId = Guid.NewGuid()
            };
        }

        [Test]
        public void ShouldAddDayOffInTheDictionarySuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var scheduleDay = _schedulePartFactory.CreatePartWithMainShift();
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
            var dictionary = _mock.DynamicMock<IScheduleDictionary>();
        	var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
            var scheduleTag = new ScheduleTag { Description = "test" }; 
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Load(_person.Id.GetValueOrDefault())).Return(_person);
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
				Expect.Call(_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(null, null, _dateOnlyPeriod, _scenario)).IgnoreArguments().Return(dictionary);
                Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
                Expect.Call(_dayOffRepository.Load(_dayOff.Id.GetValueOrDefault())).Return(_dayOff);
                Expect.Call(scheduleRangeMock.ScheduledDay(_startDate)).Return(scheduleDay);
            	Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(_scheduleTagAssembler.DtoToDomainEntity(null)).IgnoreArguments().Return(scheduleTag);
                Expect.Call(() => _saveSchedulePartService.Save(scheduleDay, rules, scheduleTag));
            }
            using (_mock.Playback())
            {
                _target.Handle(_addAbsenceCommandDto);
                scheduleDay.HasDayOff().Should().Be.True();
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ForGiven"), Test]
		public void ShouldAddDayOffInTheDictionaryForGivenScenarioSuccessfully()
		{
			var scenarioId = Guid.NewGuid();
			var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
			var scheduleDay = _schedulePartFactory.CreatePartWithMainShift();
			var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
			var dictionary = _mock.DynamicMock<IScheduleDictionary>();
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
            var scheduleTag = new ScheduleTag { Description = "test" };
			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
				Expect.Call(_personRepository.Load(_person.Id.GetValueOrDefault())).Return(_person);
				Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
				Expect.Call(_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(null, null, _dateOnlyPeriod, _scenario)).IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(_dayOffRepository.Load(_dayOff.Id.GetValueOrDefault())).Return(_dayOff);
				Expect.Call(scheduleRangeMock.ScheduledDay(_startDate)).Return(scheduleDay);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(_scheduleTagAssembler.DtoToDomainEntity(null)).IgnoreArguments().Return(scheduleTag);
				Expect.Call(() => _saveSchedulePartService.Save(scheduleDay, rules, scheduleTag));
			}
			using (_mock.Playback())
			{
				_addAbsenceCommandDto.ScenarioId = scenarioId;
				_target.Handle(_addAbsenceCommandDto);
				scheduleDay.HasDayOff().Should().Be.True();
			}
		}
    }
}
