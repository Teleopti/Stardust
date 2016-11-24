using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Archiving;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
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
	[Category("BucketB")]
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
		public void ShouldNotArchiveForOtherPerson()
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

		private static readonly TargetAbsenceSplitTestCase[] targetAbsenceSplitTestCases =
		{
			new TargetAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 3),
				AbsenceEndLocal = new DateTime(2016, 1, 6, 23, 59, 0),
				ArchiveStart = new DateOnly(2016, 1, 4),
				ArchiveEnd = new DateOnly(2016, 1, 6),
				Asserts = (testCase, defaultAbsences, targetAbsences) =>
				{
					targetAbsences.Count.Should().Be.EqualTo(1);
					targetAbsences.First().Layer.Period.StartDateTime.Should().Be.EqualTo(testCase.AbsenceStartLocal);
					targetAbsences.First().Layer.Period.EndDateTime.Should().Be.EqualTo(testCase.ArchiveStart.Date.AddMinutes(-1));
				}
			},
			new TargetAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 4),
				AbsenceEndLocal = new DateTime(2016, 1, 7, 23, 59, 0),
				ArchiveStart = new DateOnly(2016, 1, 4),
				ArchiveEnd = new DateOnly(2016, 1, 6),
				Asserts = (testCase, defaultAbsences, targetAbsences) =>
				{
					targetAbsences.Count.Should().Be.EqualTo(1);
					targetAbsences.First().Layer.Period.StartDateTime.Should().Be.EqualTo(testCase.ArchiveEnd.Date.AddDays(1));
					targetAbsences.First().Layer.Period.EndDateTime.Should().Be.EqualTo(testCase.AbsenceEndLocal);
				}
			},
			new TargetAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 3),
				AbsenceEndLocal = new DateTime(2016, 1, 7, 23, 59, 0),
				ArchiveStart = new DateOnly(2016, 1, 4),
				ArchiveEnd = new DateOnly(2016, 1, 6),
				Asserts = (testCase, defaultAbsences, targetAbsences) =>
				{
					targetAbsences.Count.Should().Be.EqualTo(2);
					targetAbsences.First().Layer.Period.StartDateTime.Should().Be.EqualTo(testCase.AbsenceStartLocal);
					targetAbsences.First().Layer.Period.EndDateTime.Should().Be.EqualTo(testCase.ArchiveStart.Date.AddMinutes(-1));

					targetAbsences.Second().Layer.Period.StartDateTime.Should().Be.EqualTo(testCase.ArchiveEnd.Date.AddDays(1));
					targetAbsences.Second().Layer.Period.EndDateTime.Should().Be.EqualTo(testCase.AbsenceEndLocal);
				}
			},
			new TargetAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 4),
				AbsenceEndLocal = new DateTime(2016, 1, 6, 23, 59, 0),
				ArchiveStart = new DateOnly(2016, 1, 4),
				ArchiveEnd = new DateOnly(2016, 1, 6),
				Asserts = (testCase, defaultAbsences, targetAbsences) =>
				{
					targetAbsences.Count.Should().Be.EqualTo(0);
				}
			},
			new TargetAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 3),
				AbsenceEndLocal = new DateTime(2016, 1, 3, 23, 59, 0),
				ArchiveStart = new DateOnly(2016, 1, 4),
				ArchiveEnd = new DateOnly(2016, 1, 6),
				Asserts = (testCase, defaultAbsences, targetAbsences) =>
				{
					targetAbsences.Count.Should().Be.EqualTo(1);
					targetAbsences.First().Layer.Period.StartDateTime.Should().Be.EqualTo(testCase.AbsenceStartLocal);
					targetAbsences.First().Layer.Period.EndDateTime.Should().Be.EqualTo(testCase.AbsenceEndLocal);
				}
			},
			new TargetAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 7),
				AbsenceEndLocal = new DateTime(2016, 1, 7, 23, 59, 0),
				ArchiveStart = new DateOnly(2016, 1, 4),
				ArchiveEnd = new DateOnly(2016, 1, 6),
				Asserts = (testCase, defaultAbsences, targetAbsences) =>
				{
					targetAbsences.Count.Should().Be.EqualTo(1);
					targetAbsences.First().Layer.Period.StartDateTime.Should().Be.EqualTo(testCase.AbsenceStartLocal);
					targetAbsences.First().Layer.Period.EndDateTime.Should().Be.EqualTo(testCase.AbsenceEndLocal);
				}
			}
		};

		//[Ignore("Until Erik fixed them in handler")]
		[Test]
		public void ShouldSplitTargetAbsenceCorrectly([ValueSource(nameof(targetAbsenceSplitTestCases))] TargetAbsenceSplitTestCase testCase)
		{
			testCase.AbsenceStartLocal = DateTime.SpecifyKind(testCase.AbsenceStartLocal, DateTimeKind.Utc);
			testCase.AbsenceEndLocal = DateTime.SpecifyKind(testCase.AbsenceEndLocal, DateTimeKind.Utc);
			_person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			addDefaultTypesToRepositories();
			_archivePeriod = new DateOnlyPeriod(testCase.ArchiveStart, testCase.ArchiveEnd);

			// Given Absence
			var absence = AbsenceFactory.CreateAbsence("gone");
			WithUnitOfWork.Do(() => AbsenceRepository.Add(absence));

			// Given Person absence in target scenario
			var personAbsence = new PersonAbsence(_person, _targetScenario, new AbsenceLayer(absence, new DateTimePeriod(testCase.AbsenceStartLocal.ToUniversalTime(), testCase.AbsenceEndLocal.ToUniversalTime())));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(personAbsence));

			// When calling handler
			WithUnitOfWork.Do(() => Target.Handle(createArchiveEvent()));

			// Then there should be a splitted absence in target instead where empty for the archived period
			var defaultPersonAbsences = WithUnitOfWork.Get(() => PersonAbsenceRepository.LoadAll().Where(x => x.Scenario.Id == _defaultScenario.Id)).ToList();
			defaultPersonAbsences.Should().Not.Be.Null();
			defaultPersonAbsences.Count.Should().Be.EqualTo(0);

			var archivedPersonAbsences = WithUnitOfWork.Get(() => PersonAbsenceRepository.LoadAll().Where(x => x.Scenario.Id == _targetScenario.Id))
				.OrderBy(a => a.Layer.Period.StartDateTime).ToList();
			archivedPersonAbsences.Should().Not.Be.Null();

			testCase.Asserts(testCase, defaultPersonAbsences, archivedPersonAbsences);
		}

		[Test, Combinatorial]
		public void ShouldSplitAbsenceCorrectly([ValueSource(nameof(sourceAbsenceSplitTestCases))] SourceAbsenceSplitTestCase testCase, [ValueSource(nameof(agentTimeZones))] TimeZoneInfo timeZoneInfo)
		{
			testCase.Setup(timeZoneInfo);
			Console.WriteLine(testCase);

			_person.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);
			addDefaultTypesToRepositories();

			// Given Absence
			_archivePeriod = new DateOnlyPeriod(testCase.ArchiveStart, testCase.ArchiveEnd);
			var absence = AbsenceFactory.CreateAbsence("gone");
			WithUnitOfWork.Do(() => AbsenceRepository.Add(absence));

			// Given Person absence
			var personAbsence = new PersonAbsence(_person, _defaultScenario, new AbsenceLayer(absence, new DateTimePeriod(testCase.AbsenceStartUtc.ToUniversalTime(), testCase.AbsenceEndUtc.ToUniversalTime())));
			Console.WriteLine($"Absence is {personAbsence.Layer.Period.ElapsedTime().TotalMinutes} minutes");
			WithUnitOfWork.Do(() => ScheduleStorage.Add(personAbsence));

			// When calling handler
			WithUnitOfWork.Do(() => Target.Handle(createArchiveEvent()));

			// Then
			var archivedPersonAbsences = WithUnitOfWork.Get(() => PersonAbsenceRepository.LoadAll().Where(x => x.Scenario.Id == _targetScenario.Id)).ToList();
			archivedPersonAbsences.Should().Not.Be.Null();

			var defaultPersonAbsences = WithUnitOfWork.Get(() => PersonAbsenceRepository.LoadAll().Where(x => x.Scenario.Id == _defaultScenario.Id)).ToList();
			defaultPersonAbsences.Should().Not.Be.Null();
			defaultPersonAbsences.Count.Should().Be.EqualTo(1);

			foreach (var defaultPersonAbsence in defaultPersonAbsences)
			{
				Console.WriteLine($"Absence in default [{defaultPersonAbsence.Id}]: {SplitTestCase.DateTimePeriodInTimeZoneToString(defaultPersonAbsence.Period, timeZoneInfo)}");
			}

			foreach (var archivePersonAbsence in archivedPersonAbsences)
			{
				Console.WriteLine($"Absence in target  [{archivePersonAbsence.Id}]: {SplitTestCase.DateTimePeriodInTimeZoneToString(archivePersonAbsence.Period, timeZoneInfo)}");
			}

			if (testCase.ExpectedOutcome == SourceAbsenceSplitTestCase.Expectations.NothingArchived)
			{
				archivedPersonAbsences.Count.Should().Be.EqualTo(0);
			}

			if (testCase.ExpectedOutcome == SourceAbsenceSplitTestCase.Expectations.OneArchived)
			{
				archivedPersonAbsences.Count.Should().Be.EqualTo(1);
				TimeZoneInfo.ConvertTime(archivedPersonAbsences.First().Period.StartDateTime, timeZoneInfo).Should().Be.EqualTo(testCase.ExpectedStart());
				TimeZoneInfo.ConvertTime(archivedPersonAbsences.First().Period.EndDateTime, timeZoneInfo).Should().Be.EqualTo(testCase.ExpectedEnd());
			}
		}

		private static readonly TimeZoneInfo[] agentTimeZones =
		{
			TimeZoneInfoFactory.UtcTimeZoneInfo(),
			TimeZoneInfoFactory.ChinaTimeZoneInfo(),
			TimeZoneInfoFactory.DenverTimeZoneInfo()
		};

		private static readonly SourceAbsenceSplitTestCase[] sourceAbsenceSplitTestCases = {
			new SourceAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 2, 10, 0, 0),
				AbsenceEndLocal =   new DateTime(2016, 1, 2, 12, 0, 0),
				ArchiveStart = new DateOnly(2016, 1, 1),
				ArchiveEnd =   new DateOnly(2016, 1, 6),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.OneArchived
			},
			new SourceAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 1),
				AbsenceEndLocal =   new DateTime(2016, 1, 10),
				ArchiveStart = new DateOnly(2016, 1, 4),
				ArchiveEnd =   new DateOnly(2016, 1, 6),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.OneArchived
			},
			new SourceAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 1),
				AbsenceEndLocal =   new DateTime(2016, 1, 2),
				ArchiveStart = new DateOnly(2016, 1, 4),
				ArchiveEnd =   new DateOnly(2016, 1, 6),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.NothingArchived
			},
			new SourceAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 6),
				AbsenceEndLocal =   new DateTime(2016, 1, 10),
				ArchiveStart = new DateOnly(2016, 1, 4),
				ArchiveEnd =   new DateOnly(2016, 1, 4),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.NothingArchived
			},
			new SourceAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 10, 10, 0, 0),
				AbsenceEndLocal =   new DateTime(2016, 1, 11, 10, 0, 0),
				ArchiveStart = new DateOnly(2016, 1, 1),
				ArchiveEnd =   new DateOnly(2016, 1, 10),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.OneArchived
			},
			new SourceAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 6, 10, 0, 0),
				AbsenceEndLocal =   new DateTime(2016, 1, 11, 10, 0, 0),
				ArchiveStart = new DateOnly(2016, 1, 10),
				ArchiveEnd =   new DateOnly(2016, 1, 12),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.OneArchived
			},
			new SourceAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 13, 0, 0, 0),
				AbsenceEndLocal =   new DateTime(2016, 1, 14, 0, 0, 0),
				ArchiveStart = new DateOnly(2016, 1, 10),
				ArchiveEnd =   new DateOnly(2016, 1, 12),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.NothingArchived
			},
			new SourceAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 8, 0, 0, 0),
				AbsenceEndLocal =   new DateTime(2016, 1, 10, 0, 0, 0),
				ArchiveStart = new DateOnly(2016, 1, 10),
				ArchiveEnd =   new DateOnly(2016, 1, 12),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.NothingArchived
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