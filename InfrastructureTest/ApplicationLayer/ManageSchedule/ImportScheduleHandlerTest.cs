using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ManageSchedule;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.ManageSchedule
{
	[Category("BucketB")]
	[DatabaseTest]
	public class ImportScheduleHandlerTest : ScheduleManagementTestBase
	{
		public ImportScheduleHandler Target;
		public IPersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public IContractRepository ContractRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public ITeamRepository TeamRepository;
		public ISiteRepository SiteRepository;

		[SetUp]
		public void Setup()
		{
			BusinessUnit = BusinessUnitUsedInTests.BusinessUnit;
			SourceScenario = new Scenario("source");
			TargetScenario = new Scenario("default") { DefaultScenario = true };
			Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 5);
			Person = PersonFactory.CreatePerson("Tester Testersson");
		}

		[Test]
		public void ShouldMoveOneOfType([ValueSource(nameof(moveTypeTestCases))] MoveTestCase<ImportScheduleHandlerTest> testCase)
		{
			AddDefaultTypesToRepositories();
			testCase.CreateTypeInSourceScenario(WithUnitOfWork, this, Person, Period, SourceScenario, TargetScenario);

			Target.Handle(createImportEvent());

			VerifyCanBeFoundInScheduleStorageForTargetScenario(Person);

			testCase.VerifyExistsInTargetScenario(WithUnitOfWork, this, TargetScenario);

			VerifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldOverwriteEachOverwriteable([ValueSource(nameof(overwriteTestCases))] MoveTestCase<ImportScheduleHandlerTest> testCase)
		{
			AddDefaultTypesToRepositories();
			testCase.CreateTypeInSourceScenario(WithUnitOfWork, this, Person, Period, SourceScenario, TargetScenario);

			Target.Handle(createImportEvent());

			VerifyCanBeFoundInScheduleStorageForTargetScenario(Person);

			testCase.VerifyExistsInTargetScenario(WithUnitOfWork, this, TargetScenario);

			VerifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldOverwriteAShiftInTargetScenarioWhenSourceIsEmpty()
		{
			AddDefaultTypesToRepositories();

			var category = new ShiftCategory("Existing");
			var activity = new Activity("Existing");
			var assignment = new PersonAssignment(Person, TargetScenario, Period.StartDate);
			assignment.SetShiftCategory(category);
			assignment.AddActivity(activity, new TimePeriod(8, 15));
			WithUnitOfWork.Do(() =>
			{
				ShiftCategoryRepository.Add(category);
				ActivityRepository.Add(activity);
				ScheduleStorage.Add(assignment);
			});

			Target.Handle(createImportEvent());

			WithUnitOfWork.Do(() =>
			{
				var result = PersonAssignmentRepository.LoadAll().SingleOrDefault(x => x.BelongsToScenario(TargetScenario));
				result.Should().Be.Null();
			});

			VerifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldOverwriteAShiftInTargetScenarioWhenSourceHasOtherShift()
		{
			AddDefaultTypesToRepositories();

			var existingCategory = new ShiftCategory("Existing");
			var existingActivity = new Activity("Existing");
			var existingAssignment = new PersonAssignment(Person, TargetScenario, Period.StartDate);
			existingAssignment.SetShiftCategory(existingCategory);
			existingAssignment.AddActivity(existingActivity, new TimePeriod(8, 15));

			var newCategory = new ShiftCategory("Existing");
			var newActivity = new Activity("Existing");
			var newAssignment = new PersonAssignment(Person, SourceScenario, Period.StartDate);
			newAssignment.SetShiftCategory(newCategory);
			newAssignment.AddActivity(newActivity, new TimePeriod(8, 15));

			WithUnitOfWork.Do(() =>
			{
				ShiftCategoryRepository.Add(existingCategory);
				ActivityRepository.Add(existingActivity);
				ScheduleStorage.Add(existingAssignment);

				ShiftCategoryRepository.Add(newCategory);
				ActivityRepository.Add(newActivity);
				ScheduleStorage.Add(newAssignment);
			});

			Target.Handle(createImportEvent());

			WithUnitOfWork.Do(() =>
			{
				var result = PersonAssignmentRepository.LoadAll().SingleOrDefault(x => x.BelongsToScenario(TargetScenario));
				result.Should().Not.Be.Null();
			});

			VerifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldMergeAnAbsenceInTargetScenarioWhenSourceHasAssignment()
		{
			AddDefaultTypesToRepositories();

			var newCategory = new ShiftCategory("Existing");
			var newActivity = new Activity("Existing");
			var newAssignment = new PersonAssignment(Person, SourceScenario, Period.StartDate);
			newAssignment.SetShiftCategory(newCategory);
			newAssignment.AddActivity(newActivity, new TimePeriod(8, 15));

			var existingAbsence = AbsenceFactory.CreateAbsence("gone");
			var personAbsence = new PersonAbsence(Person, TargetScenario, new AbsenceLayer(existingAbsence,
					new DateTimePeriod(Period.StartDate.Date.ToUniversalTime(), (Period.StartDate.Date + TimeSpan.FromDays(1)).ToUniversalTime())));

			WithUnitOfWork.Do(() =>
			{
				ShiftCategoryRepository.Add(newCategory);
				ActivityRepository.Add(newActivity);
				ScheduleStorage.Add(newAssignment);

				AbsenceRepository.Add(existingAbsence);
				ScheduleStorage.Add(personAbsence);
			});

			Target.Handle(createImportEvent());

			WithUnitOfWork.Do(() =>
			{
				var assignment = PersonAssignmentRepository.LoadAll().SingleOrDefault(x => x.BelongsToScenario(TargetScenario));
				assignment.Should().Not.Be.Null();

				var absence = PersonAbsenceRepository.LoadAll().SingleOrDefault(x => x.BelongsToScenario(TargetScenario));
				absence.Should().Not.Be.Null();
				absence.Should().Be.EqualTo(personAbsence);
			});

			VerifyJobResultIsUpdated();
		}

		private static DateTime makeUtc(DateTime date)
		{
			return DateTime.SpecifyKind(date, DateTimeKind.Utc);
		}

		[Test]
		public void ShouldIgnoreTheSameAbsenceAsExistingInTarget()
		{
			AddDefaultTypesToRepositories();

			var existingAbsence = AbsenceFactory.CreateAbsence("gone");
			var existingPersonAbsence = new PersonAbsence(Person, TargetScenario, new AbsenceLayer(existingAbsence,
					new DateTimePeriod(makeUtc(Period.StartDate.Date), makeUtc(Period.StartDate.Date + TimeSpan.FromDays(1)))));
			var newPersonAbsence = new PersonAbsence(Person, SourceScenario, new AbsenceLayer(existingAbsence,
					new DateTimePeriod(makeUtc(Period.StartDate.Date), makeUtc(Period.StartDate.Date + TimeSpan.FromDays(1)))));


			WithUnitOfWork.Do(() =>
			{
				AbsenceRepository.Add(existingAbsence);
				ScheduleStorage.Add(existingPersonAbsence);
				ScheduleStorage.Add(newPersonAbsence);
			});

			Target.Handle(createImportEvent());

			WithUnitOfWork.Do(() =>
			{
				var absence = PersonAbsenceRepository.LoadAll().SingleOrDefault(x => x.BelongsToScenario(TargetScenario));
				absence.Should().Not.Be.Null();
				absence.Should().Be.EqualTo(existingPersonAbsence);
			});

			VerifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldAddAnAbsenceOfDifferentType()
		{
			AddDefaultTypesToRepositories();

			var existingAbsence = AbsenceFactory.CreateAbsence("gone");
			var newAbsence = AbsenceFactory.CreateAbsence("sick");
			var existingPersonAbsence = new PersonAbsence(Person, TargetScenario, new AbsenceLayer(existingAbsence,
					new DateTimePeriod(Period.StartDate.Date.ToUniversalTime(), (Period.StartDate.Date + TimeSpan.FromDays(1)).ToUniversalTime())));
			var newPersonAbsence = new PersonAbsence(Person, SourceScenario, new AbsenceLayer(newAbsence,
					new DateTimePeriod(Period.StartDate.Date.ToUniversalTime(), (Period.StartDate.Date + TimeSpan.FromDays(1)).ToUniversalTime())));


			WithUnitOfWork.Do(() =>
			{
				AbsenceRepository.Add(existingAbsence);
				AbsenceRepository.Add(newAbsence);
				ScheduleStorage.Add(existingPersonAbsence);
				ScheduleStorage.Add(newPersonAbsence);
			});

			Target.Handle(createImportEvent());

			WithUnitOfWork.Do(() =>
			{
				var absences = PersonAbsenceRepository.LoadAll().Where(x => x.BelongsToScenario(TargetScenario)).ToList();
				absences.Count.Should().Be.EqualTo(2);
				absences.Any(x => x.Layer.Payload.Name == existingAbsence.Name).Should().Be.True();
				absences.Any(x => x.Layer.Payload.Name == newAbsence.Name).Should().Be.True();
			});

			VerifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldAddAnAbsenceWithDifferentPeriod()
		{
			AddDefaultTypesToRepositories();

			var existingAbsence = AbsenceFactory.CreateAbsence("gone");
			var existingPersonAbsence = new PersonAbsence(Person, TargetScenario, new AbsenceLayer(existingAbsence,
					new DateTimePeriod(makeUtc(Period.StartDate.Date), makeUtc(Period.StartDate.Date + TimeSpan.FromDays(1)))));
			var newPersonAbsence = new PersonAbsence(Person, SourceScenario, new AbsenceLayer(existingAbsence,
					new DateTimePeriod(makeUtc(Period.StartDate.Date + TimeSpan.FromHours(1)), makeUtc(Period.StartDate.Date + TimeSpan.FromHours(2)))));

			WithUnitOfWork.Do(() =>
			{
				AbsenceRepository.Add(existingAbsence);
				ScheduleStorage.Add(existingPersonAbsence);
				ScheduleStorage.Add(newPersonAbsence);
			});

			Target.Handle(createImportEvent());

			WithUnitOfWork.Do(() =>
			{
				var absences = PersonAbsenceRepository.LoadAll().Where(x => x.BelongsToScenario(TargetScenario)).ToList();
				absences.Count.Should().Be.EqualTo(2);
				absences.Any(x => x.Period == existingPersonAbsence.Period).Should().Be.True();
				absences.Any(x => x.Period == newPersonAbsence.Period).Should().Be.True();
			});

			VerifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldUpdatePersonAccountIfNeeded()
		{
			var contract = new Contract("something");
			var partTimePercentage = new PartTimePercentage("something");
			var contractSchedule = new ContractSchedule("something");
			var site = new Site("test");
			var team = new Team { Site = site }.WithDescription(new Description("test"));

			WithUnitOfWork.Do(() =>
			{
				ContractRepository.Add(contract);
				PartTimePercentageRepository.Add(partTimePercentage);
				ContractScheduleRepository.Add(contractSchedule);
				SiteRepository.Add(site);
				TeamRepository.Add(team);
			});

			Person.AddPersonPeriod(new PersonPeriod(Period.StartDate, new PersonContract(contract, partTimePercentage, contractSchedule), team));
			AddDefaultTypesToRepositories();
			var existingAbsence = AbsenceFactory.CreateAbsence("gone");
			existingAbsence.Tracker = Tracker.CreateDayTracker();
			var absenceStartDate = makeUtc(Period.StartDate.Date + TimeSpan.FromDays(1));
			var newPersonAbsence = new PersonAbsence(Person, SourceScenario, new AbsenceLayer(existingAbsence,
					new DateTimePeriod(absenceStartDate, makeUtc(absenceStartDate + TimeSpan.FromHours(48)))));

			// Set up person account and link it to existingAbsence
			var personAbsenceAccount = new PersonAbsenceAccount(Person, existingAbsence);
			personAbsenceAccount.Add(new AccountDay(Period.StartDate) { Accrued = TimeSpan.FromDays(25) });

			WithUnitOfWork.Do(() =>
			{
				AbsenceRepository.Add(existingAbsence);
				ScheduleStorage.Add(newPersonAbsence);

				PersonAbsenceAccountRepository.Add(personAbsenceAccount);
			});

			Target.Handle(createImportEvent());

			WithUnitOfWork.Do(() =>
			{
				var absence = PersonAbsenceRepository.LoadAll().FirstOrDefault(x => x.BelongsToScenario(TargetScenario));
				absence.Should().Not.Be.Null();
				// Verify the persons account has been updated
				var accounts = PersonAbsenceAccountRepository.Find(Person);
				accounts.AllPersonAccounts().First().Remaining.Should().Be.EqualTo(TimeSpan.FromDays(23));
			});

			VerifyJobResultIsUpdated();
		}

		private ImportScheduleEvent createImportEvent(IPerson person = null)
		{
			return new ImportScheduleEvent((person ?? Person).Id.GetValueOrDefault())
			{
				EndDate = Period.EndDate,
				FromScenario = SourceScenario.Id.GetValueOrDefault(),
				StartDate = Period.StartDate,
				ToScenario = TargetScenario.Id.GetValueOrDefault(),
				JobResultId = JobResultId,
				LogOnBusinessUnitId = BusinessUnit.Id.GetValueOrDefault(),
				TotalMessages = 1,
				LogOnDatasource = InfraTestConfigReader.TenantName()
			};
		}

		private static readonly MoveTestCase<ImportScheduleHandlerTest>[] overwriteTestCases =
		{
			new MoveTestCase<ImportScheduleHandlerTest>(nameof(AgentDayScheduleTag))
			{
				CreateType = (testClass, person, period, sourceScenario, targetScenario) =>
				{
					var scheduleTag = new ScheduleTag { Description = "Something" };
					testClass.ScheduleTagRepository.Add(scheduleTag);
					testClass.ScheduleStorage.Add(new AgentDayScheduleTag(person, new DateOnly(period.StartDate.Date + TimeSpan.FromDays(1)), sourceScenario, scheduleTag));
					testClass.ScheduleStorage.Add(new AgentDayScheduleTag(person, new DateOnly(period.StartDate.Date + TimeSpan.FromDays(1)), targetScenario, scheduleTag));
				},
				LoadMethod = (testClass, targetScenario) => testClass.AgentDayScheduleTagRepository.LoadAll().SingleOrDefault(x => x.Scenario.Equals(targetScenario))
			},
			new MoveTestCase<ImportScheduleHandlerTest>(nameof(PersonAssignment))
			{
				CreateType = (testClass, person, period, sourceScenario, targetScenario) =>
				{
					testClass.ScheduleStorage.Add(new PersonAssignment(person, sourceScenario, period.StartDate));
					testClass.ScheduleStorage.Add(new PersonAssignment(person, targetScenario, period.StartDate));
				},
				LoadMethod = (testClass, targetScenario) => testClass.PersonAssignmentRepository.LoadAll().SingleOrDefault(x => x.Scenario.Equals(targetScenario))
			},
			new MoveTestCase<ImportScheduleHandlerTest>(nameof(PublicNote))
			{
				CreateType = (testClass, person, period, sourceScenario, targetScenario) =>
				{
					testClass.ScheduleStorage.Add(new PublicNote(person, period.StartDate, sourceScenario, "Test"));
					testClass.ScheduleStorage.Add(new PublicNote(person, period.StartDate, targetScenario, "Test"));
				},
				LoadMethod = (testClass, targetScenario) => testClass.PublicNoteRepository.LoadAll().SingleOrDefault(x => x.Scenario.Equals(targetScenario))
			},
			new MoveTestCase<ImportScheduleHandlerTest>(nameof(Note))
			{
				CreateType = (testClass, person, period, sourceScenario, targetScenario) =>
				{
					testClass.ScheduleStorage.Add(new Note(person, period.StartDate, sourceScenario, "Test"));
					testClass.ScheduleStorage.Add(new Note(person, period.StartDate, targetScenario, "Test"));
				},
				LoadMethod = (testClass, targetScenario) => testClass.NoteRepository.LoadAll().SingleOrDefault(x => x.Scenario.Equals(targetScenario))
			}
		};

		private static readonly MoveTestCase<ImportScheduleHandlerTest>[] moveTypeTestCases =
		{
			new MoveTestCase<ImportScheduleHandlerTest>(nameof(AgentDayScheduleTag))
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
			new MoveTestCase<ImportScheduleHandlerTest>(nameof(PersonAssignment))
			{
				CreateType = (testClass, person, period, sourceScenario, targetScenario) => testClass.ScheduleStorage.Add(new PersonAssignment(person, sourceScenario, period.StartDate)),
				LoadMethod = (testClass, targetScenario) => testClass.PersonAssignmentRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(targetScenario))
			},
			new MoveTestCase<ImportScheduleHandlerTest>(nameof(PublicNote))
			{
				CreateType = (testClass, person, period, sourceScenario, targetScenario) => testClass.ScheduleStorage.Add( new PublicNote(person, period.StartDate, sourceScenario, "Test")),
				LoadMethod = (testClass, targetScenario) => testClass.PublicNoteRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(targetScenario))
			},
			new MoveTestCase<ImportScheduleHandlerTest>(nameof(PersonAbsence))
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
			new MoveTestCase<ImportScheduleHandlerTest>(nameof(Note))
			{
				CreateType = (testClass, person, period, sourceScenario, targetScenario) => testClass.ScheduleStorage.Add(new Note(person, period.StartDate, sourceScenario, "Test")),
				LoadMethod = (testClass, targetScenario) => testClass.NoteRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(targetScenario))
			}
		};
	}
}