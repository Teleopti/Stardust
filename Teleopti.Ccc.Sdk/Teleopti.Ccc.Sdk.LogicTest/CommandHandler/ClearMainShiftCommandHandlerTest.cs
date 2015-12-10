using System;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class ClearMainShiftCommandHandlerTest
    {
        private IScheduleRepository _scheduleRepository;
        private IPersonRepository _personRepository;
        private IScenarioRepository _scenarioRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ClearMainShiftCommandHandler _target ;
        private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private readonly DateOnlyDto _dateOnlydto = new DateOnlyDto { DateTime = _startDate.Date };
        private IPerson _person;
        private IScenario _scenario;
        private ClearMainShiftCommandDto _clearMainShiftDto;
        private readonly DateTimePeriod _period = new DateTimePeriod(_startDate, _startDate.AddDays(1));
        private SchedulePartFactoryForDomain _scheduleRange;
    	private IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
	    private IScheduleTagAssembler _scheduleTagAssembler;
		 private IScheduleSaveHandler _scheduleSaveHandler;

	    [SetUp]
        public void Setup()
        {
            _scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			_unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_businessRulesForPersonalAccountUpdate = MockRepository.GenerateMock<IBusinessRulesForPersonalAccountUpdate>();
			_currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			_scheduleTagAssembler = MockRepository.GenerateMock<IScheduleTagAssembler>();
			_scheduleSaveHandler = MockRepository.GenerateMock<IScheduleSaveHandler>();

			_target = new ClearMainShiftCommandHandler(_scheduleTagAssembler, _scheduleRepository, _personRepository, _scenarioRepository, _currentUnitOfWorkFactory, _businessRulesForPersonalAccountUpdate, _scheduleSaveHandler);

            _person = PersonFactory.CreatePerson("test");
            _person.SetId(Guid.NewGuid());

            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _scenario.SetId(Guid.NewGuid());

            _clearMainShiftDto = new ClearMainShiftCommandDto { Date = _dateOnlydto, PersonId = _person.Id.GetValueOrDefault() };
            _scheduleRange = new SchedulePartFactoryForDomain(_person, _scenario, _period, SkillFactory.CreateSkill("Test Skill"));
        }

	    [Test]
	    public void ClearMainShiftFromTheDictionarySuccessfully()
	    {
		    var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
		    var schedulePart = _scheduleRange.CreatePart();
		    var scheduleRangeMock = MockRepository.GenerateMock<IScheduleRange>();
		    var dictionary = MockRepository.GenerateMock<IScheduleDictionary>();
		    var rules = MockRepository.GenerateMock<INewBusinessRuleCollection>();

		    _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
		    _currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
		    _personRepository.Stub(x => x.Load(_clearMainShiftDto.PersonId)).Return(_person);
			_scheduleRepository.Stub(x => x.FindSchedulesForPersonOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), _scenario))
		                       .IgnoreArguments()
		                       .Return(dictionary);
		    dictionary.Stub(x => x[_person]).Return(scheduleRangeMock);
		    scheduleRangeMock.Stub(x => x.ScheduledDay(new DateOnly(_startDate))).Return(schedulePart);
		    _businessRulesForPersonalAccountUpdate.Stub(x => x.FromScheduleRange(scheduleRangeMock)).Return(rules);

		    _target.Handle(_clearMainShiftDto);

			 _scheduleSaveHandler.AssertWasCalled(x => x.ProcessSave(schedulePart, rules, null));
	    }

	    [Test]
	    public void ClearMainShiftFromTheDictionaryForGivenScenarioSuccessfully()
	    {
		    var scenarioId = Guid.NewGuid();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
		    var schedulePart = _scheduleRange.CreatePart();
		    var scheduleRangeMock = MockRepository.GenerateMock<IScheduleRange>();
			var dictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var rules = MockRepository.GenerateMock<INewBusinessRuleCollection>();

		    _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
		    _currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
		    _personRepository.Stub(x => x.Load(_clearMainShiftDto.PersonId)).Return(_person);
			_scheduleRepository.Stub(x => x.FindSchedulesForPersonOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), _scenario)).IgnoreArguments().Return(dictionary);
		    dictionary.Stub(x => x[_person]).Return(scheduleRangeMock);
		    scheduleRangeMock.Stub(x => x.ScheduledDay(new DateOnly(_startDate))).Return(schedulePart);
		    _businessRulesForPersonalAccountUpdate.Stub(x => x.FromScheduleRange(scheduleRangeMock)).Return(rules);
		    
		    _clearMainShiftDto.ScenarioId = scenarioId;
		    _target.Handle(_clearMainShiftDto);

		    _scenarioRepository.AssertWasCalled(x => x.Get(scenarioId));
			 _scheduleSaveHandler.AssertWasCalled(x => x.ProcessSave(schedulePart, rules, null));
	    }

		[Test]
		public void ClearMainShiftWithScheduleTag()
		{
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var schedulePart = _scheduleRange.CreatePart();
			var scheduleRangeMock = MockRepository.GenerateMock<IScheduleRange>();
			var dictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var rules = MockRepository.GenerateMock<INewBusinessRuleCollection>();
			var tag = MockRepository.GenerateMock<IScheduleTag>();
			var tagId = Guid.NewGuid();

			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			_currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
			_personRepository.Stub(x => x.Load(_clearMainShiftDto.PersonId)).Return(_person);
			_scheduleRepository.Stub(x => x.FindSchedulesForPersonOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), _scenario)).IgnoreArguments().Return(dictionary);
			dictionary.Stub(x => x[_person]).Return(scheduleRangeMock);
			scheduleRangeMock.Stub(x => x.ScheduledDay(new DateOnly(_startDate))).Return(schedulePart);
			_businessRulesForPersonalAccountUpdate.Stub(x => x.FromScheduleRange(scheduleRangeMock)).Return(rules);
			_scheduleTagAssembler.Stub(x => x.DtoToDomainEntity(null))
			                     .Constraints(new PredicateConstraint<ScheduleTagDto>(t => t.Id == tagId))
			                     .Return(tag);

			_clearMainShiftDto.ScheduleTagId = tagId;
			_target.Handle(_clearMainShiftDto);

			_scheduleSaveHandler.AssertWasCalled(x => x.ProcessSave(schedulePart, rules, tag));
		}
    }
}
