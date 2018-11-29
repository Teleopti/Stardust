using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class AllowanceProviderTest
	{
		private Scenario _scenario;
		private IScenarioRepository _scenarioRepository;
		private ILoggedOnUser _loggedOnUser;
		private IPerson _user;
		private DateOnlyPeriod _alwaysOpenPeriod;
		private INow _now;

		[SetUp]
		public void Setup()
		{
			_scenario = new Scenario("default");
			_scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			_scenarioRepository.Expect(s => s.LoadDefaultScenario()).Repeat.Any().Return(_scenario);

			_user = PersonFactory.CreatePerson("some person");
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_loggedOnUser.Expect(l => l.CurrentUser()).Repeat.Any().Return(_user);
			_alwaysOpenPeriod = new DateOnlyPeriod(new DateOnly(1900, 1, 1), new DateOnly(2040, 1, 1));
			_now = new MutableNow(new DateTime(2013, 5, 2));
		}

		[Test]
		public void WhenNoBudgetDays_ShouldSetAllowanceToZeroOnEverydayInThePeriod()
		{
			addWorkFlowControlSetWithOpenAbsencePeriod(_alwaysOpenPeriod, _alwaysOpenPeriod);
			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();

			var period = new DateOnlyPeriod(new DateOnly(2001, 1, 1), new DateOnly(2001, 1, 10));
			budgetDayRepository.Stub(x => x.Find(_scenario, null, period)).Return(new List<IBudgetDay>());

			var target = new AllowanceProvider(budgetDayRepository, _loggedOnUser, _scenarioRepository,
				new ExtractBudgetGroupPeriods(), _now);
			var result = target.GetAllowanceForPeriod(period).ToList();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);
			verifyAllAvailability(result, true);
			allShouldBeEmptyAllowanceDay(result);
		}

		[Test]
		public void WhenBudgetDayExistWithinThePeriod_ShouldSetTheAllowanceInMinutesForThatDate()
		{
			addWorkFlowControlSetWithOpenAbsencePeriod(_alwaysOpenPeriod, _alwaysOpenPeriod);
			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();

			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate);

			var budgetGroup = new BudgetGroup();
			personPeriod1.BudgetGroup = budgetGroup;

			var budgetDays = new List<IBudgetDay>();
			var allowance = TimeSpan.FromHours(40);

			var budgetDay = createBudgetDayWithAllowance(budgetGroup, period.StartDate, allowance.TotalHours);
			budgetDays.Add(budgetDay);

			_user.AddPersonPeriod(personPeriod1);
			budgetDayRepository.Stub(x => x.Find(_scenario, budgetGroup, period)).Return(budgetDays);

			var target = new AllowanceProvider(budgetDayRepository, _loggedOnUser, _scenarioRepository,
				new ExtractBudgetGroupPeriods(), _now);
			var result = target.GetAllowanceForPeriod(period).ToList();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);

			var allowanceDay = result.First();
			Assert.AreEqual(allowance, allowanceDay.Time);
			Assert.AreEqual(TimeSpan.FromHours(budgetDay.FulltimeEquivalentHours), allowanceDay.Heads);
			Assert.AreEqual(allowance.TotalHours, allowanceDay.AllowanceHeads);
			Assert.AreEqual(true, allowanceDay.Availability);
			Assert.AreEqual(false, allowanceDay.UseHeadCount);
			Assert.AreEqual(true, allowanceDay.ValidateBudgetGroup);
		}

		[Test]
		public void WhenTodayIsOutsideOpenPeriod_ShouldSetTheAllowanceToZero()
		{
			var closedPeriod = new DateOnlyPeriod(new DateOnly(2000, 1, 1), new DateOnly(2005, 1, 1));
			addWorkFlowControlSetWithOpenAbsencePeriod(closedPeriod, _alwaysOpenPeriod);
			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();

			var period = new DateOnlyPeriod(new DateOnly(2001, 3, 1), new DateOnly(2001, 3, 8));
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate);

			var budgetGroup = new BudgetGroup();
			personPeriod1.BudgetGroup = budgetGroup;

			var budgetDays = new List<IBudgetDay>();
			var allowance = TimeSpan.FromHours(40);

			var budgetDay = createBudgetDayWithAllowance(budgetGroup, period.StartDate, allowance.TotalHours);
			budgetDays.Add(budgetDay);

			_user.AddPersonPeriod(personPeriod1);
			budgetDayRepository.Stub(x => x.Find(_scenario, budgetGroup, period)).Return(budgetDays);

			var target = new AllowanceProvider(budgetDayRepository, _loggedOnUser, _scenarioRepository,
				new ExtractBudgetGroupPeriods(), new Now());

			var result = target.GetAllowanceForPeriod(period).ToList();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);
			verifyAllAvailability(result, false);
			allShouldBeEmptyAllowanceDay(result);
		}

		[Test]
		public void WhenAbsenceDayIsOutsidePreferencePeriod_ShouldSetTheAllowanceToZero()
		{
			var startDate = new DateOnly(2001, 1, 1);
			var endDate = new DateOnly(2001, 1, 10);

			var openPeriod = new DateOnlyPeriod(new DateOnly(1999, 5, 5), new DateOnly(1999, 7, 5));
			addWorkFlowControlSetWithOpenAbsencePeriod(_alwaysOpenPeriod, openPeriod);

			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();

			var period = new DateOnlyPeriod(startDate, endDate);
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate);

			var budgetGroup = new BudgetGroup();
			personPeriod1.BudgetGroup = budgetGroup;

			var budgetDays = new List<IBudgetDay>();
			var allowance = TimeSpan.FromHours(40);

			var budgetDay = createBudgetDayWithAllowance(budgetGroup, period.StartDate, allowance.TotalHours);
			budgetDays.Add(budgetDay);

			_user.AddPersonPeriod(personPeriod1);
			budgetDayRepository.Stub(x => x.Find(_scenario, budgetGroup, period)).Return(budgetDays);

			var target = new AllowanceProvider(budgetDayRepository, _loggedOnUser, _scenarioRepository,
				new ExtractBudgetGroupPeriods(), _now);
			var result = target.GetAllowanceForPeriod(period).ToList();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);
			verifyAllAvailability(result, false);
			allShouldBeEmptyAllowanceDay(result);
		}

		[Test]
		public void
			WhenThereAreMultiplePersonPeriodWithDifferentBudgetGroupssWithinThatPeriod_ShouldUseAllBudgetGroupsToFindTheBudgetDays()
		{
			addWorkFlowControlSetWithOpenAbsencePeriod(_alwaysOpenPeriod, _alwaysOpenPeriod);
			var period = new DateOnlyPeriod(new DateOnly(2001, 1, 1), new DateOnly(2001, 1, 10));
			var budgetDayRepo = MockRepository.GenerateMock<IBudgetDayRepository>();
			var extractBudgetGroupPeriods = MockRepository.GenerateMock<IExtractBudgetGroupPeriods>();

			var budgetGroup1 = new BudgetGroup();
			var period1 = new DateOnlyPeriod(new DateOnly(2001, 1, 1), new DateOnly(2001, 1, 5));
			var day1 = period1.StartDate;
			var allowance1 = TimeSpan.FromHours(10);
			var budgetDay1 = createBudgetDayWithAllowance(budgetGroup1, day1, allowance1.TotalHours);

			var budgetGroup2 = new BudgetGroup();
			var period2 = new DateOnlyPeriod(new DateOnly(2001, 1, 6), new DateOnly(2001, 1, 10));
			var day2 = period2.StartDate;
			var allowance2 = TimeSpan.FromHours(8);
			var budgetDay2 = createBudgetDayWithAllowance(budgetGroup2, day2, allowance2.TotalHours);

			extractBudgetGroupPeriods.Expect(e => e.BudgetGroupsForPeriod(_user, period))
				.Return(new List<Tuple<DateOnlyPeriod, IBudgetGroup>>
				{
					new Tuple<DateOnlyPeriod, IBudgetGroup>(period1, budgetGroup1),
					new Tuple<DateOnlyPeriod, IBudgetGroup>(period2, budgetGroup2)
				});

			budgetDayRepo.Expect(b => b.Find(_scenario, budgetGroup1, period1)).Return(new List<IBudgetDay> {budgetDay1});
			budgetDayRepo.Expect(b => b.Find(_scenario, budgetGroup2, period2)).Return(new List<IBudgetDay> {budgetDay2});
			var target = new AllowanceProvider(budgetDayRepo, _loggedOnUser, _scenarioRepository, extractBudgetGroupPeriods,
				_now);

			var result = target.GetAllowanceForPeriod(period).ToList();
			budgetDayRepo.VerifyAllExpectations();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);
			verifyAllAvailability(result, true);

			foreach (var allowanceDay in result)
			{
				if (allowanceDay.Date == day1)
				{
					Assert.AreEqual(allowance1, allowanceDay.Time);
					Assert.AreEqual(TimeSpan.FromHours(budgetDay1.FulltimeEquivalentHours), allowanceDay.Heads);
					Assert.AreEqual(allowance1.TotalHours, allowanceDay.AllowanceHeads);
					Assert.AreEqual(false, allowanceDay.UseHeadCount);
					Assert.AreEqual(true, allowanceDay.ValidateBudgetGroup);
				}
				else if (allowanceDay.Date == day2)
				{
					Assert.AreEqual(allowance2, allowanceDay.Time);
					Assert.AreEqual(TimeSpan.FromHours(budgetDay2.FulltimeEquivalentHours), allowanceDay.Heads);
					Assert.AreEqual(allowance2.TotalHours, allowanceDay.AllowanceHeads);
					Assert.AreEqual(false, allowanceDay.UseHeadCount);
					Assert.AreEqual(true, allowanceDay.ValidateBudgetGroup);
				}
				else
				{
					shouldBeEmptyAllowanceDay(allowanceDay);
				}
			}
		}

		[Test]
		public void WithoutOpenAbsenceRequestPeriods_ShouldSetAllowanceToZeroOnEverydayInThePeriod()
		{
			_user.WorkflowControlSet = new WorkflowControlSet("workflow controlset without open absenceperiod");
			var period = new DateOnlyPeriod(new DateOnly(2001, 1, 1), new DateOnly(2001, 1, 10));
			var budgetDayRepo = MockRepository.GenerateMock<IBudgetDayRepository>();

			var target = new AllowanceProvider(budgetDayRepo, _loggedOnUser, _scenarioRepository,
				new ExtractBudgetGroupPeriods(), _now);
			var result = target.GetAllowanceForPeriod(period).ToList();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);
			verifyAllAvailability(result, false);
			allShouldBeEmptyAllowanceDay(result);
		}

		[Test]
		public void WhenNoWorkFlowControlSet_ShouldSetAllowanceToZeroOnEverydayInThePeriod()
		{
			var period = new DateOnlyPeriod(new DateOnly(2001, 1, 1), new DateOnly(2001, 1, 10));
			var budgetDayRepo = MockRepository.GenerateMock<IBudgetDayRepository>();

			var target = new AllowanceProvider(budgetDayRepo, _loggedOnUser, _scenarioRepository,
				new ExtractBudgetGroupPeriods(), _now);
			var result = target.GetAllowanceForPeriod(period).ToList();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);
			verifyAllAvailability(result, false);
			allShouldBeEmptyAllowanceDay(result);
		}

		[Test]
		public void WhenThereAreDayWithNegativeValues_ShouldSetAllowanceToZero()
		{
			addWorkFlowControlSetWithOpenAbsencePeriod(_alwaysOpenPeriod, _alwaysOpenPeriod);
			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();

			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate);

			var budgetGroup = new BudgetGroup();
			personPeriod.BudgetGroup = budgetGroup;

			var hoursOfAllowance = -20.0;
			var budgetDayWithNegativeValue = createBudgetDayWithAllowance(budgetGroup, period.StartDate, hoursOfAllowance);

			_user.AddPersonPeriod(personPeriod);
			budgetDayRepository.Expect(x => x.Find(_scenario, budgetGroup, period))
				.Return(new List<IBudgetDay> {budgetDayWithNegativeValue});

			var target = new AllowanceProvider(budgetDayRepository, _loggedOnUser, _scenarioRepository,
				new ExtractBudgetGroupPeriods(), _now);
			var result = target.GetAllowanceForPeriod(period).ToList();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);
			var allowanceDay = result.First();
			Assert.AreEqual(TimeSpan.Zero, allowanceDay.Time);
			Assert.AreEqual(TimeSpan.FromHours(budgetDayWithNegativeValue.FulltimeEquivalentHours), allowanceDay.Heads);
			Assert.AreEqual(hoursOfAllowance, allowanceDay.AllowanceHeads);
			Assert.AreEqual(true, allowanceDay.Availability);
			Assert.AreEqual(false, allowanceDay.UseHeadCount);
			Assert.AreEqual(true, allowanceDay.ValidateBudgetGroup);
		}

		[Test]
		public void WhenTodayExistWithinThePeriod_ShouldSetTheAllowanceInMinutesForThatDate()
		{
			addWorkFlowControlSetWithOpenAbsencePeriod(_alwaysOpenPeriod, _alwaysOpenPeriod);
			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();

			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate);

			var budgetGroup = new BudgetGroup();
			personPeriod1.BudgetGroup = budgetGroup;

			var budgetDays = new List<IBudgetDay>();
			var allowance = TimeSpan.FromHours(40);

			var budgetDay = createBudgetDayWithAllowance(budgetGroup, period.StartDate, allowance.TotalHours);
			budgetDays.Add(budgetDay);

			_user.AddPersonPeriod(personPeriod1);
			budgetDayRepository.Stub(x => x.Find(_scenario, budgetGroup, period)).Return(budgetDays);

			var target = new AllowanceProvider(budgetDayRepository, _loggedOnUser, _scenarioRepository,
				new ExtractBudgetGroupPeriods(), _now);
			var result = target.GetAllowanceForPeriod(period).ToList();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);

			verifyAllAvailability(result, true);
			foreach (var allowanceDay in result)
			{
				if (allowanceDay.Date == period.StartDate)
				{
					Assert.AreEqual(allowance, allowanceDay.Time);
					Assert.AreEqual(TimeSpan.FromHours(budgetDay.FulltimeEquivalentHours), allowanceDay.Heads);
					Assert.AreEqual(allowance.TotalHours, allowanceDay.AllowanceHeads);
					Assert.AreEqual(false, allowanceDay.UseHeadCount);
					Assert.AreEqual(true, allowanceDay.ValidateBudgetGroup);
				}
				else
				{
					shouldBeEmptyAllowanceDay(allowanceDay);
				}
			}
		}

		[Test]
		public void WhenAutoDeny_ShouldSetAllowanceToZero()
		{
			var workflowControlSet = new WorkflowControlSet("_workflowControlSet");
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("theAbsence"),
				OpenForRequestsPeriod = _alwaysOpenPeriod,
				Period = _alwaysOpenPeriod,
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator(),
				AbsenceRequestProcess = new DenyAbsenceRequest()
			});

			_user.WorkflowControlSet = workflowControlSet;
			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();

			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate);

			var budgetGroup = new BudgetGroup();
			personPeriod1.BudgetGroup = budgetGroup;

			var budgetDays = new List<IBudgetDay>();
			var allowance = TimeSpan.FromHours(40);

			var budgetDay = createBudgetDayWithAllowance(budgetGroup, period.StartDate, allowance.TotalHours);
			budgetDays.Add(budgetDay);

			_user.AddPersonPeriod(personPeriod1);
			budgetDayRepository.Stub(x => x.Find(_scenario, budgetGroup, period)).Return(budgetDays);

			var target = new AllowanceProvider(budgetDayRepository, _loggedOnUser, _scenarioRepository,
				new ExtractBudgetGroupPeriods(), _now);
			var result = target.GetAllowanceForPeriod(period).ToList();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);
			verifyAllAvailability(result, false);
			allShouldBeEmptyAllowanceDay(result);
		}

		[Test]
		public void WhenBudgetgroupStaffingCheck_ShouldSetAvailabilityToTrue()
		{
			var workflowControlSet = new WorkflowControlSet("_workflowControlSet");
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("theAbsence"),
				OpenForRequestsPeriod = _alwaysOpenPeriod,
				Period = _alwaysOpenPeriod,
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator()
			});

			_user.WorkflowControlSet = workflowControlSet;

			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();

			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate);

			var budgetGroup = new BudgetGroup();
			personPeriod1.BudgetGroup = budgetGroup;

			var budgetDays = new List<IBudgetDay>();
			var allowance = TimeSpan.FromHours(40);

			var budgetDay = createBudgetDayWithAllowance(budgetGroup, period.StartDate, allowance.TotalHours);
			budgetDays.Add(budgetDay);

			_user.AddPersonPeriod(personPeriod1);
			budgetDayRepository.Stub(x => x.Find(_scenario, budgetGroup, period)).Return(budgetDays);

			var target = new AllowanceProvider(budgetDayRepository, _loggedOnUser, _scenarioRepository,
				new ExtractBudgetGroupPeriods(), _now);
			var result = target.GetAllowanceForPeriod(period).ToList();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);
			var allowanceDay = result.First();
			Assert.AreEqual(allowance, allowanceDay.Time);
			Assert.AreEqual(TimeSpan.FromHours(budgetDay.FulltimeEquivalentHours), allowanceDay.Heads);
			Assert.AreEqual(allowance.TotalHours, allowanceDay.AllowanceHeads);
			Assert.AreEqual(true, allowanceDay.Availability);
			Assert.AreEqual(false, allowanceDay.UseHeadCount);
			Assert.AreEqual(true, allowanceDay.ValidateBudgetGroup);
		}

		[Test]
		public void WhenIntradayStaffingCheck_ShouldSetAvailabilityToFalse()
		{
			var workflowControlSet = new WorkflowControlSet("_workflowControlSet");
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("theAbsence"),
				OpenForRequestsPeriod = _alwaysOpenPeriod,
				Period = _alwaysOpenPeriod,
				StaffingThresholdValidator = new StaffingThresholdValidator()
			});

			_user.WorkflowControlSet = workflowControlSet;

			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();

			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate);

			var budgetGroup = new BudgetGroup();
			personPeriod1.BudgetGroup = budgetGroup;

			var budgetDays = new List<IBudgetDay>();
			var allowance = TimeSpan.FromHours(40);

			var budgetDay = createBudgetDayWithAllowance(budgetGroup, period.StartDate, allowance.TotalHours);
			budgetDays.Add(budgetDay);

			_user.AddPersonPeriod(personPeriod1);
			budgetDayRepository.Stub(x => x.Find(_scenario, budgetGroup, period)).Return(budgetDays);

			var target = new AllowanceProvider(budgetDayRepository, _loggedOnUser, _scenarioRepository,
				new ExtractBudgetGroupPeriods(), _now);
			var result = target.GetAllowanceForPeriod(period).ToList();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);
			verifyAllAvailability(result, false);
			allShouldBeEmptyAllowanceDay(result);
		}

		[Test]
		public void WhenBudgetgroupStaffingCheckNextToIntradayStaffingCheck_ShouldSetAvailabilityToTrue()
		{
			createValidAndInvalidAbsenceForAvailability();

			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();

			var period = new DateOnlyPeriod(DateOnly.Today.AddDays(4), DateOnly.Today.AddDays(4));
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate);

			var budgetGroup = new BudgetGroup();
			personPeriod1.BudgetGroup = budgetGroup;

			var budgetDays = new List<IBudgetDay>();
			var allowance = TimeSpan.FromHours(40);

			var budgetDay = createBudgetDayWithAllowance(budgetGroup, period.StartDate, allowance.TotalHours);
			budgetDays.Add(budgetDay);

			_user.AddPersonPeriod(personPeriod1);
			budgetDayRepository.Stub(x => x.Find(_scenario, budgetGroup, period)).Return(budgetDays);

			var target = new AllowanceProvider(budgetDayRepository, _loggedOnUser, _scenarioRepository,
				new ExtractBudgetGroupPeriods(), _now);
			var result = target.GetAllowanceForPeriod(period).ToList();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);
			var allowanceDay = result.First();
			Assert.AreEqual(allowance, allowanceDay.Time);
			Assert.AreEqual(TimeSpan.FromHours(budgetDay.FulltimeEquivalentHours), allowanceDay.Heads);
			Assert.AreEqual(allowance.TotalHours, allowanceDay.AllowanceHeads);
			Assert.AreEqual(true, allowanceDay.Availability);
			Assert.AreEqual(false, allowanceDay.UseHeadCount);
			Assert.AreEqual(true, allowanceDay.ValidateBudgetGroup);
		}

		[Test]
		public void WhenHeadCountStaffingCheck_ShouldSetAvailabilityToTrue()
		{
			var workflowControlSet = new WorkflowControlSet("_workflowControlSet");
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("theAbsence"),
				OpenForRequestsPeriod = _alwaysOpenPeriod,
				Period = _alwaysOpenPeriod,
				StaffingThresholdValidator = new BudgetGroupHeadCountValidator()
			});

			_user.WorkflowControlSet = workflowControlSet;

			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();

			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate);

			var budgetGroup = new BudgetGroup();
			personPeriod1.BudgetGroup = budgetGroup;

			var budgetDays = new List<IBudgetDay>();
			var allowance = TimeSpan.FromHours(40);

			var budgetDay = createBudgetDayWithAllowance(budgetGroup, period.StartDate, allowance.TotalHours);
			budgetDays.Add(budgetDay);

			_user.AddPersonPeriod(personPeriod1);
			budgetDayRepository.Stub(x => x.Find(_scenario, budgetGroup, period)).Return(budgetDays);

			var target = new AllowanceProvider(budgetDayRepository, _loggedOnUser, _scenarioRepository,
				new ExtractBudgetGroupPeriods(), _now);
			var result = target.GetAllowanceForPeriod(period).ToList();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);
			var allowanceDay = result.First();
			Assert.AreEqual(allowance, allowanceDay.Time);
			Assert.AreEqual(TimeSpan.FromHours(budgetDay.FulltimeEquivalentHours), allowanceDay.Heads);
			Assert.AreEqual(allowance.TotalHours, allowanceDay.AllowanceHeads);
			Assert.AreEqual(true, allowanceDay.Availability);
			Assert.AreEqual(true, allowanceDay.UseHeadCount);
			Assert.AreEqual(true, allowanceDay.ValidateBudgetGroup);
		}

		[Test]
		public void WhenNoStaffingCheck_ShouldSetAvailabilityToFalse()
		{
			var workflowControlSet = new WorkflowControlSet("_workflowControlSet");
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("theAbsence"),
				OpenForRequestsPeriod = _alwaysOpenPeriod,
				Period = _alwaysOpenPeriod,
				StaffingThresholdValidator = new AbsenceRequestNoneValidator()
			});

			_user.WorkflowControlSet = workflowControlSet;

			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();

			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate);

			var budgetGroup = new BudgetGroup();
			personPeriod1.BudgetGroup = budgetGroup;

			var budgetDays = new List<IBudgetDay>();
			var allowance = TimeSpan.FromHours(40);

			var budgetDay = createBudgetDayWithAllowance(budgetGroup, period.StartDate, allowance.TotalHours);
			budgetDays.Add(budgetDay);

			_user.AddPersonPeriod(personPeriod1);
			budgetDayRepository.Stub(x => x.Find(_scenario, budgetGroup, period)).Return(budgetDays);

			var target = new AllowanceProvider(budgetDayRepository, _loggedOnUser, _scenarioRepository,
				new ExtractBudgetGroupPeriods(), _now);
			var result = target.GetAllowanceForPeriod(period).ToList();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);
			verifyAllAvailability(result, false);
			allShouldBeEmptyAllowanceDay(result);
		}

		[Test]
		public void WhenMultipleOpenPeriodsApplied_ShouldCheckStaffingBasedOnLatestOpenPeriodForTheDay()
		{
			createMultipleOpenPeriods();
			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();

			var period = new DateOnlyPeriod(DateOnly.Today.AddDays(-10), DateOnly.Today.AddDays(10));
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate);

			var budgetGroup = new BudgetGroup();
			personPeriod1.BudgetGroup = budgetGroup;

			var budgetDays = new List<IBudgetDay>();

			// 6 days ago will apply the open period with BudgetGroupHeadCountValidator BUT auto denied, no traffic light should displayed
			var allowance0 = TimeSpan.FromHours(20);
			var budgetDay0 = createBudgetDayWithAllowance(budgetGroup, DateOnly.Today.AddDays(-6), allowance0.TotalHours);
			budgetDays.Add(budgetDay0);

			// Yesterday will apply open period with BudgetGroupHeadCountValidator, should display traffic light
			var allowance1 = TimeSpan.FromHours(30);
			var budgetDay1 = createBudgetDayWithAllowance(budgetGroup, DateOnly.Today.AddDays(-1), allowance1.TotalHours);
			budgetDays.Add(budgetDay1);

			// Both 2 open periods with BudgetGroup validator covered today, will apply the second one since its OrderIndex is bigger
			var allowance2 = TimeSpan.FromHours(40);
			var budgetDay2 = createBudgetDayWithAllowance(budgetGroup, DateOnly.Today, allowance2.TotalHours);
			budgetDays.Add(budgetDay2);

			// Tomorrow will apply open period with BudgetGroupAllowanceValidator, should display traffic light
			var allowance3 = TimeSpan.FromHours(50);
			var budgetDay3 = createBudgetDayWithAllowance(budgetGroup, DateOnly.Today.AddDays(1), allowance3.TotalHours);
			budgetDays.Add(budgetDay3);

			_user.AddPersonPeriod(personPeriod1);
			budgetDayRepository.Stub(x => x.Find(_scenario, budgetGroup, period)).Return(budgetDays);

			var target = new AllowanceProvider(budgetDayRepository, _loggedOnUser, _scenarioRepository,
				new ExtractBudgetGroupPeriods(), _now);
			var result = target.GetAllowanceForPeriod(period).ToList();

			result.Count.Should().Be.EqualTo(period.DayCollection().Count);
			foreach (var allowanceDay in result)
			{
				// These days are covered by valid open period with budget group validators and not auto denied, so should show traffic light
				var expectedAvailability = allowanceDay.Date >= DateOnly.Today.AddDays(-7) &&
										   allowanceDay.Date <= DateOnly.Today.AddDays(7) &&
										   allowanceDay.Date != DateOnly.Today.AddDays(-6);
				Assert.AreEqual(expectedAvailability, allowanceDay.Availability);

				if (allowanceDay.Date == DateOnly.Today.AddDays(-1))
				{
					Assert.AreEqual(allowance1, allowanceDay.Time);
					Assert.AreEqual(TimeSpan.FromHours(budgetDay1.FulltimeEquivalentHours), allowanceDay.Heads);
					Assert.AreEqual(allowance1.TotalHours, allowanceDay.AllowanceHeads);
					Assert.AreEqual(true, allowanceDay.UseHeadCount);
					Assert.AreEqual(true, allowanceDay.ValidateBudgetGroup);
				}
				else if (allowanceDay.Date == DateOnly.Today)
				{
					Assert.AreEqual(allowance2, allowanceDay.Time);
					Assert.AreEqual(TimeSpan.FromHours(budgetDay2.FulltimeEquivalentHours), allowanceDay.Heads);
					Assert.AreEqual(allowance2.TotalHours, allowanceDay.AllowanceHeads);
					Assert.AreEqual(false, allowanceDay.UseHeadCount);
					Assert.AreEqual(true, allowanceDay.ValidateBudgetGroup);
				}
				else if (allowanceDay.Date == DateOnly.Today.AddDays(1))
				{
					Assert.AreEqual(allowance3, allowanceDay.Time);
					Assert.AreEqual(TimeSpan.FromHours(budgetDay3.FulltimeEquivalentHours), allowanceDay.Heads);
					Assert.AreEqual(allowance3.TotalHours, allowanceDay.AllowanceHeads);
					Assert.AreEqual(false, allowanceDay.UseHeadCount);
					Assert.AreEqual(true, allowanceDay.ValidateBudgetGroup);
				}
				else
				{
					shouldBeEmptyAllowanceDay(allowanceDay);
				}
			}
		}

		#region helpers

		private BudgetDay createBudgetDayWithAllowance(IBudgetGroup budgetGroup, DateOnly date, double hoursOfAllowance)
		{
			return new BudgetDay(budgetGroup, _scenario, date)
			{
				FulltimeEquivalentHours = 1,
				ShrinkedAllowance = hoursOfAllowance
			};
		}

		private void addWorkFlowControlSetWithOpenAbsencePeriod(DateOnlyPeriod openForRequests,
			DateOnlyPeriod preferencePeriod)
		{
			var workflowControlSet = new WorkflowControlSet("_workflowControlSet");
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("theAbsence"),
				OpenForRequestsPeriod = openForRequests,
				Period = preferencePeriod,
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator()
			});

			_user.WorkflowControlSet = workflowControlSet;
		}

		private void verifyAllAvailability(IEnumerable<IAllowanceDay> allowanceDays, bool expectedAvailability)
		{
			Assert.AreEqual(true, allowanceDays.All(ad => ad.Availability == expectedAvailability));
		}

		private static void shouldBeEmptyAllowanceDay(IAllowanceDay allowanceDay)
		{
			Assert.AreEqual(TimeSpan.Zero, allowanceDay.Time);
			Assert.AreEqual(TimeSpan.Zero, allowanceDay.Heads);
			Assert.AreEqual(.0, allowanceDay.AllowanceHeads);
			Assert.AreEqual(false, allowanceDay.UseHeadCount);
			Assert.AreEqual(false, allowanceDay.ValidateBudgetGroup);
		}

		private static void allShouldBeEmptyAllowanceDay(IEnumerable<IAllowanceDay> allowanceDayList)
		{
			foreach (var allowanceDay in allowanceDayList)
			{
				shouldBeEmptyAllowanceDay(allowanceDay);
			}
		}

		private void createValidAndInvalidAbsenceForAvailability()
		{
			var workflowControlSet = new WorkflowControlSet("_workflowControlSet");
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("IntradayAbsence"),
				OpenForRequestsPeriod = _alwaysOpenPeriod,
				Period = new DateOnlyPeriod(DateOnly.Today.AddDays(-7), DateOnly.Today),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			});
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("BudgetgroupAbsence"),
				OpenForRequestsPeriod = _alwaysOpenPeriod,
				Period = new DateOnlyPeriod(DateOnly.Today.AddDays(1), DateOnly.Today.AddDays(7)),
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator()
			});

			_user.WorkflowControlSet = workflowControlSet;
		}

		private void createMultipleOpenPeriods()
		{
			// OrderIndex depends on order when open periods added, could not set it manually
			var workflowControlSet = new WorkflowControlSet("_workflowControlSet");
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("IntradayAbsence"),
				OpenForRequestsPeriod = _alwaysOpenPeriod,
				Period = new DateOnlyPeriod(DateOnly.Today.AddDays(-10), DateOnly.Today.AddDays(10)),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			});
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("HeadCountAbsence"),
				OpenForRequestsPeriod = _alwaysOpenPeriod,
				Period = new DateOnlyPeriod(DateOnly.Today.AddDays(-7), DateOnly.Today.AddDays(1)),
				StaffingThresholdValidator = new BudgetGroupHeadCountValidator()
			});
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("BudgetgroupAbsence"),
				OpenForRequestsPeriod = _alwaysOpenPeriod,
				Period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(7)),
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator()
			});
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("HeadCountAbsence-AutoDeny"),
				OpenForRequestsPeriod = _alwaysOpenPeriod,
				Period = new DateOnlyPeriod(DateOnly.Today.AddDays(-6), DateOnly.Today.AddDays(-6)),
				StaffingThresholdValidator = new BudgetGroupHeadCountValidator(),
				AbsenceRequestProcess = new DenyAbsenceRequest()
			});
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("HeadCountAbsence-Expired"),
				OpenForRequestsPeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(-10), DateOnly.Today.AddDays(-1)),
				Period = new DateOnlyPeriod(DateOnly.Today.AddDays(6), DateOnly.Today.AddDays(6)),
				StaffingThresholdValidator = new BudgetGroupHeadCountValidator(),
				AbsenceRequestProcess = new DenyAbsenceRequest()
			});

			_user.WorkflowControlSet = workflowControlSet;
		}

		#endregion helpers
	}
}