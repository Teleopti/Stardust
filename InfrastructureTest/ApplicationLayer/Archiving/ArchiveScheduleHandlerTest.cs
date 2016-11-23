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
		private DateOnlyPeriod _archivePeriod;
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
			_archivePeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 5);
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

			var assignment = new PersonAssignment(_person, _defaultScenario, _archivePeriod.StartDate);
			WithUnitOfWork.Do(() => ScheduleStorage.Add(assignment));
			var @event = new ArchiveScheduleEvent
			{
				EndDate = _archivePeriod.EndDate,
				FromScenario = _defaultScenario.Id.GetValueOrDefault(),
				StartDate = _archivePeriod.StartDate,
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

			var assignment = new PersonAssignment(_person, _defaultScenario, _archivePeriod.StartDate);
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

			var assignment = new PersonAssignment(_person, _defaultScenario, _archivePeriod.StartDate);
			var secondAssignment = new PersonAssignment(secondPerson, _defaultScenario, _archivePeriod.StartDate);
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

			var assignment = new PersonAssignment(_person, _defaultScenario, _archivePeriod.StartDate);
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
			var assignment = new PersonAssignment(_person, _defaultScenario, _archivePeriod.StartDate);
			assignment.SetShiftCategory(newCategory);

			assignment.AddActivity(newActivity, new TimePeriod(8, 15));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(assignment));
			var existingAssignment = new PersonAssignment(_person, _targetScenario, _archivePeriod.StartDate);
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
			var agentDayScheduleTag = new AgentDayScheduleTag(_person, new DateOnly(_archivePeriod.StartDate.Date + TimeSpan.FromDays(1)), _defaultScenario, scheduleTag);
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
			var theDay = _archivePeriod.StartDate;
			_archivePeriod = new DateOnlyPeriod(theDay, theDay);
			addDefaultTypesToRepositories();

			var theDateBeforePeriod = _archivePeriod.StartDate.AddDays(-1);
			var theDateAfterPeriod = _archivePeriod.EndDate.AddDays(1);
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

		[Ignore("Ignored until handler is fixed")]
		[TestCaseSource(nameof(splitTestCases))]
		public void ShouldSplitAbsenceCorrectly(SplitTestCase testCase)
		{
			testCase.Setup();
			addDefaultTypesToRepositories();

			_archivePeriod = new DateOnlyPeriod(testCase.ArchiveStart, testCase.ArchiveEnd);

			var absence = AbsenceFactory.CreateAbsence("gone");
			WithUnitOfWork.Do(() => AbsenceRepository.Add(absence));
			var personAbsence = new PersonAbsence(_person, _defaultScenario, new AbsenceLayer(absence, new DateTimePeriod(testCase.AbsenceStart.ToUniversalTime(), testCase.AbsenceEnd.ToUniversalTime())));
			Console.WriteLine($"Absence is {personAbsence.Layer.Period.ElapsedTime().TotalMinutes} minutes");
			WithUnitOfWork.Do(() => ScheduleStorage.Add(personAbsence));

			WithUnitOfWork.Do(() => Target.Handle(createArchiveEvent()));
			var archivedPersonAbsences = WithUnitOfWork.Get(() => PersonAbsenceRepository.LoadAll().Where(x => x.Scenario.Id == _targetScenario.Id)).ToList();
			archivedPersonAbsences.Should().Not.Be.Null();

			var defaultPersonAbsences = WithUnitOfWork.Get(() => PersonAbsenceRepository.LoadAll().Where(x => x.Scenario.Id == _defaultScenario.Id)).ToList();
			defaultPersonAbsences.Should().Not.Be.Null();
			defaultPersonAbsences.Count.Should().Be.EqualTo(1);

			if (testCase.ExpectedOutcome == SplitTestCase.Expectations.NothingArchived)
			{
				archivedPersonAbsences.Count.Should().Be.EqualTo(0);
			}

			if (testCase.ExpectedOutcome == SplitTestCase.Expectations.OneArchived)
			{
				archivedPersonAbsences.Count.Should().Be.EqualTo(1);
				archivedPersonAbsences.First().Period.StartDateTime.Should().Be.EqualTo(testCase.CommonStart());
				archivedPersonAbsences.First().Period.EndDateTime.Should().Be.EqualTo(testCase.CommonEnd());
			}
		}

		public class SplitTestCase
		{
			public enum Expectations
			{
				NothingArchived,
				OneArchived
			}

			public DateTime AbsenceStart;
			public DateTime AbsenceEnd;
			public DateOnly ArchiveStart;
			public DateOnly ArchiveEnd;
			public Expectations ExpectedOutcome;

			public void Setup()
			{
				Console.WriteLine(ToString());

				AbsenceStart = DateTime.SpecifyKind(AbsenceStart, DateTimeKind());
				AbsenceEnd = DateTime.SpecifyKind(AbsenceEnd, DateTimeKind());
			}

			public DateTimeKind DateTimeKind()
			{
				return System.DateTimeKind.Utc;
			}

			public DateTime CommonStart()
			{
				return ArchiveStart.Date < AbsenceStart ? AbsenceStart : ArchiveStart.Date;
			}

			public DateTime CommonEnd()
			{
				return ArchiveEnd.Date < AbsenceEnd ? ArchiveEnd.Date : AbsenceEnd;
			}

			public override string ToString()
			{
				return
					$"Archiving {dateTimeFormat(ArchiveStart)} - {dateTimeFormat(ArchiveEnd)} with " +
					$"Absence {dateTimeFormat(AbsenceStart)} - {dateTimeFormat(AbsenceEnd)} ({DateTimeKind()}). " +
					$"Expected {ExpectedOutcome}.";
			}

			private static string dateTimeFormat(DateOnly dateOnly)
			{
				return dateTimeFormat(dateOnly.Date);
			}

			private static string dateTimeFormat(DateTime dateTime)
			{
				if (dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.Second == 0)
					return dateTime.ToString("yyyy-MM-dd");
				return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
			}
		}

		private static readonly SplitTestCase[] splitTestCases = {
			new SplitTestCase
			{
				AbsenceStart = new DateTime(2016, 1, 2, 10, 0, 0),
				AbsenceEnd =   new DateTime(2016, 1, 2, 12, 0, 0),
				ArchiveStart = new DateOnly(2016, 1, 1),
				ArchiveEnd =   new DateOnly(2016, 1, 6),
				ExpectedOutcome = SplitTestCase.Expectations.OneArchived
			},
			new SplitTestCase
			{
				AbsenceStart = new DateTime(2016, 1, 1),
				AbsenceEnd =   new DateTime(2016, 1, 10),
				ArchiveStart = new DateOnly(2016, 1, 4),
				ArchiveEnd =   new DateOnly(2016, 1, 6),
				ExpectedOutcome = SplitTestCase.Expectations.OneArchived
			},
			new SplitTestCase
			{
				AbsenceStart = new DateTime(2016, 1, 1),
				AbsenceEnd =   new DateTime(2016, 1, 2),
				ArchiveStart = new DateOnly(2016, 1, 4),
				ArchiveEnd =   new DateOnly(2016, 1, 6),
				ExpectedOutcome = SplitTestCase.Expectations.NothingArchived
			},
			new SplitTestCase
			{
				AbsenceStart = new DateTime(2016, 1, 6),
				AbsenceEnd =   new DateTime(2016, 1, 10),
				ArchiveStart = new DateOnly(2016, 1, 4),
				ArchiveEnd =   new DateOnly(2016, 1, 4),
				ExpectedOutcome = SplitTestCase.Expectations.NothingArchived
			}
		};

		[Test]
		public void ShouldOverwriteToEmpty()
		{
			addDefaultTypesToRepositories();

			var note = new Note(_person, _archivePeriod.StartDate, _targetScenario, "Test");
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
			var theDateBeforePeriod = _archivePeriod.StartDate.AddDays(-1);
			var theDateAfterPeriod = _archivePeriod.EndDate.AddDays(1);
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
			var theDateBeforePeriod = _archivePeriod.StartDate.AddDays(-1);
			var theDateAfterPeriod = _archivePeriod.EndDate.AddDays(1);
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

			var note = new Note(_person, _archivePeriod.StartDate, _defaultScenario, "Test");
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

			var note = new PublicNote(_person, _archivePeriod.StartDate, _defaultScenario, "Test");
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
					new DateTimePeriod(_archivePeriod.StartDate.Date.ToUniversalTime(), (_archivePeriod.StartDate.Date + TimeSpan.FromDays(1)).ToUniversalTime())));
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
							_archivePeriod, _targetScenario));
			result[person].ScheduledDayCollection(_archivePeriod).Should().Not.Be.Empty();
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
				EndDate = _archivePeriod.EndDate,
				FromScenario = _defaultScenario.Id.GetValueOrDefault(),
				StartDate = _archivePeriod.StartDate,
				ToScenario = _targetScenario.Id.GetValueOrDefault(),
				JobResultId = _jobResultId,
				LogOnBusinessUnitId = _businessUnit.Id.GetValueOrDefault(),
				TotalMessages = 1
			};
		}
	}
}