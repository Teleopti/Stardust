using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
	[TestFixture, DomainTest]
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_xx)]
	public class ScheduleStaffingPossibilityCalculatorTest : ISetup
	{
		public INow Now;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeScheduleDataReadScheduleStorage ScheduleStorage;
		public FakeCurrentScenario CurrentScenario;
		public IScheduleStaffingPossibilityCalculator Target;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeSkillCombinationResourceRepository CombinationRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;

		private readonly DateTime _today = new DateTime(2017, 2, 7, 13, 30, 0, DateTimeKind.Utc);

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			var staffingViewModelCreator = MockRepository.GenerateMock<IStaffingViewModelCreator>();
			system.UseTestDouble(staffingViewModelCreator).For<IStaffingViewModelCreator>();

			system.UseTestDouble(new MutableNow(_today)).For<INow>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeScheduleDataReadScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScenarioRepository>().For<IScenarioRepository>();
			system.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			system.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
			system.UseTestDouble<FakeScheduleForecastSkillReadModelRepository>().For<IScheduleForecastSkillReadModelRepository>();
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
		}

		[Test]
		public void ShouldReturnEmptyPossibilities()
		{
			var person = PersonFactory.CreatePersonWithId();
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var possibilities = Target.CalculateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilities()
		{
			setupTestDataForOneSkill();
			var possibilities = Target.CalculateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test, Ignore("not prepare for it yet")]
		public void ShouldGetPossibilitiesForDays()
		{
			setupTestDataForOneSkill();
			var today = new DateOnly(_today);
			var period = new DateOnlyPeriod(today, today.AddDays(4));
			var possibilitiesForDays = Target.CalculateIntradayAbsenceIntervalPossibilities(period);
			Assert.AreEqual(5, possibilitiesForDays.Count);
			period.DayCollection().ToList().ForEach(day =>
			{
				Assert.IsTrue(possibilitiesForDays.ContainsKey(day));
				Assert.AreEqual(2, possibilitiesForDays[day].Count);
			});
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
			setPersonSkill(personSkill2, new double?[] { 10d, 10d }, new double?[] { 6d, 12d });

			var personPeriod = createPersonPeriod(personSkill1, personSkill2);
			person.AddPersonPeriod(personPeriod);

			createAssignment(person, activity2);

			LoggedOnUser.SetFakeLoggedOnUser(person);

			var possibilities = Target.CalculateIntradayOvertimeIntervalPossibilities();
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
			var possibilities = Target.CalculateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilitiesWhenSomeStaffingDataIsNotAvailable()
		{
			setupTestDataForOneSkill(new double?[] { 10d }, new double?[] { 11d, 11d });
			var possibilities = Target.CalculateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(1, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilitiesWithoutIntradaySchedule()
		{
			setupTestDataForOneSkill();
			ScheduleStorage.Clear();
			var possibilities = Target.CalculateIntradayOvertimeIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForAbsenceWhenUnderstaffing()
		{
			setupTestDataForOneSkill();
			var possibilities = Target.CalculateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.Values.ElementAt(0));
			Assert.AreEqual(0, possibilities.Values.ElementAt(1));
		}

		[Test]
		public void ShouldGetGoodPossibilitiesForAbsenceWhenNotUnderstaffing()
		{
			setupTestDataForOneSkill(new double?[] { 10d, 10d }, new double?[] { 11d, 11d });
			var possibilities = Target.CalculateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.Values.ElementAt(0));
			Assert.AreEqual(1, possibilities.Values.ElementAt(1));
		}

		[Test]
		public void ShouldGetFairAndGoodPossibilitiesForAbsence()
		{
			setupTestDataForOneSkill(new double?[] { 10d, 10d }, new double?[] { 8d, 11d });
			var possibilities = Target.CalculateIntradayAbsenceIntervalPossibilities();
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

			var possibilities = Target.CalculateIntradayAbsenceIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.Values.ElementAt(0));
			Assert.AreEqual(0, possibilities.Values.ElementAt(1));
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenOverstaffing()
		{
			setupTestDataForOneSkill(new double?[] { 10d, 10d }, new double?[] { 12d, 12d });
			var possibilities = Target.CalculateIntradayOvertimeIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.Values.ElementAt(0));
			Assert.AreEqual(0, possibilities.Values.ElementAt(1));
		}

		[Test]
		public void ShouldGetGoodPossibilitiesForOvertimeWhenNotOverstaffing()
		{
			setupTestDataForOneSkill(new double?[] { 10d, 10d }, new double?[] { 6d, 6d });
			var possibilities = Target.CalculateIntradayOvertimeIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.Values.ElementAt(0));
			Assert.AreEqual(1, possibilities.Values.ElementAt(1));
		}

		[Test]
		public void ShouldGetFairAndGoodPossibilitiesForOvertime()
		{
			setupTestDataForOneSkill(new double?[] { 10d, 10d }, new double?[] { 7d, 6d });
			var possibilities = Target.CalculateIntradayOvertimeIntervalPossibilities();
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

			var possibilities = Target.CalculateIntradayOvertimeIntervalPossibilities();
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
			ScheduleStorage.Set(new List<IScheduleData> { assignment });
			var possibilities = Target.CalculateIntradayOvertimeIntervalPossibilities();
			Assert.AreEqual(2, possibilities.Count);
		}

		private void setupTestDataForOneSkill(double?[] forecastedStaffing = null, double?[] scheduledStaffing = null, bool useShrinkage = false)
		{
			var person = PersonFactory.CreatePersonWithId();
			var activity = createActivity();
			createAssignment(person, activity);
			setPersonSkill(person, activity, forecastedStaffing ?? new double?[] { 10d, 10d },
				scheduledStaffing ?? new double?[] { 8d, 8d });
			LoggedOnUser.SetFakeLoggedOnUser(person);
		}

		private void setPersonSkill(IPersonSkill personSkill, double?[] forecastedStaffing, double?[] scheduledStaffing)
		{
			personSkill.Skill.StaffingThresholds = createStaffingThresholds();
			setupIntradayStaffingViewModelForSkill(getAvailablePeriod(), personSkill.Skill, forecastedStaffing,
				scheduledStaffing);
		}

		private void setPersonSkill(IPerson person, IActivity activity, double?[] forecastedStaffing,
			double?[] scheduledStaffing)
		{
			var personSkill = createPersonSkill(activity);
			var personPeriod = createPersonPeriod(personSkill);
			person.AddPersonPeriod(personPeriod);
			personSkill.Skill.StaffingThresholds = createStaffingThresholds();
			setupIntradayStaffingViewModelForSkill(getAvailablePeriod(), personSkill.Skill, forecastedStaffing,
				scheduledStaffing);
		}

		private DateOnlyPeriod getAvailablePeriod()
		{
			var today = new DateOnly(_today);
			var period = new DateOnlyPeriod(today, today.AddDays(13));
			return period;
		}

		private StaffingThresholds createStaffingThresholds()
		{
			return new StaffingThresholds(new Percent(-0.3), new Percent(-0.1), new Percent(0.1));
		}

		private IPersonSkill createPersonSkill(IActivity activity)
		{
			var skill = SkillFactory.CreateSkill("test1").WithId();
			skill.SkillType.Description = new Description("SkillTypeInboundTelephony");
			skill.Activity = activity;
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(21,45,22,15));
			SkillRepository.Has(skill);

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

		private void setupIntradayStaffingViewModelForSkill(DateOnlyPeriod period, ISkill skill, double?[] forecastedStaffing,
			double?[] scheduledStaffing)
		{
			var startDate = _today.AddHours(8);
			var interval1 = startDate.AddMinutes(15);
			var interval2 = startDate.AddMinutes(30);

			CombinationRepository.AddSkillCombinationResource(new DateTime(),
				new[]
				{
					new SkillCombinationResource
					{
						StartDateTime = interval1,
						EndDateTime = interval2,
						Resource = scheduledStaffing[0].Value,
						SkillCombination = new[] {skill.Id.Value}
					}
				});

			if (scheduledStaffing.Length > 1)
			{
				CombinationRepository.AddSkillCombinationResource(new DateTime(),
					new[]
					{
						new SkillCombinationResource
						{
							StartDateTime = interval2,
							EndDateTime = interval2.AddMinutes(15),
							Resource = scheduledStaffing[1].Value,
							SkillCombination = new[] {skill.Id.Value}
						}
					});
			}

			var skillStaffingIntervals = new List<SkillStaffingInterval>
			{
				new SkillStaffingInterval
				{
					StartDateTime = interval1,
					EndDateTime = interval1.AddMinutes(15),
					FStaff = forecastedStaffing[0].Value,
					SkillId = skill.Id.GetValueOrDefault()
				}
			};

			if (forecastedStaffing.Length > 1)
			{
				skillStaffingIntervals.Add(new SkillStaffingInterval
				{
					StartDateTime = interval2,
					EndDateTime = interval2.AddMinutes(15),
					FStaff = forecastedStaffing[1].Value,
					SkillId = skill.Id.GetValueOrDefault()
				});
			}

			var timePeriodTuples = new List<Tuple<TimePeriod, double>>
			{
				new Tuple<TimePeriod, double>(
					new TimePeriod(interval1.TimeOfDay, interval1.TimeOfDay.Add(TimeSpan.FromMinutes(15))),
					forecastedStaffing[0].Value),
			};

			if (forecastedStaffing.Length > 1)
			{
				timePeriodTuples.Add(new Tuple<TimePeriod, double>(
					new TimePeriod(interval2.TimeOfDay, interval2.TimeOfDay.Add(TimeSpan.FromMinutes(15))),
					forecastedStaffing[1].Value));
			}

			ScheduleForecastSkillReadModelRepository.Persist(skillStaffingIntervals, new DateTime());
			SkillDayRepository.Has(skill.CreateSkillDayWithDemandOnInterval(CurrentScenario.Current(), period.StartDate, 0,
				timePeriodTuples.ToArray()));
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
