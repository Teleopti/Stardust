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
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class RemovePartPersonAbsenceCommandHandlerTest
	{
		private SaveSchedulePartService _saveSchedulePartService;
		private FakeScheduleStorage _scheduleStorage;
		private BusinessRulesForPersonalAccountUpdate _businessRulesForAccountUpdate;
		private PersonAbsenceCreator _personAbsenceCreator;
		private FakeCurrentScenario _scenario;

		[SetUp]
		public void Setup()
		{

			_scenario = new FakeCurrentScenario();

			var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			_businessRulesForAccountUpdate = new BusinessRulesForPersonalAccountUpdate(personAbsenceAccountRepository,
				new SchedulingResultStateHolder());
			_scheduleStorage = new FakeScheduleStorage();
			var scheduleDifferenceSaver = new ScheduleDifferenceSaver(_scheduleStorage, CurrentUnitOfWork.Make());
			_saveSchedulePartService = new SaveSchedulePartService(scheduleDifferenceSaver, personAbsenceAccountRepository,
				new DoNothingScheduleDayChangeCallBack(), new EmptyScheduleDayDifferenceSaver());
			_personAbsenceCreator = new PersonAbsenceCreator(_saveSchedulePartService, _businessRulesForAccountUpdate);
		}

		[Test]
		public void NothingShouldBeDoneIfPeriodToRemoveIsTotallyEalierThanAbsencePeriod()
		{
			var periodForAbsence = new DateTimePeriod(
				new DateTime(2016, 03, 2, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 17, 0, 0, DateTimeKind.Utc));

			var periodToRemove = new DateTimePeriod(
				new DateTime(2016, 03, 1, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 1, 22, 0, 0, DateTimeKind.Utc));

			var allPersonAbsences = removePartPersonAbsence(periodForAbsence, periodToRemove).ToList();
			Assert.That(allPersonAbsences.Count == 1);
			Assert.That(
				allPersonAbsences.Any(
					pa => pa.Period.StartDateTime == periodForAbsence.StartDateTime
						  && pa.Period.EndDateTime == periodForAbsence.EndDateTime));
		}

		[Test]
		public void NothingShouldBeDoneIfPeriodToRemoveIsTotallyLaterThanAbsencePeriod()
		{
			var periodForAbsence = new DateTimePeriod(
				new DateTime(2016, 03, 2, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 17, 0, 0, DateTimeKind.Utc));

			var periodToRemove = new DateTimePeriod(
				new DateTime(2016, 03, 4, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 4, 22, 0, 0, DateTimeKind.Utc));

			var allPersonAbsences = removePartPersonAbsence(periodForAbsence, periodToRemove).ToList();
			Assert.That(allPersonAbsences.Count == 1);
			Assert.That(
				allPersonAbsences.Any(
					pa => pa.Period.StartDateTime == periodForAbsence.StartDateTime
						  && pa.Period.EndDateTime == periodForAbsence.EndDateTime));
		}

		[Test]
		public void ShouldRemoveEntirePersonAbsenceWhenPeriodToRemoveCoverdAbsencePeriod()
		{
			var periodForAbsence = new DateTimePeriod(
				new DateTime(2016, 03, 2, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 17, 0, 0, DateTimeKind.Utc));

			var periodToRemove = new DateTimePeriod(
				new DateTime(2016, 03, 1, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 4, 8, 0, 0, DateTimeKind.Utc));

			var allPersonAbsences = removePartPersonAbsence(periodForAbsence, periodToRemove).ToList();
			Assert.That(!allPersonAbsences.Any());
		}

		[Test]
		public void ShouldRemovePartPersonAbsenceWhenAbsencePeriodCoveredPeriodToRemove()
		{
			var periodForAbsence = new DateTimePeriod(
				new DateTime(2016, 03, 1, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 4, 17, 0, 0, DateTimeKind.Utc));

			var periodToRemove = new DateTimePeriod(
				new DateTime(2016, 03, 2, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 8, 0, 0, DateTimeKind.Utc));

			var allPersonAbsences = removePartPersonAbsence(periodForAbsence, periodToRemove).ToList();
			Assert.That(allPersonAbsences.Count == 2);
			Assert.That(
				allPersonAbsences.Any(
					pa => pa.Period.StartDateTime == periodForAbsence.StartDateTime
						  && pa.Period.EndDateTime == periodToRemove.StartDateTime));
			Assert.That(
				allPersonAbsences.Any(
					pa => pa.Period.StartDateTime == periodToRemove.EndDateTime
						  && pa.Period.EndDateTime == periodForAbsence.EndDateTime));
		}

		[Test]
		public void ShouldCreateCorrectNewPersonAbsenceWhenPeriodToRemoveIncludeAbsencePeriodEnd()
		{
			var periodForAbsence = new DateTimePeriod(
				new DateTime(2016, 03, 1, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 17, 0, 0, DateTimeKind.Utc));

			var periodToRemove = new DateTimePeriod(
				new DateTime(2016, 03, 3, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 22, 0, 0, DateTimeKind.Utc));

			var allPersonAbsences = removePartPersonAbsence(periodForAbsence, periodToRemove).ToList();
			Assert.That(allPersonAbsences.Count == 1);
			Assert.That(
				allPersonAbsences.Any(
					pa => pa.Period.StartDateTime == periodForAbsence.StartDateTime
						  && pa.Period.EndDateTime == periodToRemove.StartDateTime));
		}

		[Test]
		public void ShouldCreateCorrectNewPersonAbsenceWhenPeriodToRemoveIncludeAbsencePeriodStart()
		{
			var periodForAbsence = new DateTimePeriod(
				new DateTime(2016, 03, 2, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 17, 0, 0, DateTimeKind.Utc));

			var periodToRemove = new DateTimePeriod(
				new DateTime(2016, 03, 1, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 0, 0, 0, DateTimeKind.Utc));

			var allPersonAbsences = removePartPersonAbsence(periodForAbsence, periodToRemove).ToList();
			Assert.That(allPersonAbsences.Count == 1);
			Assert.That(
				allPersonAbsences.Any(
					pa => pa.Period.StartDateTime == periodToRemove.EndDateTime
						  && pa.Period.EndDateTime == periodForAbsence.EndDateTime));
		}

		[Test]
		public void ShouldRaisePersonAbsenceRemovedEvent()
		{
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			person.SetId(Guid.NewGuid());
			
			var startDate = new DateTime(2016, 01, 01, 00, 00, 00, DateTimeKind.Utc);
			var endDate = new DateTime(2016, 01, 01, 01, 00, 00, DateTimeKind.Utc);
			var period = new DateTimePeriod(startDate, endDate);
			var layer = new AbsenceLayer(new Absence(), period);
			var personAbsence = new PersonAbsence(person, _scenario.Current(), layer);
			personAbsence.SetId(Guid.NewGuid());

			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var personAbsenceRemover = new PersonAbsenceRemover(_businessRulesForAccountUpdate, _saveSchedulePartService, _personAbsenceCreator, loggedOnUser,
				new CheckingPersonalAccountDaysProvider(new FakePersonAbsenceAccountRepository()));

			var target = new RemovePartPersonAbsenceCommandHandler(personAbsenceRemover, _scheduleStorage, _scenario);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new RemovePartPersonAbsenceCommand
			{
				ScheduleDate = startDate,
				Person = person,
				PersonAbsence = personAbsence,
				PeriodToRemove = period.ChangeEndTime(new TimeSpan(0, 0, 1)),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			target.Handle(command);
			var @event = personAbsence.PopAllEvents().Single() as PersonAbsenceRemovedEvent;
			@event.PersonId.Should().Be(personAbsence.Person.Id.Value);
			@event.ScenarioId.Should().Be(personAbsence.Scenario.Id.Value);
			@event.StartDateTime.Should().Be(personAbsence.Layer.Period.StartDateTime);
			@event.EndDateTime.Should().Be(personAbsence.Layer.Period.EndDateTime);
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(_scenario.Current().BusinessUnit.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldReturnErrorMessages()
		{
			var startDate = new DateTime(2016, 01, 01, 00, 00, 00, DateTimeKind.Utc);
			var endDate = new DateTime(2016, 01, 01, 01, 00, 00, DateTimeKind.Utc);
			var period = new DateTimePeriod(startDate, endDate);
			var layer = new AbsenceLayer(new Absence(), period);
			var person = PersonFactory.CreatePersonWithId();
			var personAbsence = new PersonAbsence(person, _scenario.Current(), layer);
			personAbsence.SetId(Guid.NewGuid());

			var periodToRemove = period.ChangeEndTime(new TimeSpan(0, 0, 1));

			var personAbsenceRemover = MockRepository.GenerateMock<IPersonAbsenceRemover>();
			var errorMessages = new List<string>
			{
				string.Format("Error message {0}", Guid.NewGuid()),
				string.Format("Error message {0}", Guid.NewGuid())
			};
			personAbsenceRemover.Stub(x => x.RemovePartPersonAbsence(new DateOnly(startDate), person, personAbsence, periodToRemove, null))
				.IgnoreArguments().Return(errorMessages);

			var target = new RemovePartPersonAbsenceCommandHandler(personAbsenceRemover, _scheduleStorage, _scenario);

			var command = new RemovePartPersonAbsenceCommand
			{
				ScheduleDate = startDate,
				Person = person,
				PersonAbsence = personAbsence,
				PeriodToRemove = periodToRemove
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
		
		private IEnumerable<IPersistableScheduleData> removePartPersonAbsence(DateTimePeriod periodForAbsence,
			DateTimePeriod periodToRemove)
		{
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();

			var absenceLayer = new AbsenceLayer(new Absence(), periodForAbsence);
			var personAbsence = new PersonAbsence(person, _scenario.Current(), absenceLayer);
			personAbsence.SetId(Guid.NewGuid());
			_scheduleStorage.Add(personAbsence);

			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var personAbsenceRemover = new PersonAbsenceRemover(_businessRulesForAccountUpdate, _saveSchedulePartService, _personAbsenceCreator, loggedOnUser
				, new CheckingPersonalAccountDaysProvider(new FakePersonAbsenceAccountRepository()));
			var target = new RemovePartPersonAbsenceCommandHandler(personAbsenceRemover, _scheduleStorage, _scenario);

			var command = new RemovePartPersonAbsenceCommand
			{
				ScheduleDate = periodToRemove.StartDateTime.Date,
				Person = person,
				PersonAbsence = personAbsence,
				PeriodToRemove = periodToRemove
			};

			target.Handle(command);

			var allPersonAbsences = _scheduleStorage.LoadAll();
			return allPersonAbsences;
		}
	}
}