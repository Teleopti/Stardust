using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ManageSchedule;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.ManageSchedule
{
	[Category("BucketB")]
	[DatabaseTest]
	public class CopyScheduleHandlerTest : ScheduleManagementTestBase
	{
		public CopyScheduleHandler Target;

		[SetUp]
		public void Setup()
		{
			BusinessUnit = BusinessUnitFactory.BusinessUnitUsedInTest;
			SourceScenario = new Scenario("default") { DefaultScenario = true };
			TargetScenario = new Scenario("target");
			Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 5);
			Person = PersonFactory.CreatePerson("Tester Testersson");
		}

		[Test]
		public void ShouldNotArchiveForOtherPerson()
		{
			AddDefaultTypesToRepositories();
			var secondPerson = PersonFactory.CreatePerson("Tester Testersson 2");
			WithUnitOfWork.Do(() => PersonRepository.Add(secondPerson));

			var assignment = new PersonAssignment(Person, SourceScenario, Period.StartDate);
			WithUnitOfWork.Do(() => ScheduleStorage.Add(assignment));

			Target.Handle(createCopyEvent(secondPerson));

			var archivedAssignment = WithUnitOfWork.Get(() => PersonAssignmentRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(TargetScenario)));
			archivedAssignment.Should().Be.Null();

			VerifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldMoveAssignmentsForTwoPeople()
		{
			AddDefaultTypesToRepositories();
			var secondPerson = PersonFactory.CreatePerson("Tester Testersson 2");
			WithUnitOfWork.Do(() => PersonRepository.Add(secondPerson));

			var assignment = new PersonAssignment(Person, SourceScenario, Period.StartDate);
			var secondAssignment = new PersonAssignment(secondPerson, SourceScenario, Period.StartDate);
			WithUnitOfWork.Do(() =>
			{
				ScheduleStorage.Add(assignment);
				ScheduleStorage.Add(secondAssignment);
			});

			var copyScheduleEvent = createCopyEvent();
			copyScheduleEvent.PersonIds = copyScheduleEvent.PersonIds.Append(secondPerson.Id.GetValueOrDefault()).ToArray();
			Target.Handle(copyScheduleEvent);

			VerifyCanBeFoundInScheduleStorageForTargetScenario(Person);
			VerifyCanBeFoundInScheduleStorageForTargetScenario(secondPerson);
			var archivedAssignments = WithUnitOfWork.Get(() => PersonAssignmentRepository.LoadAll().Where(x => x.Scenario.Equals(TargetScenario)).ToList());
			archivedAssignments.Count.Should().Be.EqualTo(2);

			VerifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldBeAbleToRunTwice()
		{
			AddDefaultTypesToRepositories();

			var assignment = new PersonAssignment(Person, SourceScenario, Period.StartDate);
			WithUnitOfWork.Do(() => ScheduleStorage.Add(assignment));

			var copyScheduleEvent = createCopyEvent();
			copyScheduleEvent.TotalMessages = 2;

			Target.Handle(copyScheduleEvent);
			Target.Handle(copyScheduleEvent);

			VerifyCanBeFoundInScheduleStorageForTargetScenario(Person);
			var archivedAssignment = WithUnitOfWork.Get(() => PersonAssignmentRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(TargetScenario)));
			archivedAssignment.Should().Not.Be.Null();

			VerifyJobResultIsUpdated(2);
		}

		[Test]
		public void ShouldOverwriteExistingScheduling()
		{
			AddDefaultTypesToRepositories();
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
			var assignment = new PersonAssignment(Person, SourceScenario, Period.StartDate);
			assignment.SetShiftCategory(newCategory);

			assignment.AddActivity(newActivity, new TimePeriod(8, 15));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(assignment));
			var existingAssignment = new PersonAssignment(Person, TargetScenario, Period.StartDate);
			existingAssignment.SetShiftCategory(oldCategory);
			assignment.AddActivity(oldActivity, new TimePeriod(8, 17));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(existingAssignment));

			Target.Handle(createCopyEvent());

			VerifyCanBeFoundInScheduleStorageForTargetScenario(Person);
			WithUnitOfWork.Do(() =>
			{
				var archivedAssignments = PersonAssignmentRepository.LoadAll().Where(x => x.Scenario.Equals(TargetScenario)).ToList();
				archivedAssignments.Count.Should().Be.EqualTo(1);
				archivedAssignments.First().ShiftCategory.Should().Be.EqualTo(newCategory);
			});

			VerifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldMoveOneOfType([ValueSource(nameof(moveTypeTestCases))] MoveTestCase<CopyScheduleHandlerTest> testCase)
		{
			AddDefaultTypesToRepositories();
			testCase.CreateTypeInSourceScenario(WithUnitOfWork, this, Person, Period, SourceScenario, TargetScenario);

			Target.Handle(createCopyEvent());

			VerifyCanBeFoundInScheduleStorageForTargetScenario(Person);

			testCase.VerifyExistsInTargetScenario(WithUnitOfWork, this, TargetScenario);

			VerifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldOnlyArchiveOneDay()
		{
			var theDay = Period.StartDate;
			Period = new DateOnlyPeriod(theDay, theDay);
			AddDefaultTypesToRepositories();

			var theDateBeforePeriod = Period.StartDate.AddDays(-1);
			var theDateAfterPeriod = Period.EndDate.AddDays(1);
			var noteBefore = new Note(Person, theDateBeforePeriod, SourceScenario, "Test Before");
			var noteOnTheDay = new Note(Person, theDay, SourceScenario, "Test On The Day");
			var noteAfter = new Note(Person, theDateAfterPeriod, SourceScenario, "Test After");
			WithUnitOfWork.Do(() => ScheduleStorage.Add(noteBefore));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(noteOnTheDay));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(noteAfter));

			Target.Handle(createCopyEvent());
			var archivedNotes = WithUnitOfWork.Get(() => NoteRepository.LoadAll().Where(x => x.Scenario.Id == TargetScenario.Id).ToList());
			archivedNotes.Should().Not.Be.Null();
			archivedNotes.Count.Should().Be.EqualTo(1);
			archivedNotes.First().GetScheduleNote(new NoFormatting()).Should().Be.EqualTo(noteOnTheDay.GetScheduleNote(new NoFormatting()));
		}

		[Test]
		public void ShouldSplitTargetAbsenceCorrectly([ValueSource(nameof(targetAbsenceSplitTestCases))] TargetAbsenceSplitTestCase testCase)
		{
			testCase.AbsenceStartLocal = DateTime.SpecifyKind(testCase.AbsenceStartLocal, DateTimeKind.Utc);
			testCase.AbsenceEndLocal = DateTime.SpecifyKind(testCase.AbsenceEndLocal, DateTimeKind.Utc);
			Person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			AddDefaultTypesToRepositories();
			Period = new DateOnlyPeriod(testCase.CopyStart, testCase.CopyEnd);

			// Given Absence
			var absence = AbsenceFactory.CreateAbsence("gone");
			WithUnitOfWork.Do(() => AbsenceRepository.Add(absence));

			// Given Person absence in target scenario
			var personAbsence = new PersonAbsence(Person, TargetScenario, new AbsenceLayer(absence, new DateTimePeriod(testCase.AbsenceStartLocal.ToUniversalTime(), testCase.AbsenceEndLocal.ToUniversalTime())));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(personAbsence));

			// When calling handler
			Target.Handle(createCopyEvent());

			// Then there should be a splitted absence in target instead where empty for the archived period
			var defaultPersonAbsences = WithUnitOfWork.Get(() => PersonAbsenceRepository.LoadAll().Where(x => x.Scenario.Id == SourceScenario.Id)).ToList();
			defaultPersonAbsences.Should().Not.Be.Null();
			defaultPersonAbsences.Count.Should().Be.EqualTo(0);

			var archivedPersonAbsences = WithUnitOfWork.Get(() => PersonAbsenceRepository.LoadAll().Where(x => x.Scenario.Id == TargetScenario.Id))
				.OrderBy(a => a.Layer.Period.StartDateTime).ToList();
			archivedPersonAbsences.Should().Not.Be.Null();

			testCase.Asserts(testCase, defaultPersonAbsences, archivedPersonAbsences);
		}

		[Test, Combinatorial]
		public void ShouldSplitAbsenceCorrectly([ValueSource(nameof(sourceAbsenceSplitTestCases))] SourceAbsenceSplitTestCase testCase, [ValueSource(nameof(agentTimeZones))] TimeZoneInfo timeZoneInfo)
		{
			testCase.Setup(timeZoneInfo);
			Console.WriteLine(testCase);

			Person.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);
			AddDefaultTypesToRepositories();

			// Given Absence
			Period = new DateOnlyPeriod(testCase.CopyStart, testCase.CopyEnd);
			var absence = AbsenceFactory.CreateAbsence("gone");
			WithUnitOfWork.Do(() => AbsenceRepository.Add(absence));

			// Given Person absence
			var personAbsence = new PersonAbsence(Person, SourceScenario, new AbsenceLayer(absence, new DateTimePeriod(testCase.AbsenceStartUtc.ToUniversalTime(), testCase.AbsenceEndUtc.ToUniversalTime())));
			Console.WriteLine($"Absence is {personAbsence.Layer.Period.ElapsedTime().TotalMinutes} minutes");
			WithUnitOfWork.Do(() => ScheduleStorage.Add(personAbsence));

			// When calling handler
			Target.Handle(createCopyEvent());

			// Then
			var archivedPersonAbsences = WithUnitOfWork.Get(() => PersonAbsenceRepository.LoadAll().Where(x => x.Scenario.Id == TargetScenario.Id)).ToList();
			archivedPersonAbsences.Should().Not.Be.Null();

			var defaultPersonAbsences = WithUnitOfWork.Get(() => PersonAbsenceRepository.LoadAll().Where(x => x.Scenario.Id == SourceScenario.Id)).ToList();
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

		[Test]
		public void ShouldOverwriteToEmpty()
		{
			AddDefaultTypesToRepositories();

			var note = new Note(Person, Period.StartDate, TargetScenario, "Test");
			WithUnitOfWork.Do(() => ScheduleStorage.Add(note));

			Target.Handle(createCopyEvent());

			VerifyCanBeFoundInScheduleStorageForTargetScenario(Person);
			var archivedNote = WithUnitOfWork.Get(() => NoteRepository.LoadAll().FirstOrDefault(x => x.Scenario.Id == TargetScenario.Id));
			archivedNote.Should().Be.Null();

			VerifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldNotOverwriteNeighbouringDates()
		{
			AddDefaultTypesToRepositories();
			var theDateBeforePeriod = Period.StartDate.AddDays(-1);
			var theDateAfterPeriod = Period.EndDate.AddDays(1);
			var noteBefore = new Note(Person, theDateBeforePeriod, TargetScenario, "Test Before");
			var noteAfter = new Note(Person, theDateAfterPeriod, TargetScenario, "Test After");
			WithUnitOfWork.Do(() => ScheduleStorage.Add(noteBefore));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(noteAfter));

			Target.Handle(createCopyEvent());

			var archivedNotes = WithUnitOfWork.Get(() => NoteRepository.LoadAll().Where(x => x.Scenario.Id == TargetScenario.Id));
			archivedNotes.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotCopyScheduleOutsideThePeriod()
		{
			AddDefaultTypesToRepositories();
			var theDateBeforePeriod = Period.StartDate.AddDays(-1);
			var theDateAfterPeriod = Period.EndDate.AddDays(1);
			var noteBefore = new Note(Person, theDateBeforePeriod, SourceScenario, "Test Note Before");
			var noteAfter = new Note(Person, theDateAfterPeriod, SourceScenario, "Test Note After");
			WithUnitOfWork.Do(() => ScheduleStorage.Add(noteBefore));
			WithUnitOfWork.Do(() => ScheduleStorage.Add(noteAfter));

			Target.Handle(createCopyEvent());

			var archivedNotes = WithUnitOfWork.Get(() => NoteRepository.LoadAll().Where(x => x.Scenario.Id == TargetScenario.Id));
			archivedNotes.Should().Be.Empty();
		}

		private CopyScheduleEvent createCopyEvent(IPerson person = null)
		{
			return new CopyScheduleEvent((person ?? Person).Id.GetValueOrDefault())
			{
				EndDate = Period.EndDate,
				FromScenario = SourceScenario.Id.GetValueOrDefault(),
				StartDate = Period.StartDate,
				ToScenario = TargetScenario.Id.GetValueOrDefault(),
				JobResultId = JobResultId,
				LogOnBusinessUnitId = BusinessUnit.Id.GetValueOrDefault(),
				TotalMessages = 1,
				LogOnDatasource = DataSourceHelper.TestTenantName
			};
		}

		private static readonly MoveTestCase<CopyScheduleHandlerTest>[] moveTypeTestCases =
		{
			new MoveTestCase<CopyScheduleHandlerTest>(nameof(AgentDayScheduleTag))
			{
				CreateType = (testClass, person, period, sourceScenario, targetScenario) =>
				{
					var scheduleTag = new ScheduleTag { Description = "Something" };
					testClass.ScheduleTagRepository.Add(scheduleTag);
					var agentDayScheduleTag = new AgentDayScheduleTag(person, new DateOnly(period.StartDate.Date + TimeSpan.FromDays(1)), sourceScenario, scheduleTag);
					testClass.ScheduleStorage.Add(agentDayScheduleTag);
				},
				LoadMethod = (testClass, targetScenario) => testClass.AgentDayScheduleTagRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(targetScenario))
			},
			new MoveTestCase<CopyScheduleHandlerTest>(nameof(PersonAssignment))
			{
				CreateType = (testClass, person, period, sourceScenario, targetScenario) => testClass.ScheduleStorage.Add(new PersonAssignment(person, sourceScenario, period.StartDate)),
				LoadMethod = (testClass, targetScenario) => testClass.PersonAssignmentRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(targetScenario))
			},
			new MoveTestCase<CopyScheduleHandlerTest>(nameof(PublicNote))
			{
				CreateType = (testClass, person, period, sourceScenario, targetScenario) => testClass.ScheduleStorage.Add( new PublicNote(person, period.StartDate, sourceScenario, "Test")),
				LoadMethod = (testClass, targetScenario) => testClass.PublicNoteRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(targetScenario))
			},
			new MoveTestCase<CopyScheduleHandlerTest>(nameof(PersonAbsence))
			{
				CreateType = (testClass, person, period, sourceScenario, targetScenario) =>
				{
					var absence = AbsenceFactory.CreateAbsence("gone");
					testClass.AbsenceRepository.Add(absence);
					var personAbsence = new PersonAbsence(person, sourceScenario, new AbsenceLayer(absence,
							new DateTimePeriod(period.StartDate.Date.ToUniversalTime(), (period.StartDate.Date + TimeSpan.FromDays(1)).ToUniversalTime())));
					testClass.ScheduleStorage.Add(personAbsence);
				},
				LoadMethod = (testClass, targetScenario) => testClass.PersonAbsenceRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(targetScenario))
			},
			new MoveTestCase<CopyScheduleHandlerTest>(nameof(Note))
			{
				CreateType = (testClass, person, period, sourceScenario, targetScenario) => testClass.ScheduleStorage.Add(new Note(person, period.StartDate, sourceScenario, "Test")),
				LoadMethod = (testClass, targetScenario) => testClass.NoteRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(targetScenario))
			}
		};

		private static readonly TargetAbsenceSplitTestCase[] targetAbsenceSplitTestCases =
		{
			new TargetAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 3),
				AbsenceEndLocal = new DateTime(2016, 1, 6, 23, 59, 0),
				CopyStart = new DateOnly(2016, 1, 4),
				CopyEnd = new DateOnly(2016, 1, 6),
				Asserts = (testCase, defaultAbsences, targetAbsences) =>
				{
					targetAbsences.Count.Should().Be.EqualTo(1);
					targetAbsences.First().Layer.Period.StartDateTime.Should().Be.EqualTo(testCase.AbsenceStartLocal);
					targetAbsences.First().Layer.Period.EndDateTime.Should().Be.EqualTo(testCase.CopyStart.Date.AddMinutes(-1));
				}
			},
			new TargetAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 4),
				AbsenceEndLocal = new DateTime(2016, 1, 7, 23, 59, 0),
				CopyStart = new DateOnly(2016, 1, 4),
				CopyEnd = new DateOnly(2016, 1, 6),
				Asserts = (testCase, defaultAbsences, targetAbsences) =>
				{
					targetAbsences.Count.Should().Be.EqualTo(1);
					targetAbsences.First().Layer.Period.StartDateTime.Should().Be.EqualTo(testCase.CopyEnd.Date.AddDays(1));
					targetAbsences.First().Layer.Period.EndDateTime.Should().Be.EqualTo(testCase.AbsenceEndLocal);
				}
			},
			new TargetAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 3),
				AbsenceEndLocal = new DateTime(2016, 1, 7, 23, 59, 0),
				CopyStart = new DateOnly(2016, 1, 4),
				CopyEnd = new DateOnly(2016, 1, 6),
				Asserts = (testCase, defaultAbsences, targetAbsences) =>
				{
					targetAbsences.Count.Should().Be.EqualTo(2);
					targetAbsences.First().Layer.Period.StartDateTime.Should().Be.EqualTo(testCase.AbsenceStartLocal);
					targetAbsences.First().Layer.Period.EndDateTime.Should().Be.EqualTo(testCase.CopyStart.Date.AddMinutes(-1));

					targetAbsences.Second().Layer.Period.StartDateTime.Should().Be.EqualTo(testCase.CopyEnd.Date.AddDays(1));
					targetAbsences.Second().Layer.Period.EndDateTime.Should().Be.EqualTo(testCase.AbsenceEndLocal);
				}
			},
			new TargetAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 4),
				AbsenceEndLocal = new DateTime(2016, 1, 6, 23, 59, 0),
				CopyStart = new DateOnly(2016, 1, 4),
				CopyEnd = new DateOnly(2016, 1, 6),
				Asserts = (testCase, defaultAbsences, targetAbsences) =>
				{
					targetAbsences.Count.Should().Be.EqualTo(0);
				}
			},
			new TargetAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 3),
				AbsenceEndLocal = new DateTime(2016, 1, 3, 23, 59, 0),
				CopyStart = new DateOnly(2016, 1, 4),
				CopyEnd = new DateOnly(2016, 1, 6),
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
				CopyStart = new DateOnly(2016, 1, 4),
				CopyEnd = new DateOnly(2016, 1, 6),
				Asserts = (testCase, defaultAbsences, targetAbsences) =>
				{
					targetAbsences.Count.Should().Be.EqualTo(1);
					targetAbsences.First().Layer.Period.StartDateTime.Should().Be.EqualTo(testCase.AbsenceStartLocal);
					targetAbsences.First().Layer.Period.EndDateTime.Should().Be.EqualTo(testCase.AbsenceEndLocal);
				}
			}
		};

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
				CopyStart = new DateOnly(2016, 1, 1),
				CopyEnd =   new DateOnly(2016, 1, 6),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.OneArchived
			},
			new SourceAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 1),
				AbsenceEndLocal =   new DateTime(2016, 1, 10),
				CopyStart = new DateOnly(2016, 1, 4),
				CopyEnd =   new DateOnly(2016, 1, 6),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.OneArchived
			},
			new SourceAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 1),
				AbsenceEndLocal =   new DateTime(2016, 1, 2),
				CopyStart = new DateOnly(2016, 1, 4),
				CopyEnd =   new DateOnly(2016, 1, 6),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.NothingArchived
			},
			new SourceAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 6),
				AbsenceEndLocal =   new DateTime(2016, 1, 10),
				CopyStart = new DateOnly(2016, 1, 4),
				CopyEnd =   new DateOnly(2016, 1, 4),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.NothingArchived
			},
			new SourceAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 10, 10, 0, 0),
				AbsenceEndLocal =   new DateTime(2016, 1, 11, 10, 0, 0),
				CopyStart = new DateOnly(2016, 1, 1),
				CopyEnd =   new DateOnly(2016, 1, 10),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.OneArchived
			},
			new SourceAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 6, 10, 0, 0),
				AbsenceEndLocal =   new DateTime(2016, 1, 11, 10, 0, 0),
				CopyStart = new DateOnly(2016, 1, 10),
				CopyEnd =   new DateOnly(2016, 1, 12),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.OneArchived
			},
			new SourceAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 13, 0, 0, 0),
				AbsenceEndLocal =   new DateTime(2016, 1, 14, 0, 0, 0),
				CopyStart = new DateOnly(2016, 1, 10),
				CopyEnd =   new DateOnly(2016, 1, 12),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.NothingArchived
			},
			new SourceAbsenceSplitTestCase
			{
				AbsenceStartLocal = new DateTime(2016, 1, 8, 0, 0, 0),
				AbsenceEndLocal =   new DateTime(2016, 1, 10, 0, 0, 0),
				CopyStart = new DateOnly(2016, 1, 10),
				CopyEnd =   new DateOnly(2016, 1, 12),
				ExpectedOutcome = SourceAbsenceSplitTestCase.Expectations.NothingArchived
			}
		};
	}
}