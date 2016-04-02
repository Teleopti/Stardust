using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Shoveling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Shoveling
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_CascadingSkillsPOC_37679)]
	public class ShovelServicePocTest
	{
		public ResourceCalculationContextFactory ResourceCalculationContextFactory;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public Func<IResourceOptimizationHelperExtended> ResourceOptimizationHelperExtended;

		[Test]
		public void ShouldHandleClosedSkillDays()
		{
			var target = new ShovelServicePoc(ResourceCalculationContextFactory);
			var schedulerStateHolder = SchedulerStateHolderFrom();

			//things that need to be there
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2016, 3, 24);

			var skillList = new List<ISkill>();
			//Setup skills and demand
			var openSkill1 =
				new Skill("A, open", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(openSkill1, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var openSkillDay1 = openSkill1.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			skillList.Add(openSkill1);

			var openSkill2 =
				new Skill("B open", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(openSkill2, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var openSkillDay2 = openSkill2.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			skillList.Add(openSkill2);

			var closedSkill =
				new Skill("C closed", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadThatIsClosed(closedSkill);
			var closedSkillDay = closedSkill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60)); //this is the bad one
			skillList.Add(closedSkill);

			//Setup persons and their skills
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { openSkill2, openSkill1, closedSkill });
			person1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { openSkill1, openSkill2 });
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { openSkill2, closedSkill });
			person3.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person4 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { closedSkill });
			person4.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var personList = new List<IPerson> { person1, person2, person3, person4 };

			//Setup stateholder
			schedulerStateHolder.SchedulingResultState.Schedules = new ScheduleDictionary(scenario,
				new ScheduleDateTimePeriod(new DateTimePeriod(2016, 3, 24, 2016, 3, 24)));
			schedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>
			{
				{openSkill1, new List<ISkillDay> {openSkillDay1}},
				{openSkill2, new List<ISkillDay> {openSkillDay2}},
				{closedSkill, new List<ISkillDay> {closedSkillDay}}
			};
			schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);
			foreach (var person in personList)
			{
				schedulerStateHolder.AllPermittedPersons.Add(person);
				schedulerStateHolder.SchedulingResultState.PersonsInOrganization.Add(person);
			}
			schedulerStateHolder.SchedulingResultState.AddSkills(skillList.ToArray());

			//Setup assignments
			foreach (var person in personList)
			{
				var schedules = new List<IScheduleDay>();
				var ass = new PersonAssignment(person1, scenario, dateOnly);
				ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 9, 0));
				ass.SetShiftCategory(shiftCategory);			
				var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolder.SchedulingResultState.Schedules, person, dateOnly);
				scheduleDay.AddMainShift(ass);
				schedulerStateHolder.SchedulingResultState.Schedules.Modify(scheduleDay);
				schedules.Add(scheduleDay);
			}

			//Recuce, this can be done once when loading scheduler before creating islands
			var reducer = new SkillGroupReducerForCascadingSkills();
			foreach (var person in personList)
			{
				reducer.ReduceToPrimarySkill(person.Period(dateOnly));
			}

			//RespurceCalculate
			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			Assert.DoesNotThrow(
				() =>
					target.Execute(schedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
						new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc), schedulerStateHolder.SchedulingResultState.Skills.OrderBy(skill => skill.Name).ToList()));

		}

		[Test]
		public void ShouldShovelCorrectlyFromNearestPileAndFromTheCheapestSkillGroup()
		{
			var target = new ShovelServicePoc(ResourceCalculationContextFactory);
			var schedulerStateHolder = SchedulerStateHolderFrom();

			//things that need to be there
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2016, 3, 24);

			var skillList = new List<ISkill>();
			//Setup skills and demand
			var skillA =
				new Skill("A", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));  //this will be overstaffed
			skillList.Add(skillA);

			var skillB =
				new Skill("B", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60)); //this will be overstaffed
			skillList.Add(skillB);

			var skillC =
				new Skill("C", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillC, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayC = skillC.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(120)); //this will be understaffed
			skillList.Add(skillC);

			//Setup persons and their skills
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillA, skillB, skillC });
			person1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillC });
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillB, skillC });
			person3.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person4 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillB, skillC });
			person4.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person5 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillA, skillB, skillC });
			person5.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var personList = new List<IPerson> { person1, person2, person3, person4, person5 };

			//Setup stateholder
			schedulerStateHolder.SchedulingResultState.Schedules = new ScheduleDictionary(scenario,
				new ScheduleDateTimePeriod(new DateTimePeriod(2016, 3, 24, 2016, 3, 24)));
			schedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>
			{
				{skillA, new List<ISkillDay> {skillDayA}},
				{skillC, new List<ISkillDay> {skillDayC}},
				{skillB, new List<ISkillDay> {skillDayB}}
			};
			schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);
			foreach (var person in personList)
			{
				schedulerStateHolder.AllPermittedPersons.Add(person);
				schedulerStateHolder.SchedulingResultState.PersonsInOrganization.Add(person);
			}
			schedulerStateHolder.SchedulingResultState.AddSkills(skillList.ToArray());

			//Setup assignments
			foreach (var person in personList)
			{
				var schedules = new List<IScheduleDay>();
				var ass = new PersonAssignment(person1, scenario, dateOnly);
				ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 9, 0));
				ass.SetShiftCategory(shiftCategory);
				var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolder.SchedulingResultState.Schedules, person, dateOnly);
				scheduleDay.AddMainShift(ass);
				schedulerStateHolder.SchedulingResultState.Schedules.Modify(scheduleDay);
				schedules.Add(scheduleDay);
			}

			//Recuce, this can be done once when loading scheduler before creating islands
			var reducer = new SkillGroupReducerForCascadingSkills();
			foreach (var person in personList)
			{
				reducer.ReduceToPrimarySkill(person.Period(dateOnly));
			}

			//RespurceCalculate
			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			target.Execute(schedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
				new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc),
				schedulerStateHolder.SchedulingResultState.Skills.OrderBy(skill => skill.Name).ToList());

			skillDayA.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(1);
			skillDayB.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(0);
			skillDayC.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldShovelCorrectlyFromNextNearestPileIfNearestIsNotEnoughAndFromTheCheapestSkillGroup()
		{
			var target = new ShovelServicePoc(ResourceCalculationContextFactory);
			var schedulerStateHolder = SchedulerStateHolderFrom();

			//things that need to be there
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2016, 3, 24);

			var skillList = new List<ISkill>();
			//Setup skills and demand
			var skillA =
				new Skill("A", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));  //this will be overstaffed
			skillList.Add(skillA);

			var skillB =
				new Skill("B", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60)); //this will be overstaffed
			skillList.Add(skillB);

			var skillC =
				new Skill("C", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillC, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayC = skillC.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(150)); //this will be understaffed
			skillList.Add(skillC);

			//Setup persons and their skills
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillA, skillB, skillC });
			person1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillC });
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillB, skillC });
			person3.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person4 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillB, skillC });
			person4.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person5 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillA, skillB, skillC });
			person5.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var personList = new List<IPerson> { person1, person2, person3, person4, person5 };

			//Setup stateholder
			schedulerStateHolder.SchedulingResultState.Schedules = new ScheduleDictionary(scenario,
				new ScheduleDateTimePeriod(new DateTimePeriod(2016, 3, 24, 2016, 3, 24)));
			schedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>
			{
				{skillA, new List<ISkillDay> {skillDayA}},
				{skillC, new List<ISkillDay> {skillDayC}},
				{skillB, new List<ISkillDay> {skillDayB}}
			};
			schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);
			foreach (var person in personList)
			{
				schedulerStateHolder.AllPermittedPersons.Add(person);
				schedulerStateHolder.SchedulingResultState.PersonsInOrganization.Add(person);
			}
			schedulerStateHolder.SchedulingResultState.AddSkills(skillList.ToArray());

			//Setup assignments
			foreach (var person in personList)
			{
				var schedules = new List<IScheduleDay>();
				var ass = new PersonAssignment(person1, scenario, dateOnly);
				ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 9, 0));
				ass.SetShiftCategory(shiftCategory);
				var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolder.SchedulingResultState.Schedules, person, dateOnly);
				scheduleDay.AddMainShift(ass);
				schedulerStateHolder.SchedulingResultState.Schedules.Modify(scheduleDay);
				schedules.Add(scheduleDay);
			}

			//Recuce, this can be done once when loading scheduler before creating islands
			var reducer = new SkillGroupReducerForCascadingSkills();
			foreach (var person in personList)
			{
				reducer.ReduceToPrimarySkill(person.Period(dateOnly));
			}

			//RespurceCalculate
			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			target.Execute(schedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
				new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc),
				schedulerStateHolder.SchedulingResultState.Skills.OrderBy(skill => skill.Name).ToList());

			skillDayA.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(0.5);
			skillDayB.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(0);
			skillDayC.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldShovelCorrectlyTryToCoverMostExpensivePotFirst()
		{
			var target = new ShovelServicePoc(ResourceCalculationContextFactory);
			var schedulerStateHolder = SchedulerStateHolderFrom();

			//things that need to be there
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2016, 3, 24);

			var skillList = new List<ISkill>();
			//Setup skills and demand
			var skillA =
				new Skill("A", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));  //this will be overstaffed
			skillList.Add(skillA);

			var skillB =
				new Skill("B", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(120)); //this will be understaffed
			skillList.Add(skillB);

			var skillC =
				new Skill("C", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillC, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayC = skillC.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(150)); //this will be understaffed
			skillList.Add(skillC);

			//Setup persons and their skills
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillA, skillB, skillC });
			person1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillC });
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillB, skillC });
			person3.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person4 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillA, skillB, skillC });
			person4.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var personList = new List<IPerson> { person1, person2, person3, person4 };

			//Setup stateholder
			schedulerStateHolder.SchedulingResultState.Schedules = new ScheduleDictionary(scenario,
				new ScheduleDateTimePeriod(new DateTimePeriod(2016, 3, 24, 2016, 3, 24)));
			schedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>
			{
				{skillA, new List<ISkillDay> {skillDayA}},
				{skillC, new List<ISkillDay> {skillDayC}},
				{skillB, new List<ISkillDay> {skillDayB}}
			};
			schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);
			foreach (var person in personList)
			{
				schedulerStateHolder.AllPermittedPersons.Add(person);
				schedulerStateHolder.SchedulingResultState.PersonsInOrganization.Add(person);
			}
			schedulerStateHolder.SchedulingResultState.AddSkills(skillList.ToArray());

			//Setup assignments
			foreach (var person in personList)
			{
				var schedules = new List<IScheduleDay>();
				var ass = new PersonAssignment(person1, scenario, dateOnly);
				ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 9, 0));
				ass.SetShiftCategory(shiftCategory);
				var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolder.SchedulingResultState.Schedules, person, dateOnly);
				scheduleDay.AddMainShift(ass);
				schedulerStateHolder.SchedulingResultState.Schedules.Modify(scheduleDay);
				schedules.Add(scheduleDay);
			}

			//Recuce, this can be done once when loading scheduler before creating islands
			var reducer = new SkillGroupReducerForCascadingSkills();
			foreach (var person in personList)
			{
				reducer.ReduceToPrimarySkill(person.Period(dateOnly));
			}

			//RespurceCalculate
			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			target.Execute(schedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
				new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc),
				schedulerStateHolder.SchedulingResultState.Skills.OrderBy(skill => skill.Name).ToList());

			skillDayA.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(0);
			skillDayB.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(0);
			skillDayC.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(-1.5);
		}

		[Test]
		public void ShouldShovelCorrectlyLeaveMoreOfExpensivePileIfMoreThanOnePile()
		{
			var target = new ShovelServicePoc(ResourceCalculationContextFactory);
			var schedulerStateHolder = SchedulerStateHolderFrom();

			//things that need to be there
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2016, 3, 24);

			var skillList = new List<ISkill>();
			//Setup skills and demand
			var skillA =
				new Skill("A", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));  //+1
			skillList.Add(skillA);

			var skillB =
				new Skill("B", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(30)); //+0.5
			skillList.Add(skillB);

			var skillC =
				new Skill("C", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillC, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayC = skillC.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(120)); //-1
			skillList.Add(skillC);

			//Setup persons and their skills
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillA, skillB, skillC });
			person1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillC });
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillB, skillC });
			person3.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person4 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillA, skillB, skillC });
			person4.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var personList = new List<IPerson> { person1, person2, person3, person4 };

			//Setup stateholder
			schedulerStateHolder.SchedulingResultState.Schedules = new ScheduleDictionary(scenario,
				new ScheduleDateTimePeriod(new DateTimePeriod(2016, 3, 24, 2016, 3, 24)));
			schedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>
			{
				{skillA, new List<ISkillDay> {skillDayA}},
				{skillC, new List<ISkillDay> {skillDayC}},
				{skillB, new List<ISkillDay> {skillDayB}}
			};
			schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);
			foreach (var person in personList)
			{
				schedulerStateHolder.AllPermittedPersons.Add(person);
				schedulerStateHolder.SchedulingResultState.PersonsInOrganization.Add(person);
			}
			schedulerStateHolder.SchedulingResultState.AddSkills(skillList.ToArray());

			//Setup assignments
			foreach (var person in personList)
			{
				var schedules = new List<IScheduleDay>();
				var ass = new PersonAssignment(person1, scenario, dateOnly);
				ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 9, 0));
				ass.SetShiftCategory(shiftCategory);
				var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolder.SchedulingResultState.Schedules, person, dateOnly);
				scheduleDay.AddMainShift(ass);
				schedulerStateHolder.SchedulingResultState.Schedules.Modify(scheduleDay);
				schedules.Add(scheduleDay);
			}

			//Recuce, this can be done once when loading scheduler before creating islands
			var reducer = new SkillGroupReducerForCascadingSkills();
			foreach (var person in personList)
			{
				reducer.ReduceToPrimarySkill(person.Period(dateOnly));
			}

			//RespurceCalculate
			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			target.Execute(schedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
				new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc),
				schedulerStateHolder.SchedulingResultState.Skills.OrderBy(skill => skill.Name).ToList());

			skillDayA.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(0.5);
			skillDayB.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(0);
			skillDayC.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldShovelCorrectlyMayNotTransferMoreThanAllovedForSkillGroup()
		{
			var target = new ShovelServicePoc(ResourceCalculationContextFactory);
			var schedulerStateHolder = SchedulerStateHolderFrom();

			//things that need to be there
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2016, 3, 24);

			var skillList = new List<ISkill>();
			//Setup skills and demand
			var skillA =
				new Skill("A", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			skillList.Add(skillA);

			var skillB =
				new Skill("B", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(30)); 
			skillList.Add(skillB);

			var skillC =
				new Skill("C", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillC, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayC = skillC.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60)); 
			skillList.Add(skillC);

			var skillD =
				new Skill("D", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillD, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var skillDayD = skillD.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60)); 
			skillList.Add(skillD);

			//Setup persons and their skills
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillA, skillB, skillC, skillD });
			person1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillA });
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillA });
			person3.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person4 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillC });
			person4.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person5 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillC });
			person5.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var personList = new List<IPerson> { person1, person2, person3, person4, person5 };

			//Setup stateholder
			schedulerStateHolder.SchedulingResultState.Schedules = new ScheduleDictionary(scenario,
				new ScheduleDateTimePeriod(new DateTimePeriod(2016, 3, 24, 2016, 3, 24)));
			schedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>
			{
				{skillA, new List<ISkillDay> {skillDayA}},
				{skillC, new List<ISkillDay> {skillDayC}},
				{skillB, new List<ISkillDay> {skillDayB}},
				{skillD, new List<ISkillDay> {skillDayD} }
			};
			schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);
			foreach (var person in personList)
			{
				schedulerStateHolder.AllPermittedPersons.Add(person);
				schedulerStateHolder.SchedulingResultState.PersonsInOrganization.Add(person);
			}
			schedulerStateHolder.SchedulingResultState.AddSkills(skillList.ToArray());

			//Setup assignments
			foreach (var person in personList)
			{
				var schedules = new List<IScheduleDay>();
				var ass = new PersonAssignment(person1, scenario, dateOnly);
				ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 9, 0));
				ass.SetShiftCategory(shiftCategory);
				var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolder.SchedulingResultState.Schedules, person, dateOnly);
				scheduleDay.AddMainShift(ass);
				schedulerStateHolder.SchedulingResultState.Schedules.Modify(scheduleDay);
				schedules.Add(scheduleDay);
			}

			//Recuce, this can be done once when loading scheduler before creating islands
			var reducer = new SkillGroupReducerForCascadingSkills();
			foreach (var person in personList)
			{
				reducer.ReduceToPrimarySkill(person.Period(dateOnly));
			}

			//RespurceCalculate
			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			target.Execute(schedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
				new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc),
				schedulerStateHolder.SchedulingResultState.Skills.OrderBy(skill => skill.Name).ToList());

			skillDayA.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(1);
			skillDayB.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(0);
			skillDayC.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(1);
			skillDayD.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(-0.5);
		}

		[Test]
		public void ShouldHandleMaxSeatSkills()
		{
			var target = new ShovelServicePoc(ResourceCalculationContextFactory);
			var schedulerStateHolder = SchedulerStateHolderFrom();

			//things that need to be there
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2016, 3, 24);

			var skillList = new List<ISkill>();
			//Setup skills and demand
			var skillA =
				new Skill("A, open", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var openSkillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			skillList.Add(skillA);

			var skillB =
				new Skill("B open", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var openSkillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			skillList.Add(skillB);

			var maxSeatSkill = 
				new Skill("C max seat", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.MaxSeatSkill))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(maxSeatSkill, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
			var maxSeatSkillDay = maxSeatSkill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			skillList.Add(maxSeatSkill);

			//Setup persons and their skills
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillA, skillB, maxSeatSkill });
			person1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skillA, skillB });
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var personList = new List<IPerson> { person1, person2 };

			//Setup stateholder
			schedulerStateHolder.SchedulingResultState.Schedules = new ScheduleDictionary(scenario,
				new ScheduleDateTimePeriod(new DateTimePeriod(2016, 3, 24, 2016, 3, 24)));
			schedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>
			{
				{skillA, new List<ISkillDay> {openSkillDayA}},
				{skillB, new List<ISkillDay> {openSkillDayB}},
				{maxSeatSkill, new List<ISkillDay> {maxSeatSkillDay}}
			};
			schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);
			foreach (var person in personList)
			{
				schedulerStateHolder.AllPermittedPersons.Add(person);
				schedulerStateHolder.SchedulingResultState.PersonsInOrganization.Add(person);
			}
			schedulerStateHolder.SchedulingResultState.AddSkills(skillList.ToArray());

			//Setup assignments
			foreach (var person in personList)
			{
				var schedules = new List<IScheduleDay>();
				var ass = new PersonAssignment(person1, scenario, dateOnly);
				ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 9, 0));
				ass.SetShiftCategory(shiftCategory);
				var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolder.SchedulingResultState.Schedules, person, dateOnly);
				scheduleDay.AddMainShift(ass);
				schedulerStateHolder.SchedulingResultState.Schedules.Modify(scheduleDay);
				schedules.Add(scheduleDay);
			}

			//Recuce, this can be done once when loading scheduler before creating islands
			var reducer = new SkillGroupReducerForCascadingSkills();
			foreach (var person in personList)
			{
				reducer.ReduceToPrimarySkill(person.Period(dateOnly));
			}

			//RespurceCalculate
			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			var orderedSkillList =
						schedulerStateHolder.SchedulingResultState.Skills.Where(
							skill => skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill).OrderBy(skill => skill.Name).ToList();
			Assert.DoesNotThrow(
				() =>
					target.Execute(schedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
						new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc), orderedSkillList));

			openSkillDayA.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(0);
			openSkillDayB.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(0);
			maxSeatSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(-1);
		}
	}

	[TestFixture]
	public class ShovelTest
	{		
		[Test]
		public void MayNotTransferMoreThanAllovedForSkillGroup()
		{
			var target = new Shovel();
			var skillA = SkillFactory.CreateSkill("levelA");
			var periodA = createSkillStaffPeriod(skillA, 2);
			var skillB = SkillFactory.CreateSkill("levelB");
			var periodB = createSkillStaffPeriod(skillB, -1);
			var skillC = SkillFactory.CreateSkill("levelC");
			var periodC = createSkillStaffPeriod(skillC, 1);
			var skillD = SkillFactory.CreateSkill("levelD");
			var periodD = createSkillStaffPeriod(skillD, -1.5);

			var periodList = new List<ISkillStaffPeriod> { periodA, periodB, periodC, periodD };
			target.Execute(periodList, 2);

			periodA.AbsoluteDifference.Should().Be.EqualTo(0);
			periodB.AbsoluteDifference.Should().Be.EqualTo(0);
			periodC.AbsoluteDifference.Should().Be.EqualTo(1);
			periodD.AbsoluteDifference.Should().Be.EqualTo(-0.5);
		}

		[Test]
		public void ShouldHandleEverythingUnderStaffed()
		{
			var target = new Shovel();
			var skillA = SkillFactory.CreateSkill("levelA");
			var periodA = createSkillStaffPeriod(skillA, -1);
			var skillB = SkillFactory.CreateSkill("levelB");
			var periodB = createSkillStaffPeriod(skillB, -1);
			var skillC = SkillFactory.CreateSkill("levelC");
			var periodC = createSkillStaffPeriod(skillC, -1);

			var periodList = new List<ISkillStaffPeriod> { periodA, periodB, periodC };
			target.Execute(periodList, 2);

			periodA.AbsoluteDifference.Should().Be.EqualTo(-1);
			periodB.AbsoluteDifference.Should().Be.EqualTo(-1);
			periodC.AbsoluteDifference.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldHandleEverythingOverStaffed()
		{
			var target = new Shovel();
			var skillA = SkillFactory.CreateSkill("levelA");
			var periodA = createSkillStaffPeriod(skillA, 1);
			var skillB = SkillFactory.CreateSkill("levelB");
			var periodB = createSkillStaffPeriod(skillB, 1);
			var skillC = SkillFactory.CreateSkill("levelC");
			var periodC = createSkillStaffPeriod(skillC, 1);

			var periodList = new List<ISkillStaffPeriod> { periodA, periodB, periodC };
			target.Execute(periodList, 2);

			periodA.AbsoluteDifference.Should().Be.EqualTo(1);
			periodB.AbsoluteDifference.Should().Be.EqualTo(1);
			periodC.AbsoluteDifference.Should().Be.EqualTo(1);
		}

		private ISkillStaffPeriod createSkillStaffPeriod(ISkill skill, double absoluteDifference)
		{
			var forecasted = Math.Abs(absoluteDifference);
			var resource = forecasted + absoluteDifference;
			var period = SkillStaffPeriodFactory.CreateSkillStaffPeriod(skill, new DateTime(2016, 3, 19, 12, 0, 0, DateTimeKind.Utc), forecasted, 0);
			period.SetCalculatedResource65(resource);

			return period;
		}
	}

	[TestFixture]
	public class SkillGroupPriotizerTest
	{
		[Test]
		public void HiLevelSkillgroupsShouldBeAtTheBottomAndLowerLevelsHigherUp()
		{
			var skillA = SkillFactory.CreateSkill("levelA");
			var skillB = SkillFactory.CreateSkill("levelB");
			var skillC = SkillFactory.CreateSkill("levelC");

			var g1 = new AffectedSkills { Skills = new List<ISkill> { skillA, skillC } }; //2 
			var g2 = new AffectedSkills { Skills = new List<ISkill> { skillB, skillC } }; //1
			var g3 = new AffectedSkills { Skills = new List<ISkill> { skillA, skillB, skillC } }; //4
			var g4 = new AffectedSkills { Skills = new List<ISkill> { skillA, skillB } }; //3

			var unorderedSkillGroupList = new List<AffectedSkills> { g1, g2, g3, g4 };

			var result = new SkillGroupPriotizer().Sort(unorderedSkillGroupList, new List<ISkill> { skillA, skillB, skillC });
			result[0].Skills.ToList().Should().Contain(skillB).And.Contain(skillC);
			result[1].Skills.ToList().Should().Contain(skillA).And.Contain(skillC);
			result[2].Skills.ToList().Should().Contain(skillA).And.Contain(skillB);
			result[3].Skills.ToList().Should().Contain(skillA).And.Contain(skillB).And.Contain(skillC);
		}

		[Test]
		public void ShouldHandle64SkillsWithFastAlgoritm()
		{
			var skillList = new List<ISkill>();
			for (int i = 0; i < 64; i++)
			{
				skillList.Add(SkillFactory.CreateSkill(i.ToString()));
			}

			var skillGroup = new AffectedSkills { Skills = skillList };
			var result = new SkillGroupPriotizer().Sort(new List<AffectedSkills> { skillGroup }, skillList);
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldHandleMoreThan64SkillsWithSlowAlgoritm()
		{
			var skillList = new List<ISkill>();
			var skillGroupSkills1 = new List<ISkill>();
			var skillGroupSkills2 = new List<ISkill>();
			var skill0 = SkillFactory.CreateSkill("0");
			var skill99 = SkillFactory.CreateSkill("99");
			for (int i = 0; i < 100; i++)
			{
				var skillToAdd = SkillFactory.CreateSkill(i.ToString());
				if (i == 0)
					skillToAdd = skill0;
				else if (i == 99)
					skillToAdd = skill99;

				skillList.Add(skillToAdd);
				if (i < 25)
					skillGroupSkills1.Add(skillToAdd);
				else
					skillGroupSkills2.Add(skillToAdd);
			}

			var skillGroup1 = new AffectedSkills { Skills = skillGroupSkills1 };
			var skillGroup2 = new AffectedSkills { Skills = skillGroupSkills2 };
			var result = new SkillGroupPriotizer().Sort(new List<AffectedSkills> { skillGroup1, skillGroup2 }, skillList);
			result[0].Skills.Should().Contain(skill99);
			result[1].Skills.Should().Contain(skill0);
		}
	}
}