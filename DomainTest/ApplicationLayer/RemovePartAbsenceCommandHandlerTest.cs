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
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class RemovePartPersonAbsenceCommandHandlerTest
	{
		private SaveSchedulePartService _saveSchedulePartService;
		private FakeScheduleStorage _scheduleStorage;
		private BusinessRulesForPersonalAccountUpdate _businessRulesForAccountUpdate;
		private PersonAbsenceCreator _personAbsenceCreator;

		[SetUp]
		public void Setup()
		{
			var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			_businessRulesForAccountUpdate = new BusinessRulesForPersonalAccountUpdate(personAbsenceAccountRepository,
				new SchedulingResultStateHolder());
			_scheduleStorage = new FakeScheduleStorage();
			var scheduleDifferenceSaver = new ScheduleDifferenceSaver(_scheduleStorage);
			_saveSchedulePartService = new SaveSchedulePartService(scheduleDifferenceSaver, personAbsenceAccountRepository);
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
			var scenario = new FakeCurrentScenario().Current();
			var layer = MockRepository.GenerateMock<IAbsenceLayer>();
			var personAbsence = new PersonAbsence(PersonFactory.CreatePersonWithId(), scenario, layer);
			personAbsence.SetId(Guid.Empty);

			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence> {personAbsence};

			var target = new RemovePartPersonAbsenceCommandHandler(personAbsenceRepository, _scheduleStorage,
				_businessRulesForAccountUpdate, _saveSchedulePartService, _personAbsenceCreator);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new RemovePartPersonAbsenceCommand
			{
				PersonAbsenceId = personAbsence.Id.Value,
				PeriodToRemove = personAbsence.Period,
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
			@event.TrackId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(scenario.BusinessUnit.Id.GetValueOrDefault());
		}

		private IEnumerable<IPersistableScheduleData> removePartPersonAbsence(DateTimePeriod periodForAbsence,
			DateTimePeriod periodToRemove)
		{
			var absenceLayer = new AbsenceLayer(new Absence(), periodForAbsence);
			var personAbsence = new PersonAbsence(PersonFactory.CreatePersonWithId(), new FakeCurrentScenario().Current(),
				absenceLayer);
			personAbsence.SetId(Guid.Empty);
			_scheduleStorage.Add(personAbsence);

			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence> {personAbsence};

			var target = new RemovePartPersonAbsenceCommandHandler(personAbsenceRepository, _scheduleStorage,
				_businessRulesForAccountUpdate, _saveSchedulePartService, _personAbsenceCreator);

			var command = new RemovePartPersonAbsenceCommand
			{
				PersonAbsenceId = personAbsence.Id.Value,
				PeriodToRemove = periodToRemove
			};

			target.Handle(command);

			var allPersonAbsences = _scheduleStorage.LoadAll();
			return allPersonAbsences;
		}
	}
}