using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for PersonAssignment domain object
    /// </summary>
    public static class PersonAssignmentFactory
    {
        /// <summary>
        /// Creates the agent assignment aggregate.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="mainShift">The main shift.</param>
        /// <param name="personalShiftCollection">The personal shift collection.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        public static IPersonAssignment CreatePersonAssignmentAggregate(IPerson agent,
                                                                     IMainShift mainShift,
                                                                     ICollection<IPersonalShift> personalShiftCollection,
                                                                     IScenario scenario)
        {
            IPersonAssignment ret = new PersonAssignment(agent, scenario);
            ret.SetMainShift(mainShift);
            //todo: rk - lägg till en AddRange istället!
            foreach (IPersonalShift personalShift in personalShiftCollection)
            {
                ret.AddPersonalShift(personalShift);
            }
            return ret;
        }


        /// <summary>
        /// Creates a person assignment test data.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <returns></returns>
        public static IPersonAssignment CreatePersonAssignment(IPerson agent)
        {
            return CreatePersonAssignment(agent, ScenarioFactory.CreateScenarioAggregate());
        }

        public static IPersonAssignment CreatePersonAssignment(IPerson agent, IScenario scenario)
        {
            IPersonAssignment ret = new PersonAssignment(agent, scenario);
            return ret;
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
            IPersonAssignment ass = new PersonAssignment(agent, scenario);
            ass.AddPersonalShift(PersonalShiftFactory.CreatePersonalShift(activity, period));
            ass.SetMainShift(MainShiftFactory.CreateMainShift(activity, period, category));
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
            PersonAssignment ass = new PersonAssignment(agent, scenario);
            ass.SetMainShift(MainShiftFactory.CreateMainShift(activity, period, category));
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
            return CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("sdf"),
                                                 person,
                                                 period,
                                                 ShiftCategoryFactory.CreateShiftCategory("sdf"),
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
            IPersonAssignment ass = new PersonAssignment(person, scenario);
            ass.AddPersonalShift(PersonalShiftFactory.CreatePersonalShift(activity, period));
            return ass;
        }

        public static IPersonAssignment CreateAssignmentWithPersonalShift(IPerson person,
                                                                       DateTimePeriod period)
        {
            return CreateAssignmentWithPersonalShift(new Activity("Activity"), person, period,new Scenario("Scenario"));
        }

		/// <summary>
		/// Creates an assignment with main shift and an overtime shift.
		/// </summary>
		/// <param name="activity">The activity.</param>
		/// <param name="agent">The agent.</param>
		/// <param name="period">The period.</param>
		/// <param name="category">The category</param>
		/// <param name="scenario">The scenario</param>
		/// <returns></returns>
		public static IPersonAssignment CreateAssignmentWithMainShiftAndOvertimeShift(IScenario scenario,
															 IPerson person,
															 DateTimePeriod period)
		{
			var activity = ActivityFactory.CreateActivity("sdf");
			var category = ShiftCategoryFactory.CreateShiftCategory("sdf");
			var ass = CreateAssignmentWithMainShift(activity,
												 person,
												 period,
												 category,
												 scenario);
			ass.SetMainShift(MainShiftFactory.CreateMainShift(activity, period, category));
			IMultiplicatorDefinitionSet multiplicatorDefinitionSet =
				MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("a", MultiplicatorType.Overtime);
			OvertimeShiftFactory.CreateOvertimeShift(activity, period, multiplicatorDefinitionSet, ass);
			return ass;
		}
		
		public static IPersonAssignment CreateAssignmentWithOvertimeShift(IActivity activity,
																		IPerson person,
																		DateTimePeriod period,
																		IScenario scenario)
		{
			IPersonAssignment ass = new PersonAssignment(person, scenario);
			IMultiplicatorDefinitionSet multiplicatorDefinitionSet =
				MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("a", MultiplicatorType.Overtime);
			ass.AddOvertimeShift(OvertimeShiftFactory.CreateOvertimeShift(activity, period, multiplicatorDefinitionSet, ass));
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
            IPersonAssignment ass = new PersonAssignment(agent, scenario);
            return ass;
        }

        /// <summary>
        /// Creates the complete AgentAssignment list for GUI test.
        /// </summary>
        /// <returns></returns>
        public static IList<IPersonAssignment> CreateCompletePersonAssignmentListForGuiTest()
        {
            IShiftCategory caMorning = ShiftCategoryFactory.CreateShiftCategory("Morning");
            IScenario scDefault = ScenarioFactory.CreateScenarioAggregate("Heja Gnaget!", false);

            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2000, 1, 2, 10, 30, 0, DateTimeKind.Utc), new DateTime(2000, 1, 2, 11, 30, 0, DateTimeKind.Utc));
            DateTimePeriod period2 =
                new DateTimePeriod(new DateTime(2000, 1, 2, 11, 30, 0, DateTimeKind.Utc), new DateTime(2000, 1, 2, 12, 0, 0, DateTimeKind.Utc));
            DateTimePeriod period3 =
                new DateTimePeriod(new DateTime(2000, 1, 2, 12, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 2, 14, 0, 0, DateTimeKind.Utc));
            DateTimePeriod period4 =
                new DateTimePeriod(new DateTime(2000, 1, 2, 14, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 2, 14, 30, 0, DateTimeKind.Utc));
            DateTimePeriod period5 =
                new DateTimePeriod(new DateTime(2000, 1, 2, 14, 30, 0, DateTimeKind.Utc), new DateTime(2000, 1, 2, 17, 0, 0, DateTimeKind.Utc));
            DateTimePeriod periodMeeting =
                new DateTimePeriod(new DateTime(2000, 1, 2, 13, 30, 0, DateTimeKind.Utc), new DateTime(2000, 1, 2, 14, 30, 0, DateTimeKind.Utc));

            IActivity acTelephone = ActivityFactory.CreateActivity("Telephone");
            IActivity acLunch = ActivityFactory.CreateActivity("Lunch", Color.Yellow);
            IActivity acBreak = ActivityFactory.CreateActivity("Break", Color.Red);
            IActivity acMeeting = ActivityFactory.CreateActivity("Meeting", Color.Red);

            MainShiftActivityLayer al1Tel = new MainShiftActivityLayer(acTelephone, period1);
            MainShiftActivityLayer al2Lunch = new MainShiftActivityLayer(acLunch, period2);
            MainShiftActivityLayer al3Tel = new MainShiftActivityLayer(acTelephone, period3);
            MainShiftActivityLayer al4Break = new MainShiftActivityLayer(acBreak, period4);
            MainShiftActivityLayer al5Tel = new MainShiftActivityLayer(acTelephone, period5);

            MainShift msh = new MainShift(caMorning);
            msh.LayerCollection.Add(al1Tel);
            msh.LayerCollection.Add(al2Lunch);
            msh.LayerCollection.Add(al3Tel);
            msh.LayerCollection.Add(al4Break);
            msh.LayerCollection.Add(al5Tel);

            IPersonalShift psh = PersonalShiftFactory.CreatePersonalShift(acMeeting, periodMeeting);

            IPerson agentAndreas = PersonFactory.CreatePerson("Andreas");

            // create AgentAssignment
            IPersonAssignment as1 = new PersonAssignment(agentAndreas, scDefault);
            as1.SetMainShift(msh);
            as1.AddPersonalShift(psh);

            IList<IPersonAssignment> listOfAssingments = new List<IPersonAssignment>();

            listOfAssingments.Add(as1);

            return listOfAssingments;
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
            IPersonAssignment assignment1 = new PersonAssignment(container.ContainedPersons["Person1"], scDefault);
            IPersonAssignment assignment2 = new PersonAssignment(container.ContainedPersons["Person2"], scDefault);
            IPersonAssignment assignment3 = new PersonAssignment(container.ContainedPersons["Person3"], scDefault);
            IPersonAssignment assignment4 = new PersonAssignment(container.ContainedPersons["Person4"], scDefault);

            // periods
            DateTimePeriod prdPerson1Phone = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 9, 30, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 5, 0, DateTimeKind.Utc));
            DateTimePeriod prdPerson1Break = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 5, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 10, 0, DateTimeKind.Utc));
            DateTimePeriod prdPerson1Office = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 10, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 45, 0, DateTimeKind.Utc));
            DateTimePeriod prdPerson2Phone = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 9, 0, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 11, 0, 0, DateTimeKind.Utc));
            DateTimePeriod prdPerson3Lunch = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 0, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 30, 0, DateTimeKind.Utc));
            DateTimePeriod prdPerson4Phone = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 9, 30, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 10, 0, DateTimeKind.Utc));
            DateTimePeriod prdPerson4Office = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 10, 0, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 45, 0, DateTimeKind.Utc));

            // activity layers
            MainShiftActivityLayer alPerson1Phone = new MainShiftActivityLayer(container.ContainedActivities["Phone"], prdPerson1Phone);
            MainShiftActivityLayer alPerson1Break = new MainShiftActivityLayer(container.ContainedActivities["Break"], prdPerson1Break);
            MainShiftActivityLayer alPerson1Office = new MainShiftActivityLayer(container.ContainedActivities["Office"], prdPerson1Office);
            MainShiftActivityLayer alPerson2Phone = new MainShiftActivityLayer(container.ContainedActivities["Phone"], prdPerson2Phone);
            MainShiftActivityLayer alPerson3Lunch = new MainShiftActivityLayer(container.ContainedActivities["Lunch"], prdPerson3Lunch);
            MainShiftActivityLayer alPerson4Phone = new MainShiftActivityLayer(container.ContainedActivities["Phone"], prdPerson4Phone);
            MainShiftActivityLayer alPerson4Office = new MainShiftActivityLayer(container.ContainedActivities["Office"], prdPerson4Office);

            // main shifts
            MainShift msPerson1 = new MainShift(caMorning);
            msPerson1.LayerCollection.Add(alPerson1Phone);
            msPerson1.LayerCollection.Add(alPerson1Break);
            msPerson1.LayerCollection.Add(alPerson1Office);
            assignment1.SetMainShift(msPerson1);
            MainShift msPerson2 = new MainShift(caMorning);
            msPerson2.LayerCollection.Add(alPerson2Phone);
            assignment2.SetMainShift(msPerson2);
            MainShift msPerson3 = new MainShift(caMorning);
            msPerson3.LayerCollection.Add(alPerson3Lunch);
            assignment3.SetMainShift(msPerson3);
            MainShift msPerson4 = new MainShift(caMorning);
            msPerson4.LayerCollection.Add(alPerson4Phone);
            msPerson4.LayerCollection.Add(alPerson4Office);
            assignment4.SetMainShift(msPerson4);

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