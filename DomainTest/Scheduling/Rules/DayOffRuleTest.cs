﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[DomainTest]
	public class DayOffRuleTest
    {
        private IActivity _activity;
        private ShiftCategory _category;
        private IPersonAssignment _personAssignmentJustBeforeDayOff;
        private IPerson _person;
        private DateTimePeriod _range;
        private IScenario _scenario;
        private IScheduleRange _scheduleRange;
        private DayOffRule _target;
        private MockRepository _mocks;
        private IScheduleDictionary _dic;
	    private IPersistableScheduleDataPermissionChecker _permissionChecker;

		public FullScheduling Target;
		public FakeActivityRepository ActivityRepository;
	    public FakeSkillRepository SkillRepository;
	    public FakeScenarioRepository ScenarioRepository;
	    public FakePersonRepository PersonRepository;
	    public FakeSkillDayRepository SkillDayRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public SchedulingOptionsProvider SchedulingOptionsProvider;
		public FakeBusinessUnitRepository BusinessUnitRepository;

        private void setup()
        {
            _mocks = new MockRepository();
            _dic = _mocks.StrictMock<IScheduleDictionary>();
           _start = new DateTime(2007, 8, 2, 8, 30, 0, DateTimeKind.Utc);
           _end = new DateTime(2007, 8, 2, 17, 30, 0, DateTimeKind.Utc);
            _range = new DateTimePeriod(2007, 8, 1, 2007, 8, 5);
			_permissionChecker = new PersistableScheduleDataPermissionChecker();


		   _scenario = ScenarioFactory.CreateScenarioAggregate();
           _category = ShiftCategoryFactory.CreateShiftCategory("myCategory");
           _activity = ActivityFactory.CreateActivity("Phone");
           _person = PersonFactory.CreatePerson();
           _person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

           DayOffTemplate dayOff1 = new DayOffTemplate(new Description("test"));
           dayOff1.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(9));
           dayOff1.Anchor = new TimeSpan(10, 30, 0);

           // add this and the day off cannot be moved backwards
           _personAssignmentJustBeforeDayOff = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(_person, _scenario, _activity, new DateTimePeriod(_start.AddHours(-3), _end.AddHours(-3)), _category);

           Expect.Call(_dic.Scenario).Return(_scenario).Repeat.Any();
           _mocks.Replay(_dic);
           _scheduleRange =
               new ScheduleRange(_dic,
                   new ScheduleParameters(_scenario, _person, _range), _permissionChecker);
        }

        private void createDayOffRule()
        {
            _target = new DayOffRule();
        }


        private DateTime _start, _end;

		[Test, SetCulture("en-US"), SetUICulture("en-US")]
		public void ShouldBeAbleToScheduleWith36HourDayOffExposesBug38242()
		{
			setup();
			var firstDay = new DateOnly(2016, 5, 22);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var site = new Site("site");
			var team = new Team { Description = new Description("team") };
			site.AddTeam(team);
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_");
			var agent1 = PersonRepository.Has(contract, ContractScheduleFactory.CreateWorkingWeekContractSchedule(),
				new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.NewYorkTimeZoneInfo());
			agent1.PermissionInformation.SetCulture(CultureInfoFactory.CreateUsCulture());
			agent1.FirstDayOfWeek = DayOfWeek.Sunday;
			var shiftCategory = new ShiftCategory("_").WithId();
			var normalRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 30, 8, 30, 15), new TimePeriodWithSegment(16, 30, 16, 30, 15), shiftCategory));
			var ruleSetBag = new RuleSetBag(normalRuleSet);
			agent1.Period(firstDay).RuleSetBag = ruleSetBag;

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				10,
				10,
				10,
				10,
				10,
				10,
				10)
				);

			var dayOffTemplate = new DayOffTemplate(new Description("_"));
			dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(6));
			dayOffTemplate.Anchor = TimeSpan.FromHours(12.5);
			AssignmentRepository.Has(agent1, scenario, dayOffTemplate, firstDay.AddDays(1));
			AssignmentRepository.Add(new PersonAssignment(agent1, scenario, firstDay.AddDays(2))); //empty person assignment
			AssignmentRepository.Has(agent1, scenario, dayOffTemplate, firstDay.AddDays(3));
			DayOffTemplateRepository.Add(dayOffTemplate);
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
			});

			Target.DoScheduling(period);

			var assignments = AssignmentRepository.Find(new[] { agent1 }, new DateOnlyPeriod(firstDay, firstDay), scenario);
			assignments.Count.Should().Be.EqualTo(1);

		}


		#region LatestStartTimeForAssignment Tests

		[Test]
        public void VerifyBeforeDayOffWithAssignmentAfter()
        {
			setup();
			createDayOffRule();
            var dayOff = new DayOffTemplate(new Description("test"));
            dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(4));
            dayOff.Anchor = TimeSpan.FromHours(14); //för att få 12:00 UTC
            ((Schedule)_scheduleRange).Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(_person, _scenario, new DateOnly(2007, 8, 2), dayOff));

            ((Schedule)_scheduleRange).Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person,
                                                                                  _scenario, new DateTimePeriod(new DateTime(2007, 8, 3, 1, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 3, 3, 0, 0, DateTimeKind.Utc))));

            var expected = new DateTimePeriod(new DateTime(2007, 8, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 2, 1, 0, 0, DateTimeKind.Utc));

            var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, new DateOnly(2007, 8, 1));
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void VerifyBeforeDayOffWithNoAssignmentAfter()
		{
			setup();
			createDayOffRule();
            DayOffTemplate dayOff = new DayOffTemplate(new Description("test"));
            dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(4));
            dayOff.Anchor = TimeSpan.FromHours(14); //för att få 12:00 UTC

						((Schedule)_scheduleRange).Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(_person, _scenario, new DateOnly(2007, 8, 2), dayOff));


            var expected = new DateTimePeriod(new DateTime(2007, 8, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 2, 4, 0, 0, DateTimeKind.Utc));

            var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, new DateOnly(2007, 8, 1));
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void VerifyAfterDayOffWithAssignmentBeforeOnTheSameDayAsDayOff()
        {
			setup();
			createDayOffRule();
            DayOffTemplate dayOff = new DayOffTemplate(new Description("test"));
            dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(6));
            dayOff.Anchor = TimeSpan.FromHours(12);

	        var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person,
	                                           _scenario, new DateTimePeriod(new DateTime(2007, 8, 2, 10, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 2, 11, 0, 0, DateTimeKind.Utc)));
					ass.SetDayOff(dayOff);

            ((Schedule)_scheduleRange).Add(ass);

            var expected = new DateTimePeriod(new DateTime(2007, 8, 2, 16, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 6, 22, 0, 0, DateTimeKind.Utc));

            var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, new DateOnly(2007, 8, 3));
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void VerifyBeforeDayOffWithAssignmentAfterOnTheSameDayAsDayOff()
        {
			setup();
			createDayOffRule();
            DayOffTemplate dayOff = new DayOffTemplate(new Description("test"));
            dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(6));
            dayOff.Anchor = TimeSpan.FromHours(12);

	        var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person,
	                                                                        _scenario, new DateTimePeriod(new DateTime(2007, 8, 2, 13, 0, 0, DateTimeKind.Utc),
			        new DateTime(2007, 8, 2, 14, 0, 0, DateTimeKind.Utc)));
					ass.SetDayOff(dayOff);
            ((Schedule)_scheduleRange).Add(ass);

            var expected = new DateTimePeriod(new DateTime(2007, 8, 2, 16, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 6, 22, 0, 0, DateTimeKind.Utc));

            var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, new DateOnly(2007, 8, 3));
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void VerifyAfterDayOffWithNoAssignmentBefore()
        {
			setup();
			createDayOffRule();
            DayOffTemplate dayOff = new DayOffTemplate(new Description("test"));
            dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(4));
            dayOff.Anchor = TimeSpan.FromHours(14); //för att få 12:00 UTC
            ((Schedule)_scheduleRange).Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(_person, _scenario , new DateOnly(2007, 8, 2), dayOff));

            var expected = new DateTimePeriod(new DateTime(2007, 8, 2, 20, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 6, 22, 0, 0, DateTimeKind.Utc));

            var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, new DateOnly(2007, 8, 3));
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void VerifyCanFindLongestDateTimePeriodForAssignmentWhenSeekingBetweenTwoDayOffs()
        {
			setup();
			createSchedulePartWithTwoDayOffs();
            DateTime anchorDayOffTwo = new DateTime(2007, 8, 5, 8, 30, 0);
            DateTime anchorDayOffOne = new DateTime(2007, 8, 3, 8, 30, 0);
 
            createDayOffRule();
            var expected = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(anchorDayOffOne.AddHours(9), anchorDayOffTwo.AddHours(-9), _person.PermissionInformation.DefaultTimeZone());
            var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, new DateOnly(2007, 8, 3));
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void VerifyCanFindLongestDateTimePeriodForAssignmentWhenSeekingBetweenTwoDayOffsAndAssignmentAfterHindersBend()
        {
			setup();
			createSchedulePartWithTwoDayOffs();
            //DateTime anchorDayOffTwo = new DateTime(2007, 8, 5, 8, 30, 0);
            DateTime anchorDayOffOne = new DateTime(2007, 8, 3, 8, 30, 0);
            DateTime two = new DateTime(2007, 8, 5, 8, 30, 0 , DateTimeKind.Utc);
            IPersonAssignment personAssignmentJustAfterDayOffTwo = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(_person, _scenario, _activity, new DateTimePeriod(two.AddHours(18), two.AddHours(26)), _category);
            ((Schedule)_scheduleRange).Add(personAssignmentJustAfterDayOffTwo);

            createDayOffRule();
            var expected = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(anchorDayOffOne.AddHours(9), _scheduleRange.Person.PermissionInformation.DefaultTimeZone()),
                personAssignmentJustAfterDayOffTwo.Period.StartDateTime.Add(TimeSpan.FromHours(-36)));
            var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, new DateOnly(2007, 8, 3));
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void VerifyCanFindLongestDateTimePeriodForAssignmentWhenSeekingBetweenTwoDayOffsAndAssignmentBeforeHindersBend()
        {
			setup();
			createSchedulePartWithTwoDayOffs();
            DateTime anchorDayOffTwo = new DateTime(2007, 8, 5, 8, 30, 0);
            //DateTime anchorDayOffOne = new DateTime(2007, 8, 3, 8, 30, 0);

            ((Schedule)_scheduleRange).Add(_personAssignmentJustBeforeDayOff);

            createDayOffRule();

            DateTime es = _personAssignmentJustBeforeDayOff.Period.EndDateTime.Add(TimeSpan.FromHours(36));
            var expected = new DateTimePeriod(es, TimeZoneHelper.ConvertToUtc(anchorDayOffTwo.AddHours(-9), _scheduleRange.Person.PermissionInformation.DefaultTimeZone()));
            var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, new DateOnly(2007, 8, 3));
            Assert.AreEqual(expected, result);
        }

        private void createSchedulePartWithTwoDayOffs()
        {
            // create a longer range
            _range = new DateTimePeriod(2007, 8, 1, 2007, 8, 15);

						DayOffTemplate dayOff = new DayOffTemplate(new Description("test"));
						dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(9));
						dayOff.Anchor = new TimeSpan(8, 30, 0);
						var personDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(_person, _scenario, new DateOnly(2007, 8, 3), dayOff);


            DayOffTemplate dOff = new DayOffTemplate(new Description("test"));
            dOff.Anchor = new TimeSpan(8, 30, 0);
            dOff.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(9));
						var personDayOff2 = PersonAssignmentFactory.CreateAssignmentWithDayOff(_person, _scenario, new DateOnly(2007, 8, 5), dOff);
            

           _scheduleRange = new ScheduleRange(_dic, new ScheduleParameters(_scenario, _person, _range), _permissionChecker);

           ((Schedule)_scheduleRange).Add(personDayOff);
           ((Schedule)_scheduleRange).Add(personDayOff2);

        }

        #endregion

        //[TearDown]
        //public void Teardown()
        //{
        //    _mocks.Verify(_dic);
        //}
	}
}
