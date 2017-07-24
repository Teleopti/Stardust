using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
		public FakeScheduleDataReadScheduleStorage ScheduleStorage;
		public ICurrentScenario Scenario;
		public MutableNow Now;
		public FakeSkillRepository SkillRepository;
		public IPrimaryPersonSkillFilter PrimaryPersonSkillFilter;
		public ISupportedSkillsInIntradayProvider SupportedSkillsInIntradayProvider;
		public IOvertimeRequestSkillProvider OvertimeRequestSkillProvider;
		public ISkillOpenHourFilter SkillOpenHourFilter;
		public ICommandDispatcher CommandDispatcher;
		public FakeWriteSideRepository<IActivity> ActivityProxyForId;
		public FakeWriteSideRepository<IPerson> PersonProxyForId;
		public FakeWriteSideRepository<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSetProxyForId;
		public FakePersonAssignmentWriteSideRepository PersonAssignmentWriteSideRepository;

		private OvertimeRequestApprovalService _target;

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
			system.UseTestDouble<FakeWriteSideRepository<IActivity>>().For<IProxyForId<IActivity>>();
			system.UseTestDouble<FakeWriteSideRepository<IPerson>>().For<IProxyForId<IPerson>>();
			system.UseTestDouble<FakeScheduleDataReadScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakeWriteSideRepository<IMultiplicatorDefinitionSet>>().For<IProxyForId<IMultiplicatorDefinitionSet>>();
			system.UseTestDouble<FakePersonAssignmentWriteSideRepository>()
				.For<IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey>>();
			_intervals = createIntervals();
		}

		[Test]
		public void ShouldAddActivityOfSkillWhenApproved()
		{
			var person = getCurrentUser();
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

			var personAssignment = PersonAssignmentWriteSideRepository.LoadAggregate(new PersonAssignmentKey
			{
				Person = person,
				Date = new DateOnly(requestPeriod.StartDateTime),
				Scenario = _scenario
			});

			result.Count().Should().Be(0);
			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(1);
			personAssignment.OvertimeActivities().First().Payload.Should().Be(skill1.Activity);
			personAssignment.OvertimeActivities().First().Period.Should().Be(requestPeriod);
		}

		[Test]
		public void ShouldAddActivityOfTheFirstSkillWhenApproved()
		{
			var person = getCurrentUser();
			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");
			var skill1 = createSkill("skill1");
			var skill2 = createSkill("skill2");
			var personSkill1 = createPersonSkill(activity1, skill1);
			var personSkill2 = createPersonSkill(activity2, skill2);
			setupIntradayStaffingForSkill(skill1, 10d, 6d);
			setupIntradayStaffingForSkill(skill2, 10d, 6d);
			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);
			createAssignment(person, activity1);

			var requestPeriod = Now.ServerDate_DontUse().ToDateTimePeriod(new TimePeriod(19, 21), person.PermissionInformation.DefaultTimeZone());
			var personRequest = createOvertimeRequest(person, requestPeriod);

			_target = createTarget();
			var result = _target.Approve(personRequest.Request);

			var personAssignment = PersonAssignmentWriteSideRepository.LoadAggregate(new PersonAssignmentKey
			{
				Person = person,
				Date = new DateOnly(requestPeriod.StartDateTime),
				Scenario = _scenario
			});

			result.Count().Should().Be(0);
			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(1);
			personAssignment.OvertimeActivities().First().Payload.Should().Be(skill1.Activity);
			personAssignment.OvertimeActivities().First().Period.Should().Be(requestPeriod);
		}

		[Test]
		public void ShouldAddActivityOfPrimarySkillWhenApproved()
		{
			var person = getCurrentUser();
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

			var personAssignment = PersonAssignmentWriteSideRepository.LoadAggregate(new PersonAssignmentKey
			{
				Person = person,
				Date = new DateOnly(requestPeriod.StartDateTime),
				Scenario = _scenario
			});

			result.Count().Should().Be(0);
			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(1);
			personAssignment.OvertimeActivities().First().Payload.Should().Be(primarySkill.Activity);
			personAssignment.OvertimeActivities().First().Period.Should().Be(requestPeriod);
		}

		[Test]
		public void ShouldAddActivityOfTheFirstSkillWithoutOvertimeWhenApproved()
		{
			var person = getCurrentUser();
			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");
			var skill1 = createSkill("skill1");
			var skill2 = createSkill("skill2");
			var personSkill1 = createPersonSkill(activity1, skill1);
			var personSkill2 = createPersonSkill(activity2, skill2);
			setupIntradayStaffingForSkill(skill1, 10d, 6d);
			setupIntradayStaffingForSkill(skill2, 10d, 6d);
			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var assignment = PersonAssignmentFactory.CreatePersonAssignment(person, _scenario, Now.ServerDate_DontUse());

			assignment.AddOvertimeActivity(activity1, Now.ServerDate_DontUse().ToDateTimePeriod(new TimePeriod(19, 20), person.PermissionInformation.DefaultTimeZone()),
				new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime));
			assignment.AddActivity(activity2, new TimePeriod(8, 17));

			PersonAssignmentWriteSideRepository.Add(assignment);
			ScheduleStorage.Add(assignment);

			var requestPeriod = Now.ServerDate_DontUse().ToDateTimePeriod(new TimePeriod(19, 21), person.PermissionInformation.DefaultTimeZone());
			var personRequest = createOvertimeRequest(person, requestPeriod);

			_target = createTarget();
			var result = _target.Approve(personRequest.Request);

			var personAssignment = PersonAssignmentWriteSideRepository.LoadAggregate(new PersonAssignmentKey
			{
				Person = person,
				Date = new DateOnly(requestPeriod.StartDateTime),
				Scenario = _scenario
			});

			result.Count().Should().Be(0);
			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(2);
			personAssignment.OvertimeActivities().Select(o=>o.Payload).Contains(skill2.Activity).Should().Be(true);
			personAssignment.OvertimeActivities().Select(o => o.Period).Contains(requestPeriod).Should().Be(true);
		}

		[Test]
		public void ShouldApproveCrossDayRequest()
		{
			var person = getCurrentUser();
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

			var personAssignment = PersonAssignmentWriteSideRepository.LoadAggregate(new PersonAssignmentKey
			{
				Person = person,
				Date = new DateOnly(requestPeriod.StartDateTime),
				Scenario = _scenario
			});

			result.Count().Should().Be(0);
			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(1);
			personAssignment.OvertimeActivities().First().Payload.Should().Be(skill1.Activity);
			personAssignment.OvertimeActivities().First().Period.Should().Be(requestPeriod);
		}

		[Test]
		public void ShouldNotApprovedWhenSkillOpenHourIsNotAvailable()
		{
			var person = getCurrentUser();
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

			var personAssignment = PersonAssignmentWriteSideRepository.LoadAggregate(new PersonAssignmentKey
			{
				Person = person,
				Date = new DateOnly(requestPeriod.StartDateTime),
				Scenario = _scenario
			});

			result.Count().Should().Be(1);
			result.First().Message.Should().Be(Resources.PeriodIsOutOfSkillOpenHours);
			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotApprovedWhenAgentSkillIsOutOfPersonPeriod()
		{
			var person = getCurrentUser();
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

			var personAssignment = PersonAssignmentWriteSideRepository.LoadAggregate(new PersonAssignmentKey
			{
				Person = person,
				Date = startDate.AddDays(-1),
				Scenario = _scenario
			});

			result.Count().Should().Be(1);
			result.First().Message.Should().Be(Resources.NoAvailableSkillForOvertime);
			personAssignment.Should().Be.Null();
		}

		[Test]
		public void ShouldNotApprovedWhenHasNoCriticalUnderStaffingSkill()
		{
			var person = getCurrentUser();
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

			var personAssignment = PersonAssignmentWriteSideRepository.LoadAggregate(new PersonAssignmentKey
			{
				Person = person,
				Date = new DateOnly(requestPeriod.StartDateTime),
				Scenario = _scenario
			});

			result.Count().Should().Be(1);
			result.First().Message.Should().Be(Resources.NoUnderStaffingSkill);
			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotApprovedWhenOnlyUnderStaffingButNoCriticalSkill()
		{
			var person = getCurrentUser();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, 10d, 20d);
			addPersonSkillsToPersonPeriod(personSkill1);
			createAssignment(person, activity1);

			var requestPeriod = Now.ServerDate_DontUse().ToDateTimePeriod(new TimePeriod(19, 21), person.PermissionInformation.DefaultTimeZone());
			var personRequest = createOvertimeRequest(person, requestPeriod);

			_target = createTarget();
			var result = _target.Approve(personRequest.Request);

			var personAssignment = PersonAssignmentWriteSideRepository.LoadAggregate(new PersonAssignmentKey
			{
				Person = person,
				Date = new DateOnly(requestPeriod.StartDateTime),
				Scenario = _scenario
			});

			result.Count().Should().Be(1);
			result.First().Message.Should().Be(Resources.NoUnderStaffingSkill);
			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotApprovedWhenAnySkillIsNotCriticalUnderStaffing()
		{
			var person = getCurrentUser();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");

			var notUnderStaffingSkill = createSkill("notUnderStaffingSkill");
			var criticalUnderStaffingSkill = createSkill("criticalUnderStaffingSkill");

			var personSkill1 = createPersonSkill(activity1, notUnderStaffingSkill);
			var personSkill2 = createPersonSkill(activity2, criticalUnderStaffingSkill);

			setupIntradayStaffingForSkill(notUnderStaffingSkill, 10d, 10d);
			setupIntradayStaffingForSkill(criticalUnderStaffingSkill, 10d, 6d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);
			createAssignment(person, activity1, activity2);

			var requestPeriod = Now.ServerDate_DontUse().ToDateTimePeriod(new TimePeriod(19, 21), person.PermissionInformation.DefaultTimeZone());
			var personRequest = createOvertimeRequest(person, requestPeriod);

			_target = createTarget();
			var result = _target.Approve(personRequest.Request);

			var personAssignment = PersonAssignmentWriteSideRepository.LoadAggregate(new PersonAssignmentKey
			{
				Person = person,
				Date = new DateOnly(requestPeriod.StartDateTime),
				Scenario = _scenario
			});

			result.Count().Should().Be(1);
			result.First().Message.Should().Be(Resources.NoUnderStaffingSkill);
			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotApprovedWhenThereIsOvertimeActivityWithinRequestPeriod()
		{
			var person = getCurrentUser();
			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, 10d, 6d);
			addPersonSkillsToPersonPeriod(personSkill1);

			var requestPeriod = Now.ServerDate_DontUse()
				.ToDateTimePeriod(new TimePeriod(19, 21), person.PermissionInformation.DefaultTimeZone());

			createOvertimeAssignment(Now.ServerDate_DontUse(), new TimePeriod(19, 21), person, activity1);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			_target = createTarget();
			var result = _target.Approve(personRequest.Request);

			result.Count().Should().Be(1);
			result.First().Message.Should().Be("This activity is already scheduled within this period.");
		}

		[Test]
		public void ShouldNotApprovedWhenThereIsOvertimeActivityPartialWithInRequestPeriod()
		{
			var person = getCurrentUser();
			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, 10d, 6d);
			addPersonSkillsToPersonPeriod(personSkill1);

			var requestPeriod = Now.ServerDate_DontUse()
				.ToDateTimePeriod(new TimePeriod(19, 21), person.PermissionInformation.DefaultTimeZone());

			createOvertimeAssignment(Now.ServerDate_DontUse(), new TimePeriod(18, 21), person, activity1);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			_target = createTarget();
			var result = _target.Approve(personRequest.Request);

			result.Count().Should().Be(1);
			result.First().Message.Should().Be("This activity is already scheduled within this period.");
		}

		[Test]
		public void ShouldNotApprovedWhenThereSameActivityWithinRequestPeriod()
		{
			var person = getCurrentUser();
			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, 10d, 6d);
			addPersonSkillsToPersonPeriod(personSkill1);
			createAssignment(person, activity1);

			var requestPeriod = Now.ServerDate_DontUse().ToDateTimePeriod(new TimePeriod(16, 17), person.PermissionInformation.DefaultTimeZone());
			var personRequest = createOvertimeRequest(person, requestPeriod);

			_target = createTarget();
			var result = _target.Approve(personRequest.Request);

			result.Count().Should().Be(1);
			result.First().Message.Should().Be("This activity is already scheduled within this period.");
		}

		private OvertimeRequestApprovalService createTarget()
		{
			return new OvertimeRequestApprovalService(OvertimeRequestUnderStaffingSkillProvider, OvertimeRequestSkillProvider, SkillOpenHourFilter,
				CommandDispatcher, ScheduleStorage, _scenario);
		}

		private IPersonRequest createOvertimeRequest(IPerson person, DateTimePeriod period)
		{
			var personRequestFactory = new PersonRequestFactory();
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("name", MultiplicatorType.Overtime);
			MultiplicatorDefinitionSetProxyForId.Add(multiplicatorDefinitionSet);

			var personRequest = personRequestFactory.CreatePersonRequest(person);
			var overTimeRequest = new OvertimeRequest(multiplicatorDefinitionSet, period);
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

		private IActivity createActivity(string name)
		{
			var activity = ActivityFactory.CreateActivity(name);
			activity.RequiresSkill = true;
			ActivityProxyForId.Add(activity);
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
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				Scenario.Current(), Now.ServerDate_DontUse().ToDateTimePeriod(new TimePeriod(8, 17), person.PermissionInformation.DefaultTimeZone()),
				ShiftCategoryFactory.CreateShiftCategory(), activities);
			PersonAssignmentWriteSideRepository.Add(assignment);
			ScheduleStorage.Add(assignment);
		}

		private void createOvertimeAssignment(DateOnly date, TimePeriod period, IPerson person, IActivity activity)
		{
			var overtimeAssignment = PersonAssignmentFactory
				.CreateAssignmentWithOvertimeShift(person, _scenario, activity, date
					.ToDateTimePeriod(period, person.PermissionInformation.DefaultTimeZone()));
			PersonAssignmentWriteSideRepository.Add(overtimeAssignment);
			ScheduleStorage.Add(overtimeAssignment);
		}

		private IPerson getCurrentUser()
		{
			var person = User.CurrentUser();
			PersonProxyForId.Add(person);
			return person;
		}
	}
}
