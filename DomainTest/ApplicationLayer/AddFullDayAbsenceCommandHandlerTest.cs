using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class AddFullDayAbsenceCommandHandlerTest
	{
		private FakeWriteSideRepository<IPerson> _personRepository;
		private FakeWriteSideRepository<IAbsence> _absenceRepository;
		private FakeCurrentScenario _currentScenario;
		private FakeScheduleStorage _scheduleStorage;
		private PersonAbsenceCreator _personAccountCreator;
		private IAbsenceCommandConverter _absenceCommandConverter;

		[SetUp]
		public void Setup()
		{
			 var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			_personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			_absenceRepository = new FakeWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			_currentScenario = new FakeCurrentScenario();
			_scheduleStorage = new FakeScheduleStorage();

			var scheduleDifferenceSaver = new FakeScheduleDifferenceSaver(_scheduleStorage, new EmptyScheduleDayDifferenceSaver());
			var businessRulesForAccountUpdate = new BusinessRulesForPersonalAccountUpdate(personAbsenceAccountRepository, new SchedulingResultStateHolder());
			var saveSchedulePartService = new SaveSchedulePartService(scheduleDifferenceSaver, personAbsenceAccountRepository,
				new DoNothingScheduleDayChangeCallBack());
			_personAccountCreator = new PersonAbsenceCreator (saveSchedulePartService, businessRulesForAccountUpdate);

			_absenceCommandConverter = new AbsenceCommandConverter(_currentScenario, _personRepository, _absenceRepository, _scheduleStorage, null);
		}

		[Test]
		public void ShouldRaiseFullDayAbsenceAddedEvent()
		{
			var target = new AddFullDayAbsenceCommandHandler(_personAccountCreator, _absenceCommandConverter);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddFullDayAbsenceCommand
				{
					AbsenceId = _absenceRepository.Single().Id.Value,
					PersonId = _personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25),
					TrackedCommandInfo = new TrackedCommandInfo()
					{
						OperatedPersonId = operatedPersonId,
						TrackId = trackId
					}
				};
			target.Handle(command);
			var personAbsence = _scheduleStorage.LoadAll().Single() as PersonAbsence;
			var @event = personAbsence.PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.AbsenceId.Should().Be(personAbsence.Layer.Payload.Id.Value);
			@event.PersonId.Should().Be(_personRepository.Single().Id.Value);
			@event.ScenarioId.Should().Be(_currentScenario.Current().Id.Value);
			@event.StartDateTime.Should().Be(command.StartDate);
			@event.EndDateTime.Should().Be(command.EndDate.AddHours(24).AddMinutes(-1));
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(_currentScenario.Current().BusinessUnit.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldSetupEntityState()
		{
			var target = new AddFullDayAbsenceCommandHandler(_personAccountCreator, _absenceCommandConverter);

			var command = new AddFullDayAbsenceCommand
				{
					AbsenceId = _absenceRepository.Single().Id.Value,
					PersonId = _personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25)
				};
			target.Handle(command);

			var personAbsence = _scheduleStorage.LoadAll().Single() as PersonAbsence;
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(_personRepository.Single());
			absenceLayer.Payload.Should().Be(_absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(command.StartDate);
			absenceLayer.Period.EndDateTime.Should().Be(command.EndDate.AddHours(24).AddMinutes(-1));
		}

		[Test]
		public void ShouldConvertFromAgentsTimeZone()
		{
			var person = PersonFactory.CreatePersonWithId();
			var agentsTimeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			person.PermissionInformation.SetDefaultTimeZone(agentsTimeZone);
			var personRepository = new FakeWriteSideRepository<IPerson> { person };
			var absenceCommandConverter = new AbsenceCommandConverter(_currentScenario, personRepository, _absenceRepository, _scheduleStorage, null);
			var target = new AddFullDayAbsenceCommandHandler(_personAccountCreator, absenceCommandConverter);

			var command = new AddFullDayAbsenceCommand
				{
					AbsenceId = _absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25)
				};
			target.Handle(command);

			var personAbsence = _scheduleStorage.LoadAll().Single() as PersonAbsence;
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartDate, agentsTimeZone));
			absenceLayer.Period.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndDate.AddHours(24).AddMinutes(-1), agentsTimeZone));
			var @event = personAbsence.PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartDate, agentsTimeZone));
			@event.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndDate.AddHours(24).AddMinutes(-1), agentsTimeZone));
		}

		[Test]
		public void ShouldOverlapShift()
		{
			var person = _personRepository.Single();
			var personAssignmentPeriod = new DateTimePeriod(2013, 3, 25, 10, 2013, 3, 25, 15);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, _currentScenario.Current(), personAssignmentPeriod);
			_scheduleStorage.Add(personAssignment);
			var target = new AddFullDayAbsenceCommandHandler(_personAccountCreator, _absenceCommandConverter);

			target.Handle(new AddFullDayAbsenceCommand
				{
					AbsenceId = _absenceRepository.Single().Id.Value,
					PersonId = _personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25)
				});

			var personAbsence = _scheduleStorage.LoadAll().Single(scheduleItem => scheduleItem is PersonAbsence) as PersonAbsence;
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.StartDateTime.Should().Be(personAssignmentPeriod.StartDateTime);
			absenceLayer.Period.EndDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
			var @event = personAbsence.PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(personAssignmentPeriod.StartDateTime);
			@event.EndDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
		}

		[Test]
		public void ShouldNotOverlapNightShiftFromDayBeforeStartDate()
		{

			var person = _personRepository.Single();
			var personAssignmentPeriod = new DateTimePeriod(2013, 3, 24, 18, 2013, 3, 25, 5);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, _currentScenario.Current(), personAssignmentPeriod);

			_scheduleStorage.Add(personAssignment);
			var target = new AddFullDayAbsenceCommandHandler(_personAccountCreator, _absenceCommandConverter);

			target.Handle(new AddFullDayAbsenceCommand
				{
					AbsenceId = _absenceRepository.Single().Id.Value,
					PersonId = _personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25)
				});

			var personAbsence = _scheduleStorage.LoadAll().Single(scheduleItem => scheduleItem is PersonAbsence) as PersonAbsence;
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.StartDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
			var @event = personAbsence.PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
		}

		[Test]
		public void ShouldFullyOverlapNightShiftOnEndDate()
		{
			var person = _personRepository.Single();
			var personAssignmentPeriod = new DateTimePeriod(2013, 3, 25, 18, 2013, 3, 26, 5);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, _currentScenario.Current(), personAssignmentPeriod);
			_scheduleStorage.Add(personAssignment);
			var target = new AddFullDayAbsenceCommandHandler(_personAccountCreator, _absenceCommandConverter);

			target.Handle(new AddFullDayAbsenceCommand
				{
					AbsenceId = _absenceRepository.Single().Id.Value,
					PersonId = _personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25)
				});

			var personAbsence = _scheduleStorage.LoadAll().Single(scheduleItem => scheduleItem is PersonAbsence) as PersonAbsence;
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.EndDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
			var @event = personAbsence.PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.EndDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
		}
	}
}