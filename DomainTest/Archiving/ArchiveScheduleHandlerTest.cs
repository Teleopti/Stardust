using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Archiving;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Archiving
{
	[DomainTest]
	[Toggle(Toggles.Wfm_ArchiveSchedule_41498)]
	public class ArchiveScheduleHandlerTest
	{
		public IScheduleStorage ScheduleStorage;
		public ArchiveScheduleHandler Target;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeAgentDayScheduleTagRepository AgentDayScheduleTagRepository;
		public FakeNoteRepository NoteRepository;
		public FakePublicNoteRepository PublicNoteRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;

		private Scenario _defaultScenario;
		private Scenario _targetScenario;
		private DateOnlyPeriod _period;
		private Person _person;
		private BusinessUnit _businessUnit;

		[SetUp]
		public void Setup()
		{
			_defaultScenario = new Scenario("default") { DefaultScenario = true }.WithId();
			_targetScenario = new Scenario("target").WithId();
			_period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 5);
			_businessUnit = new BusinessUnit("BU").WithId();
			_person = new Person().WithId();
		}

		private void addDefaultTypesToRepositories()
		{
			ScenarioRepository.Add(_defaultScenario);
			ScenarioRepository.Add(_targetScenario);
			BusinessUnitRepository.Add(_businessUnit);
			PersonRepository.Add(_person);
		}

		[Test]
		public void ShouldMoveOneAssignment()
		{
			addDefaultTypesToRepositories();

			var assignment = new PersonAssignment(_person, _defaultScenario, _period.StartDate);
			ScheduleStorage.Add(assignment);
			var @event = new ArchiveScheduleEvent
			{
				PersonId = _person.Id.GetValueOrDefault(),
				EndDate = _period.EndDate.Date,
				FromScenario = _defaultScenario.Id.GetValueOrDefault(),
				StartDate = _period.StartDate.Date,
				ToScenario = _targetScenario.Id.GetValueOrDefault(),
				TrackingId = Guid.NewGuid(),
				LogOnBusinessUnitId = _businessUnit.Id.GetValueOrDefault()
			};
			Target.Handle(@event);

			var result = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(true, true), _period, _targetScenario);
			result[_person].ScheduledDayCollection(_period).Should().Not.Be.Empty();
			var archivedAssignment = PersonAssignmentRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(_targetScenario));
			archivedAssignment.Should().Not.Be.Null();
		}

		[Test]
		[Ignore("Will be moved to integrationtests soon, and doesn't work right now!")]
		public void ShouldMoveOneAgentDayScheduleTag()
		{
			addDefaultTypesToRepositories();

			var scheduleTag = new ScheduleTag {Description = "Something"};
			scheduleTag.SetBusinessUnit(_businessUnit);
			var agentDayScheduleTag = new AgentDayScheduleTag(_person, new DateOnly(_period.StartDate.Date+TimeSpan.FromDays(1)), _defaultScenario, scheduleTag);
			ScheduleStorage.Add(agentDayScheduleTag);
			var @event = new ArchiveScheduleEvent
			{
				PersonId = _person.Id.GetValueOrDefault(),
				EndDate = _period.EndDate.Date,
				FromScenario = _defaultScenario.Id.GetValueOrDefault(),
				StartDate = _period.StartDate.Date,
				ToScenario = _targetScenario.Id.GetValueOrDefault(),
				TrackingId = Guid.NewGuid(),
				LogOnBusinessUnitId = _businessUnit.Id.GetValueOrDefault()
			};
			Target.Handle(@event);

			var result = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(true, true), _period, _targetScenario);
			result[_person].ScheduledDayCollection(_period).Should().Not.Be.Empty();
			var archivedAgentDayScheduleTag = AgentDayScheduleTagRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(_targetScenario));
			archivedAgentDayScheduleTag.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMoveOneNote()
		{
			addDefaultTypesToRepositories();

			var note = new Note(_person, _period.StartDate, _defaultScenario, "Test");
			ScheduleStorage.Add(note);
			var @event = new ArchiveScheduleEvent
			{
				PersonId = _person.Id.GetValueOrDefault(),
				EndDate = _period.EndDate.Date,
				FromScenario = _defaultScenario.Id.GetValueOrDefault(),
				StartDate = _period.StartDate.Date,
				ToScenario = _targetScenario.Id.GetValueOrDefault(),
				TrackingId = Guid.NewGuid(),
				LogOnBusinessUnitId = _businessUnit.Id.GetValueOrDefault()
			};
			Target.Handle(@event);

			var result = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(true, true), _period, _targetScenario);
			result[_person].ScheduledDayCollection(_period).Should().Not.Be.Empty();
			var archivedNote = NoteRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(_targetScenario));
			archivedNote.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMoveOnePublicNote()
		{
			addDefaultTypesToRepositories();

			var note = new PublicNote(_person, _period.StartDate, _defaultScenario, "Test");
			ScheduleStorage.Add(note);
			var @event = new ArchiveScheduleEvent
			{
				PersonId = _person.Id.GetValueOrDefault(),
				EndDate = _period.EndDate.Date,
				FromScenario = _defaultScenario.Id.GetValueOrDefault(),
				StartDate = _period.StartDate.Date,
				ToScenario = _targetScenario.Id.GetValueOrDefault(),
				TrackingId = Guid.NewGuid(),
				LogOnBusinessUnitId = _businessUnit.Id.GetValueOrDefault()
			};
			Target.Handle(@event);

			var result = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(true, true), _period, _targetScenario);
			result[_person].ScheduledDayCollection(_period).Should().Not.Be.Empty();
			var archivedNote = PublicNoteRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(_targetScenario));
			archivedNote.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMoveOneAbsence()
		{
			addDefaultTypesToRepositories();

			var personAbsence = new PersonAbsence(_person, _defaultScenario, new AbsenceLayer(new Absence(),
					new DateTimePeriod(_period.StartDate.Date.ToUniversalTime(), (_period.StartDate.Date + TimeSpan.FromDays(1)).ToUniversalTime())));
			ScheduleStorage.Add(personAbsence);
			var @event = new ArchiveScheduleEvent
			{
				PersonId = _person.Id.GetValueOrDefault(),
				EndDate = _period.EndDate.Date,
				FromScenario = _defaultScenario.Id.GetValueOrDefault(),
				StartDate = _period.StartDate.Date,
				ToScenario = _targetScenario.Id.GetValueOrDefault(),
				TrackingId = Guid.NewGuid(),
				LogOnBusinessUnitId = _businessUnit.Id.GetValueOrDefault()
			};
			Target.Handle(@event);

			var result = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(true, true), _period, _targetScenario);
			result[_person].ScheduledDayCollection(_period).Should().Not.Be.Empty();
			var archivedPersonAbsence = PersonAbsenceRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(_targetScenario));
			archivedPersonAbsence.Should().Not.Be.Null();
		}
	}
}