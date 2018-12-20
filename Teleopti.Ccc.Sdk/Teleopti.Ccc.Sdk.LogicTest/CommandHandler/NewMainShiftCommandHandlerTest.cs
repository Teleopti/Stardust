using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class NewMainShiftCommandHandlerTest
    {
        private MockRepository _mock;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IShiftCategoryRepository _shiftCategoryRepository;
        private IActivityLayerAssembler<MainShiftLayer> _mainActivityLayerAssembler;
        private IScheduleStorage _scheduleStorage;
        private IScenarioRepository _scenarioRepository;
        private IPersonRepository _personRepository;
        private NewMainShiftCommandHandler _target;
        private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private readonly DateOnlyDto _dateOnlydto = new DateOnlyDto { DateTime = _startDate.Date };
        private IPerson _person;
        private IScenario _scenario;
        private IShiftCategory _shiftCategory;
        private NewMainShiftCommandDto _newMainShiftCommandDto;
        private readonly DateTimePeriod _period = new DateTimePeriod(_startDate, _startDate.AddDays(1));
        private SchedulePartFactoryForDomain _scheduleRange;
        private Collection<ActivityLayerDto> _activityLayerDtoCollection;
        private Collection<MainShiftLayer> _mainShiftActivityLayerCollection;
    	private IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
	    private IScheduleTagAssembler _scheduleTagAssembler;
		 private IScheduleSaveHandler _scheduleSaveHandler;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = _mock.DynamicMock<ICurrentUnitOfWorkFactory>();
            _shiftCategoryRepository = _mock.StrictMock<IShiftCategoryRepository>();
            _mainActivityLayerAssembler = _mock.StrictMock<IActivityLayerAssembler<MainShiftLayer>>();
            _scheduleStorage = _mock.StrictMock<IScheduleStorage>();
            _scenarioRepository = _mock.StrictMock<IScenarioRepository>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
    		_businessRulesForPersonalAccountUpdate = _mock.DynamicMock<IBusinessRulesForPersonalAccountUpdate>();
    		_scheduleTagAssembler = _mock.DynamicMock<IScheduleTagAssembler>();
			_scheduleSaveHandler = _mock.DynamicMock<IScheduleSaveHandler>();

			_target = new NewMainShiftCommandHandler(_currentUnitOfWorkFactory, _shiftCategoryRepository, _mainActivityLayerAssembler, _scheduleTagAssembler, _scheduleStorage, _scenarioRepository, _personRepository, _businessRulesForPersonalAccountUpdate, _scheduleSaveHandler);

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
	        var activityLayerDto = new ActivityLayerDto();
	        var period = new DateTimePeriodDto();
	        var dtp = new DateTimePeriod(2013, 1, 1, 8, 2013, 1, 1, 9);
	        period.LocalStartDateTime = dtp.StartDateTimeLocal(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone);
	        period.LocalEndDateTime = dtp.EndDateTimeLocal(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone);
			activityLayerDto.Period = period;
			_activityLayerDtoCollection.Add(new ActivityLayerDto());
            _mainShiftActivityLayerCollection = new Collection<MainShiftLayer>();
			_mainShiftActivityLayerCollection.Add(new MainShiftLayer(new Activity("hej"), dtp));
        }

        [Test]
        public void NewMainShiftShouldBeAddedSuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var schedulePart = _scheduleRange.CreatePart();
            var scheduleRangeMock = _mock.DynamicMock<IScheduleRange>();
            var dictionary = _mock.DynamicMock<IScheduleDictionary>();
        	var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
	        var tag = _mock.DynamicMock<IScheduleTag>();
            
            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Load(_newMainShiftCommandDto.PersonId)).Return(_person);
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(_scenario);
                Expect.Call(_mainActivityLayerAssembler.DtosToDomainEntities(_activityLayerDtoCollection)).
                    IgnoreArguments().Return(_mainShiftActivityLayerCollection);
				Expect.Call(_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), _scenario)).
                    IgnoreArguments().Return(dictionary);
                Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
                Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(schedulePart);
                Expect.Call(_shiftCategoryRepository.Load(_newMainShiftCommandDto.ShiftCategoryId)).Return(_shiftCategory);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
	            Expect.Call(_scheduleTagAssembler.DtoToDomainEntity(null)).IgnoreArguments().Return(tag);
					 Expect.Call(() => _scheduleSaveHandler.ProcessSave(schedulePart, rules, tag));
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
			var rules = _mock.DynamicMock<INewBusinessRuleCollection>();
			var tag = _mock.DynamicMock<IScheduleTag>();
			var tagId = Guid.NewGuid();
            
			using (_mock.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
				Expect.Call(_personRepository.Load(_newMainShiftCommandDto.PersonId)).Return(_person);
				Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
				Expect.Call(_mainActivityLayerAssembler.DtosToDomainEntities(_activityLayerDtoCollection)).
					IgnoreArguments().Return(_mainShiftActivityLayerCollection);
				Expect.Call(_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), _scenario)).
					IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[_person]).Return(scheduleRangeMock);
				Expect.Call(scheduleRangeMock.ScheduledDay(new DateOnly(_startDate))).Return(schedulePart);
				Expect.Call(_shiftCategoryRepository.Load(_newMainShiftCommandDto.ShiftCategoryId)).Return(_shiftCategory);
				Expect.Call(_businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRangeMock)).Return(rules);
				Expect.Call(_scheduleTagAssembler.DtoToDomainEntity(null))
				      .Constraints(new PredicateConstraint<ScheduleTagDto>(t => t.Id == tagId)).Return(tag);
				Expect.Call(() => _scheduleSaveHandler.ProcessSave(schedulePart, rules, tag));
			}
			using (_mock.Playback())
			{
				_newMainShiftCommandDto.ScheduleTagId = tagId;
				_newMainShiftCommandDto.ScenarioId = scenarioId;
				_target.Handle(_newMainShiftCommandDto);
			}
		}
    }
}
