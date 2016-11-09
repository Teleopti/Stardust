using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Archiving;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Archiving
{
	[Toggle(Toggles.Wfm_ArchiveSchedule_41498)]
	[DatabaseTest]
	public class ArchiveScheduleHandlerTest
	{
		public ArchiveScheduleHandler Target;

		public IScheduleStorage ScheduleStorage;
		public IPersonRepository PersonRepository;
		public IScenarioRepository ScenarioRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IBusinessUnitRepository BusinessUnitRepository;
		public IAgentDayScheduleTagRepository AgentDayScheduleTagRepository;
		public INoteRepository NoteRepository;
		public IPublicNoteRepository PublicNoteRepository;
		public IPersonAbsenceRepository PersonAbsenceRepository;
		public IScheduleTagRepository ScheduleTagRepository;
		public IAbsenceRepository AbsenceRepository;
		public WithUnitOfWork WithUnitOfWork;
		
		private Scenario _defaultScenario;
		private Scenario _targetScenario;
		private DateOnlyPeriod _period;
		private IPerson _person;
		private IBusinessUnit _businessUnit;

		[SetUp]
		public void Setup()
		{
			_businessUnit = BusinessUnitFactory.BusinessUnitUsedInTest;
			_defaultScenario = new Scenario("default") { DefaultScenario = true};
			_targetScenario = new Scenario("target");
			_period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 5);
			_person = PersonFactory.CreatePerson("Tester Testersson");
		}

		private void addDefaultTypesToRepositories()
		{
			WithUnitOfWork.Do(() =>
			{
				ScenarioRepository.Add(_defaultScenario);
				ScenarioRepository.Add(_targetScenario);
				PersonRepository.Add(_person);
			});
		}

		[Test]
		public void ShouldMoveOneAssignment()
		{
			addDefaultTypesToRepositories();

			var assignment = new PersonAssignment(_person, _defaultScenario, _period.StartDate);
			WithUnitOfWork.Do(() => ScheduleStorage.Add(assignment));
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
			WithUnitOfWork.Do(() => Target.Handle(@event));

			var result = WithUnitOfWork.Get(() => ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(true, true), _period, _targetScenario));
			result[_person].ScheduledDayCollection(_period).Should().Not.Be.Empty();
			var archivedAssignment = WithUnitOfWork.Get(() => PersonAssignmentRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(_targetScenario)));
			archivedAssignment.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMoveOneAgentDayScheduleTag()
		{
			addDefaultTypesToRepositories();

			var scheduleTag = new ScheduleTag { Description = "Something" };
			WithUnitOfWork.Do(() => ScheduleTagRepository.Add(scheduleTag));
			var agentDayScheduleTag = new AgentDayScheduleTag(_person, new DateOnly(_period.StartDate.Date + TimeSpan.FromDays(1)), _defaultScenario, scheduleTag);
			WithUnitOfWork.Do(() => ScheduleStorage.Add(agentDayScheduleTag));
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
			WithUnitOfWork.Do(() => Target.Handle(@event));

			var result = WithUnitOfWork.Get(() => ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(true, true), _period, _targetScenario));
			result[_person].ScheduledDayCollection(_period).Should().Not.Be.Empty();
			var archivedAgentDayScheduleTag = WithUnitOfWork.Get(() => AgentDayScheduleTagRepository.LoadAll().FirstOrDefault(x => x.Scenario.Id == _targetScenario.Id));
			archivedAgentDayScheduleTag.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMoveOneNote()
		{
			addDefaultTypesToRepositories();

			var note = new Note(_person, _period.StartDate, _defaultScenario, "Test");
			WithUnitOfWork.Do(() => ScheduleStorage.Add(note));
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
			WithUnitOfWork.Do(() => Target.Handle(@event));

			var result = WithUnitOfWork.Get(() => ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(true, true), _period, _targetScenario));
			result[_person].ScheduledDayCollection(_period).Should().Not.Be.Empty();
			var archivedNote = WithUnitOfWork.Get(() => NoteRepository.LoadAll().FirstOrDefault(x => x.Scenario.Id == _targetScenario.Id));
			archivedNote.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMoveOnePublicNote()
		{
			addDefaultTypesToRepositories();

			var note = new PublicNote(_person, _period.StartDate, _defaultScenario, "Test");
			WithUnitOfWork.Do(() => ScheduleStorage.Add(note));
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
			WithUnitOfWork.Do(() => Target.Handle(@event));

			var result = WithUnitOfWork.Get(() => ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(true, true), _period, _targetScenario));
			result[_person].ScheduledDayCollection(_period).Should().Not.Be.Empty();
			var archivedNote = WithUnitOfWork.Get(() => PublicNoteRepository.LoadAll().FirstOrDefault(x => x.Scenario.Id == _targetScenario.Id));
			archivedNote.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMoveOneAbsence()
		{
			addDefaultTypesToRepositories();

			var absence = AbsenceFactory.CreateAbsence("gone");
			WithUnitOfWork.Do(() => AbsenceRepository.Add(absence));
			var personAbsence = new PersonAbsence(_person, _defaultScenario, new AbsenceLayer(absence,
					new DateTimePeriod(_period.StartDate.Date.ToUniversalTime(), (_period.StartDate.Date + TimeSpan.FromDays(1)).ToUniversalTime())));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(personAbsence));
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
			WithUnitOfWork.Do(() => Target.Handle(@event));

			var result = WithUnitOfWork.Get(() => ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(true, true), _period, _targetScenario));
			result[_person].ScheduledDayCollection(_period).Should().Not.Be.Empty();
			var archivedPersonAbsence = WithUnitOfWork.Get(() => PersonAbsenceRepository.LoadAll().FirstOrDefault(x => x.Scenario.Id == _targetScenario.Id));
			archivedPersonAbsence.Should().Not.Be.Null();
		}
	}
}