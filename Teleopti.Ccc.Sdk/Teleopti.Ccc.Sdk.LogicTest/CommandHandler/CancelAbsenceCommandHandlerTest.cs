using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class CancelAbsenceCommandHandlerTest
    {
        private FakeScheduleRepository _scheduleRepository;
        private IScenarioRepository _scenarioRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private CancelAbsenceCommandHandler _target;
        private IPerson _person;
        private IAbsence _absence;
        private IScenario _scenario;
        private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTimePeriod _period = new DateTimePeriod(_startDate, _startDate.AddDays(1));
        private readonly DateTimePeriodDto _periodDto = new DateTimePeriodDto
        {
            UtcStartTime = _startDate,
            UtcEndTime = _startDate.AddDays(1)
        };

	    [SetUp]
        public void Setup()
        {
			_person = PersonFactory.CreatePerson().WithId();
			_absence = AbsenceFactory.CreateAbsence("Sick").WithId();
			_scenario = ScenarioFactory.CreateScenarioWithId("Default",true);

			_scheduleRepository = new FakeScheduleRepository();
			_scenarioRepository = new FakeScenarioRepository(_scenario);
            _unitOfWorkFactory = new FakeUnitOfWorkFactory();
            
			var currentUnitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var personRepository = new FakePersonRepository(_person);
			var fakePersonAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
		    var businessRulesForPersonalAccountUpdate = new BusinessRulesForPersonalAccountUpdate(fakePersonAbsenceAccountRepository, new SchedulingResultStateHolder());
			var scheduleTagAssembler = new ScheduleTagAssembler(new FakeScheduleTagRepository());
			var scheduleSaveHandler = new ScheduleSaveHandler(new SaveSchedulePartService(new ScheduleDifferenceSaver(_scheduleRepository), fakePersonAbsenceAccountRepository));

			_target = new CancelAbsenceCommandHandler(dateTimePeriodAssembler, scheduleTagAssembler, _scheduleRepository, personRepository, _scenarioRepository, currentUnitOfWorkFactory, businessRulesForPersonalAccountUpdate, scheduleSaveHandler);
        }

	    [Test]
	    public void AbsenceCancelSuccessfully()
	    {
		    _scheduleRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person, _period).WithId());
		    _scheduleRepository.Add(new PersonAbsence(_person, _scenario, new AbsenceLayer(_absence, _period)).WithId());

		    _scheduleRepository.SetUnitOfWork(_unitOfWorkFactory.CurrentUnitOfWork());
		    _target.Handle(new CancelAbsenceCommandDto { Period = _periodDto, PersonId = _person.Id.GetValueOrDefault() });

		    _scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(_person,
			    new ScheduleDictionaryLoadOptions(false, false), _period,
			    _scenario)[_person].ScheduledDay(new DateOnly(_startDate)).PersonAbsenceCollection().Count.Should().Be.EqualTo(0);
	    }

	    [Test]
	    public void AbsenceCancelSuccessfullyForGivenType()
	    {
		    var absence2 = AbsenceFactory.CreateAbsence("Holiday").WithId();

			_scheduleRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person, _period).WithId());
			_scheduleRepository.Add(new PersonAbsence(_person, _scenario, new AbsenceLayer(_absence, _period)).WithId());
			_scheduleRepository.Add(new PersonAbsence(_person, _scenario, new AbsenceLayer(_absence, _period)).WithId());
			_scheduleRepository.Add(new PersonAbsence(_person, _scenario, new AbsenceLayer(absence2, _period)).WithId());

			_scheduleRepository.SetUnitOfWork(_unitOfWorkFactory.CurrentUnitOfWork());
			
			_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(_person,
				new ScheduleDictionaryLoadOptions(false, false), _period,
				_scenario)[_person].ScheduledDay(new DateOnly(_startDate)).PersonAbsenceCollection().Count.Should().Be.EqualTo(3);
		    _target.Handle(new CancelAbsenceCommandDto { Period = _periodDto, PersonId = _person.Id.GetValueOrDefault(),AbsenceId = _absence.Id});

			_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(_person,
				new ScheduleDictionaryLoadOptions(false, false), _period,
				_scenario)[_person].ScheduledDay(new DateOnly(_startDate)).PersonAbsenceCollection().Count.Should().Be.EqualTo(1);
	    }

	    [Test]
        public void AbsenceCancelSuccessfullyForGivenScenario()
        {
        	var otherScenario = ScenarioFactory.CreateScenarioWithId("other",false);
			_scenarioRepository.Add(otherScenario);
			
			_scheduleRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person, _period).WithId());
			_scheduleRepository.Add(new PersonAbsence(_person, _scenario, new AbsenceLayer(_absence, _period)).WithId());

			_scheduleRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(otherScenario, _person, _period).WithId());
			_scheduleRepository.Add(new PersonAbsence(_person, otherScenario, new AbsenceLayer(_absence, _period)).WithId());

			_scheduleRepository.SetUnitOfWork(_unitOfWorkFactory.CurrentUnitOfWork());
			
			_target.Handle(new CancelAbsenceCommandDto { Period = _periodDto, PersonId = _person.Id.GetValueOrDefault(),ScenarioId = otherScenario.Id.GetValueOrDefault()});

			_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(_person,
				new ScheduleDictionaryLoadOptions(false, false), _period,
				otherScenario)[_person].ScheduledDay(new DateOnly(_startDate)).PersonAbsenceCollection().Count.Should().Be.EqualTo(0);
			_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(_person,
					new ScheduleDictionaryLoadOptions(false, false), _period,
					_scenario)[_person].ScheduledDay(new DateOnly(_startDate)).PersonAbsenceCollection().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void AbsenceCancelSuccessfullyForTwoOverlappingAbsences()
		{
			_scheduleRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person, _period).WithId());
			_scheduleRepository.Add(new PersonAbsence(_person, _scenario, new AbsenceLayer(_absence, _period.ChangeEndTime(TimeSpan.FromDays(2)))).WithId());
			_scheduleRepository.Add(new PersonAbsence(_person, _scenario, new AbsenceLayer(_absence, _period.ChangeEndTime(TimeSpan.FromDays(4)))).WithId());

			_scheduleRepository.SetUnitOfWork(_unitOfWorkFactory.CurrentUnitOfWork());

			_periodDto.UtcEndTime = _period.EndDateTime.Add(TimeSpan.FromDays(2));
			_target.Handle(new CancelAbsenceCommandDto { Period = _periodDto, PersonId = _person.Id.GetValueOrDefault() });

			_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(_person,
					new ScheduleDictionaryLoadOptions(false, false), _period.ChangeEndTime(TimeSpan.FromDays(3)),
					_scenario)[_person].ScheduledDay(new DateOnly(_startDate).AddDays(3)).PersonAbsenceCollection(true).Count.Should().Be.EqualTo(1);
		}
    }
}
