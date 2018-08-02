using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
	[TestFixture]
	public class BudgetGroupAllowanceValidatorTest
	{
		private IAbsenceRequestValidator _target;

		[SetUp]
		public void Setup()
		{
			_target = new BudgetGroupAllowanceValidator();
		}

		[Test]
		public void ShouldHaveDisplayText()
		{
			Assert.AreEqual(UserTexts.Resources.BudgetGroup, _target.DisplayText);
		}

		[Test]
		public void ShouldBeValidIfEnoughAllowanceLeft()
		{
			var specification = MockRepository.GenerateStrictMock<IBudgetGroupAllowanceSpecification>();
			var absenceRequest = MockRepository.GenerateStrictMock<IAbsenceRequest>();
			specification.Stub(x => x.IsSatisfied(new AbsenceRequstAndSchedules())).IgnoreArguments().Return(new ValidatedRequest { IsValid = true, ValidationErrors = string.Empty });
			var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(null, null, null, specification, null));
			Assert.IsTrue(result.IsValid);
		}

		[Test]
		public void ShouldBeInvalidIfNotEnoughAllowanceLeft()
		{
			var specification = MockRepository.GenerateStrictMock<IBudgetGroupAllowanceSpecification>();
			var absenceRequest = MockRepository.GenerateStrictMock<IAbsenceRequest>();
			specification.Stub(x => x.IsSatisfied(new AbsenceRequstAndSchedules())).IgnoreArguments().Return(new ValidatedRequest { IsValid = false, ValidationErrors = string.Empty });
			var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(null, null, null, specification, null));
			Assert.IsFalse(result.IsValid);
		}
		

		[Test]
		public void ShouldCreateNewInstance()
		{
			var newInstance = _target.CreateInstance();
			Assert.AreNotSame(_target, newInstance);
			Assert.IsTrue(typeof(BudgetGroupAllowanceValidator).IsInstanceOfType(newInstance));
		}

		[Test]
		public void ShouldAllInstancesBeEqual()
		{
			var otherValidatorOfSameKind = new BudgetGroupAllowanceValidator();
			Assert.IsTrue(otherValidatorOfSameKind.Equals(_target));
		}

		[Test]
		public void ShouldNotEqualIfTheyAreInstancesOfDifferentType()
		{
			var otherValidator = new AbsenceRequestNoneValidator();
			Assert.IsFalse(_target.Equals(otherValidator));
		}

		[Test]
		public void GetHashCodeCorrectly()
		{
			var result = _target.GetHashCode();
			Assert.IsNotNull(result);
		}
	}

	[TestFixture]
	[DomainTest, Ignore("WIP")]
	public class BudgetGroupAllowanceValidatorNoMockTest :  IIsolateSystem
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
		public void ShouldRun()
		{
			var absence = AbsenceFactory.CreateAbsence("holiday");
			//var person = PersonFactory.CreatePersonWithId();
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Add(scenario);
			var budgetGroup = getBudgetGroup();
			BudgetGroupRepository.Add(budgetGroup);

			var budgetDayOne = new BudgetDay(budgetGroup, scenario, new DateOnly(2018,08,15))
			{
				FulltimeEquivalentHours = 8d,
				ShrinkedAllowance = 1,
				AbsenceOverride = 1d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			BudgetDayRepository.Add(budgetDayOne);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2018, 08, 15));
			personPeriod.BudgetGroup = budgetGroup;
			//personPeriod.PersonContract = PersonContractFactory.CreatePersonContract();
			//personPeriod.PersonContract.ContractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			//personPeriod.PersonContract.ContractSchedule.ContractScheduleWeeks.First().Add(DayOfWeek.Monday, false);

			absence.InContractTime = true;
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
			//PersonAssignmentRepository.Has(assignment);
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2018, 08, 15, 12, 2018, 08, 17, 13));
			scheduleDictionary.AddPersonAssignment(assignment);
			SchedulingResultStateHolder.Schedules = scheduleDictionary;
			var requiredForHandlingAbsenceRequest = new RequiredForHandlingAbsenceRequest(SchedulingResultStateHolder, null,
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
