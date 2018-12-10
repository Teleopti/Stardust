using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class PersonAssignmentFactory
	{
		public static IPersonAssignment CreatePersonAssignment(IPerson agent)
		{
			return CreatePersonAssignment(agent, ScenarioFactory.CreateScenarioAggregate());
		}

		public static IPersonAssignment CreatePersonAssignmentWithId(IPerson agent)
		{
			var personAssignment = CreatePersonAssignment(agent, ScenarioFactory.CreateScenarioAggregate());
			personAssignment.SetId(Guid.NewGuid());
			return personAssignment;
		}

		public static IPersonAssignment CreatePersonAssignmentWithId(IPerson agent, DateOnly date)
		{
			var personAssignment = CreatePersonAssignment(agent, ScenarioFactory.CreateScenarioAggregate(), date);
			personAssignment.SetId(Guid.NewGuid());
			return personAssignment;
		}

		public static IPersonAssignment CreatePersonAssignment(IPerson agent, IScenario scenario)
		{
			return CreatePersonAssignment(agent, scenario, new DateOnly(2000, 1, 1));
		}

		public static IPersonAssignment CreatePersonAssignment(IPerson agent, IScenario scenario, DateOnly date)
		{
			return new PersonAssignment(agent, scenario, date);
		}

		public static IPersonAssignment CreateAssignmentWithMainShiftAndPersonalShift(IPerson agent, IScenario scenario, IActivity activity, DateTimePeriod period, IShiftCategory category)
		{
			IPersonAssignment ass = new PersonAssignment(agent, scenario, new DateOnly(period.StartDateTimeLocal(agent.PermissionInformation.DefaultTimeZone())));
			ass.AddPersonalActivity(activity, period);
			ass.AddActivity(activity, period);
			ass.SetShiftCategory(category);
			return ass;
		}
		
		public static IPersonAssignment CreateAssignmentWithMainShiftAndPersonalShift(IPerson person, IScenario scenario, DateTimePeriod period)
		{
			var activity = ActivityFactory.CreateActivity("sdf");
			var category = ShiftCategoryFactory.CreateShiftCategory("sdf");
			IPersonAssignment ass = new PersonAssignment(person, scenario, new DateOnly(period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone())));
			ass.AddPersonalActivity(activity, period);
			ass.AddActivity(activity, period);
			ass.SetShiftCategory(category);
			return ass;
		}

		public static IPersonAssignment CreateAssignmentWithMainShift(IPerson agent, IScenario scenario, IActivity activity, DateTimePeriod period, IShiftCategory category)
		{
			return CreateAssignmentWithMainShift(agent, scenario, period, category, activity);
		}

		public static IPersonAssignment CreateAssignmentWithMainShift(IPerson agent, IScenario scenario, DateTimePeriod period, IShiftCategory category, params IActivity[] activities)
		{
			var date = new DateOnly(TimeZoneHelper.ConvertFromUtc(period.StartDateTime, agent.PermissionInformation.DefaultTimeZone()));
			var ass = new PersonAssignment(agent, scenario, date);
			foreach (var activity in activities)
			{
				ass.AddActivity(activity, period);
			}
			ass.SetShiftCategory(category);
			return ass;
		}

		public static IPersonAssignment CreateAssignmentWithMainShift(IPerson agent, IActivity activity, DateTimePeriod period)
		{
			var date = new DateOnly(TimeZoneHelper.ConvertFromUtc(period.StartDateTime, agent.PermissionInformation.DefaultTimeZone()));
			var ass = new PersonAssignment(agent, ScenarioFactory.CreateScenarioWithId("scenario", true), date);
			ass.AddActivity(activity, period);
			ass.SetShiftCategory(ShiftCategoryFactory.CreateShiftCategory("shiftcategory"));
			return ass;
		}

		public static IPersonAssignment CreateAssignmentWithMainShift(IPerson person,
																	 DateTimePeriod period)
		{
			return CreateAssignmentWithMainShift(person,
												 ScenarioFactory.CreateScenarioAggregate(), period);
		}

		public static IPersonAssignment CreateEmptyAssignment(IPerson person, IScenario scenario, DateTimePeriod period)
		{
			IPersonAssignment ass = new PersonAssignment(person, scenario, new DateOnly(period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone())));
			return ass;
		}

		public static IPersonAssignment CreateAssignmentWithMainShift(IPerson person, IScenario scenario, DateTimePeriod period)
		{
			return CreateAssignmentWithMainShift(person,
												 scenario,
												 period, ShiftCategoryFactory.CreateShiftCategory("sdf"));
		}

		public static IPersonAssignment CreateAssignmentWithMainShift(IPerson person, IScenario scenario, DateTimePeriod period, IShiftCategory shiftCategory)
		{
			return CreateAssignmentWithMainShift(person, scenario, new Activity("ass activity"), period, shiftCategory);
		}

		public static IPersonAssignment CreateAssignmentWithPersonalShift(IPerson person, IScenario scenario, IActivity activity, DateTimePeriod period)
		{
			IPersonAssignment ass = new PersonAssignment(person, scenario, new DateOnly(period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone())));
			ass.AddPersonalActivity(activity, period);
			return ass;
		}

		public static IPersonAssignment CreateAssignmentWithPersonalShift(IPerson person,
																	   DateTimePeriod period)
		{
			return CreateAssignmentWithPersonalShift(person, new Scenario("Scenario"), new Activity("Activity"), period);
		}
		
		public static IPersonAssignment CreateAssignmentWithMainShiftAndOvertimeShift(IPerson person, IScenario scenario, DateTimePeriod period)
		{
			var activity = ActivityFactory.CreateActivity("sdf");
			var category = ShiftCategoryFactory.CreateShiftCategory("sdf");
			var ass = CreateAssignmentWithMainShift(person,
												 scenario, activity, period, category);
			IMultiplicatorDefinitionSet multiplicatorDefinitionSet =
				MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("a", MultiplicatorType.Overtime);
			ass.AddOvertimeActivity(activity, period, multiplicatorDefinitionSet);
			return ass;
		}

		public static IPersonAssignment CreateAssignmentWithOvertimeShift(IPerson person, IScenario scenario, IActivity activity, DateTimePeriod period)
		{
			IPersonAssignment ass = new PersonAssignment(person, scenario, new DateOnly(period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone())));
			IMultiplicatorDefinitionSet multiplicatorDefinitionSet =
				MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("a", MultiplicatorType.Overtime);
			ass.AddOvertimeActivity(activity, period, multiplicatorDefinitionSet);
			return ass;
		}
		
		public static IPersonAssignment CreatePersonAssignmentEmpty()
		{
			IPerson agent = PersonFactory.CreatePerson("grisisi");
			IScenario scenario = ScenarioFactory.CreateScenarioAggregate("Heja Gnaget!", false);
			IPersonAssignment ass = new PersonAssignment(agent, scenario, new DateOnly(2000, 1, 1));
			return ass;
		}

		public static PersonAssignmentListContainer CreatePersonAssignmentListForActivityDividerTest()
		{
			var list = new List<Tuple<IProjectionService, IPerson>>();
			var container = new PersonAssignmentListContainer(list);

			var caMorning = ShiftCategoryFactory.CreateShiftCategory("Morning");
			var scDefault = ScenarioFactory.CreateScenarioAggregate("Default", false);
			container.Scenario = scDefault;
			var team = TeamFactory.CreateSimpleTeam();
			var personContract = PersonContractFactory.CreatePersonContract();

			var authorization = CurrentAuthorization.Make();
			var dic = new ScheduleDictionary(scDefault,
				new ScheduleDateTimePeriod(new DateTimePeriod(1900, 1, 1, 2200, 1, 1)),
				new PersistableScheduleDataPermissionChecker(authorization), authorization);
			
			createActivitiesAndAddToContainer(container);
			createSkillsAndAddToContainer(container);
			createPersonsAndAddToContainer(container);

			var assignment1 = new PersonAssignment(container.ContainedPersons["Person1"], scDefault, new DateOnly(2008, 1, 2));
			var assignment2 = new PersonAssignment(container.ContainedPersons["Person2"], scDefault, new DateOnly(2008, 1, 2));
			var assignment3 = new PersonAssignment(container.ContainedPersons["Person3"], scDefault, new DateOnly(2008, 1, 2));
			var assignment4 = new PersonAssignment(container.ContainedPersons["Person4"], scDefault, new DateOnly(2008, 1, 2));

			var prdPerson1Phone = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 9, 30, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 5, 0, DateTimeKind.Utc));
			var prdPerson1Break = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 5, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 10, 0, DateTimeKind.Utc));
			var prdPerson1Office = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 10, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 45, 0, DateTimeKind.Utc));
			var prdPerson2Phone = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 9, 0, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 11, 0, 0, DateTimeKind.Utc));
			var prdPerson3Lunch = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 0, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 30, 0, DateTimeKind.Utc));
			var prdPerson4Phone = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 9, 30, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 10, 0, DateTimeKind.Utc));
			var prdPerson4Office = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 10, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 45, 0, DateTimeKind.Utc));
			
			assignment1.AddActivity(container.ContainedActivities["Phone"], prdPerson1Phone);
			assignment1.AddActivity(container.ContainedActivities["Break"], prdPerson1Break);
			assignment1.AddActivity(container.ContainedActivities["Office"], prdPerson1Office);
			assignment1.SetShiftCategory(caMorning);

			assignment2.AddActivity(container.ContainedActivities["Phone"], prdPerson2Phone);
			assignment2.SetShiftCategory(caMorning);

			assignment3.AddActivity(container.ContainedActivities["Lunch"], prdPerson3Lunch);
			assignment3.SetShiftCategory(caMorning);

			assignment4.AddActivity(container.ContainedActivities["Phone"], prdPerson4Phone);
			assignment4.AddActivity(container.ContainedActivities["Office"], prdPerson4Office);
			assignment4.SetShiftCategory(caMorning);
			
			var ppPerson1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), personContract, team);
			container.ContainedPersons["Person1"].AddPersonPeriod(ppPerson1);
			container.ContainedPersons["Person1"].AddSkill(PersonSkillFactory.CreatePersonSkill(container.ContainedSkills["PhoneA"], 2), ppPerson1);
			container.ContainedPersons["Person1"].AddSkill(PersonSkillFactory.CreatePersonSkill(container.ContainedSkills["PhoneB"], 1), ppPerson1);
			container.ContainedPersons["Person1"].AddSkill(PersonSkillFactory.CreatePersonSkill(container.ContainedSkills["OfficeA"], 1), ppPerson1);
			container.ContainedPersons["Person1"].AddSkill(PersonSkillFactory.CreatePersonSkill(container.ContainedSkills["OfficeB"], 1), ppPerson1);
			var ppPerson2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), personContract, team);
			container.ContainedPersons["Person2"].AddPersonPeriod(ppPerson2);
			container.ContainedPersons["Person2"].AddSkill(
				PersonSkillFactory.CreatePersonSkill(container.ContainedSkills["PhoneA"], 1), ppPerson2);
			container.ContainedPersons["Person2"].AddSkill(
				PersonSkillFactory.CreatePersonSkill(container.ContainedSkills["PhoneB"], 1), ppPerson2);
			var ppPerson3 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), personContract, team);
			container.ContainedPersons["Person3"].AddPersonPeriod(ppPerson3);
			var ppPerson4 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), personContract, team);
			container.ContainedPersons["Person4"].AddPersonPeriod(ppPerson4);
			container.ContainedPersons["Person4"].AddSkill(PersonSkillFactory.CreatePersonSkill(container.ContainedSkills["PhoneB"], 1), ppPerson4);

			var scheduleRange = ExtractedSchedule.CreateScheduleDay(dic, container.ContainedPersons["Person1"], new DateOnly(2008, 1, 2), authorization);
			scheduleRange.Add(assignment1);
			container.PersonAssignmentListForActivityDividerTest.Add(assignment1);
			var svc = scheduleRange.ProjectionService();
			svc.CreateProjection();
			list.Add(new Tuple<IProjectionService, IPerson>(svc, container.ContainedPersons["Person1"]));

			scheduleRange = ExtractedSchedule.CreateScheduleDay(dic, container.ContainedPersons["Person2"], new DateOnly(2008, 1, 2), authorization);
			scheduleRange.Add(assignment2);
			container.PersonAssignmentListForActivityDividerTest.Add(assignment2);
			svc = scheduleRange.ProjectionService();
			svc.CreateProjection();
			list.Add(new Tuple<IProjectionService, IPerson>(svc,container.ContainedPersons["Person2"]));

			scheduleRange = ExtractedSchedule.CreateScheduleDay(dic, container.ContainedPersons["Person3"], new DateOnly(2008, 1, 2), authorization);
			scheduleRange.Add(assignment3);
			container.PersonAssignmentListForActivityDividerTest.Add(assignment3);
			svc = scheduleRange.ProjectionService();
			svc.CreateProjection();
			list.Add(new Tuple<IProjectionService, IPerson>(svc, container.ContainedPersons["Person3"]));

			scheduleRange = ExtractedSchedule.CreateScheduleDay(dic, container.ContainedPersons["Person4"], new DateOnly(2008, 1, 2), authorization);
			scheduleRange.Add(assignment4);
			container.PersonAssignmentListForActivityDividerTest.Add(assignment4);
			svc = scheduleRange.ProjectionService();
			svc.CreateProjection();
			list.Add(new Tuple<IProjectionService, IPerson>(svc, container.ContainedPersons["Person4"]));

			return container;
		}
		
		private static void createActivitiesAndAddToContainer(PersonAssignmentListContainer container)
		{
			var acPhone = ActivityFactory.CreateActivity("Phone", Color.DarkGreen).WithId();
			var acOffice = ActivityFactory.CreateActivity("Office", Color.Yellow).WithId();
			var acBreak = ActivityFactory.CreateActivity("Break", Color.Red).WithId();
			var acLunch = ActivityFactory.CreateActivity("Lunch", Color.Red).WithId();
			container.ContainedActivities.Add(acPhone.Description.Name, acPhone);
			container.ContainedActivities.Add(acOffice.Description.Name, acOffice);
			container.ContainedActivities.Add(acBreak.Description.Name, acBreak);
			container.ContainedActivities.Add(acLunch.Description.Name, acLunch);
		}

		private static void createPersonsAndAddToContainer(PersonAssignmentListContainer container)
		{
			var person1 = PersonFactory.CreatePerson("Person1");
			var person2 = PersonFactory.CreatePerson("Person2");
			var person3 = PersonFactory.CreatePerson("Person3");
			var person4 = PersonFactory.CreatePerson("Person4");
			container.ContainedPersons.Add(person1.Name.FirstName, person1);
			container.ContainedPersons.Add(person2.Name.FirstName, person2);
			container.ContainedPersons.Add(person3.Name.FirstName, person3);
			container.ContainedPersons.Add(person4.Name.FirstName, person4);
		}

		private static void createSkillsAndAddToContainer(PersonAssignmentListContainer container)
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			var skPhoneA = SkillFactory.CreateSkill("PhoneA").WithId();
			skPhoneA.Activity = container.ContainedActivities["Phone"];
			skPhoneA.TimeZone = timeZone;
			var skPhoneB = SkillFactory.CreateSkill("PhoneB").WithId();
			skPhoneB.Activity = container.ContainedActivities["Phone"];
			skPhoneB.TimeZone = timeZone;
			var skPhoneC = SkillFactory.CreateSkill("PhoneC").WithId();
			skPhoneC.Activity = container.ContainedActivities["Phone"];
			skPhoneC.TimeZone = timeZone;
			var skOfficeA = SkillFactory.CreateSkill("OfficeA").WithId();
			skOfficeA.Activity = container.ContainedActivities["Office"];
			skOfficeA.TimeZone = timeZone;
			var skOfficeB = SkillFactory.CreateSkill("OfficeB").WithId();
			skOfficeB.Activity = container.ContainedActivities["Office"];
			skOfficeB.TimeZone = timeZone;
			container.AllSkills.Add(skPhoneA);
			container.AllSkills.Add(skPhoneB);
			container.AllSkills.Add(skPhoneC);
			container.AllSkills.Add(skOfficeA);
			container.AllSkills.Add(skOfficeB);
			container.ContainedSkills.Add(skPhoneA.Name, skPhoneA);
			container.ContainedSkills.Add(skPhoneB.Name, skPhoneB);
			container.ContainedSkills.Add(skPhoneC.Name, skPhoneC);
			container.ContainedSkills.Add(skOfficeA.Name, skOfficeA);
			container.ContainedSkills.Add(skOfficeB.Name, skOfficeB);
		}

		public static IPersonAssignment CreateAssignmentWithOvertimePersonalAndMainshiftLayers()
		{
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var ret = new PersonAssignment(new Person(), new Scenario("d"), new DateOnly(2000, 1, 1));
			ret.AddActivity(new Activity("1"), new DateTimePeriod(start, start.AddHours(1)));
			ret.AddPersonalActivity(new Activity("2"), new DateTimePeriod(start, start.AddHours(2)));
			ret.AddOvertimeActivity(new Activity("3"), new DateTimePeriod(start, start.AddHours(3)), new MultiplicatorDefinitionSet("multiplicatorset", MultiplicatorType.Overtime));
			ret.SetShiftCategory(new ShiftCategory("test"));
			return ret;
		}

		public static IPersonAssignment CreateAssignmentWithThreeMainshiftLayers()
		{
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var ret = new PersonAssignment(new Person(), new Scenario("d"), new DateOnly(2000, 1, 1));
			ret.AddActivity(new Activity("1"), new DateTimePeriod(start, start.AddHours(1)));
			ret.AddActivity(new Activity("2"), new DateTimePeriod(start, start.AddHours(2)));
			ret.AddActivity(new Activity("3"), new DateTimePeriod(start, start.AddHours(3)));
			ret.SetShiftCategory(new ShiftCategory("test"));
			return ret;
		}

		public static IPersonAssignment CreateAssignmentWithThreeOvertimeLayers()
		{
			var p = new Person();
			var contract = new Contract("sdf");
			var multi = new MultiplicatorDefinitionSet("sdfsdf", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(multi);
			p.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
											   new PersonContract(contract, new PartTimePercentage("d"),
																  new ContractSchedule("d")), new Team()));
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var ret = new PersonAssignment(p, new Scenario("d"), new DateOnly(2000, 1, 1));
			var act = new Activity("overtime");
			ret.AddOvertimeActivity(act, new DateTimePeriod(start.AddHours(5), start.AddHours(6)), multi);
			ret.AddOvertimeActivity(act, new DateTimePeriod(start.AddHours(5), start.AddHours(7)), multi);
			ret.AddOvertimeActivity(act, new DateTimePeriod(start.AddHours(5), start.AddHours(8)), multi);
			return ret;
		}

		public static IPersonAssignment CreateAssignmentWithDayOff(IPerson person, IScenario scenario, DateOnly date, TimeSpan length, TimeSpan flexibility, TimeSpan anchor)
		{
			var ass = new PersonAssignment(person, scenario, date);
			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("test"));
			dayOffTemplate.Anchor = anchor;
			dayOffTemplate.SetTargetAndFlexibility(length, flexibility);
			ass.SetDayOff(dayOffTemplate);
			return ass;
		}

		public static IPersonAssignment CreateAssignmentWithDayOff(IPerson person, IScenario scenario, DateOnly date, IDayOffTemplate template)
		{
			var ass = new PersonAssignment(person, scenario, date);
			ass.SetDayOff(template);
			return ass;
		}

		public static IPersonAssignment CreateAssignmentWithDayOff()
		{
			return CreateAssignmentWithDayOff(new Person(), new Scenario("scenario"), new DateOnly(2000, 1, 1), new DayOffTemplate(new Description("for", "test")));
		}
	}
}