using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	public class MyTeamRemovePersonAbsenceCommandHandlerTest : IExtendSystem
	{
		public IScheduleStorage ScheduleStorage;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeScenarioRepository ScenarioRepository;
		public MyTeamRemovePersonAbsenceCommandHandler Target;

		[Test]
		public void ShouldRemovePersonAbsenceFromScheduleDay()
		{
			var scenario = ScenarioRepository.Has("Default");
			var dateTimePeriod = new DateTimePeriod(
				new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc));

			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var personAbsence = new PersonAbsence(PersonFactory.CreatePersonWithId(), scenario,
				absenceLayer).WithId();
			PersonAbsenceRepository.Add(personAbsence);
			
			var command = new MyTeamRemovePersonAbsenceCommand
			{
				PersonAbsenceId = personAbsence.Id.Value
			};

			Target.Handle(command);

			Assert.That(PersonAbsenceRepository.LoadAll().Any() == false);
		}

		[Test]
		public void ShouldRaisePersonAbsenceRemovedEvent()
		{
			var scenario = ScenarioRepository.Has("Default");
			var dateTimePeriod = new DateTimePeriod(
				new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc));

			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var personAbsence = new PersonAbsence(PersonFactory.CreatePersonWithId(), scenario, absenceLayer).WithId();

			PersonAbsenceRepository.Add(personAbsence);
			
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new MyTeamRemovePersonAbsenceCommand
			{
				PersonAbsenceId = personAbsence.Id.Value,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			Target.Handle(command);
			var @event = personAbsence.PopAllEvents(null).Single() as PersonAbsenceRemovedEvent;
			@event.PersonId.Should().Be(personAbsence.Person.Id.Value);
			@event.ScenarioId.Should().Be(personAbsence.Scenario.Id.Value);
			@event.StartDateTime.Should().Be(personAbsence.Layer.Period.StartDateTime);
			@event.EndDateTime.Should().Be(personAbsence.Layer.Period.EndDateTime);
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(scenario.BusinessUnit.Id.GetValueOrDefault());
		}

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<MyTeamRemovePersonAbsenceCommandHandler>();
		}
	}
}