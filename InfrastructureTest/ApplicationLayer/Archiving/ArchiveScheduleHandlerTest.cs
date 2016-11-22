using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Archiving;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Archiving
{
	[Toggle(Toggles.Wfm_ArchiveSchedule_41498)]
	[DatabaseTest]
	public class ArchiveScheduleHandlerTest : ISetup
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
		public IShiftCategoryRepository ShiftCategoryRepository;
		public IActivityRepository ActivityRepository;
		public IJobResultRepository JobResultRepository;
		public WithUnitOfWork WithUnitOfWork;

		private Scenario _defaultScenario;
		private Scenario _targetScenario;
		private DateOnlyPeriod _period;
		private IPerson _person;
		private IBusinessUnit _businessUnit;
		private Guid _jobResultId;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
		}

		[SetUp]
		public void Setup()
		{
			_businessUnit = BusinessUnitFactory.BusinessUnitUsedInTest;
			_defaultScenario = new Scenario("default") { DefaultScenario = true };
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

			var jobResult = new JobResult(JobCategory.ArchiveSchedule, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), _person, DateTime.UtcNow);
			WithUnitOfWork.Do(() =>
			{
				JobResultRepository.Add(jobResult);
			});
			_jobResultId = jobResult.Id.GetValueOrDefault();
		}

		[Test]
		public void ShouldNotMoveAnyNote()
		{
			addDefaultTypesToRepositories();
			var secondPerson = PersonFactory.CreatePerson("Tester Testersson 2");
			WithUnitOfWork.Do(() => PersonRepository.Add(secondPerson));

			var assignment = new PersonAssignment(_person, _defaultScenario, _period.StartDate);
			WithUnitOfWork.Do(() => ScheduleStorage.Add(assignment));
			var @event = new ArchiveScheduleEvent
			{
				EndDate = _period.EndDate.Date,
				FromScenario = _defaultScenario.Id.GetValueOrDefault(),
				StartDate = _period.StartDate.Date,
				ToScenario = _targetScenario.Id.GetValueOrDefault(),
				JobResultId = _jobResultId,
				LogOnBusinessUnitId = _businessUnit.Id.GetValueOrDefault(),
				TotalMessages = 1
			};
			@event.PersonIds.Add(secondPerson.Id.GetValueOrDefault());
			WithUnitOfWork.Do(() => Target.Handle(@event));

			var archivedAssignment = WithUnitOfWork.Get(() => PersonAssignmentRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(_targetScenario)));
			archivedAssignment.Should().Be.Null();

			verifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldMoveOneAssignment()
		{
			addDefaultTypesToRepositories();

			var assignment = new PersonAssignment(_person, _defaultScenario, _period.StartDate);
			WithUnitOfWork.Do(() => ScheduleStorage.Add(assignment));

			WithUnitOfWork.Do(() => Target.Handle(createArchiveEvent()));

			verifyCanBeFoundInScheduleStorageForTargetScenario(_person);
			var archivedAssignment = WithUnitOfWork.Get(() => PersonAssignmentRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(_targetScenario)));
			archivedAssignment.Should().Not.Be.Null();

			verifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldMoveAssignmentsForTwoPeople()
		{
			addDefaultTypesToRepositories();
			var secondPerson = PersonFactory.CreatePerson("Tester Testersson 2");
			WithUnitOfWork.Do(() => PersonRepository.Add(secondPerson));

			var assignment = new PersonAssignment(_person, _defaultScenario, _period.StartDate);
			var secondAssignment = new PersonAssignment(secondPerson, _defaultScenario, _period.StartDate);
			WithUnitOfWork.Do(() =>
			{
				ScheduleStorage.Add(assignment);
				ScheduleStorage.Add(secondAssignment);
			});

			WithUnitOfWork.Do(() =>
			{
				var archiveScheduleEvent = createArchiveEvent();
				archiveScheduleEvent.PersonIds.Add(secondPerson.Id.GetValueOrDefault());
				Target.Handle(archiveScheduleEvent);
			});

			verifyCanBeFoundInScheduleStorageForTargetScenario(_person);
			verifyCanBeFoundInScheduleStorageForTargetScenario(secondPerson);
			var archivedAssignments = WithUnitOfWork.Get(() => PersonAssignmentRepository.LoadAll().Where(x => x.Scenario.Equals(_targetScenario)).ToList());
			archivedAssignments.Count.Should().Be.EqualTo(2);

			verifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldBeAbleToRunTwice()
		{
			addDefaultTypesToRepositories();

			var assignment = new PersonAssignment(_person, _defaultScenario, _period.StartDate);
			WithUnitOfWork.Do(() => ScheduleStorage.Add(assignment));

			var archiveScheduleEvent = createArchiveEvent();
			archiveScheduleEvent.TotalMessages = 2;
			WithUnitOfWork.Do(() => Target.Handle(archiveScheduleEvent));
			WithUnitOfWork.Do(() => Target.Handle(archiveScheduleEvent));

			verifyCanBeFoundInScheduleStorageForTargetScenario(_person);
			var archivedAssignment = WithUnitOfWork.Get(() => PersonAssignmentRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(_targetScenario)));
			archivedAssignment.Should().Not.Be.Null();

			verifyJobResultIsUpdated(2);
		}

		[Test]
		public void ShouldOverwriteExistingScheduling()
		{
			addDefaultTypesToRepositories();
			var oldCategory = new ShiftCategory("Old");
			var newCategory = new ShiftCategory("New");
			var newActivity = new Activity("New");
			var oldActivity = new Activity("Old");
			WithUnitOfWork.Do(() =>
			{
				ShiftCategoryRepository.Add(oldCategory);
				ShiftCategoryRepository.Add(newCategory);
				ActivityRepository.Add(oldActivity);
				ActivityRepository.Add(newActivity);
			});
			var assignment = new PersonAssignment(_person, _defaultScenario, _period.StartDate);
			assignment.SetShiftCategory(newCategory);

			assignment.AddActivity(newActivity, new TimePeriod(8, 15));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(assignment));
			var existingAssignment = new PersonAssignment(_person, _targetScenario, _period.StartDate);
			existingAssignment.SetShiftCategory(oldCategory);
			assignment.AddActivity(oldActivity, new TimePeriod(8, 17));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(existingAssignment));

			WithUnitOfWork.Do(() => Target.Handle(createArchiveEvent()));

			verifyCanBeFoundInScheduleStorageForTargetScenario(_person);
			WithUnitOfWork.Do(() =>
			{
				var archivedAssignments = PersonAssignmentRepository.LoadAll().Where(x => x.Scenario.Equals(_targetScenario)).ToList();
				archivedAssignments.Count.Should().Be.EqualTo(1);
				archivedAssignments.First().ShiftCategory.Should().Be.EqualTo(newCategory);
			});

			verifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldMoveOneAgentDayScheduleTag()
		{
			addDefaultTypesToRepositories();

			var scheduleTag = new ScheduleTag { Description = "Something" };
			WithUnitOfWork.Do(() => ScheduleTagRepository.Add(scheduleTag));
			var agentDayScheduleTag = new AgentDayScheduleTag(_person, new DateOnly(_period.StartDate.Date + TimeSpan.FromDays(1)), _defaultScenario, scheduleTag);
			WithUnitOfWork.Do(() => ScheduleStorage.Add(agentDayScheduleTag));

			WithUnitOfWork.Do(() => Target.Handle(createArchiveEvent()));

			verifyCanBeFoundInScheduleStorageForTargetScenario(_person);
			var archivedAgentDayScheduleTag = WithUnitOfWork.Get(() => AgentDayScheduleTagRepository.LoadAll().FirstOrDefault(x => x.Scenario.Id == _targetScenario.Id));
			archivedAgentDayScheduleTag.Should().Not.Be.Null();

			verifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldOnlyArchiveOneDay()
		{
			var theDay = _period.StartDate;
			_period = new DateOnlyPeriod(theDay, theDay);
			addDefaultTypesToRepositories();

			var theDateBeforePeriod = _period.StartDate.AddDays(-1);
			var theDateAfterPeriod = _period.EndDate.AddDays(1);
			var noteBefore = new Note(_person, theDateBeforePeriod, _defaultScenario, "Test Before");
			var noteOnTheDay = new Note(_person, theDay, _defaultScenario, "Test On The Day");
			var noteAfter = new Note(_person, theDateAfterPeriod, _defaultScenario, "Test After");
			WithUnitOfWork.Do(() => ScheduleStorage.Add(noteBefore));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(noteOnTheDay));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(noteAfter));

			WithUnitOfWork.Do(() => Target.Handle(createArchiveEvent()));
			var archivedNotes = WithUnitOfWork.Get(() => NoteRepository.LoadAll().Where(x => x.Scenario.Id == _targetScenario.Id).ToList());
			archivedNotes.Should().Not.Be.Null();
			archivedNotes.Count.Should().Be.EqualTo(1);
			archivedNotes.First().GetScheduleNote(new NoFormatting()).Should().Be.EqualTo(noteOnTheDay.GetScheduleNote(new NoFormatting()));
		}

		[Test]
		public void ShouldOverwriteToEmpty()
		{
			addDefaultTypesToRepositories();

			var note = new Note(_person, _period.StartDate, _targetScenario, "Test");
			WithUnitOfWork.Do(() => ScheduleStorage.Add(note));

			WithUnitOfWork.Do(() => Target.Handle(createArchiveEvent()));

			verifyCanBeFoundInScheduleStorageForTargetScenario(_person);
			var archivedNote = WithUnitOfWork.Get(() => NoteRepository.LoadAll().FirstOrDefault(x => x.Scenario.Id == _targetScenario.Id));
			archivedNote.Should().Be.Null();

			verifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldNotOverwriteNeighbouringDates()
		{
			addDefaultTypesToRepositories();
			var theDateBeforePeriod = _period.StartDate.AddDays(-1);
			var theDateAfterPeriod = _period.EndDate.AddDays(1);
			var noteBefore = new Note(_person, theDateBeforePeriod, _targetScenario, "Test Before");
			var noteAfter = new Note(_person, theDateAfterPeriod, _targetScenario, "Test After");
			WithUnitOfWork.Do(() => ScheduleStorage.Add(noteBefore));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(noteAfter));

			WithUnitOfWork.Do(() => Target.Handle(createArchiveEvent()));
			var archivedNotes = WithUnitOfWork.Get(() => NoteRepository.LoadAll().Where(x => x.Scenario.Id == _targetScenario.Id));
			archivedNotes.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotArchiveScheduleOutsideThePeriod()
		{
			addDefaultTypesToRepositories();
			var theDateBeforePeriod = _period.StartDate.AddDays(-1);
			var theDateAfterPeriod = _period.EndDate.AddDays(1);
			var noteBefore = new Note(_person, theDateBeforePeriod, _defaultScenario, "Test Note Before");
			var noteAfter = new Note(_person, theDateAfterPeriod, _defaultScenario, "Test Note After");
			WithUnitOfWork.Do(() => ScheduleStorage.Add(noteBefore));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(noteAfter));

			WithUnitOfWork.Do(() => Target.Handle(createArchiveEvent()));

			var archivedNotes = WithUnitOfWork.Get(() => NoteRepository.LoadAll().Where(x => x.Scenario.Id == _targetScenario.Id));
			archivedNotes.Should().Be.Empty();
		}

		[Test]
		public void ShouldMoveOneNote()
		{
			addDefaultTypesToRepositories();

			var note = new Note(_person, _period.StartDate, _defaultScenario, "Test");
			WithUnitOfWork.Do(() => ScheduleStorage.Add(note));

			WithUnitOfWork.Do(() => Target.Handle(createArchiveEvent()));

			verifyCanBeFoundInScheduleStorageForTargetScenario(_person);
			var archivedNote = WithUnitOfWork.Get(() => NoteRepository.LoadAll().FirstOrDefault(x => x.Scenario.Id == _targetScenario.Id));
			archivedNote.Should().Not.Be.Null();

			verifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldMoveOnePublicNote()
		{
			addDefaultTypesToRepositories();

			var note = new PublicNote(_person, _period.StartDate, _defaultScenario, "Test");
			WithUnitOfWork.Do(() => ScheduleStorage.Add(note));

			WithUnitOfWork.Do(() => Target.Handle(createArchiveEvent()));

			verifyCanBeFoundInScheduleStorageForTargetScenario(_person);
			var archivedNote = WithUnitOfWork.Get(() => PublicNoteRepository.LoadAll().FirstOrDefault(x => x.Scenario.Id == _targetScenario.Id));
			archivedNote.Should().Not.Be.Null();

			verifyJobResultIsUpdated();
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

			WithUnitOfWork.Do(() => Target.Handle(createArchiveEvent()));

			verifyCanBeFoundInScheduleStorageForTargetScenario(_person);
			var archivedPersonAbsence = WithUnitOfWork.Get(() => PersonAbsenceRepository.LoadAll().FirstOrDefault(x => x.Scenario.Id == _targetScenario.Id));
			archivedPersonAbsence.Should().Not.Be.Null();

			verifyJobResultIsUpdated();
		}

		private void verifyCanBeFoundInScheduleStorageForTargetScenario(IPerson person)
		{
			var result = WithUnitOfWork.Get(() => ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(true, true),
							_period, _targetScenario));
			result[person].ScheduledDayCollection(_period).Should().Not.Be.Empty();
		}

		private void verifyJobResultIsUpdated(int numberOfDetails = 1)
		{
			WithUnitOfWork.Do(() =>
			{
				var jobResult = JobResultRepository.Get(_jobResultId);
				jobResult.FinishedOk.Should().Be.True();
				jobResult.Details.Count(x => x.DetailLevel == DetailLevel.Info && x.ExceptionMessage == null).Should().Be(numberOfDetails);
			});
		}

		private ArchiveScheduleEvent createArchiveEvent()
		{
			return new ArchiveScheduleEvent(_person.Id.GetValueOrDefault())
			{
				EndDate = _period.EndDate.Date,
				FromScenario = _defaultScenario.Id.GetValueOrDefault(),
				StartDate = _period.StartDate.Date,
				ToScenario = _targetScenario.Id.GetValueOrDefault(),
				JobResultId = _jobResultId,
				LogOnBusinessUnitId = _businessUnit.Id.GetValueOrDefault(),
				TotalMessages = 1
			};
		}
	}
}