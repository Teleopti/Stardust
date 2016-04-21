using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
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
		private ICurrentScenario _scenario;

		[SetUp]
		public void Setup()
		{
			_scenario = new FakeCurrentScenario();

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
			_personAbsenceRemover = new PersonAbsenceRemover(_scenario, _scheduleStorage,
				_businessRulesForAccountUpdate, _saveSchedulePartService, personAbsenceCreator,
				loggedOnUser, new PersonRequestAuthorizationCheckerForTest());
		}

		[Test]
		public void ShouldRemovePersonAbsenceFromScheduleDay()
		{
			var startDate = new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithId();
			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var personAbsence = new PersonAbsence(person, _scenario.Current(), absenceLayer).WithId();

			_scheduleStorage.Add(personAbsence);

			var target = new RemovePersonAbsenceCommandHandler(_personAbsenceRemover);

			var command = new RemovePersonAbsenceCommand
			{
				ScheduleDate = startDate,
				Person = person,
				PersonAbsences = new[] { personAbsence }
			};

			target.Handle(command);

			Assert.That(_scheduleStorage.LoadAll().Any() == false);
		}

		[Test]
		public void ShouldCancelApprovedRequestWhenRelatedPersonAbsenceIsRemoved()
		{
			var startDate = new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithId();
			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);

			var absence = AbsenceFactory.CreateAbsence("Holiday").WithId();
			var absenceRequest = new AbsenceRequest(absence, dateTimePeriod);
			var personRequest = new PersonRequest(person, absenceRequest);
			var personAbsence = new PersonAbsence(person, _scenario.Current(), absenceLayer, absenceRequest).WithId();
			personRequest.Pending();
			personRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());

			_scheduleStorage.Add(personAbsence);

			var target = new RemovePersonAbsenceCommandHandler(_personAbsenceRemover);

			var command = new RemovePersonAbsenceCommand
			{
				ScheduleDate = startDate,
				Person = person,
				PersonAbsences = new[] { personAbsence }
			};

			target.Handle(command);

			Assert.That(_scheduleStorage.LoadAll().Any() == false);
			Assert.That(personRequest.IsCancelled);

		}

		[Test]
		public void ShouldNotCancelApprovedRequestWhenRelatedPersonAbsenceIsRemovedAndTheRequestHasSplitPartsRemaining()
		{
			var startDate = new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(startDate, endDate);
			var dateTimePeriod2 = new DateTimePeriod(new DateTime(2015, 10, 3, 13, 0, 0, DateTimeKind.Utc), new DateTime(2015, 10, 3, 17, 0, 0, DateTimeKind.Utc));

			var person = PersonFactory.CreatePersonWithId();

			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var absenceLayer2 = new AbsenceLayer(new Absence(), dateTimePeriod2);

			var absence = AbsenceFactory.CreateAbsence("Holiday").WithId();
			var absenceRequest = new AbsenceRequest(absence, dateTimePeriod);
			var personRequest = new PersonRequest(person, absenceRequest);

			personRequest.Pending();
			personRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());

			var personAbsence = new PersonAbsence(person, _scenario.Current(), absenceLayer, absenceRequest).WithId();
			var personAbsence2 = new PersonAbsence(person, _scenario.Current(), absenceLayer2, absenceRequest).WithId();
			absenceRequest.PersonAbsences.Add(personAbsence);
			absenceRequest.PersonAbsences.Add(personAbsence2);

			_scheduleStorage.Add(personAbsence);
			_scheduleStorage.Add(personAbsence2);

			var target = new RemovePersonAbsenceCommandHandler(_personAbsenceRemover);

			var command = new RemovePersonAbsenceCommand
			{
				ScheduleDate = startDate,
				Person = person,
				PersonAbsences = new[] { personAbsence }
			};

			target.Handle(command);

			Assert.AreEqual(1, _scheduleStorage.LoadAll().Count);
			Assert.IsFalse(personRequest.IsCancelled);
		}



		[Test]
		public void ShouldReturnErrorMessages()
		{
			var startDate = new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithId();
			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var personAbsence = new PersonAbsence(person, _scenario.Current(), absenceLayer).WithId();

			_scheduleStorage.Add(personAbsence);

			var personAbsenceRemover = MockRepository.GenerateMock<IPersonAbsenceRemover>();
			var errorMessages = new List<string>
			{
				string.Format("Error message {0}", Guid.NewGuid()),
				string.Format("Error message {0}", Guid.NewGuid())
			};
			personAbsenceRemover.Stub(x => x.RemovePersonAbsence(new DateOnly(dateTimePeriod.StartDateTime), person, new[] { personAbsence }))
				.IgnoreArguments().Return(errorMessages);

			var target = new RemovePersonAbsenceCommandHandler(personAbsenceRemover);

			var command = new RemovePersonAbsenceCommand
			{
				ScheduleDate = startDate,
				Person = person,
				PersonAbsences = new[] { personAbsence }
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
			var startDate = new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithId();
			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var personAbsence = new PersonAbsence(person, _scenario.Current(), absenceLayer).WithId();

			_scheduleStorage.Add(personAbsence);

			var target = new RemovePersonAbsenceCommandHandler(_personAbsenceRemover);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new RemovePersonAbsenceCommand
			{
				Person = person,
				PersonAbsences = new[] { personAbsence },
				ScheduleDate = startDate,
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
			@event.LogOnBusinessUnitId.Should().Be(_scenario.Current().BusinessUnit.Id.GetValueOrDefault());
		}
	}
}