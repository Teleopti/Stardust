using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	[DomainTest]
	public class OvertimeRequestApprovalServiceTest : IIsolateSystem
	{
		public FakeSkillRepository SkillRepository;
		public FakeMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;
		public FakeCommandDispatcher CommandDispatcher;
		public FakeActivityRepository ActivityRepository;
		public IScheduleStorage ScheduleStorage;
		public MutableNow Now;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeSkillTypeRepository SkillTypeRepository;
		public SkillIntradayStaffingFactory SkillIntradayStaffingFactory;
		public IRequestApprovalServiceFactory RequestApprovalServiceFactory;
		public ICurrentScenario Scenario;

		private readonly ISkillType _phoneSkillType =
			new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();

		private readonly ISkillType _emailSkillType =
			new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();

		private readonly ISkillType _chatSkillType =
			new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat).WithId();

		private readonly TimePeriod _defaultOpenPeriod = new TimePeriod(8, 00, 21, 00);
		private readonly DateOnly _periodStartDate = new DateOnly(2016, 1, 1);
		private readonly int _defaultIntervalInMinutes = 15;
		private readonly int _emailIntervalInMinutes = 60;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
			isolate.UseTestDouble<SkillIntradayStaffingFactory>().For<SkillIntradayStaffingFactory>();
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

			var criticalUnderStaffingSkillPhone = createSkill("criticalUnderStaffingSkillPhone", null, timeZone);
			criticalUnderStaffingSkillPhone.SkillType = _phoneSkillType;
			criticalUnderStaffingSkillPhone.DefaultResolution = _defaultIntervalInMinutes;

			var criticalUnderStaffingSkillEmail = createSkill("criticalUnderStaffingSkillEmail", null, timeZone);
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
		public void ShouldMergeContinuousPeriodOfActivityOfTheDifferentSkillWhenApproved()
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

			var criticalUnderStaffingSkillPhone = createSkill("criticalUnderStaffingSkillPhone", null, timeZone);
			criticalUnderStaffingSkillPhone.SkillType = _phoneSkillType;
			criticalUnderStaffingSkillPhone.DefaultResolution = _defaultIntervalInMinutes;

			var criticalUnderStaffingSkillPhone2 = createSkill("criticalUnderStaffingSkillPhone2", null, timeZone);
			criticalUnderStaffingSkillPhone2.SkillType = _phoneSkillType;
			criticalUnderStaffingSkillPhone2.DefaultResolution = _defaultIntervalInMinutes;

			var phoneActivity = createActivity("phone activity");

			var personSkillPhone = createPersonSkill(phoneActivity, criticalUnderStaffingSkillPhone);
			var personSkillPhone2 = createPersonSkill(phoneActivity, criticalUnderStaffingSkillPhone2);
			addPersonSkillsToPersonPeriod(personSkillPhone, personSkillPhone2);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffingSkillPhone,
				new DateOnly(period.StartDateTime), new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = new DateTimePeriod(Now.UtcDateTime().AddHours(8), Now.UtcDateTime().AddHours(8).AddMinutes(15))
					},new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 2d,
						Period = new DateTimePeriod(Now.UtcDateTime().AddHours(8).AddMinutes(15), Now.UtcDateTime().AddHours(8).AddMinutes(30))
					},new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 2d,
						Period =new DateTimePeriod(Now.UtcDateTime().AddHours(8).AddMinutes(30), Now.UtcDateTime().AddHours(8).AddMinutes(45))

					},new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period =new DateTimePeriod(Now.UtcDateTime().AddHours(8).AddMinutes(45), Now.UtcDateTime().AddHours(9))
					}
				}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffingSkillPhone2,
				new DateOnly(period.StartDateTime), new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 2d,
						Period = new DateTimePeriod(Now.UtcDateTime().AddHours(8), Now.UtcDateTime().AddHours(8).AddMinutes(15))
					},new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = new DateTimePeriod(Now.UtcDateTime().AddHours(8).AddMinutes(15), Now.UtcDateTime().AddHours(8).AddMinutes(30))
					},new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period =new DateTimePeriod(Now.UtcDateTime().AddHours(8).AddMinutes(30), Now.UtcDateTime().AddHours(8).AddMinutes(45))

					},new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 2d,
						Period =new DateTimePeriod(Now.UtcDateTime().AddHours(8).AddMinutes(45), Now.UtcDateTime().AddHours(9))
					}
				}, timeZone);

			var personRequest = createOvertimeRequest(LoggedOnUser.CurrentUser(), period);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);
			CommandDispatcher.AllComands.Clear();
			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			CommandDispatcher.AllComands.Count.Should().Be(1);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(phoneActivity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(period);
		}

		[Test]
		public void ShouldMergePeriodOfActivityOfTheDifferentSkillWhenApproved()
		{
			setupPerson(0, 24);

			Now.Is(new DateTime(2018, 01, 01, 0, 0, 0, DateTimeKind.Utc));

			var timeZone = TimeZoneInfoFactory.UtcTimeZoneInfo();

			var period = new DateTimePeriod(Now.UtcDateTime().AddHours(8), Now.UtcDateTime().AddHours(9).AddMinutes(30));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType ,_emailSkillType})
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 5),
				OrderIndex = 1
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var criticalUnderStaffingSkillEmail = createSkill("criticalUnderStaffingSkillEmail", null, timeZone);
			criticalUnderStaffingSkillEmail.SkillType = _emailSkillType;
			criticalUnderStaffingSkillEmail.DefaultResolution = _emailIntervalInMinutes;

			var criticalUnderStaffingSkillPhone = createSkill("criticalUnderStaffingSkillPhone", null, timeZone);
			criticalUnderStaffingSkillPhone.SkillType = _phoneSkillType;
			criticalUnderStaffingSkillPhone.DefaultResolution = _defaultIntervalInMinutes;

			var criticalUnderStaffingSkillPhone2 = createSkill("criticalUnderStaffingSkillPhone2", null, timeZone);
			criticalUnderStaffingSkillPhone2.SkillType = _phoneSkillType;
			criticalUnderStaffingSkillPhone2.DefaultResolution = _defaultIntervalInMinutes;

			var phoneActivity = createActivity("phone activity");
			var emailActivity = createActivity("email activity");

			var personSkillPhone = createPersonSkill(phoneActivity, criticalUnderStaffingSkillPhone);
			var personSkillPhone2 = createPersonSkill(phoneActivity, criticalUnderStaffingSkillPhone2);
			var personSkillfEmail = createPersonSkill(emailActivity, criticalUnderStaffingSkillEmail);
			addPersonSkillsToPersonPeriod(personSkillPhone, personSkillPhone2, personSkillfEmail);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffingSkillPhone,
				new DateOnly(period.StartDateTime), new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = new DateTimePeriod(Now.UtcDateTime().AddHours(8), Now.UtcDateTime().AddHours(8).AddMinutes(15))
					},new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 2d,
						Period = new DateTimePeriod(Now.UtcDateTime().AddHours(8).AddMinutes(15), Now.UtcDateTime().AddHours(8).AddMinutes(30))
					},new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 2d,
						Period =new DateTimePeriod(Now.UtcDateTime().AddHours(8).AddMinutes(30), Now.UtcDateTime().AddHours(8).AddMinutes(45))

					},new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period =new DateTimePeriod(Now.UtcDateTime().AddHours(8).AddMinutes(45), Now.UtcDateTime().AddHours(9))
					}
				}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffingSkillPhone2,
				new DateOnly(period.StartDateTime), new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 2d,
						Period = new DateTimePeriod(Now.UtcDateTime().AddHours(8), Now.UtcDateTime().AddHours(8).AddMinutes(15))
					},new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = new DateTimePeriod(Now.UtcDateTime().AddHours(8).AddMinutes(15), Now.UtcDateTime().AddHours(8).AddMinutes(30))
					},new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period =new DateTimePeriod(Now.UtcDateTime().AddHours(8).AddMinutes(30), Now.UtcDateTime().AddHours(8).AddMinutes(45))

					},new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 2d,
						Period =new DateTimePeriod(Now.UtcDateTime().AddHours(8).AddMinutes(45), Now.UtcDateTime().AddHours(9))
					}
				}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffingSkillEmail,
				new DateOnly(period.StartDateTime), new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 2d,
						Period = new DateTimePeriod(Now.UtcDateTime().AddHours(9), Now.UtcDateTime().AddHours(9).AddMinutes(15))
					},new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = new DateTimePeriod(Now.UtcDateTime().AddHours(9).AddMinutes(15), Now.UtcDateTime().AddHours(9).AddMinutes(30))
					}
				}, timeZone); 

			var personRequest = createOvertimeRequest(LoggedOnUser.CurrentUser(), period);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);
			CommandDispatcher.AllComands.Clear();
			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			CommandDispatcher.AllComands.Count.Should().Be(2);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(emailActivity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(period.ChangeStartTime(TimeSpan.FromHours(1)));
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

			var criticalUnderStaffingSkillEmail = createSkill($"criticalUnderStaffingSkillPhone", null, timeZone);
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

			var criticalUnderStaffingSkillPhone =
				createSkill("criticalUnderStaffingSkillPhone", new TimePeriod(0, 24), timeZone);
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
						Period = new DateTimePeriod(period.StartDateTime,
							period.StartDateTime.Date.AddDays(1).Subtract(TimeSpan.FromMinutes(1)))
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
		public void ShouldAllowToAddDisconnnectedOvertimeActivity()
		{
			var todayShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 01, 20, 0, 0, DateTimeKind.Utc), new DateTime(2018, 01, 02, 02, 0, 0, DateTimeKind.Utc));

			var requestPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 03, 0, 0, DateTimeKind.Utc), new DateTime(2018, 01, 02, 10, 0, 0, DateTimeKind.Utc));

			doBelongsToDateTest(todayShiftPeriod, requestPeriod, null);

			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.AllowDisconnected.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldChangeBelongsToDateWhenOvertimeIsConnectedToPreviousDayShift()
		{
			var todayShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 01, 20, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 02, 0, 0, DateTimeKind.Utc));

			var requestPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 02, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 10, 0, 0, DateTimeKind.Utc));

			doBelongsToDateTest(todayShiftPeriod, requestPeriod, null);

			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.Date.Should().Be.EqualTo(new DateOnly(todayShiftPeriod.StartDateTime));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldChangeBelongsToDateWhenOvertimeIsConnectedToPreviousDayShiftWithoutOverNightShift()
		{
			var todayShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 01, 20, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 01, 23, 0, 0, DateTimeKind.Utc));

			var requestPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 10, 0, 0, DateTimeKind.Utc));

			doBelongsToDateTest(todayShiftPeriod, requestPeriod, null);

			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.Date.Should().Be.EqualTo(new DateOnly(todayShiftPeriod.StartDateTime));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldNotChangeBelongsToDateWhenOvertimeIsStartedFromPreviousDay()
		{
			var todayShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 01, 16, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 01, 20, 0, 0, DateTimeKind.Utc));

			var requestPeriod = new DateTimePeriod(new DateTime(2018, 01, 01, 23, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 6, 0, 0, DateTimeKind.Utc));

			var tomorrowShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 7, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 18, 0, 0, DateTimeKind.Utc));

			doBelongsToDateTest(todayShiftPeriod, requestPeriod, tomorrowShiftPeriod);

			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.Date.Should().Be.EqualTo(new DateOnly(requestPeriod.StartDateTime));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldChangeBelongsToDateWhenOvertimeIsConnectedToPreviousDayShiftWithLastLayerAsOvertime()
		{
			Now.Is(new DateTime(2018, 01, 01, 0, 0, 0, DateTimeKind.Utc));
			setupPerson(0, 24);

			var todayShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 01, 20, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 02, 0, 0, DateTimeKind.Utc));
			var todayPersonAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(LoggedOnUser.CurrentUser(),
				Scenario.Current(), new Activity("test").WithId(), todayShiftPeriod, new ShiftCategory());
			todayPersonAssignment.AddOvertimeActivity(new Activity("ot").WithId(),
				new DateTimePeriod(new DateTime(2018, 01, 02, 02, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 01, 02, 04, 0, 0, DateTimeKind.Utc)),
				new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime));

			var requestPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 04, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 10, 0, 0, DateTimeKind.Utc));
			doBelongsToDateTestByAssignment(todayPersonAssignment, requestPeriod, null);

			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.Date.Should().Be.EqualTo(new DateOnly(todayShiftPeriod.StartDateTime));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldChangeBelongsToDateWhenGapToPreviousDayShiftIsWithin2Hours()
		{
			var todayShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 01, 20, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 02, 0, 0, DateTimeKind.Utc));

			var requestPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 03, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 10, 0, 0, DateTimeKind.Utc));

			doBelongsToDateTest(todayShiftPeriod, requestPeriod, null);

			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.Date.Should().Be.EqualTo(new DateOnly(todayShiftPeriod.StartDateTime));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldChangeBelongsToDateWhenGapToPreviousDayShiftIs2Hours()
		{
			var todayShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 01, 20, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 02, 0, 0, DateTimeKind.Utc));

			var requestPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 04, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 10, 0, 0, DateTimeKind.Utc));

			doBelongsToDateTest(todayShiftPeriod, requestPeriod, null);

			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.Date.Should().Be.EqualTo(new DateOnly(todayShiftPeriod.StartDateTime));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldNotChangeBelongsToDateWhenGapToPreviousDayShiftIsGreaterThan2Hours()
		{
			var todayShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 01, 20, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 02, 0, 0, DateTimeKind.Utc));

			var requestPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 05, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 10, 0, 0, DateTimeKind.Utc));

			doBelongsToDateTest(todayShiftPeriod, requestPeriod, null);

			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.Date.Should().Be.EqualTo(new DateOnly(requestPeriod.StartDateTime));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldNotChangeBelongsToDateWhenGapToPreviousDayShiftIsLarger()
		{
			var todayShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 01, 20, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 02, 0, 0, DateTimeKind.Utc));

			var requestPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 04, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 09, 0, 0, DateTimeKind.Utc));

			var tomorrowShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 10, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 18, 0, 0, DateTimeKind.Utc));

			doBelongsToDateTest(todayShiftPeriod, requestPeriod, tomorrowShiftPeriod);

			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.Date.Should().Be.EqualTo(new DateOnly(requestPeriod.StartDateTime));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldChangeBelongsToDateWhenGapToPreviousDayShiftIsShorter()
		{
			var todayShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 01, 20, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 02, 0, 0, DateTimeKind.Utc));

			var requestPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 03, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 09, 0, 0, DateTimeKind.Utc));

			var tomorrowShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 10, 30, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 18, 0, 0, DateTimeKind.Utc));

			doBelongsToDateTest(todayShiftPeriod, requestPeriod, tomorrowShiftPeriod);

			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.Date.Should().Be.EqualTo(new DateOnly(todayShiftPeriod.StartDateTime));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldSetBelongsToDateSameToPreviousShiftIfGapToPreviousDayShiftAndNextShiftAreBoth2Hours()
		{
			var todayShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 01, 20, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 02, 0, 0, DateTimeKind.Utc));

			var requestPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 04, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 08, 0, 0, DateTimeKind.Utc));

			var tomorrowShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 10, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 18, 0, 0, DateTimeKind.Utc));

			doBelongsToDateTest(todayShiftPeriod, requestPeriod, tomorrowShiftPeriod);

			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand?.Date.Should().Be.EqualTo(new DateOnly(todayShiftPeriod.StartDateTime));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldNotChangeBelongsToDateWhenOvertimeIsConnectedToNextDayShift()
		{
			var todayShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 01, 20, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 02, 0, 0, DateTimeKind.Utc));

			var requestPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 04, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 10, 0, 0, DateTimeKind.Utc));

			var tomorrowShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 10, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 18, 0, 0, DateTimeKind.Utc));

			doBelongsToDateTest(todayShiftPeriod, requestPeriod, tomorrowShiftPeriod);

			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.Date.Should().Be.EqualTo(new DateOnly(requestPeriod.StartDateTime));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldNotChangeBelongsToDateWhenNoPreviousShiftAndNextShift()
		{
			var requestPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 04, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 01, 02, 10, 0, 0, DateTimeKind.Utc));

			doBelongsToDateTest(null, requestPeriod, null);

			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.Date.Should().Be.EqualTo(new DateOnly(requestPeriod.StartDateTime));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldChangeBelongsToDateWhenOvertimeIsConnectedToPreviousFullDayAbsence()
		{
			Now.Is(new DateTime(2018, 01, 01, 0, 0, 0, DateTimeKind.Utc));

			setupPerson(0, 24);

			LoggedOnUser.CurrentUser().PersonPeriodCollection[0].PersonContract.Contract.WorkTime = new WorkTime(TimeSpan.FromHours(15));

			ScheduleStorage.Add(new PersonAbsence(LoggedOnUser.CurrentUser(), Scenario.Current(), new AbsenceLayer(new Absence(),
				new DateTimePeriod(new DateTime(2018, 01, 01, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 01, 01, 23, 59, 0, DateTimeKind.Utc)))).WithId());

			var requestPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 01, 0, 0, DateTimeKind.Utc), new DateTime(2018, 01, 02, 05, 0, 0, DateTimeKind.Utc));

			doBelongsToDateTestByAssignment(null, requestPeriod, null);

			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.Date.Should().Be.EqualTo(new DateOnly(2018, 1, 1));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldChangeBelongsToDateWhenOvertimeIsConnectedToPreviousDayShiftWithLastLayerAsAbsence()
		{
			Now.Is(new DateTime(2018, 01, 01, 0, 0, 0, DateTimeKind.Utc));

			setupPerson(0, 24);

			var todayShiftPeriod = new DateTimePeriod(new DateTime(2018, 01, 01, 20, 0, 0, DateTimeKind.Utc), new DateTime(2018, 01, 02, 01, 0, 0, DateTimeKind.Utc));
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(LoggedOnUser.CurrentUser(), Scenario.Current(), new Activity("test").WithId(), todayShiftPeriod, new ShiftCategory());
			ScheduleStorage.Add(personAssignment);
			ScheduleStorage.Add(new PersonAbsence(LoggedOnUser.CurrentUser(), Scenario.Current(), new AbsenceLayer(new Absence(),
				new DateTimePeriod(new DateTime(2018, 01, 01, 23, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 01, 02, 01, 0, 0, DateTimeKind.Utc)))).WithId());


			var requestPeriod = new DateTimePeriod(new DateTime(2018, 01, 02, 03, 0, 0, DateTimeKind.Utc), new DateTime(2018, 01, 02, 10, 0, 0, DateTimeKind.Utc));

			doBelongsToDateTestByAssignment(personAssignment, requestPeriod, null);

			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.Date.Should().Be.EqualTo(new DateOnly(todayShiftPeriod.StartDateTime));
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
			var personPeriod = (PersonPeriod)PersonPeriodFactory.CreatePersonPeriod(
				new DateOnly(period.EndDateTime.AddDays(30)), PersonContractFactory.CreatePersonContract(), team);
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
		public void
			ShouldApprovedAndAssginActivityCorrectlyBaseOnSkillsWhenMultipleSkillTypesSelectionIsEnabledInOvertimeRequestOpenPeriodSetting()
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
			workflowControlSet.AddOpenOvertimeRequestPeriod(
				new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType, _phoneSkillType })
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
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(partlyUnderStaffingSkillPhone, date,
				new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 20d,
						Period = new DateTimePeriod(periodStartDateTime, periodStartDateTime.AddMinutes(_defaultIntervalInMinutes))
					},
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 2d,
						Period = new DateTimePeriod(periodStartDateTime.AddMinutes(_defaultIntervalInMinutes),
							periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 2))
					},
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 2d,
						Period = new DateTimePeriod(periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 2),
							periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 3))
					},
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 20d,
						Period = new DateTimePeriod(periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 3),
							periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 4))
					},
				}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(notUnderStaffingSkillEmail, date,
				new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 20d,
						Period = new DateTimePeriod(periodStartDateTime, periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 4))
					}
				}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(underStaffingSkillChat, date,
				new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = new DateTimePeriod(periodStartDateTime, periodStartDateTime.AddMinutes(_defaultIntervalInMinutes))
					},
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = new DateTimePeriod(periodStartDateTime.AddMinutes(_defaultIntervalInMinutes),
							periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 2))
					},
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = new DateTimePeriod(periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 2),
							periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 3))
					},
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = new DateTimePeriod(periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 3),
							periodStartDateTime.AddMinutes(_defaultIntervalInMinutes * 4))
					},
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

		private static IPersonSkill createPersonSkill(IActivity activity, ISkill skill)
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
			var personPeriod =
				(PersonPeriod)LoggedOnUser.CurrentUser().PersonPeriods(startDate.ToDateOnlyPeriod()).FirstOrDefault();
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

		private ISkill createSkill(string name, TimePeriod? skillOpenHourPeriod = null, TimeZoneInfo timeZone = null,
			ISkillType skillType = null)
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

		private void doBelongsToDateTest(DateTimePeriod? todayShiftPeriod, DateTimePeriod requestPeriod,
			DateTimePeriod? tomorrowShiftPeriod)
		{
			Now.Is(new DateTime(2018, 01, 01, 0, 0, 0, DateTimeKind.Utc));

			setupPerson(0, 24);
			IPersonAssignment todayPersonAssignment = null;
			if (todayShiftPeriod.HasValue)
			{
				todayPersonAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(LoggedOnUser.CurrentUser(),
					Scenario.Current(), new Activity("test").WithId(), todayShiftPeriod.Value, new ShiftCategory());
			}

			IPersonAssignment tomorrowPersonAssignment = null;
			if (tomorrowShiftPeriod.HasValue)
				tomorrowPersonAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(LoggedOnUser.CurrentUser(),
					Scenario.Current(), new Activity("test").WithId(), tomorrowShiftPeriod.Value, new ShiftCategory());

			doBelongsToDateTestByAssignment(todayPersonAssignment, requestPeriod, tomorrowPersonAssignment);
		}

		private void doBelongsToDateTestByAssignment(IPersonAssignment todayPersonAssignment, DateTimePeriod requestPeriod,
			IPersonAssignment tomorrowPersonAssignment)
		{

			if (todayPersonAssignment != null)
			{
				ScheduleStorage.Add(todayPersonAssignment);
			}

			if (tomorrowPersonAssignment != null)
			{
				ScheduleStorage.Add(tomorrowPersonAssignment);
			}

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 5),
				OrderIndex = 1
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var timeZone = TimeZoneInfoFactory.UtcTimeZoneInfo();

			var criticalUnderStaffingSkillPhone =
				createSkill("criticalUnderStaffingSkillPhone", new TimePeriod(0, 24), timeZone);
			criticalUnderStaffingSkillPhone.SkillType = _phoneSkillType;
			criticalUnderStaffingSkillPhone.DefaultResolution = _defaultIntervalInMinutes;

			var phoneActivity = createActivity("phone activity");

			var personSkillPhone = createPersonSkill(phoneActivity, criticalUnderStaffingSkillPhone);
			addPersonSkillsToPersonPeriod(personSkillPhone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffingSkillPhone,
				new DateOnly(requestPeriod.EndDateTime), new List<StaffingPeriodData>
				{
					new StaffingPeriodData
					{
						ForecastedStaffing = 10d,
						ScheduledStaffing = 1d,
						Period = new DateTimePeriod(requestPeriod.EndDateTime.Date, requestPeriod.EndDateTime)
					}
				}, timeZone);

			var personRequest = createOvertimeRequest(LoggedOnUser.CurrentUser(), requestPeriod);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			target.Approve(personRequest.Request);
		}
	}
}
