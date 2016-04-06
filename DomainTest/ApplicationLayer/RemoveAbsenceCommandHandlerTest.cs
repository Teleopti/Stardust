using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class RemovePersonAbsenceCommandHandlerTest
	{
		private SaveSchedulePartService _saveSchedulePartService;
		private FakeScheduleStorage _scheduleStorage;
		private BusinessRulesForPersonalAccountUpdate _businessRulesForAccountUpdate;
		private PersonAbsenceRemover _personAbsenceRemover;

		[SetUp]
		public void Setup()
		{
			var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			_businessRulesForAccountUpdate = new BusinessRulesForPersonalAccountUpdate(personAbsenceAccountRepository,
				new SchedulingResultStateHolder());
			_scheduleStorage = new FakeScheduleStorage();
			var scheduleDifferenceSaver = new ScheduleDifferenceSaver(_scheduleStorage);
			_saveSchedulePartService = new SaveSchedulePartService(scheduleDifferenceSaver, personAbsenceAccountRepository);

			var personAbsenceCreator = new PersonAbsenceCreator(_saveSchedulePartService, _businessRulesForAccountUpdate);

			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			_personAbsenceRemover = new PersonAbsenceRemover(_scheduleStorage, _businessRulesForAccountUpdate,
				_saveSchedulePartService, personAbsenceCreator, loggedOnUser);
		}

		[Test]
		public void ShouldRemovePersonAbsenceFromScheduleDay()
		{
			var dateTimePeriod = new DateTimePeriod(
				new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc));

			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var personAbsence = new PersonAbsence(PersonFactory.CreatePersonWithId(), new FakeCurrentScenario().Current(),
				absenceLayer);
			personAbsence.SetId(Guid.Empty);
			_scheduleStorage.Add(personAbsence);

			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence> {personAbsence};

			var target = new RemovePersonAbsenceCommandHandler(personAbsenceRepository, _personAbsenceRemover );

			var command = new RemovePersonAbsenceCommand
			{
				PersonAbsenceId = personAbsence.Id.Value
			};

			target.Handle(command);

			Assert.That(_scheduleStorage.LoadAll().Any() == false);
		}

		[Test]
		public void ShouldReturnErrorMessages()
		{
			var dateTimePeriod = new DateTimePeriod(
				new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc));

			var person = PersonFactory.CreatePersonWithId();
			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var personAbsence = new PersonAbsence(person, new FakeCurrentScenario().Current(),
				absenceLayer);
			personAbsence.SetId(Guid.Empty);
			_scheduleStorage.Add(personAbsence);

			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence> { personAbsence };

			var personAbsenceRemover = MockRepository.GenerateMock<IPersonAbsenceRemover>();
			var errorMessages = new List<string>
			{
				string.Format("Error message {0}", Guid.NewGuid()),
				string.Format("Error message {0}", Guid.NewGuid())
			};
			personAbsenceRemover.Stub(x => x.RemovePersonAbsence(personAbsence)).IgnoreArguments()
				.Return(errorMessages);

			var target = new RemovePersonAbsenceCommandHandler(personAbsenceRepository, personAbsenceRemover);

			var command = new RemovePersonAbsenceCommand
			{
				PersonAbsenceId = personAbsence.Id.Value
			};

			target.Handle(command);

			var error = command.Errors;
			Assert.That(error != null);
			Assert.That(error.PersonId == person.Id.Value);
			Assert.That(error.PersonName == person.Name);
			Assert.That(error.ErrorMessages.Count() == errorMessages.Count);
			foreach (var message in errorMessages)
			{
				Assert.That(error.ErrorMessages.Contains(message));
			}
		}

		[Test]
		public void ShouldRaisePersonAbsenceRemovedEvent()
		{
			var scenario = new FakeCurrentScenario().Current();
			var personAbsence = new PersonAbsence(PersonFactory.CreatePersonWithId(), scenario, MockRepository.GenerateMock<IAbsenceLayer>());
			personAbsence.SetId(Guid.Empty);

			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence> { personAbsence };

			var target = new RemovePersonAbsenceCommandHandler(personAbsenceRepository, _personAbsenceRemover);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new RemovePersonAbsenceCommand
			{
				PersonAbsenceId = personAbsence.Id.Value,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			target.Handle(command);
			var @event = personAbsence.PopAllEvents(new Now()).Single() as PersonAbsenceRemovedEvent;
			@event.PersonId.Should().Be(personAbsence.Person.Id.Value);
			@event.ScenarioId.Should().Be(personAbsence.Scenario.Id.Value);
			@event.StartDateTime.Should().Be(personAbsence.Layer.Period.StartDateTime);
			@event.EndDateTime.Should().Be(personAbsence.Layer.Period.EndDateTime);
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(scenario.BusinessUnit.Id.GetValueOrDefault());
		}
	}
}