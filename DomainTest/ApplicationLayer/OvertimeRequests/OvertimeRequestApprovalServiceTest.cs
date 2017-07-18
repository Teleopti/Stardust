﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	[DomainTest]
	[TestFixture]
	public class OvertimeRequestApprovalServiceTest : ISetup
	{
		public IOvertimeRequestUnderStaffingSkillProvider OvertimeRequestUnderStaffingSkillProvider;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeSkillCombinationResourceRepository CombinationRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public ILoggedOnUser User;
		public ICurrentScenario Scenario;
		public MutableNow Now;
		public FakeSkillRepository SkillRepository;
		public IPrimaryPersonSkillFilter PrimaryPersonSkillFilter;
		public ISupportedSkillsInIntradayProvider SupportedSkillsInIntradayProvider;
		public IOvertimeRequestSkillProvider OvertimeRequestSkillProvider;
		public ISkillOpenHourFilter SkillOpenHourFilter;

		private OvertimeRequestApprovalService _target;
		private IScheduleDictionary _scheduleDictionary;
		private readonly IScenario _scenario = new Scenario("default") { DefaultScenario = true };
		private TimeSpan[] _intervals;
		private readonly TimePeriod _defaultOpenPeriod = new TimePeriod(8, 00, 21, 00);

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(_scenario).For<IScenario>();
			system.UseTestDouble<ScheduleDictionaryForTest>().For<IScheduleDictionary>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<SkillStaffingReadModelDataLoader>().For<ISkillStaffingReadModelDataLoader>();
			system.UseTestDouble<OvertimeRequestUnderStaffingSkillProvider>().For<IOvertimeRequestUnderStaffingSkillProvider>();
			system.UseTestDouble<PrimaryPersonSkillFilter>().For<IPrimaryPersonSkillFilter>();
			system.UseTestDouble<SupportedSkillsInIntradayProvider>().For<ISupportedSkillsInIntradayProvider>();
			system.UseTestDouble<OvertimeRequestSkillProvider>().For<IOvertimeRequestSkillProvider>();
			system.UseTestDouble<SkillOpenHourFilter>().For<ISkillOpenHourFilter>();
			_intervals = createIntervals();
		}

		[Test]
		public void ShouldAddCriticalUnderStaffingSkillActivityWhenIntradayRequestIsApproved()
		{
			var person = User.CurrentUser();
			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, 10d, 6d);
			addPersonSkillsToPersonPeriod(personSkill1);
			createAssignment(person, activity1);

			var requestPeriod = Now.ServerDate_DontUse().ToDateTimePeriod(new TimePeriod(19, 21), person.PermissionInformation.DefaultTimeZone());
			var personRequest = createOvertimeRequest(person, requestPeriod);

			_target = createTarget();
			var result = _target.Approve(personRequest.Request);

			var personAssignment = _scheduleDictionary.SchedulesForDay(Now.ServerDate_DontUse()).FirstOrDefault()?.PersonAssignment();

			result.Count().Should().Be(0);
			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(1);
			personAssignment.OvertimeActivities().First().Payload.Should().Be(skill1.Activity);
			personAssignment.OvertimeActivities().First().Period.Should().Be(requestPeriod);
		}

		[Test]
		public void ShouldAddCriticalUnderStaffingSkillActivityForPrimarySkillWhenIntradayRequestIsApproved()
		{
			var person = User.CurrentUser();
			var primarySkill = createSkill("primarySkill");
			primarySkill.SetCascadingIndex(1);
			setupIntradayStaffingForSkill(primarySkill, 10d, 6d);

			var nonPrimarySkill = createSkill("nonPrimarySkill");
			setupIntradayStaffingForSkill(nonPrimarySkill, 10d, 6d);

			var activity = createActivity("activity1");
			createAssignment(User.CurrentUser(), activity);
			var primaryPersonSkill = createPersonSkill(activity, primarySkill);
			var nonPrimaryPersonSkill = createPersonSkill(activity, nonPrimarySkill);

			addPersonSkillsToPersonPeriod(primaryPersonSkill, nonPrimaryPersonSkill);

			var requestPeriod = Now.ServerDate_DontUse().ToDateTimePeriod(new TimePeriod(19, 21), person.PermissionInformation.DefaultTimeZone());
			var personRequest = createOvertimeRequest(person, requestPeriod);

			_target = createTarget();
			var result = _target.Approve(personRequest.Request);

			var personAssignment = _scheduleDictionary.SchedulesForDay(Now.ServerDate_DontUse()).FirstOrDefault()?.PersonAssignment();

			result.Count().Should().Be(0);
			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(1);
			personAssignment.OvertimeActivities().First().Payload.Should().Be(primarySkill.Activity);
			personAssignment.OvertimeActivities().First().Period.Should().Be(requestPeriod);
		}

		[Test]
		public void ShouldAddCriticalUnderStaffingSkillActivityWhenCrossDayRequestIsApproved()
		{
			var person = User.CurrentUser();
			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1", new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1)));
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, 10d, 6d);
			addPersonSkillsToPersonPeriod(personSkill1);
			createAssignment(person, activity1);

			var requestPeriod = Now.ServerDate_DontUse().ToDateTimePeriod(new TimePeriod(19, 25), person.PermissionInformation.DefaultTimeZone());
			var personRequest = createOvertimeRequest(person, requestPeriod);

			_target = createTarget();
			var result = _target.Approve(personRequest.Request);

			var personAssignment = _scheduleDictionary.SchedulesForDay(Now.ServerDate_DontUse()).FirstOrDefault()?.PersonAssignment();

			result.Count().Should().Be(0);
			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(1);
			personAssignment.OvertimeActivities().First().Payload.Should().Be(skill1.Activity);
			personAssignment.OvertimeActivities().First().Period.Should().Be(requestPeriod);
		}

		[Test]
		public void ShouldNotApprovedWhenSkillOpenHourIsNotAvailable()
		{
			var person = User.CurrentUser();
			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, 10d, 6d);
			addPersonSkillsToPersonPeriod(personSkill1);
			createAssignment(person, activity1);

			var requestPeriod = Now.ServerDate_DontUse().ToDateTimePeriod(new TimePeriod(21, 22), person.PermissionInformation.DefaultTimeZone());
			var personRequest = createOvertimeRequest(person, requestPeriod);

			_target = createTarget();
			var result = _target.Approve(personRequest.Request);

			var personAssignment = _scheduleDictionary.SchedulesForDay(Now.ServerDate_DontUse()).FirstOrDefault()?.PersonAssignment();

			result.Count().Should().Be(1);
			result.First().Message.Should().Be(Resources.PeriodIsOutOfSkillOpenHours);
			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotApprovedWhenAgentSkillIsOutOfPersonPeriod()
		{
			var person = User.CurrentUser();
			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			var startDate = new DateOnly(2017, 7, 17);
			setupIntradayStaffingForSkill(skill1, 10d, 6d);
			addPersonSkillsToPersonPeriod(startDate, personSkill1);
			createAssignment(person, activity1);

			var requestPeriod = startDate.AddDays(-1).ToDateTimePeriod(new TimePeriod(21, 22), person.PermissionInformation.DefaultTimeZone());
			var personRequest = createOvertimeRequest(person, requestPeriod);

			_target = createTarget();
			var result = _target.Approve(personRequest.Request);

			var personAssignment = _scheduleDictionary.SchedulesForDay(Now.ServerDate_DontUse()).FirstOrDefault()?.PersonAssignment();

			result.Count().Should().Be(1);
			result.First().Message.Should().Be(Resources.NoAvailableSkillForOvertime);
			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotApprovedWhenHasNoCriticalUnderStaffingSkill()
		{
			var person = User.CurrentUser();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, 6d, 10d);
			addPersonSkillsToPersonPeriod(personSkill1);
			createAssignment(person, activity1);

			var requestPeriod = Now.ServerDate_DontUse().ToDateTimePeriod(new TimePeriod(19, 21), person.PermissionInformation.DefaultTimeZone());
			var personRequest = createOvertimeRequest(person, requestPeriod);

			_target = createTarget();
			var result = _target.Approve(personRequest.Request);

			var personAssignment = _scheduleDictionary.SchedulesForDay(Now.ServerDate_DontUse()).FirstOrDefault()?.PersonAssignment();

			result.Count().Should().Be(1);
			result.First().Message.Should().Be(Resources.NoUnderStaffingSkill);
			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(0);
		}

		private OvertimeRequestApprovalService createTarget()
		{
			return new OvertimeRequestApprovalService(_scheduleDictionary, new DoNothingScheduleDayChangeCallBack()
				, OvertimeRequestUnderStaffingSkillProvider, OvertimeRequestSkillProvider, SkillOpenHourFilter);
		}

		private IPersonRequest createOvertimeRequest(IPerson person, DateTimePeriod period)
		{
			var personRequestFactory = new PersonRequestFactory();

			var personRequest = personRequestFactory.CreatePersonRequest(person);
			var overTimeRequest = new OvertimeRequest(new MultiplicatorDefinitionSet("name", MultiplicatorType.Overtime),
				period);
			personRequest.Request = overTimeRequest;
			return personRequest;
		}

		private TimeSpan[] createIntervals()
		{
			var intervals = new List<TimeSpan>();
			for (var i = 00; i < 1440; i += 15)
			{
				intervals.Add(TimeSpan.FromMinutes(i));
			}
			return intervals.ToArray();
		}

		private void setupIntradayStaffingForSkill(ISkill skill, double forecastedStaffing,
			double scheduledStaffing)
		{
			var period = getAvailablePeriod();
			period.DayCollection().ToList().ForEach(day =>
			{
				var utcDate = TimeZoneHelper.ConvertToUtc(day.Date,
					User.CurrentUser().PermissionInformation.DefaultTimeZone());

				for (var i = 0; i < _intervals.Length; i++)
				{
					CombinationRepository.AddSkillCombinationResource(new DateTime(),
						new[]
						{
							new SkillCombinationResource
							{
								StartDateTime = utcDate.Add(_intervals[i]),
								EndDateTime = utcDate.Add(_intervals[i]).AddMinutes(15),
								Resource = scheduledStaffing,
								SkillCombination = new[] {skill.Id.Value}
							}
						});
				}

				var timePeriodTuples = new List<Tuple<TimePeriod, double>>();
				for (var i = 0; i < _intervals.Length; i++)
				{
					timePeriodTuples.Add(new Tuple<TimePeriod, double>(
						new TimePeriod(_intervals[i], _intervals[i].Add(TimeSpan.FromMinutes(15))),
						forecastedStaffing));
				}
				SkillDayRepository.Has(skill.CreateSkillDayWithDemandOnInterval(Scenario.Current(), day, 0,
					timePeriodTuples.ToArray()));
			});
		}

		private DateOnlyPeriod getAvailablePeriod()
		{
			var today = Now.ServerDate_DontUse();
			var period = new DateOnlyPeriod(today, today.AddDays(13)).Inflate(1);
			return period;
		}

		private ISkill createSkill(string name)
		{
			return createSkill(name, _defaultOpenPeriod);
		}

		private ISkill createSkill(string name, TimePeriod openPeriod)
		{
			var skill = SkillFactory.CreateSkill(name).WithId();
			skill.SkillType.Description = new Description("SkillTypeInboundTelephony");
			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openPeriod);
			SkillRepository.Has(skill);
			return skill;
		}

		private static StaffingThresholds createStaffingThresholds()
		{
			return new StaffingThresholds(new Percent(-0.3), new Percent(-0.1), new Percent(0.1));
		}

		private static IActivity createActivity(string name)
		{
			var activity = ActivityFactory.CreateActivity(name);
			activity.RequiresSkill = true;
			return activity;
		}

		private IPersonSkill createPersonSkill(IActivity activity, ISkill skill)
		{
			skill.Activity = activity;
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			return personSkill;
		}

		private void addPersonSkillsToPersonPeriod(DateOnly startDate, params IPersonSkill[] personSkills)
		{
			var personPeriod = getOrAddPersonPeriod(startDate);
			foreach (var personSkill in personSkills)
			{
				personPeriod.AddPersonSkill(personSkill);
			}
		}

		private void addPersonSkillsToPersonPeriod(params IPersonSkill[] personSkills)
		{
			addPersonSkillsToPersonPeriod(Now.ServerDate_DontUse(), personSkills);
		}


		private PersonPeriod getOrAddPersonPeriod(DateOnly startDate)
		{
			var personPeriod = (PersonPeriod)User.CurrentUser().PersonPeriods(startDate.ToDateOnlyPeriod()).FirstOrDefault();
			if (personPeriod != null) return personPeriod;
			var team = TeamFactory.CreateTeam("team1", "site1");
			personPeriod =
				(PersonPeriod)
				PersonPeriodFactory.CreatePersonPeriod(startDate, PersonContractFactory.CreatePersonContract(), team);
			User.CurrentUser().AddPersonPeriod(personPeriod);
			return personPeriod;
		}

		private void createAssignment(IPerson person, params IActivity[] activities)
		{
			var startDate = Now.UtcDateTime().Date.AddHours(8);
			var endDate = Now.UtcDateTime().Date.AddHours(17);
			var scheduleDatas = new List<IScheduleData>();
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				Scenario.Current(), new DateTimePeriod(startDate, endDate),
				ShiftCategoryFactory.CreateShiftCategory(), activities);
			scheduleDatas.Add(assignment);
			_scheduleDictionary = ScheduleDictionaryForTest.WithScheduleDataForManyPeople(_scenario,
				new DateTimePeriod(startDate, endDate), scheduleDatas.ToArray());
		}
	}
}
