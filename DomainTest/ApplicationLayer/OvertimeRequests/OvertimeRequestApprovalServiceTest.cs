using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
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
	public class OvertimeRequestApprovalServiceTest : ISetup
	{
		public FakeSkillRepository SkillRepository;
		public FakeMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;
		public FakeCommandDispatcher CommandDispatcher;
		public FakeActivityRepository ActivityRepository;
		public MutableNow Now;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeSkillTypeRepository SkillTypeRepository;
		public SkillIntradayStaffingFactory SkillIntradayStaffingFactory;
		public IRequestApprovalServiceFactory RequestApprovalServiceFactory;
		public ICurrentScenario Scenario;

		private readonly ISkillType _phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();
		private readonly ISkillType _emailSkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();
		private readonly ISkillType _chatSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat).WithId();
		private readonly TimePeriod _defaultOpenPeriod = new TimePeriod(8, 00, 21, 00);
		private readonly DateOnly _periodStartDate = new DateOnly(2016, 1, 1);
		private readonly int _defaultIntervalInMinutes = 15;
		private readonly int _emailIntervalInMinutes = 60;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
			system.UseTestDouble(new FakeScenarioRepository(new Scenario("default") { DefaultScenario = true }))
				.For<IScenarioRepository>();
		}

		[Test]
		public void ShouldAddActivityOfSkillWhenApproved()
		{
			setupPerson(0, 24);

			Now.Is(new DateTime(2018, 01, 01, 0, 0, 0, DateTimeKind.Utc));

			var timeZone = TimeZoneInfoFactory.UtcTimeZoneInfo();

			var period = new DateTimePeriod(Now.UtcDateTime().AddHours(8), Now.UtcDateTime().AddHours(9));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 5),
				OrderIndex = 1
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;


			var criticalUnderStaffingSkillEmail = createSkill("criticalUnderStaffingSkillEmail", null, timeZone);
			criticalUnderStaffingSkillEmail.SkillType = _emailSkillType;
			criticalUnderStaffingSkillEmail.DefaultResolution = _emailIntervalInMinutes;

			var emailActivity = createActivity("email activity");
			var personSkillEmail = createPersonSkill(emailActivity, criticalUnderStaffingSkillEmail);
			addPersonSkillsToPersonPeriod(personSkillEmail);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffingSkillEmail,
				new DateOnly(period.StartDateTime), new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = period
					}
				}, timeZone);

			var personRequest = createOvertimeRequest(LoggedOnUser.CurrentUser(), period);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(emailActivity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(period);
		}

		[Test]
		public void ShouldAddActivityOfTheFirstSkillWhenApproved()
		{
			setupPerson(0, 24);

			Now.Is(new DateTime(2018, 01, 01, 0, 0, 0, DateTimeKind.Utc));

			var timeZone = TimeZoneInfoFactory.UtcTimeZoneInfo();

			var period = new DateTimePeriod(Now.UtcDateTime().AddHours(8), Now.UtcDateTime().AddHours(9));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 5),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 5),
				OrderIndex = 2
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var criticalUnderStaffingSkillPhone = createSkill("criticalUnderStaffingSkillEmail", null, timeZone);
			criticalUnderStaffingSkillPhone.SkillType = _phoneSkillType;
			criticalUnderStaffingSkillPhone.DefaultResolution = _defaultIntervalInMinutes;

			var criticalUnderStaffingSkillEmail = createSkill("criticalUnderStaffingSkillPhone", null, timeZone);
			criticalUnderStaffingSkillEmail.SkillType = _emailSkillType;
			criticalUnderStaffingSkillEmail.DefaultResolution = _emailIntervalInMinutes;

			var phoneActivity = createActivity("phone activity");
			var emailActivity = createActivity("email activity");

			var personSkillPhone = createPersonSkill(phoneActivity, criticalUnderStaffingSkillPhone);
			var personSkillEmail = createPersonSkill(emailActivity, criticalUnderStaffingSkillEmail);
			addPersonSkillsToPersonPeriod(personSkillPhone, personSkillEmail);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffingSkillPhone,
				new DateOnly(period.StartDateTime), new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = period
					}
				}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffingSkillEmail,
				new DateOnly(period.StartDateTime), new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = period
					}
				}, timeZone);

			var personRequest = createOvertimeRequest(LoggedOnUser.CurrentUser(), period);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(phoneActivity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(period);
		}

		[Test]
		public void ShouldAddActivityOfPrimarySkillWhenApproved()
		{
			setupPerson(0, 24);

			Now.Is(new DateTime(2018, 01, 01, 0, 0, 0, DateTimeKind.Utc));

			var timeZone = TimeZoneInfoFactory.UtcTimeZoneInfo();

			var period = new DateTimePeriod(Now.UtcDateTime().AddHours(8), Now.UtcDateTime().AddHours(9));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 5),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 5),
				OrderIndex = 2
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var criticalUnderStaffingSkillPhone = createSkill("criticalUnderStaffingSkillEmail", null, timeZone);
			criticalUnderStaffingSkillPhone.SkillType = _phoneSkillType;
			criticalUnderStaffingSkillPhone.DefaultResolution = _defaultIntervalInMinutes;
			criticalUnderStaffingSkillPhone.CascadingIndex(1);

			var criticalUnderStaffingSkillEmail = createSkill("criticalUnderStaffingSkillPhone", null, timeZone);
			criticalUnderStaffingSkillEmail.SkillType = _emailSkillType;
			criticalUnderStaffingSkillEmail.DefaultResolution = _emailIntervalInMinutes;
			criticalUnderStaffingSkillEmail.CascadingIndex(2);

			var phoneActivity = createActivity("phone activity");
			var emailActivity = createActivity("email activity");

			var personSkillPhone = createPersonSkill(phoneActivity, criticalUnderStaffingSkillPhone);
			var personSkillEmail = createPersonSkill(emailActivity, criticalUnderStaffingSkillEmail);
			addPersonSkillsToPersonPeriod(personSkillPhone, personSkillEmail);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffingSkillPhone,
				new DateOnly(period.StartDateTime), new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = period
					}
				}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffingSkillEmail,
				new DateOnly(period.StartDateTime), new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = period
					}
				}, timeZone);

			var personRequest = createOvertimeRequest(LoggedOnUser.CurrentUser(), period);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(phoneActivity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(period);
		}

		[Test]
		public void ShouldApproveCrossDayRequest()
		{
			setupPerson(0, 24);

			Now.Is(new DateTime(2018, 01, 01, 0, 0, 0, DateTimeKind.Utc));

			var timeZone = TimeZoneInfoFactory.UtcTimeZoneInfo();

			var period = new DateTimePeriod(Now.UtcDateTime().AddHours(23), Now.UtcDateTime().AddDays(1).AddHours(1));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 5),
				OrderIndex = 1
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var criticalUnderStaffingSkillPhone = createSkill("criticalUnderStaffingSkillPhone", new TimePeriod(0, 24), timeZone);
			criticalUnderStaffingSkillPhone.SkillType = _phoneSkillType;
			criticalUnderStaffingSkillPhone.DefaultResolution = _defaultIntervalInMinutes;

			var phoneActivity = createActivity("phone activity");

			var personSkillPhone = createPersonSkill(phoneActivity, criticalUnderStaffingSkillPhone);
			addPersonSkillsToPersonPeriod(personSkillPhone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffingSkillPhone,
				new DateOnly(period.StartDateTime), new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = new DateTimePeriod(period.StartDateTime, period.StartDateTime.Date.AddDays(1).Subtract(TimeSpan.FromMinutes(1)))
					}
				}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffingSkillPhone,
				new DateOnly(period.EndDateTime.Date), new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = new DateTimePeriod(period.EndDateTime.Date, period.EndDateTime)
					}
				}, timeZone);

			var personRequest = createOvertimeRequest(LoggedOnUser.CurrentUser(), period);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(phoneActivity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(period);
		}

		[Test]
		public void ShouldNotApproveWhenAgentSkillIsOutOfPersonPeriod()
		{
			setupPerson(0, 24);

			Now.Is(new DateTime(2018, 01, 01, 0, 0, 0, DateTimeKind.Utc));

			var timeZone = TimeZoneInfoFactory.UtcTimeZoneInfo();

			var period = new DateTimePeriod(Now.UtcDateTime().AddHours(8), Now.UtcDateTime().AddHours(9));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 5),
				OrderIndex = 1
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var criticalUnderStaffingSkillPhone = createSkill("criticalUnderStaffingSkillEmail", null, timeZone);
			criticalUnderStaffingSkillPhone.SkillType = _phoneSkillType;
			criticalUnderStaffingSkillPhone.DefaultResolution = _defaultIntervalInMinutes;
			criticalUnderStaffingSkillPhone.CascadingIndex(1);

			var phoneActivity = createActivity("phone activity");
			var personSkillPhone = createPersonSkill(phoneActivity, criticalUnderStaffingSkillPhone);

			var team = TeamFactory.CreateTeam("team1", "site1");
			var personPeriod = (PersonPeriod)PersonPeriodFactory.CreatePersonPeriod(new DateOnly(period.EndDateTime.AddDays(30)), PersonContractFactory.CreatePersonContract(), team);
			LoggedOnUser.CurrentUser().AddPersonPeriod(personPeriod);

			personPeriod.AddPersonSkill(personSkillPhone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffingSkillPhone,
				new DateOnly(period.StartDateTime), new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = period
					}
				}, timeZone);

			var personRequest = createOvertimeRequest(LoggedOnUser.CurrentUser(), period);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(1);
			result.First().Message.Should().Be(Resources.ThereIsNoAvailableSkillForOvertime);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestSupportMultiSelectionSkillTypes_74945)]
		public void ShouldApprovedAndAssginActivityCorrectlyBaseOnSkillsWhenMultipleSkillTypesSelectionIsEnabledInOvertimeRequestOpenPeriodSetting()
		{
			Now.Is(new DateTime(2018, 01, 01));
			var date = new DateOnly(Now.UtcDateTime());

			setupPerson(0, 24);

			var phoneActivity = createActivity("phone activity");
			var emailActivity = createActivity("email activity");
			var chatActivity = createActivity("chat activity");

			var timeZone = TimeZoneInfoFactory.UtcTimeZoneInfo();

			var partlyUnderStaffingSkillPhone = createSkill("partlyCriticalUnderStaffingSkillPhone", null, timeZone);
			partlyUnderStaffingSkillPhone.SkillType = _phoneSkillType;

			var notUnderStaffingSkillEmail = createSkill("notCriticalUnderStaffingSkillEmail", null, timeZone);
			notUnderStaffingSkillEmail.SkillType = _emailSkillType;
			notUnderStaffingSkillEmail.DefaultResolution = 60;

			var underStaffingSkillChat = createSkill("criticalUnderStaffingSkillChat", null, timeZone);
			underStaffingSkillChat.SkillType = _chatSkillType;

			var personSkillPhone = createPersonSkill(phoneActivity, partlyUnderStaffingSkillPhone);
			var personSkillEmail = createPersonSkill(emailActivity, notUnderStaffingSkillEmail);
			var personSkillChat = createPersonSkill(chatActivity, underStaffingSkillChat);

			addPersonSkillsToPersonPeriod(personSkillPhone, personSkillEmail, personSkillChat);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType, _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 7),
				OrderIndex = 1
			});

			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(0, 5),
				OrderIndex = 2
			});

			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				BetweenDays = new MinMax<int>(0, 5),
				OrderIndex = 3
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var periodStartDateTime = new DateTime(2018, 01, 01, 08, 0, 0, 0, DateTimeKind.Utc);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(partlyUnderStaffingSkillPhone, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = new DateTimePeriod(periodStartDateTime, periodStartDateTime.AddMinutes(_defaultIntervalInMinutes))},
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = new DateTimePeriod(periodStartDateTime.AddMinutes(_defaultIntervalInMinutes), periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 2))},
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = new DateTimePeriod(periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 2), periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 3))},
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = new DateTimePeriod(periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 3), periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 4))},
			}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(notUnderStaffingSkillEmail, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData
				{
					ForecastedStaffing = 10d,
					ScheduledStaffing = 20d,
					Period = new DateTimePeriod(periodStartDateTime, periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 4))
				}
			}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(underStaffingSkillChat, date, new List<StaffingPeriodData>{
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = new DateTimePeriod(periodStartDateTime, periodStartDateTime.AddMinutes(_defaultIntervalInMinutes))},
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = new DateTimePeriod(periodStartDateTime.AddMinutes(_defaultIntervalInMinutes), periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 2))},
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = new DateTimePeriod(periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 2), periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 3))},
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = new DateTimePeriod(periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 3), periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 4))},
			}, timeZone);

			var requestPeriod = new DateTimePeriod(2018, 01, 01, 8, 2018, 01, 01, 9);

			var personRequest = createOvertimeRequest(LoggedOnUser.CurrentUser(), requestPeriod);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(chatActivity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		private void setupPerson(int siteOpenStartHour = 8, int siteOpenEndHour = 17, bool isOpenHoursClosed = false)
		{
			var person = createPersonWithSiteOpenHours(siteOpenStartHour, siteOpenEndHour, isOpenHoursClosed);
			person.PermissionInformation.SetUICulture(CultureInfoFactory.CreateUsCulture());
			person.PermissionInformation.SetCulture(CultureInfoFactory.CreateUsCulture());
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 30)
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			SkillTypeRepository.Add(_phoneSkillType);
		}

		private IPerson createPersonWithSiteOpenHours(int startHour, int endHour, bool isOpenHoursClosed = false)
		{
			var team = TeamFactory.CreateTeam("team", "site");
			var siteOpenHour = new SiteOpenHour
			{
				Parent = team.Site,
				TimePeriod = new TimePeriod(startHour, 0, endHour, 0),
				WeekDay = DayOfWeek.Monday,
				IsClosed = isOpenHoursClosed
			};
			team.Site.AddOpenHour(siteOpenHour);
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(_periodStartDate, team);
			return person;
		}

		private IPersonSkill createPersonSkill(IActivity activity, ISkill skill)
		{
			skill.Activity = activity;
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			return personSkill;
		}

		private void addPersonSkillsToPersonPeriod(params IPersonSkill[] personSkills)
		{
			var personPeriod = getOrAddPersonPeriod(Now.ServerDate_DontUse());
			foreach (var personSkill in personSkills)
			{
				personPeriod.AddPersonSkill(personSkill);
			}
		}

		private PersonPeriod getOrAddPersonPeriod(DateOnly startDate)
		{
			var personPeriod = (PersonPeriod)LoggedOnUser.CurrentUser().PersonPeriods(startDate.ToDateOnlyPeriod()).FirstOrDefault();
			if (personPeriod != null) return personPeriod;
			var team = TeamFactory.CreateTeam("team1", "site1");
			personPeriod =
				(PersonPeriod)
				PersonPeriodFactory.CreatePersonPeriod(startDate, PersonContractFactory.CreatePersonContract(), team);
			LoggedOnUser.CurrentUser().AddPersonPeriod(personPeriod);
			return personPeriod;
		}

		private IActivity createActivity(string name)
		{
			var activity = ActivityFactory.CreateActivity(name);
			activity.RequiresSkill = true;
			activity.InWorkTime = true;
			ActivityRepository.Add(activity);
			return activity;
		}

		private ISkill createSkill(string name, TimePeriod? skillOpenHourPeriod = null, TimeZoneInfo timeZone = null, ISkillType skillType = null)
		{
			var skill = SkillFactory.CreateSkill(name, timeZone).WithId();
			skill.SkillType = skillType ?? _phoneSkillType;

			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, skillOpenHourPeriod ?? _defaultOpenPeriod);
			SkillRepository.Has(skill);
			return skill;
		}

		private StaffingThresholds createStaffingThresholds()
		{
			return new StaffingThresholds(new Percent(-0.3), new Percent(-0.1), new Percent(0.1));
		}

		private IPersonRequest createOvertimeRequest(IPerson person, DateTimePeriod period)
		{
			var personRequestFactory = new PersonRequestFactory();
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("name", MultiplicatorType.Overtime);
			MultiplicatorDefinitionSetRepository.Has(multiplicatorDefinitionSet);

			var personRequest = personRequestFactory.CreatePersonRequest(person);
			var overTimeRequest = new OvertimeRequest(multiplicatorDefinitionSet, period);
			personRequest.Request = overTimeRequest;
			return personRequest;
		}
	}
}
