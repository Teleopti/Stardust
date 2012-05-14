﻿using System;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
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
    public class CancelAbsenceCommandHandlerTest
    {
        private MockRepository _mock;
        private IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private IScheduleRepository _scheduleRepository;
        private IPersonRepository _personRepository;
        private IScenarioRepository _scenarioRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ISaveSchedulePartService _saveSchedulePartService;
        private IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
        private CancelAbsenceCommandHandler _target;
        private IPerson _person;
        private IAbsence _absence;
        private IScenario _scenario;
        private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTimePeriod _period = new DateTimePeriod(_startDate, _startDate.AddDays(1));
        private readonly DateTimePeriodDto _periodDto = new DateTimePeriodDto()
        {
            UtcStartTime = _startDate,
            UtcEndTime = _startDate.AddDays(1)
        };

        private SchedulePartFactoryForDomain _schedulePartFactoryForDomain;
        private CancelAbsenceCommandDto _cancelAbsenceCommandDto;


        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _dateTimePeriodAssembler = _mock.StrictMock<IAssembler<DateTimePeriod, DateTimePeriodDto>>();
            _scheduleRepository = _mock.StrictMock<IScheduleRepository>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _scenarioRepository = _mock.StrictMock<IScenarioRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _saveSchedulePartService = _mock.StrictMock<ISaveSchedulePartService>();
            _messageBrokerEnablerFactory = _mock.DynamicMock<IMessageBrokerEnablerFactory>();
            _target = new CancelAbsenceCommandHandler(_dateTimePeriodAssembler,_scheduleRepository,_personRepository,_scenarioRepository,_unitOfWorkFactory,_saveSchedulePartService,_messageBrokerEnablerFactory);

            _person = PersonFactory.CreatePerson();
            _person.SetId(Guid.NewGuid());

            _absence = AbsenceFactory.CreateAbsence("Sick");
            _absence.SetId(Guid.NewGuid());

            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _schedulePartFactoryForDomain = new SchedulePartFactoryForDomain(_person, _scenario, _period, SkillFactory.CreateSkill("Test Skill"));

            _cancelAbsenceCommandDto = new CancelAbsenceCommandDto { Period = _periodDto, PersonId = _person.Id.GetValueOrDefault() };
        }

        [Test]
        public void AbsenceCancelSuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var scheduleDay = _schedulePartFactoryForDomain.CreatePart();
            scheduleDay.CreateAndAddAbsence(new AbsenceLayer(_absence,_period));
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
            var dictionary = _mock.DynamicMock<IScheduleDictionary>();
            
            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personRepository.Load(_cancelAbsenceCommandDto.PersonId)).Return(_person);
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
                Expect.Call(_dateTimePeriodAssembler.DtoToDomainEntity(_cancelAbsenceCommandDto.Period)).Return(_period);
                Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, _period, _scenario)).
                    IgnoreArguments().Return(dictionary);
                Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
                Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(scheduleDay);
                Expect.Call(()=>_saveSchedulePartService.Save(unitOfWork, scheduleDay));
            }
            using(_mock.Playback())
            {
                _target.Handle(_cancelAbsenceCommandDto);
                scheduleDay.PersonAbsenceCollection().Count.Should().Be.EqualTo(0);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowExceptionIfScheduleDayIsNull()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
            var dictionary = _mock.DynamicMock<IScheduleDictionary>();

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personRepository.Load(_cancelAbsenceCommandDto.PersonId)).Return(_person);
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
                Expect.Call(_dateTimePeriodAssembler.DtoToDomainEntity(_cancelAbsenceCommandDto.Period)).Return(_period);
                Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, _period, _scenario)).
                    IgnoreArguments().Return(dictionary);
                Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
                Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(null);
                Expect.Call(() => _saveSchedulePartService.Save(unitOfWork, null));
            }
            using (_mock.Playback())
            {
                _target.Handle(_cancelAbsenceCommandDto);
            }
        }

    }
}
