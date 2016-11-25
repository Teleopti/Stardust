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
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Archiving
{
	[Category("BucketB")]
	[Toggle(Toggles.Wfm_ImportSchedule_41247)]
	[DatabaseTest]
	public class ImportScheduleHandlerTest : ScheduleManagementTestBase
	{
		public ImportScheduleHandler Target;

		[SetUp]
		public void Setup()
		{
			BusinessUnit = BusinessUnitFactory.BusinessUnitUsedInTest;
			SourceScenario = new Scenario("source");
			TargetScenario = new Scenario("default") { DefaultScenario = true };
			Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 5);
			Person = PersonFactory.CreatePerson("Tester Testersson");
		}

		[Test]
		public void ShouldMoveOneOfType([ValueSource(nameof(moveTypeTestCases))] MoveTestCase<ImportScheduleHandlerTest> testCase)
		{
			AddDefaultTypesToRepositories();
			testCase.CreateTypeInSourceScenario(WithUnitOfWork, this, Person, Period, SourceScenario);

			WithUnitOfWork.Do(() => Target.Handle(createImportEvent()));

			VerifyCanBeFoundInScheduleStorageForTargetScenario(Person);

			testCase.VerifyExistsInTargetScenario(WithUnitOfWork, this, TargetScenario);

			VerifyJobResultIsUpdated();
		}

		[Test]
		public void ShouldNotOverwriteAShiftInTargetScenarioWhenSourceIsEmpty()
		{
			AddDefaultTypesToRepositories();

			var oldCategory = new ShiftCategory("Existing");
			var oldActivity = new Activity("Existing");
			var assignment = new PersonAssignment(Person, TargetScenario, Period.StartDate);
			assignment.SetShiftCategory(oldCategory);
			assignment.AddActivity(oldActivity, new TimePeriod(8, 15));
			WithUnitOfWork.Do(() =>
			{
				ShiftCategoryRepository.Add(oldCategory);
				ActivityRepository.Add(oldActivity);
				ScheduleStorage.Add(assignment);
			});

			WithUnitOfWork.Do(() => Target.Handle(createImportEvent()));

			WithUnitOfWork.Do(() =>
			{
				var result = PersonAssignmentRepository.LoadAll().FirstOrDefault(x => x.BelongsToScenario(TargetScenario));
				result.Should().Not.Be.Null();
				result.Should().Be.EqualTo(assignment);
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
				TotalMessages = 1
			};
		}

		private static readonly MoveTestCase<ImportScheduleHandlerTest>[] moveTypeTestCases =
		{
			new MoveTestCase<ImportScheduleHandlerTest>(nameof(AgentDayScheduleTag))
			{
				CreateType = (testClass, person, period, sourceScenario) =>
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
				CreateType = (testClass, person, period, sourceScenario) => testClass.ScheduleStorage.Add(new PersonAssignment(person, sourceScenario, period.StartDate)),
				LoadMethod = (testClass, targetScenario) => testClass.PersonAssignmentRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(targetScenario))
			},
			new MoveTestCase<ImportScheduleHandlerTest>(nameof(PublicNote))
			{
				CreateType = (testClass, person, period, sourceScenario) => testClass.ScheduleStorage.Add( new PublicNote(person, period.StartDate, sourceScenario, "Test")),
				LoadMethod = (testClass, targetScenario) => testClass.PublicNoteRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(targetScenario))
			},
			new MoveTestCase<ImportScheduleHandlerTest>(nameof(PersonAbsence))
			{
				CreateType = (testClass, person, period, sourceScenario) =>
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
				CreateType = (testClass, person, period, sourceScenario) => testClass.ScheduleStorage.Add(new Note(person, period.StartDate, sourceScenario, "Test")),
				LoadMethod = (testClass, targetScenario) => testClass.NoteRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(targetScenario))
			}
		};
	}
}