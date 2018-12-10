using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
	[TestFixture]
	[DomainTest]
	public class BudgetGroupAllowanceValidatorTest :  IIsolateSystem
	{

		public IAbsenceRequestValidator Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public ISchedulingResultStateHolder SchedulingResultStateHolder;
		public IResourceCalculation ResourceCalculation;
		public IBudgetGroupAllowanceSpecification BudgetGroupAllowanceSpecification;
		public FakeBudgetDayRepository BudgetDayRepository;
		public FakeBudgetGroupRepository BudgetGroupRepository;
		public FakeScenarioRepository ScenarioRepository;

		[Test]
		public void ShouldUseWholeShiftWhileCalculatingBudget()
		{
			var absence = AbsenceFactory.CreateAbsence("holiday");
			absence.InContractTime = true;
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Add(scenario);
			var budgetGroup = getBudgetGroup();
			BudgetGroupRepository.Add(budgetGroup);

			var budgetDayOne = new BudgetDay(budgetGroup, scenario, new DateOnly(2018,08,15))
			{
				FulltimeEquivalentHours = 8d,
				ShrinkedAllowance = 1,
				AbsenceOverride = 0d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			BudgetDayRepository.Add(budgetDayOne);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2018, 08, 15));
			personPeriod.BudgetGroup = budgetGroup;

			
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator(),
				Period = new DateOnlyPeriod(2018, 08, 1, 2018,08, 31),
				OpenForRequestsPeriod = new DateOnlyPeriod(2018, 08, 1, 2018, 08, 31),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var personOne = PersonFactory.CreatePerson(wfcs).WithId();
			personOne.AddPersonPeriod(personPeriod);

			var absenceRequest = new AbsenceRequest(absence,
				new DateTimePeriod(new DateTime(2018, 08, 15, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 08, 15, 23, 59, 0, DateTimeKind.Utc)));
			var personRequest = new PersonRequest(personOne, absenceRequest).WithId();
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(personOne, scenario, new DateTimePeriod(new DateTime(2018, 08, 15, 22, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 08, 16, 8, 0, 0, DateTimeKind.Utc)));
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2018, 08, 15, 12, 2018, 08, 17, 13));
			scheduleDictionary.AddPersonAssignment(assignment);
			SchedulingResultStateHolder.Schedules = scheduleDictionary;
			var requiredForHandlingAbsenceRequest = new RequiredForHandlingAbsenceRequest(SchedulingResultStateHolder, new BudgetGroupState(), null,
				ResourceCalculation, BudgetGroupAllowanceSpecification);
			var result = Target.Validate(absenceRequest, requiredForHandlingAbsenceRequest);
			result.IsValid.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldUseWholeShiftWhileCalculatingBudgetForMultipleDayAbsence()
		{
			var absence = AbsenceFactory.CreateAbsence("holiday");
			absence.InContractTime = true;
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Add(scenario);
			var budgetGroup = getBudgetGroup();
			BudgetGroupRepository.Add(budgetGroup);

			var budgetDayOne = new BudgetDay(budgetGroup, scenario, new DateOnly(2018, 08, 15))
			{
				FulltimeEquivalentHours = 8d,
				ShrinkedAllowance = 1,
				AbsenceOverride = 0d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			var budgetDaytwo = new BudgetDay(budgetGroup, scenario, new DateOnly(2018, 08, 16))
			{
				FulltimeEquivalentHours = 8d,
				ShrinkedAllowance = 1,
				AbsenceOverride = 0d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			var budgetDaythree = new BudgetDay(budgetGroup, scenario, new DateOnly(2018, 08, 17))
			{
				FulltimeEquivalentHours = 8d,
				ShrinkedAllowance = 1,
				AbsenceOverride = 0d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			BudgetDayRepository.Add(budgetDayOne);
			BudgetDayRepository.Add(budgetDaytwo);
			BudgetDayRepository.Add(budgetDaythree);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2018, 08, 15));
			personPeriod.BudgetGroup = budgetGroup;


			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator(),
				Period = new DateOnlyPeriod(2018, 08, 1, 2018, 08, 31),
				OpenForRequestsPeriod = new DateOnlyPeriod(2018, 08, 1, 2018, 08, 31),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var personOne = PersonFactory.CreatePerson(wfcs).WithId();
			personOne.AddPersonPeriod(personPeriod);

			var absenceRequest = new AbsenceRequest(absence,
				new DateTimePeriod(new DateTime(2018, 08, 15, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 08, 16, 23, 59, 0, DateTimeKind.Utc)));
			new PersonRequest(personOne, absenceRequest).WithId();
			var assignmentShort = PersonAssignmentFactory.CreateAssignmentWithMainShift(personOne, scenario, new DateTimePeriod(new DateTime(2018, 08, 15, 22, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 08, 16, 6, 0, 0, DateTimeKind.Utc)));
			var assignmentLong = PersonAssignmentFactory.CreateAssignmentWithMainShift(personOne, scenario, new DateTimePeriod(new DateTime(2018, 08, 16, 22, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 08, 17, 8, 0, 0, DateTimeKind.Utc)));
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2018, 08, 15, 12, 2018, 08, 17, 13));

			scheduleDictionary.AddPersonAssignment(assignmentShort);
			scheduleDictionary.AddPersonAssignment(assignmentLong);

			SchedulingResultStateHolder.Schedules = scheduleDictionary;
			var requiredForHandlingAbsenceRequest = new RequiredForHandlingAbsenceRequest(SchedulingResultStateHolder, new BudgetGroupState(), null,
				ResourceCalculation, BudgetGroupAllowanceSpecification);
			var result = Target.Validate(absenceRequest, requiredForHandlingAbsenceRequest);
			result.IsValid.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShoulCaluclateBudgetWhenAbsenceIsOnNextDay()
		{
			var absence = AbsenceFactory.CreateAbsence("holiday");
			absence.InContractTime = true;
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Add(scenario);
			var budgetGroup = getBudgetGroup();
			BudgetGroupRepository.Add(budgetGroup);

			var budgetDayOne = new BudgetDay(budgetGroup, scenario, new DateOnly(2018, 08, 15))
			{
				FulltimeEquivalentHours = 8d,
				ShrinkedAllowance = 0,
				AbsenceOverride = 0d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};

			var budgetDayTwo = new BudgetDay(budgetGroup, scenario, new DateOnly(2018, 08, 16))
			{
				FulltimeEquivalentHours = 8d,
				ShrinkedAllowance = 1,
				AbsenceOverride = 0d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			BudgetDayRepository.Add(budgetDayOne);
			BudgetDayRepository.Add(budgetDayTwo);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2018, 08, 15));
			personPeriod.BudgetGroup = budgetGroup;


			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator(),
				Period = new DateOnlyPeriod(2018, 08, 1, 2018, 08, 31),
				OpenForRequestsPeriod = new DateOnlyPeriod(2018, 08, 1, 2018, 08, 31),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var personOne = PersonFactory.CreatePerson(wfcs).WithId();
			personOne.AddPersonPeriod(personPeriod);

			var absenceRequest = new AbsenceRequest(absence,
				new DateTimePeriod(new DateTime(2018, 08, 16, 2, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 08, 16, 4, 0, 0, DateTimeKind.Utc)));
			new PersonRequest(personOne, absenceRequest).WithId();
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(personOne, scenario, new DateTimePeriod(new DateTime(2018, 08, 15, 22, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 08, 16, 8, 0, 0, DateTimeKind.Utc)));
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2018, 08, 15, 12, 2018, 08, 17, 13));
			scheduleDictionary.AddPersonAssignment(assignment);
			SchedulingResultStateHolder.Schedules = scheduleDictionary;
			var requiredForHandlingAbsenceRequest = new RequiredForHandlingAbsenceRequest(SchedulingResultStateHolder, new BudgetGroupState(),  null,
				ResourceCalculation, BudgetGroupAllowanceSpecification);
			var result = Target.Validate(absenceRequest, requiredForHandlingAbsenceRequest);
			result.IsValid.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldBeValidIfReuqestWithinBudget()
		{
			var absence = AbsenceFactory.CreateAbsence("holiday");
			absence.InContractTime = true;
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Add(scenario);
			var budgetGroup = getBudgetGroup();
			BudgetGroupRepository.Add(budgetGroup);

			var budgetDayOne = new BudgetDay(budgetGroup, scenario, new DateOnly(2018, 08, 16))
			{
				FulltimeEquivalentHours = 8d,
				ShrinkedAllowance = 1,
				AbsenceOverride = 0d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			BudgetDayRepository.Add(budgetDayOne);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2018, 08, 16));
			personPeriod.BudgetGroup = budgetGroup;


			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator(),
				Period = new DateOnlyPeriod(2018, 08, 1, 2018, 08, 31),
				OpenForRequestsPeriod = new DateOnlyPeriod(2018, 08, 1, 2018, 08, 31),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var personOne = PersonFactory.CreatePerson(wfcs).WithId();
			personOne.AddPersonPeriod(personPeriod);

			var absenceRequest = new AbsenceRequest(absence,
				new DateTimePeriod(new DateTime(2018, 08, 16, 4, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 08, 16, 6, 0, 0, DateTimeKind.Utc)));
			new PersonRequest(personOne, absenceRequest).WithId();
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(personOne, scenario, new DateTimePeriod(new DateTime(2018, 08, 16, 1, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 08, 16, 8, 0, 0, DateTimeKind.Utc)));
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2018, 08, 15, 12, 2018, 08, 17, 13));
			scheduleDictionary.AddPersonAssignment(assignment);
			SchedulingResultStateHolder.Schedules = scheduleDictionary;
			var requiredForHandlingAbsenceRequest = new RequiredForHandlingAbsenceRequest(SchedulingResultStateHolder, new BudgetGroupState(), null,
				ResourceCalculation, BudgetGroupAllowanceSpecification);
			var result = Target.Validate(absenceRequest, requiredForHandlingAbsenceRequest);
			result.IsValid.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldApproveIfAbsenceIsOutsideScheduledHours()
		{
			var absence = AbsenceFactory.CreateAbsence("holiday");
			absence.InContractTime = true;
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Add(scenario);
			var budgetGroup = getBudgetGroup();
			BudgetGroupRepository.Add(budgetGroup);

			var budgetDayOne = new BudgetDay(budgetGroup, scenario, new DateOnly(2018, 08, 15))
			{
				FulltimeEquivalentHours = 8d,
				ShrinkedAllowance = 0,
				AbsenceOverride = 0d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			var budgetDayTwo = new BudgetDay(budgetGroup, scenario, new DateOnly(2018, 08, 16))
			{
				FulltimeEquivalentHours = 8d,
				ShrinkedAllowance = 0,
				AbsenceOverride = 0d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			BudgetDayRepository.Add(budgetDayOne);
			BudgetDayRepository.Add(budgetDayTwo);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2018, 08, 15));
			personPeriod.BudgetGroup = budgetGroup;


			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator(),
				Period = new DateOnlyPeriod(2018, 08, 1, 2018, 08, 31),
				OpenForRequestsPeriod = new DateOnlyPeriod(2018, 08, 1, 2018, 08, 31),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var personOne = PersonFactory.CreatePerson(wfcs).WithId();
			personOne.AddPersonPeriod(personPeriod);

			var absenceRequest = new AbsenceRequest(absence,
				new DateTimePeriod(new DateTime(2018, 08, 16, 4, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 08, 16, 6, 0, 0, DateTimeKind.Utc)));
			new PersonRequest(personOne, absenceRequest).WithId();
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(personOne, scenario, new DateTimePeriod(new DateTime(2018, 08, 15, 1, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 08, 15, 8, 0, 0, DateTimeKind.Utc)));

			var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(personOne, scenario, new DateTimePeriod(new DateTime(2018, 08, 16, 22, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 08, 17, 8, 0, 0, DateTimeKind.Utc)));
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2018, 08, 15, 12, 2018, 08, 18, 13));
			scheduleDictionary.AddPersonAssignment(assignment);
			scheduleDictionary.AddPersonAssignment(assignment2);
			SchedulingResultStateHolder.Schedules = scheduleDictionary;
			var requiredForHandlingAbsenceRequest = new RequiredForHandlingAbsenceRequest(SchedulingResultStateHolder, new BudgetGroupState(), null,
				ResourceCalculation, BudgetGroupAllowanceSpecification);
			var result = Target.Validate(absenceRequest, requiredForHandlingAbsenceRequest);
			result.IsValid.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldValidateRequestOnMidnightWithEmptyAssignmentNextDay()
		{
			var absence = AbsenceFactory.CreateAbsence("holiday");
			absence.InContractTime = true;
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Add(scenario);
			var budgetGroup = getBudgetGroup();
			BudgetGroupRepository.Add(budgetGroup);

			var budgetDayOne = new BudgetDay(budgetGroup, scenario, new DateOnly(2018, 08, 15))
			{
				FulltimeEquivalentHours = 1,
				ShrinkedAllowance = 2,
				AbsenceOverride = 0d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			var budgetDayTwo = new BudgetDay(budgetGroup, scenario, new DateOnly(2018, 08, 16))
			{
				FulltimeEquivalentHours = 1,
				ShrinkedAllowance = 1,
				AbsenceOverride = 0d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			BudgetDayRepository.Add(budgetDayOne);
			BudgetDayRepository.Add(budgetDayTwo);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2018, 08, 15));
			personPeriod.BudgetGroup = budgetGroup;


			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator(),
				Period = new DateOnlyPeriod(2018, 08, 1, 2018, 08, 31),
				OpenForRequestsPeriod = new DateOnlyPeriod(2018, 08, 1, 2018, 08, 31),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var personOne = PersonFactory.CreatePerson(wfcs).WithId();
			personOne.AddPersonPeriod(personPeriod);

			var absenceRequest = new AbsenceRequest(absence,
				new DateTimePeriod(new DateTime(2018, 08, 15, 23, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 08, 16,1, 0, 0, DateTimeKind.Utc)));
			new PersonRequest(personOne, absenceRequest).WithId();
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(personOne, scenario, new DateTimePeriod(new DateTime(2018, 08, 15, 22, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 08, 16, 8, 0, 0, DateTimeKind.Utc)));

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2018, 08, 15, 12, 2018, 08, 18, 13));
			scheduleDictionary.AddPersonAssignment(assignment);
			SchedulingResultStateHolder.Schedules = scheduleDictionary;
			var requiredForHandlingAbsenceRequest = new RequiredForHandlingAbsenceRequest(SchedulingResultStateHolder, new BudgetGroupState(), null,
				ResourceCalculation, BudgetGroupAllowanceSpecification);
			var result = Target.Validate(absenceRequest, requiredForHandlingAbsenceRequest);
			result.IsValid.Should().Be.EqualTo(true);
		}
		[Test]
		public void ShouldValidateRequestOnMidnightWithAssignmentNextDay()
		{
			var absence = AbsenceFactory.CreateAbsence("holiday");
			absence.InContractTime = true;
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Add(scenario);
			var budgetGroup = getBudgetGroup();
			BudgetGroupRepository.Add(budgetGroup);

			var budgetDayOne = new BudgetDay(budgetGroup, scenario, new DateOnly(2018, 08, 15))
			{
				FulltimeEquivalentHours = 1,
				ShrinkedAllowance = 2,
				AbsenceOverride = 0d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			var budgetDayTwo = new BudgetDay(budgetGroup, scenario, new DateOnly(2018, 08, 16))
			{
				FulltimeEquivalentHours = 1,
				ShrinkedAllowance = 1,
				AbsenceOverride = 0d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			BudgetDayRepository.Add(budgetDayOne);
			BudgetDayRepository.Add(budgetDayTwo);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2018, 08, 15));
			personPeriod.BudgetGroup = budgetGroup;


			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator(),
				Period = new DateOnlyPeriod(2018, 08, 1, 2018, 08, 31),
				OpenForRequestsPeriod = new DateOnlyPeriod(2018, 08, 1, 2018, 08, 31),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var personOne = PersonFactory.CreatePerson(wfcs).WithId();
			personOne.AddPersonPeriod(personPeriod);

			var absenceRequest = new AbsenceRequest(absence,
				new DateTimePeriod(new DateTime(2018, 08, 15, 23, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 08, 16, 1, 0, 0, DateTimeKind.Utc)));
			new PersonRequest(personOne, absenceRequest).WithId();
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(personOne, scenario, new DateTimePeriod(new DateTime(2018, 08, 15, 22, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 08, 16, 8, 0, 0, DateTimeKind.Utc)));

			var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(personOne, scenario, new DateTimePeriod(new DateTime(2018, 08, 16, 22, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 08, 17, 8, 0, 0, DateTimeKind.Utc)));
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2018, 08, 15, 12, 2018, 08, 18, 13));
			scheduleDictionary.AddPersonAssignment(assignment);
			scheduleDictionary.AddPersonAssignment(assignment2);
			SchedulingResultStateHolder.Schedules = scheduleDictionary;
			var requiredForHandlingAbsenceRequest = new RequiredForHandlingAbsenceRequest(SchedulingResultStateHolder, new BudgetGroupState(), null,
				ResourceCalculation, BudgetGroupAllowanceSpecification);
			var result = Target.Validate(absenceRequest, requiredForHandlingAbsenceRequest);
			result.IsValid.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldUseWholeShiftWhileCalculatingBudgetAndUseCorrectTimezone()
		{
			var absence = AbsenceFactory.CreateAbsence("holiday");
			absence.InContractTime = true;
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Add(scenario);
			var budgetGroup = getBudgetGroup();
			BudgetGroupRepository.Add(budgetGroup);

			var budgetDayOne = new BudgetDay(budgetGroup, scenario, new DateOnly(2018, 08, 15))
			{
				FulltimeEquivalentHours = 8d,
				ShrinkedAllowance = 1,
				AbsenceOverride = 0d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			//should need this
			var budgetDayTwo = new BudgetDay(budgetGroup, scenario, new DateOnly(2018, 08, 16))
			{
				FulltimeEquivalentHours = 8d,
				ShrinkedAllowance = 1,
				AbsenceOverride = 0d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			BudgetDayRepository.Add(budgetDayOne);
			BudgetDayRepository.Add(budgetDayTwo);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2018, 08, 15));
			personPeriod.BudgetGroup = budgetGroup;


			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator(),
				Period = new DateOnlyPeriod(2018, 08, 1, 2018, 08, 31),
				OpenForRequestsPeriod = new DateOnlyPeriod(2018, 08, 1, 2018, 08, 31),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var personOne = PersonFactory.CreatePerson(wfcs).WithId();
			personOne.AddPersonPeriod(personPeriod);
			personOne.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

			var absenceRequest = new AbsenceRequest(absence,
				new DateTimePeriod(new DateTime(2018, 08, 14, 22, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 08, 15, 21, 59, 0, DateTimeKind.Utc)));
			new PersonRequest(personOne, absenceRequest).WithId();
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(personOne, scenario, new DateTimePeriod(new DateTime(2018, 08, 15, 20, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 08, 16, 6, 0, 0, DateTimeKind.Utc)));
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2018, 08, 15, 12, 2018, 08, 17, 13));
			scheduleDictionary.AddPersonAssignment(assignment);
			SchedulingResultStateHolder.Schedules = scheduleDictionary;
			var requiredForHandlingAbsenceRequest = new RequiredForHandlingAbsenceRequest(SchedulingResultStateHolder, new BudgetGroupState(), null,
				ResourceCalculation, BudgetGroupAllowanceSpecification);
			var result = Target.Validate(absenceRequest, requiredForHandlingAbsenceRequest);
			result.IsValid.Should().Be.EqualTo(false);
		}

		private static IBudgetGroup getBudgetGroup()
		{
			var budgetGroup = new BudgetGroup { Name = "BG1" };
			budgetGroup.SetId(Guid.NewGuid());
			return budgetGroup;
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<BudgetGroupAllowanceValidator>().For<IAbsenceRequestValidator>();
		}
	}
}
