using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	public class AddFullDayAbsenceCommandHandlerTest : IExtendSystem
	{
		public FakeWriteSideRepository<IPerson> PersonRepository;
		public FakeWriteSideRepository<IAbsence> AbsenceRepository;
		public FakeScenarioRepository CurrentScenario;
		public IScheduleStorage ScheduleStorage;
		public PersonAbsenceCreator PersonAccountCreator;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		
		public AddFullDayAbsenceCommandHandler Target;

		[Test]
		public void ShouldRaiseFullDayAbsenceAddedEvent()
		{
			var scenario = CurrentScenario.Has("Default");
			AbsenceRepository.Add(new Absence().WithId());
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddFullDayAbsenceCommand
				{
					AbsenceId = AbsenceRepository.Single().Id.Value,
					PersonId = PersonRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25),
					TrackedCommandInfo = new TrackedCommandInfo()
					{
						OperatedPersonId = operatedPersonId,
						TrackId = trackId
					}
				};
			Target.Handle(command);
			var personAbsence = PersonAbsenceRepository.LoadAll().Single() as PersonAbsence;
			var @event = personAbsence.PopAllEvents(null).Single() as FullDayAbsenceAddedEvent;
			@event.AbsenceId.Should().Be(personAbsence.Layer.Payload.Id.Value);
			@event.PersonId.Should().Be(PersonRepository.Single().Id.Value);
			@event.ScenarioId.Should().Be(scenario.Id.Value);
			@event.StartDateTime.Should().Be(command.StartDate);
			@event.EndDateTime.Should().Be(command.EndDate.AddHours(24).AddMinutes(-1));
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(scenario.BusinessUnit.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldSetupEntityState()
		{
			CurrentScenario.Has("Default");
			AbsenceRepository.Add(new Absence().WithId());
			var command = new AddFullDayAbsenceCommand
				{
					AbsenceId = AbsenceRepository.Single().Id.Value,
					PersonId = PersonRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25)
				};
			Target.Handle(command);

			var personAbsence = PersonAbsenceRepository.LoadAll().Single() as PersonAbsence;
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(PersonRepository.Single());
			absenceLayer.Payload.Should().Be(AbsenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(command.StartDate);
			absenceLayer.Period.EndDateTime.Should().Be(command.EndDate.AddHours(24).AddMinutes(-1));
		}

		[Test]
		public void ShouldConvertFromAgentsTimeZone()
		{
			CurrentScenario.Has("Default");
			AbsenceRepository.Add(new Absence().WithId());
			var person = PersonFactory.CreatePersonWithId();
			var agentsTimeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			person.PermissionInformation.SetDefaultTimeZone(agentsTimeZone);
			PersonRepository.Remove(PersonRepository.Single());
			PersonRepository.Add(person);
			
			var command = new AddFullDayAbsenceCommand
				{
					AbsenceId = AbsenceRepository.Single().Id.Value,
					PersonId = PersonRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25)
				};
			Target.Handle(command);

			var personAbsence = PersonAbsenceRepository.LoadAll().Single() as PersonAbsence;
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartDate, agentsTimeZone));
			absenceLayer.Period.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndDate.AddHours(24).AddMinutes(-1), agentsTimeZone));
			var @event = personAbsence.PopAllEvents(null).Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartDate, agentsTimeZone));
			@event.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndDate.AddHours(24).AddMinutes(-1), agentsTimeZone));
		}

		[Test]
		public void ShouldOverlapShift()
		{
			var scenario = CurrentScenario.Has("Default");
			AbsenceRepository.Add(new Absence().WithId());
			var person = PersonRepository.Single();
			var personAssignmentPeriod = new DateTimePeriod(2013, 3, 25, 10, 2013, 3, 25, 15);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, personAssignmentPeriod);
			ScheduleStorage.Add(personAssignment);
			
			Target.Handle(new AddFullDayAbsenceCommand
				{
					AbsenceId = AbsenceRepository.Single().Id.Value,
					PersonId = PersonRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25)
				});

			var personAbsence = PersonAbsenceRepository.LoadAll().Single(scheduleItem => scheduleItem is PersonAbsence) as PersonAbsence;
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.StartDateTime.Should().Be(personAssignmentPeriod.StartDateTime);
			absenceLayer.Period.EndDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
			var @event = personAbsence.PopAllEvents(null).Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(personAssignmentPeriod.StartDateTime);
			@event.EndDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
		}

		[Test]
		public void ShouldNotOverlapNightShiftFromDayBeforeStartDate()
		{
			var scenario = CurrentScenario.Has("Default");
			AbsenceRepository.Add(new Absence().WithId());
			var person = PersonRepository.Single();
			var personAssignmentPeriod = new DateTimePeriod(2013, 3, 24, 18, 2013, 3, 25, 5);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, personAssignmentPeriod);

			ScheduleStorage.Add(personAssignment);
			
			Target.Handle(new AddFullDayAbsenceCommand
				{
					AbsenceId = AbsenceRepository.Single().Id.Value,
					PersonId = PersonRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25)
				});

			var personAbsence = PersonAbsenceRepository.LoadAll().Single(scheduleItem => scheduleItem is PersonAbsence) as PersonAbsence;
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.StartDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
			var @event = personAbsence.PopAllEvents(null).Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
		}

		[Test]
		public void ShouldFullyOverlapNightShiftOnEndDate()
		{
			var scenario = CurrentScenario.Has("Default");
			AbsenceRepository.Add(new Absence().WithId());
			var person = PersonRepository.Single();
			var personAssignmentPeriod = new DateTimePeriod(2013, 3, 25, 18, 2013, 3, 26, 5);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, personAssignmentPeriod);
			ScheduleStorage.Add(personAssignment);
			
			Target.Handle(new AddFullDayAbsenceCommand
				{
					AbsenceId = AbsenceRepository.Single().Id.Value,
					PersonId = PersonRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25)
				});

			var personAbsence = PersonAbsenceRepository.LoadAll().Single(scheduleItem => scheduleItem is PersonAbsence) as PersonAbsence;
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.EndDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
			var @event = personAbsence.PopAllEvents(null).Single() as FullDayAbsenceAddedEvent;
			@event.EndDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
		}
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<FakeWriteSideRepository<IPerson>>();
			extend.AddService<FakeWriteSideRepository<IAbsence>>();
			extend.AddService<PersonAbsenceCreator>();
			extend.AddService<SaveSchedulePartService>();
			extend.AddService<AddFullDayAbsenceCommandHandler>();
			extend.AddService<AbsenceCommandConverter>();
		}
	}
}