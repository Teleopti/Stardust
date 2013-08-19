﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for PersonAssignment domain object
    /// </summary>
    public static class PersonAssignmentFactory
    {
        public static IPersonAssignment CreatePersonAssignment(IPerson agent)
        {
            return CreatePersonAssignment(agent, ScenarioFactory.CreateScenarioAggregate());
        }

        public static IPersonAssignment CreatePersonAssignment(IPerson agent, IScenario scenario)
        {
            return CreatePersonAssignment(agent, scenario, new DateOnly(2000,1,1));
        }

			public static IPersonAssignment CreatePersonAssignment(IPerson agent, IScenario scenario, DateOnly date)
			{
				return new PersonAssignment(agent, scenario, date);
			}

        /// <summary>
        /// Creates an assignment with personal and main shift.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="agent">The agent.</param>
        /// <param name="period">The period.</param>
        /// <param name="category">The category</param>
        /// <param name="scenario">The scenario</param>
        /// <returns></returns>
        public static IPersonAssignment CreateAssignmentWithMainShiftAndPersonalShift(IActivity activity,
                                                                                    IPerson agent,
                                                                                    DateTimePeriod period,
                                                                                    IShiftCategory category,
                                                                                    IScenario scenario)
        {
            IPersonAssignment ass = new PersonAssignment(agent, scenario, new DateOnly(period.LocalStartDateTime));
					ass.AddPersonalLayer(activity, period);
	        ass.SetMainShiftLayers(new [] {new MainShiftLayer(activity, period)}, category);
            return ass;
        }

        /// <summary>
        /// Creates an assignment with main shift.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="agent">The agent.</param>
        /// <param name="period">The period.</param>
        /// <param name="category">The category</param>
        /// <param name="scenario">The scenario</param>
        /// <returns></returns>
        public static IPersonAssignment CreateAssignmentWithMainShift(IActivity activity,
                                                                    IPerson agent,
                                                                    DateTimePeriod period,
                                                                    IShiftCategory category,
                                                                    IScenario scenario)
        {
	        var date =new DateOnly(TimeZoneHelper.ConvertFromUtc(period.StartDateTime, agent.PermissionInformation.DefaultTimeZone()));
            var ass = new PersonAssignment(agent, scenario, date);
					ass.SetMainShiftLayers(new[]{new MainShiftLayer(activity, period)}, category);
            return ass;
        }

        public static IPersonAssignment CreateAssignmentWithMainShift(IPerson person,
                                                                     DateTimePeriod period)
        {
            return CreateAssignmentWithMainShift(ScenarioFactory.CreateScenarioAggregate(),
                                                 person,
                                                 period);
        }

        public static IPersonAssignment CreateAssignmentWithMainShift(IScenario scenario,
                                                                     IPerson person, 
                                                                     DateTimePeriod period)
        {
            return CreateAssignmentWithMainShift(scenario,
                                                 person,
                                                 period,
                                                 ShiftCategoryFactory.CreateShiftCategory("sdf"));
        }

				public static IPersonAssignment CreateAssignmentWithMainShift(IScenario scenario,
																															 IPerson person,
																															 DateTimePeriod period,
																															IShiftCategory shiftCategory)
				{
					return CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("sdf"),
																							 person,
																							 period,
																							 shiftCategory,
																							 scenario);
				}

        /// <summary>
        /// Creates an assignment with personal shift.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="person">The agent.</param>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        public static IPersonAssignment CreateAssignmentWithPersonalShift(IActivity activity,
                                                                        IPerson person,
                                                                        DateTimePeriod period,
                                                                        IScenario scenario)
        {
					IPersonAssignment ass = new PersonAssignment(person, scenario, new DateOnly(period.LocalStartDateTime));
					ass.AddPersonalLayer(activity, period);
            return ass;
        }

        public static IPersonAssignment CreateAssignmentWithPersonalShift(IPerson person,
                                                                       DateTimePeriod period)
        {
            return CreateAssignmentWithPersonalShift(new Activity("Activity"), person, period,new Scenario("Scenario"));
        }

		public static IPersonAssignment CreateAssignmentWithOvertimeShift(IActivity activity,
																		IPerson person,
																		DateTimePeriod period,
																		IScenario scenario)
		{
			IPersonAssignment ass = new PersonAssignment(person, scenario, new DateOnly(period.LocalStartDateTime));
			IMultiplicatorDefinitionSet multiplicatorDefinitionSet =
				MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("a", MultiplicatorType.Overtime);
			ass.AddOvertimeLayer(activity, period, multiplicatorDefinitionSet);
			return ass;
		}

        /// <summary>
        /// Creates an empty agentassignment.
        /// </summary>
        /// <returns></returns>
        public static IPersonAssignment CreatePersonAssignmentEmpty()
        {
            IPerson agent = PersonFactory.CreatePerson("grisisi");
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate("Heja Gnaget!", false);
            IPersonAssignment ass = new PersonAssignment(agent, scenario, new DateOnly(2000,1,1));
            return ass;
        }

        /// <summary>
        /// Creates the person assignment list for Resource Divider test.
        /// </summary>
        public static PersonAssignmentListContainer CreatePersonAssignmentListForActivityDividerTest()
        {
            IList<IProjectionService> list = new List<IProjectionService>();
            PersonAssignmentListContainer container = new PersonAssignmentListContainer(list);

            // general
            IShiftCategory caMorning = ShiftCategoryFactory.CreateShiftCategory("Morning");
            IScenario scDefault = ScenarioFactory.CreateScenarioAggregate("Default", false);
            container.Scenario = scDefault;
            ITeam team = TeamFactory.CreateSimpleTeam();
            IPersonContract personContract = PersonContractFactory.CreatePersonContract();

            IScheduleDictionary dic = new ScheduleDictionary(scDefault, new ScheduleDateTimePeriod(new DateTimePeriod(1900,1,1,2200,1,1)));


            // create activities
            CreateActivitiesAndAddToContainer(container);

            // create skills
            CreateSkillsAndAddToContainer(container);

            // create persons
            CreatePersonsAndAddToContainer(container);

            // assignments
            IPersonAssignment assignment1 = new PersonAssignment(container.ContainedPersons["Person1"], scDefault, new DateOnly(2008,1,2));
						IPersonAssignment assignment2 = new PersonAssignment(container.ContainedPersons["Person2"], scDefault, new DateOnly(2008, 1, 2));
						IPersonAssignment assignment3 = new PersonAssignment(container.ContainedPersons["Person3"], scDefault, new DateOnly(2008, 1, 2));
						IPersonAssignment assignment4 = new PersonAssignment(container.ContainedPersons["Person4"], scDefault, new DateOnly(2008, 1, 2));

            // periods
            DateTimePeriod prdPerson1Phone = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 9, 30, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 5, 0, DateTimeKind.Utc));
            DateTimePeriod prdPerson1Break = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 5, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 10, 0, DateTimeKind.Utc));
            DateTimePeriod prdPerson1Office = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 10, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 45, 0, DateTimeKind.Utc));
            DateTimePeriod prdPerson2Phone = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 9, 0, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 11, 0, 0, DateTimeKind.Utc));
            DateTimePeriod prdPerson3Lunch = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 0, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 30, 0, DateTimeKind.Utc));
            DateTimePeriod prdPerson4Phone = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 9, 30, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 10, 0, DateTimeKind.Utc));
            DateTimePeriod prdPerson4Office = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 10, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 45, 0, DateTimeKind.Utc));

            // activity layers
            var alPerson1Phone = new MainShiftLayer(container.ContainedActivities["Phone"], prdPerson1Phone);
						var alPerson1Break = new MainShiftLayer(container.ContainedActivities["Break"], prdPerson1Break);
						var alPerson1Office = new MainShiftLayer(container.ContainedActivities["Office"], prdPerson1Office);
						var alPerson2Phone = new MainShiftLayer(container.ContainedActivities["Phone"], prdPerson2Phone);
						var alPerson3Lunch = new MainShiftLayer(container.ContainedActivities["Lunch"], prdPerson3Lunch);
						var alPerson4Phone = new MainShiftLayer(container.ContainedActivities["Phone"], prdPerson4Phone);
						var alPerson4Office = new MainShiftLayer(container.ContainedActivities["Office"], prdPerson4Office);

            // main shifts
					assignment1.SetMainShiftLayers(new[]
						{
							alPerson1Phone,
							alPerson1Break,
							alPerson1Office
						}, caMorning);
					assignment2.SetMainShiftLayers(new[]{alPerson2Phone}, caMorning);
					assignment3.SetMainShiftLayers(new[]{alPerson3Lunch}, caMorning);
					assignment4.SetMainShiftLayers(new[]{alPerson4Phone, alPerson4Office}, caMorning);

            // Person Periods
            IPersonPeriod ppPerson1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), personContract, team);
            ppPerson1.AddPersonSkill(PersonSkillFactory.CreatePersonSkill(container.ContainedSkills["PhoneA"], 2));
            ppPerson1.AddPersonSkill(PersonSkillFactory.CreatePersonSkill(container.ContainedSkills["PhoneB"], 1));
            ppPerson1.AddPersonSkill(PersonSkillFactory.CreatePersonSkill(container.ContainedSkills["OfficeA"], 1));
            ppPerson1.AddPersonSkill(PersonSkillFactory.CreatePersonSkill(container.ContainedSkills["OfficeB"], 1));
            container.ContainedPersons["Person1"].AddPersonPeriod(ppPerson1);
            IPersonPeriod ppPerson2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), personContract, team);
            ppPerson2.AddPersonSkill(PersonSkillFactory.CreatePersonSkill(container.ContainedSkills["PhoneA"], 1));
            ppPerson2.AddPersonSkill(PersonSkillFactory.CreatePersonSkill(container.ContainedSkills["PhoneB"], 1));
            container.ContainedPersons["Person2"].AddPersonPeriod(ppPerson2);
            IPersonPeriod ppPerson3 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), personContract, team);
            container.ContainedPersons["Person3"].AddPersonPeriod(ppPerson3);
            IPersonPeriod ppPerson4 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), personContract, team);
            ppPerson4.AddPersonSkill(PersonSkillFactory.CreatePersonSkill(container.ContainedSkills["PhoneB"], 1));
            container.ContainedPersons["Person4"].AddPersonPeriod(ppPerson4);

            // create list
            var scheduleRange = ExtractedSchedule.CreateScheduleDay(dic, container.ContainedPersons["Person1"], new DateOnly(2008,1,2));
            scheduleRange.Add(assignment1);
            container.PersonAssignmentListForActivityDividerTest.Add(assignment1);
            IProjectionService svc = scheduleRange.ProjectionService();
            svc.CreateProjection();
            list.Add(svc);

            scheduleRange = ExtractedSchedule.CreateScheduleDay(dic, container.ContainedPersons["Person2"], new DateOnly(2008, 1, 2));
            scheduleRange.Add(assignment2);
            container.PersonAssignmentListForActivityDividerTest.Add(assignment2);
            svc = scheduleRange.ProjectionService();
            svc.CreateProjection();
            list.Add(svc);

            scheduleRange = ExtractedSchedule.CreateScheduleDay(dic, container.ContainedPersons["Person3"], new DateOnly(2008, 1, 2));
            scheduleRange.Add(assignment3);
            container.PersonAssignmentListForActivityDividerTest.Add(assignment3);
            svc = scheduleRange.ProjectionService();
            svc.CreateProjection();
            list.Add(svc);

            scheduleRange = ExtractedSchedule.CreateScheduleDay(dic, container.ContainedPersons["Person4"], new DateOnly(2008, 1, 2));
            scheduleRange.Add(assignment4);
            container.PersonAssignmentListForActivityDividerTest.Add(assignment4);
            svc = scheduleRange.ProjectionService();
            svc.CreateProjection();
            list.Add(svc);

            return container;
        }

        /// <summary>
        /// Creates the activity and add to container.
        /// </summary>
        /// <param name="container">The container.</param>
        private static void CreateActivitiesAndAddToContainer(PersonAssignmentListContainer container)
        {
            // CreateProjection activities
            IActivity acPhone = ActivityFactory.CreateActivity("Phone");
            IActivity acOffice = ActivityFactory.CreateActivity("Office", Color.Yellow);
            IActivity acBreak = ActivityFactory.CreateActivity("Break", Color.Red);
            IActivity acLunch = ActivityFactory.CreateActivity("Lunch", Color.Red);
            container.ContainedActivities.Add(acPhone.Description.Name, acPhone);
            container.ContainedActivities.Add(acOffice.Description.Name, acOffice);
            container.ContainedActivities.Add(acBreak.Description.Name, acBreak);
            container.ContainedActivities.Add(acLunch.Description.Name, acLunch);
        }

        private static void CreatePersonsAndAddToContainer(PersonAssignmentListContainer container)
        {
            // CreateProjection persons
            IPerson person1 = PersonFactory.CreatePerson("Person1");
            IPerson person2 = PersonFactory.CreatePerson("Person2");
            IPerson person3 = PersonFactory.CreatePerson("Person3");
            IPerson person4 = PersonFactory.CreatePerson("Person4");
            container.ContainedPersons.Add(person1.Name.FirstName, person1);
            container.ContainedPersons.Add(person2.Name.FirstName, person2);
            container.ContainedPersons.Add(person3.Name.FirstName, person3);
            container.ContainedPersons.Add(person4.Name.FirstName, person4);
        }

        private static void CreateSkillsAndAddToContainer(PersonAssignmentListContainer container)
        {
            // CreateProjection persons
            // CreateProjection Skill Type
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

            ISkill skPhoneA = SkillFactory.CreateSkill("PhoneA");
            skPhoneA.Activity = container.ContainedActivities["Phone"];
            skPhoneA.TimeZone = timeZone;
            ISkill skPhoneB = SkillFactory.CreateSkill("PhoneB");
            skPhoneB.Activity = container.ContainedActivities["Phone"];
            skPhoneB.TimeZone = timeZone;
            ISkill skPhoneC = SkillFactory.CreateSkill("PhoneC");
            skPhoneC.Activity = container.ContainedActivities["Phone"];
            skPhoneC.TimeZone = timeZone;
            ISkill skOfficeA = SkillFactory.CreateSkill("OfficeA");
            skOfficeA.Activity = container.ContainedActivities["Office"];
            skOfficeA.TimeZone = timeZone;
            ISkill skOfficeB = SkillFactory.CreateSkill("OfficeB");
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

	    public static IPersonAssignment CreateAssignmentWithThreeMainshiftLayers()
	    {
		    var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
		    var ret = new PersonAssignment(new Person(), new Scenario("d"), new DateOnly(2000, 1, 1));
		    ret.SetMainShiftLayers(
			    new[]
				    {
					    new MainShiftLayer(new Activity("1"), new DateTimePeriod(start, start.AddHours(1))),
					    new MainShiftLayer(new Activity("2"), new DateTimePeriod(start, start.AddHours(2))),
					    new MainShiftLayer(new Activity("3"), new DateTimePeriod(start, start.AddHours(3)))
				    },
			    new ShiftCategory("test"));
		    return ret;
	    }

			public static IPersonAssignment CreateAssignmentWithThreePersonalLayers()
			{
				var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
				var ret = new PersonAssignment(new Person(), new Scenario("d"), new DateOnly(2000, 1, 1));
				ret.AddPersonalLayer(new Activity("1"), new DateTimePeriod(start, start.AddHours(1)));
				ret.AddPersonalLayer(new Activity("2"), new DateTimePeriod(start, start.AddHours(2)));
				ret.AddPersonalLayer(new Activity("3"), new DateTimePeriod(start, start.AddHours(3)));
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
				ret.AddOvertimeLayer(act, new DateTimePeriod(start.AddHours(5), start.AddHours(6)), multi);
				ret.AddOvertimeLayer(act, new DateTimePeriod(start.AddHours(5), start.AddHours(7)), multi);
				ret.AddOvertimeLayer(act, new DateTimePeriod(start.AddHours(5), start.AddHours(8)), multi);
				return ret;
			}

			public static IPersonAssignment CreateAssignmentWithDayOff(IScenario scenario, IPerson person, DateOnly date, TimeSpan length, TimeSpan flexibility, TimeSpan anchor)
			{
				var ass = new PersonAssignment(person, scenario, date);
				var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("test"));
				dayOffTemplate.Anchor = anchor;
				dayOffTemplate.SetTargetAndFlexibility(length, flexibility);
				ass.SetDayOff(dayOffTemplate);
				return ass;
			}
    }

    /// <summary>
    /// Test class for PersonAssignment test class
    /// </summary>
    public class PersonAssignmentListContainer : Container<IList<IProjectionService>>
    {
        private readonly IList<ISkill> _allSkills;
        private readonly IDictionary<string, ISkill> _containedSkills;
        private readonly IDictionary<string, IActivity> _containedActivities;
        private readonly IDictionary<string, IPerson> _containedPersons;
        public IList<IPersonAssignment> PersonAssignmentListForActivityDividerTest { get; private set; }
        public IScenario Scenario { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAssignmentListContainer"/> class.
        /// </summary>
        /// <param name="testClass">The domain class.</param>
        public PersonAssignmentListContainer(IList<IProjectionService> testClass)
            : base(testClass)
        {
            _allSkills = new List<ISkill>();
            _containedSkills = new Dictionary<string, ISkill>();
            _containedActivities = new Dictionary<string, IActivity>();
            _containedPersons = new Dictionary<string, IPerson>();
            PersonAssignmentListForActivityDividerTest = new List<IPersonAssignment>();
        }

        /// <summary>
        /// Gets the contained skills.
        /// </summary>
        public IDictionary<string, ISkill> ContainedSkills
        {
            get { return _containedSkills; }
        }

        /// <summary>
        /// Gets all the skills.
        /// </summary>
        public IList<ISkill> AllSkills
        {
            get { return _allSkills; }
        }

        /// <summary>
        /// Gets the contained activity.
        /// </summary>
        public IDictionary<string, IActivity> ContainedActivities
        {
            get { return _containedActivities; }
        }

        /// <summary>
        /// Gets the contained persons.
        /// </summary>
        public IDictionary<string, IPerson> ContainedPersons
        {
            get { return _containedPersons; }
        }

        /// <summary>
        /// Gets the test data.
        /// </summary>
        /// <value>The test data.</value>
        public IList<IProjectionService> TestData
        {
            get { return Content; }
        }

		public IList<IVisualLayerCollection> TestVisualLayerCollection()
        {
			IList<IVisualLayerCollection> ret = new List<IVisualLayerCollection>();
            foreach (var projectionService in TestData)
            {
            	var projection = projectionService.CreateProjection();
                ret.Add(projection);
            }

            return ret;
        }


		public IList<IFilteredVisualLayerCollection> TestFilteredVisualLayerCollection()
		{
			IList<IFilteredVisualLayerCollection> ret = new List<IFilteredVisualLayerCollection>();
			foreach (var projectionService in TestData)
			{
				var projection = projectionService.CreateProjection();
				var coll = new FilteredVisualLayerCollection(projection.Person, projection.ToList(), new ProjectionIntersectingPeriodMerger(), projection);
				ret.Add(coll);
			}

			return ret;
		}

		public IList<IFilteredVisualLayerCollection> TestFilteredVisualLayerCollectionWithSamePerson()
		{
			IPerson person = TestData[0].CreateProjection().Person;

			return TestData
				   .Select(projectionService => projectionService.CreateProjection())
				   .Select(projection => new FilteredVisualLayerCollection(person, projection.ToList(), new ProjectionIntersectingPeriodMerger(), projection))
				   .Take(2)
				   .Cast<IFilteredVisualLayerCollection>().ToList();
		}
    }
}