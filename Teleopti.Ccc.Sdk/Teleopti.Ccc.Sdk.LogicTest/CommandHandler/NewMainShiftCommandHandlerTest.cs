﻿using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
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
    public class NewMainShiftCommandHandlerTest
    {
        private MockRepository _mock;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IShiftCategoryRepository _shiftCategoryRepository;
        private IActivityLayerAssembler<IMainShiftActivityLayer> _mainActivityLayerAssembler;
        private IScheduleRepository _scheduleRepository;
        private IScenarioRepository _scenarioRepository;
        private IPersonRepository _personRepository;
        private ISaveSchedulePartService _saveSchedulePartService;
        private IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
        private NewMainShiftCommandHandler _target;
        private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateOnlyDto _dateOnlydto = new DateOnlyDto(new DateOnly(_startDate));
        private IPerson _person;
        private IScenario _scenario;
        private IShiftCategory _shiftCategory;
        private NewMainShiftCommandDto _newMainShiftCommandDto;
        private readonly DateTimePeriod _period = new DateTimePeriod(_startDate, _startDate.AddDays(1));
        private SchedulePartFactoryForDomain _scheduleRange;
        private Collection<ActivityLayerDto> _activityLayerDtoCollection;
        private Collection<IMainShiftActivityLayer> _mainShiftActivityLayerCollection;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _shiftCategoryRepository = _mock.StrictMock<IShiftCategoryRepository>();
            _mainActivityLayerAssembler = _mock.StrictMock<IActivityLayerAssembler<IMainShiftActivityLayer>>();
            _scheduleRepository = _mock.StrictMock<IScheduleRepository>();
            _scenarioRepository = _mock.StrictMock<IScenarioRepository>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _saveSchedulePartService = _mock.StrictMock<ISaveSchedulePartService>();
            _messageBrokerEnablerFactory = _mock.DynamicMock<IMessageBrokerEnablerFactory>();
            _target = new NewMainShiftCommandHandler(_unitOfWorkFactory,_shiftCategoryRepository,_mainActivityLayerAssembler,_scheduleRepository,_scenarioRepository,_personRepository,_saveSchedulePartService,_messageBrokerEnablerFactory);

            _person = PersonFactory.CreatePerson("test");
            _person.SetId(Guid.NewGuid());
            
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _scenario.SetId(Guid.NewGuid());

            _shiftCategory = ShiftCategoryFactory.CreateShiftCategory("test Shift Category");
            _shiftCategory.SetId(Guid.NewGuid());

            _newMainShiftCommandDto = new NewMainShiftCommandDto
            {
                Date = _dateOnlydto,
                PersonId = _person.Id.GetValueOrDefault(),
                ShiftCategoryId = _shiftCategory.Id.GetValueOrDefault()
            };

            _scheduleRange = new SchedulePartFactoryForDomain(_person, _scenario, _period, SkillFactory.CreateSkill("Test Skill"));
            _activityLayerDtoCollection = new Collection<ActivityLayerDto>();
            _mainShiftActivityLayerCollection = new Collection<IMainShiftActivityLayer>();
        }

        [Test]
        public void NewMainShiftShouldBeAddedSuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var schedulePart = _scheduleRange.CreatePart();
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
            var dictionary = _mock.DynamicMock<IScheduleDictionary>();
            
            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personRepository.Load(_newMainShiftCommandDto.PersonId)).Return(_person);
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
                Expect.Call(_mainActivityLayerAssembler.DtosToDomainEntities(_activityLayerDtoCollection)).
                    IgnoreArguments().Return(_mainShiftActivityLayerCollection);
                Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, _period, _scenario)).
                    IgnoreArguments().Return(dictionary);
                Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
                Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(schedulePart);
                Expect.Call(_shiftCategoryRepository.Load(_newMainShiftCommandDto.ShiftCategoryId)).Return(_shiftCategory);
                Expect.Call(()=>_saveSchedulePartService.Save(schedulePart, null)).IgnoreArguments();
            }
            using(_mock.Playback())
            {
                _target.Handle(_newMainShiftCommandDto);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ForGiven"), Test]
		public void NewMainShiftShouldBeAddedForGivenScenarioSuccessfully()
		{
			var scenarioId = Guid.NewGuid();
			var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
			var schedulePart = _scheduleRange.CreatePart();
			var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
			var dictionary = _mock.DynamicMock<IScheduleDictionary>();

			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_personRepository.Load(_newMainShiftCommandDto.PersonId)).Return(_person);
				Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
				Expect.Call(_mainActivityLayerAssembler.DtosToDomainEntities(_activityLayerDtoCollection)).
					IgnoreArguments().Return(_mainShiftActivityLayerCollection);
				Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, _period, _scenario)).
					IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(schedulePart);
				Expect.Call(_shiftCategoryRepository.Load(_newMainShiftCommandDto.ShiftCategoryId)).Return(_shiftCategory);
				Expect.Call(() => _saveSchedulePartService.Save(schedulePart,null)).IgnoreArguments();
			}
			using (_mock.Playback())
			{
				_newMainShiftCommandDto.ScenarioId = scenarioId;
				_target.Handle(_newMainShiftCommandDto);
			}
		}
    }
}
