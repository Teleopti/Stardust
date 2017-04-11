using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo
{
	[TestFixture, IoCTest]
	public class ScheduleStaffingPossibilityCalculatorTest : ISetup
	{
		public INow Now;
		public FakeLoggedOnUser LoggedOnUser;
		public IStaffingViewModelCreator StaffingViewModelCreator;
		public FakeScheduleDataReadScheduleStorage ScheduleStorage;
		public FakeCurrentScenario CurrentScenario;
		public IScheduleStaffingPossibilityCalculator Target;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;

		private readonly DateTime _today = new DateTime(2017, 2, 7, 13, 31, 0, DateTimeKind.Utc);
		private int _callStaffingViewModelCreatorTimes = 1;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			var staffingViewModelCreator = MockRepository.GenerateMock<IStaffingViewModelCreator>();
			system.UseTestDouble(staffingViewModelCreator).For<IStaffingViewModelCreator>();

			system.UseTestDouble(new MutableNow(_today)).For<INow>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeScheduleDataReadScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<CacheableStaffingViewModelCreator>().For<ICacheableStaffingViewModelCreator>();
			system.UseTestDouble<FakeIntervalLengthFetcher>().For<IIntervalLengthFetcher>();
		}

		[Test]
		public void ShouldReturnEmptyPossibilities()
		{
			var person = PersonFactory.CreatePersonWithId();
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var possibilities = Target.CalcuateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilities()
		{
			setupTestDataForOneSkill();
			var possibilities = Target.CalcuateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilitiesFromCache()
		{
			_callStaffingViewModelCreatorTimes = 1;
			setupTestDataForOneSkill();
			Target.CalcuateIntradayAbsenceIntervalPossibilities();
			var possibilities = Target.CalcuateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			StaffingViewModelCreator.VerifyAllExpectations();
		}

		[Test]
		public void ShouldNotCacheEmptyStaffingData()
		{
			_callStaffingViewModelCreatorTimes = 2;
			setupTestDataForOneSkill(new double?[] {}, new double?[] {});
			Target.CalcuateIntradayAbsenceIntervalPossibilities();
			var possibilities = Target.CalcuateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(0, possibilities.Count);
			StaffingViewModelCreator.VerifyAllExpectations();
		}

		[Test]
		public void ShouldNotCacheAllZeroOrNullStaffingData()
		{
			_callStaffingViewModelCreatorTimes = 2;
			setupTestDataForOneSkill(new double?[] { null, 0 }, new double?[] { 0, 0 });
			Target.CalcuateIntradayAbsenceIntervalPossibilities();
			var possibilities = Target.CalcuateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(1, possibilities.Count);
			StaffingViewModelCreator.VerifyAllExpectations();
		}

		[Test]
		public void ShouldCacheStaffingDataByIntervalLength()
		{
			_callStaffingViewModelCreatorTimes = 2;
			IntervalLengthFetcher.Has(15);
			setupTestDataForOneSkill();
			Target.CalcuateIntradayAbsenceIntervalPossibilities();

			IntervalLengthFetcher.Has(30);
			Target.CalcuateIntradayAbsenceIntervalPossibilities();

			StaffingViewModelCreator.VerifyAllExpectations();
		}

		[Test]
		public void ShouldGetPossibilitiesWhenCacheIsNoAvailable()
		{
			_callStaffingViewModelCreatorTimes = 2;
			setupTestDataForOneSkill();
			Target.CalcuateIntradayAbsenceIntervalPossibilities();

			var personSkill =
				LoggedOnUser.CurrentUser()
					.PersonPeriods(new DateOnly(_today).ToDateOnlyPeriod())
					.FirstOrDefault()
					?.PersonSkillCollection.FirstOrDefault();
			var cacheKey = $"{personSkill?.Skill.Id}_{false}_{IntervalLengthFetcher.IntervalLength}";
			MemoryCache.Default.Remove(cacheKey);

			var possibilities = Target.CalcuateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			StaffingViewModelCreator.VerifyAllExpectations();
		}

		[Test]
		public void ShouldGetPossibilitiesOnlyForSkillsInSchedule()
		{
			var person = PersonFactory.CreatePersonWithId();
			var activity1 = createActivity();
			var personSkill1 = createPersonSkill(activity1);
			setPersonSkill(personSkill1, new double?[] { 10d, 10d }, new double?[] { 12d, 11d });

			var activity2 = createActivity();
			var personSkill2 = createPersonSkill(activity2);
			setPersonSkill(personSkill2, new double?[] { 10d, 10d }, new double?[] { 11d, 12d });

			var personPeriod = createPersonPeriod(personSkill1, personSkill2);
			person.AddPersonPeriod(personPeriod);

			createAssignment(person, activity2);

			LoggedOnUser.SetFakeLoggedOnUser(person);

			var possibilities = Target.CalcuateIntradayOvertimeIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.Values.ElementAt(0));
			Assert.AreEqual(0, possibilities.Values.ElementAt(1));
		}

		[Test]
		public void ShouldGetPossibilitiesWhenUsingShrinkageValidator()
		{
			setupTestDataForOneSkill(useShrinkage: true);
			var person = LoggedOnUser.CurrentUser();
			var absence = AbsenceFactory.CreateAbsenceWithId();
			var workflowControlSet = WorkflowControlSetFactory.CreateWorkFlowControlSet(absence, new PendingAbsenceRequest(),
				false);
			workflowControlSet.AbsenceRequestOpenPeriods[0].StaffingThresholdValidator =
				new StaffingThresholdWithShrinkageValidator();
			person.WorkflowControlSet = workflowControlSet;
			var possibilities = Target.CalcuateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilitiesWhenSomeStaffingDataIsNotAvailable()
		{
			setupTestDataForOneSkill(new double?[] { 10d }, new double?[] { 11d, 11d });
			var possibilities = Target.CalcuateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(1, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilitiesWithoutIntradaySchedule()
		{
			setupTestDataForOneSkill();
			ScheduleStorage.Clear();
			var possibilities = Target.CalcuateIntradayOvertimeIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForAbsenceWhenUnderstaffing()
		{
			setupTestDataForOneSkill();
			var possibilities = Target.CalcuateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.Values.ElementAt(0));
			Assert.AreEqual(0, possibilities.Values.ElementAt(1));
		}

		[Test]
		public void ShouldGetGoodPossibilitiesForAbsenceWhenNotUnderstaffing()
		{
			setupTestDataForOneSkill(new double?[] { 10d, 10d }, new double?[] { 11d, 11d });
			var possibilities = Target.CalcuateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.Values.ElementAt(0));
			Assert.AreEqual(1, possibilities.Values.ElementAt(1));
		}

		[Test]
		public void ShouldGetFairAndGoodPossibilitiesForAbsence()
		{
			setupTestDataForOneSkill(new double?[] { 10d, 10d }, new double?[] { 8d, 11d });
			var possibilities = Target.CalcuateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.Values.ElementAt(0));
			Assert.AreEqual(1, possibilities.Values.ElementAt(1));
		}

		[Test]
		public void ShouldGetFairPossibilitiesForAbsenceWhenOneOfSkillIsUnderstaffing()
		{
			var person = PersonFactory.CreatePersonWithId();
			var activity1 = createActivity();
			var personSkill1 = createPersonSkill(activity1);
			setPersonSkill(personSkill1, new double?[] { 10d, 10d }, new double?[] { 8d, 11d });

			var activity2 = createActivity();
			var personSkill2 = createPersonSkill(activity2);
			setPersonSkill(personSkill2, new double?[] { 10d, 10d }, new double?[] { 11d, 8d });

			var personPeriod = createPersonPeriod(personSkill1, personSkill2);
			person.AddPersonPeriod(personPeriod);

			createAssignment(person, activity1, activity2);

			LoggedOnUser.SetFakeLoggedOnUser(person);

			var possibilities = Target.CalcuateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.Values.ElementAt(0));
			Assert.AreEqual(0, possibilities.Values.ElementAt(1));
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenOverstaffing()
		{
			setupTestDataForOneSkill(new double?[] { 10d, 10d }, new double?[] { 12d, 12d });
			var possibilities = Target.CalcuateIntradayOvertimeIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.Values.ElementAt(0));
			Assert.AreEqual(0, possibilities.Values.ElementAt(1));
		}

		[Test]
		public void ShouldGetGoodPossibilitiesForOvertimeWhenNotOverstaffing()
		{
			setupTestDataForOneSkill(new double?[] { 10d, 10d }, new double?[] { 11d, 11d });
			var possibilities = Target.CalcuateIntradayOvertimeIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.Values.ElementAt(0));
			Assert.AreEqual(1, possibilities.Values.ElementAt(1));
		}

		[Test]
		public void ShouldGetFairAndGoodPossibilitiesForOvertime()
		{
			setupTestDataForOneSkill(new double?[] { 10d, 10d }, new double?[] { 12d, 11d });
			var possibilities = Target.CalcuateIntradayOvertimeIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.Values.ElementAt(0));
			Assert.AreEqual(1, possibilities.Values.ElementAt(1));
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenOneOfSkillIsOverstaffing()
		{
			var person = PersonFactory.CreatePersonWithId();
			var activity1 = createActivity();
			var personSkill1 = createPersonSkill(activity1);
			setPersonSkill(personSkill1, new double?[] { 10d, 10d }, new double?[] { 12d, 11d });

			var activity2 = createActivity();
			var personSkill2 = createPersonSkill(activity2);
			setPersonSkill(personSkill2, new double?[] { 10d, 10d }, new double?[] { 11d, 12d });

			var personPeriod = createPersonPeriod(personSkill1, personSkill2);
			person.AddPersonPeriod(personPeriod);

			createAssignment(person, activity1, activity2);

			LoggedOnUser.SetFakeLoggedOnUser(person);

			var possibilities = Target.CalcuateIntradayOvertimeIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.Values.ElementAt(0));
			Assert.AreEqual(0, possibilities.Values.ElementAt(1));
		}

		[Test]
		public void ShouldGetPossibilitiesForOvertimeWithDayOff()
		{
			setupTestDataForOneSkill();
			ScheduleStorage.Clear();
			var person = LoggedOnUser.CurrentUser();
			var assignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, CurrentScenario.Current(),
				new DateOnly(_today), new DayOffTemplate());
			ScheduleStorage.Set(new List<IScheduleData> {assignment});
			var possibilities = Target.CalcuateIntradayOvertimeIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
		}

		private void setupTestDataForOneSkill(double?[] forecastedStaffing = null, double?[] scheduledStaffing = null, bool useShrinkage = false)
		{
			var person = PersonFactory.CreatePersonWithId();
			var activity = createActivity();
			createAssignment(person, activity);
			setPersonSkill(person, activity, forecastedStaffing ?? new double?[] { 10d, 10d },
				scheduledStaffing ?? new double?[] { 8d, 8d }, useShrinkage);
			LoggedOnUser.SetFakeLoggedOnUser(person);
		}

		private void setPersonSkill(IPersonSkill personSkill, double?[] forecastedStaffing, double?[] scheduledStaffing)
		{
			personSkill.Skill.StaffingThresholds = createStaffingThresholds();
			setupIntradayStaffingViewModelForSkill(personSkill.Skill.Id.GetValueOrDefault(), forecastedStaffing,
				scheduledStaffing);
		}

		private void setPersonSkill(IPerson person, IActivity activity, double?[] forecastedStaffing,
			double?[] scheduledStaffing, bool useShrinkage)
		{
			var personSkill = createPersonSkill(activity);
			var personPeriod = createPersonPeriod(personSkill);
			person.AddPersonPeriod(personPeriod);
			personSkill.Skill.StaffingThresholds = createStaffingThresholds();
			setupIntradayStaffingViewModelForSkill(personSkill.Skill.Id.GetValueOrDefault(), forecastedStaffing,
				scheduledStaffing, useShrinkage);
		}

		private StaffingThresholds createStaffingThresholds()
		{
			return new StaffingThresholds(new Percent(-0.3), new Percent(-0.1), new Percent(0.1));
		}

		private static IPersonSkill createPersonSkill(IActivity activity)
		{
			var skill = SkillFactory.CreateSkillWithId("skill1");
			skill.SkillType.Description = new Description("SkillTypeInboundTelephony");
			skill.Activity = activity;
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			return personSkill;
		}

		private PersonPeriod createPersonPeriod(params IPersonSkill[] personSkills)
		{
			var personContract = PersonContractFactory.CreatePersonContract();
			var team = TeamFactory.CreateSimpleTeam();
			var personPeriod = new PersonPeriod(new DateOnly(_today), personContract, team);
			foreach (var personSkill in personSkills)
			{
				personPeriod.AddPersonSkill(personSkill);
			}
			return personPeriod;
		}

		private static IActivity createActivity()
		{
			var activity = ActivityFactory.CreateActivity("activity1");
			activity.RequiresSkill = true;
			return activity;
		}

		private void setupIntradayStaffingViewModelForSkill(Guid skillId, double?[] forecastedStaffing,
			double?[] scheduledStaffing, bool useShrinkage = false)
		{
			var startDate = _today.AddHours(8);
			var interval1 = startDate.AddMinutes(15);
			var interval2 = startDate.AddMinutes(30);

			StaffingViewModelCreator.Stub(s => s.Load(new[] {skillId}, null, useShrinkage)).Return(new IntradayStaffingViewModel
			{
				StaffingHasData = forecastedStaffing.Any(),
				DataSeries = new StaffingDataSeries
				{
					ForecastedStaffing = forecastedStaffing,
					ScheduledStaffing = scheduledStaffing,
					Time = new[] {interval1, interval2}
				}
			}).Repeat.Times(_callStaffingViewModelCreatorTimes);
		}

		private void createAssignment(IPerson person, params IActivity[] activities)
		{
			var startDate = _today.AddHours(8);
			var endDate = _today.AddHours(17);
			var scheduleDatas = new List<IScheduleData>();
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				CurrentScenario.Current(), new DateTimePeriod(startDate, endDate),
				ShiftCategoryFactory.CreateShiftCategory(), activities);
			scheduleDatas.Add(assignment);
			ScheduleStorage.Set(scheduleDatas);
		}
	}
}
