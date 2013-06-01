﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
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
    public class AddOvertimeCommandHandlerTest
    {
        private MockRepository _mock;
        private IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodMock;
        private IActivityRepository _activityRepository;
        private IScheduleRepository _scheduleRepository;
        private IPersonRepository _personRepository;
        private IScenarioRepository _scenarioRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ISaveSchedulePartService _saveSchedulePartService;
        private IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
        private AddOvertimeCommandHandler _target;
        private IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
        private IPerson _person;
        private IActivity _activity;
        private IScenario _scenario;
        private static DateOnly _startDate = new DateOnly(2012, 1, 1);
		private readonly DateOnlyDto _dateOnyldto = new DateOnlyDto { DateTime = _startDate };
        private readonly DateTimePeriodDto _periodDto = new DateTimePeriodDto
        {
            UtcStartTime = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UtcEndTime = new DateTime(2012, 1, 2, 0, 0, 0, DateTimeKind.Utc)
        };

        private static DateOnlyPeriod _dateOnlyPeriod = new DateOnlyPeriod(_startDate, _startDate.AddDays(1));
        private DateTimePeriod _period;
        private MultiplicatorDefinitionSet _multiplicatorDefinitionSet;
        private AddOvertimeCommandDto _addOvertimeCommandDto;
    	private IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
        private IScheduleTagRepository _scheduleTagRepository;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _dateTimePeriodMock = _mock.StrictMock<IAssembler<DateTimePeriod, DateTimePeriodDto>>();
            _multiplicatorDefinitionSetRepository = _mock.StrictMock<IMultiplicatorDefinitionSetRepository>();
            _activityRepository = _mock.StrictMock<IActivityRepository>();
            _scheduleRepository = _mock.StrictMock<IScheduleRepository>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _scenarioRepository = _mock.StrictMock<IScenarioRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = _mock.DynamicMock<ICurrentUnitOfWorkFactory>();
            _saveSchedulePartService = _mock.StrictMock<ISaveSchedulePartService>();
            _messageBrokerEnablerFactory = _mock.DynamicMock<IMessageBrokerEnablerFactory>();
    		_businessRulesForPersonalAccountUpdate = _mock.DynamicMock<IBusinessRulesForPersonalAccountUpdate>();
            _scheduleTagRepository = _mock.DynamicMock<IScheduleTagRepository>();

            _person = PersonFactory.CreatePerson();
            _person.SetId(Guid.NewGuid());

            _activity = ActivityFactory.CreateActivity("Test Activity");
            _activity.SetId(Guid.NewGuid());

            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _period = _dateOnlyPeriod.ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone());
            _multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime);
            _target = new AddOvertimeCommandHandler(_dateTimePeriodMock,_multiplicatorDefinitionSetRepository,_activityRepository,_scheduleRepository,_personRepository,_scenarioRepository,_currentUnitOfWorkFactory,_saveSchedulePartService,_messageBrokerEnablerFactory, _businessRulesForPersonalAccountUpdate, _scheduleTagRepository);

            _addOvertimeCommandDto = new AddOvertimeCommandDto
            {
                ActivityId = _activity.Id.GetValueOrDefault(),
                Date = _dateOnyldto,
                Period = _periodDto,
                PersonId = _person.Id.GetValueOrDefault(),
                OvertimeDefinitionSetId = Guid.NewGuid(),
                ScheduleTag = new ScheduleTagDto() { Id = Guid.NewGuid(), Description = "test" }
            };
        }

        [Test]
        public void ShouldAddOvertimeInTheDictionarySuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
            var scheduleDay = _mock.StrictMock<IScheduleDay>();
            var dictionary = _mock.DynamicMock<IScheduleDictionary>();
        	var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
            var scheduleTag = new ScheduleTag() { Description = "test" };
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Load(_person.Id.GetValueOrDefault())).Return(_person);
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
                Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, _period, _scenario)).IgnoreArguments().Return(dictionary);
                Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
                Expect.Call(_activityRepository.Load(_activity.Id.GetValueOrDefault())).Return(_activity);
                Expect.Call(_multiplicatorDefinitionSetRepository.Load(_addOvertimeCommandDto.OvertimeDefinitionSetId)).Return(_multiplicatorDefinitionSet);
                Expect.Call(()=>scheduleDay.CreateAndAddOvertime(null)).IgnoreArguments();
                Expect.Call(scheduleRangeMock.ScheduledDay(_startDate)).Return(scheduleDay);
                Expect.Call(_dateTimePeriodMock.DtoToDomainEntity(_periodDto)).Return(_period);
            	Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
                Expect.Call(_scheduleTagRepository.Get(_addOvertimeCommandDto.ScheduleTag.Id.GetValueOrDefault())).Return(scheduleTag);
                Expect.Call(() => _saveSchedulePartService.Save(scheduleDay,rules, scheduleTag));
            }
            using (_mock.Playback())
            {
                _target.Handle(_addOvertimeCommandDto);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ForGiven"), Test]
		public void ShouldAddOvertimeInTheDictionaryForGivenScenarioSuccessfully()
		{
			var scenarioId = Guid.NewGuid();
			var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
			var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
			var scheduleDay = _mock.StrictMock<IScheduleDay>();
			var dictionary = _mock.DynamicMock<IScheduleDictionary>();
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
            var scheduleTag = new ScheduleTag() { Description = "test" };
			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
				Expect.Call(_personRepository.Load(_person.Id.GetValueOrDefault())).Return(_person);
				Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
				Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, _period, _scenario)).IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(_activityRepository.Load(_activity.Id.GetValueOrDefault())).Return(_activity);
				Expect.Call(_multiplicatorDefinitionSetRepository.Load(_addOvertimeCommandDto.OvertimeDefinitionSetId)).Return(_multiplicatorDefinitionSet);
				Expect.Call(() => scheduleDay.CreateAndAddOvertime(null)).IgnoreArguments();
				Expect.Call(scheduleRangeMock.ScheduledDay(_startDate)).Return(scheduleDay);
				Expect.Call(_dateTimePeriodMock.DtoToDomainEntity(_periodDto)).Return(_period);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
                Expect.Call(_scheduleTagRepository.Get(_addOvertimeCommandDto.ScheduleTag.Id.GetValueOrDefault())).Return(scheduleTag);
				Expect.Call(() => _saveSchedulePartService.Save(scheduleDay,rules, scheduleTag));
			}
			using (_mock.Playback())
			{
				_addOvertimeCommandDto.ScenarioId = scenarioId;
				_target.Handle(_addOvertimeCommandDto);
			}
		}
    }
}
