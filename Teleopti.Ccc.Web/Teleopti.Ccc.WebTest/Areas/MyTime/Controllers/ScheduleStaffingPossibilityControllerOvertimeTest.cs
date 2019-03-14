using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.WebTest.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[MyTimeWebTest]
	public class ScheduleStaffingPossibilityControllerOvertimeTest : IIsolateSystem, ITestInterceptor
	{
		public ScheduleStaffingPossibilityController Target;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillTypeRepository SkillTypeRepository;
		public FakeToggleManager ToggleManager;
		public SkillIntradayStaffingFactory SkillIntradayStaffingFactory;

		readonly ISkillType phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();

		private readonly TimePeriod _defaultSiteOpenHour = new TimePeriod(8, 0, 17, 0);
		private readonly TimePeriod _defaultSkillOpenHour = new TimePeriod(8, 00, 9, 30);
		private DateTimePeriod _defaultAssignmentPeriod;
		private int _defaultSkillStaffingIntervalNumber;
		private TimeZoneInfo _defaultTimezone;
		private IPerson _loggedOnUser;
		private DateOnly _today;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			isolate.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			isolate.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
			isolate.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			isolate.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
			isolate.UseTestDouble<FakeSkillTypeRepository>().For<ISkillTypeRepository>();
		}

		public void OnBefore()
		{
			_today = Now.UtcDateTime().ToDateOnly();
			_loggedOnUser = User.CurrentUser();
			_defaultTimezone = User.CurrentUser().PermissionInformation.DefaultTimeZone();

			_defaultSkillStaffingIntervalNumber =
				(int)_defaultSkillOpenHour.EndTime.Subtract(_defaultSkillOpenHour.StartTime).TotalMinutes / 15;

			_defaultAssignmentPeriod =
				new DateTimePeriod(TimeZoneHelper.ConvertToUtc(_today.Date.AddHours(8), _defaultTimezone),
					TimeZoneHelper.ConvertToUtc(_today.Date.AddHours(17), _defaultTimezone));
		}

		[Test]
		public void ShouldReturnPossibilitiesForDaysLessThan48()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(7), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 10d);
			}

			var weekPeriod2 = DateHelper.GetWeekPeriod(_today.AddWeeks(6), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod2.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 10d);
			}

			var workflowControlSet = _loggedOnUser.WorkflowControlSet;
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, StaffingInfoAvailableDaysProvider.GetDays(ToggleManager))
			});

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(6), StaffingPossibilityType.Overtime)
				.ToList();

			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber * 7);
		}

		[Test]
		public void ShouldNotReturnPossibilitiesForDaysOutOfOvertimeOpenPeriod()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(7), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 10d);
			}

			var weekPeriod2 = DateHelper.GetWeekPeriod(_today.AddWeeks(8), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod2.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 10d);
			}

			var workflowControlSet = _loggedOnUser.WorkflowControlSet;
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, StaffingInfoAvailableDaysProvider.GetDays(ToggleManager))
			});

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(8), StaffingPossibilityType.Overtime)
				.ToList();

			possibilities.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnEmptyPossibilities()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today, CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 10d);
			}

			var workflowControlSet = _loggedOnUser.WorkflowControlSet;
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, StaffingInfoAvailableDaysProvider.GetDays(ToggleManager))
			});
			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today)
				.ToList();

			possibilities.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotGetOvertimePossibilitiesWhenSiteOpenHourIsClosed()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site, true);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			setupIntradayStaffingSkillFor24Hours(skill, _today, 10d, 10d);

			var result = Target.GetPossibilityViewModelsForMobileDay(_today, StaffingPossibilityType.Overtime).ToList();
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotGetOvertimePossibilitiesForDaysWhichSiteOpenHourIsClosed()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site, true);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			setupIntradayStaffingSkillFor24Hours(skill, _today, 10d, 10d);

			var result = Target.GetPossibilityViewModelsForWeek(_today, StaffingPossibilityType.Overtime).ToList();
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetPossibilitiesWithoutIntradaySchedule()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(1), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 10d);
			}

			var workflowControlSet = _loggedOnUser.WorkflowControlSet;
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, StaffingInfoAvailableDaysProvider.GetDays(ToggleManager))
			});
			PersonAssignmentRepository.Clear();

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(1), StaffingPossibilityType.Overtime)
				.ToList();

			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber * 7);
		}

		[Test]
		public void ShouldNotGetPossibilitiesForNotSupportedSkill()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(1), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 10d);
			}

			var overtimeRequestOpenPeriod = _loggedOnUser.WorkflowControlSet.OvertimeRequestOpenPeriods.FirstOrDefault();
			overtimeRequestOpenPeriod.ClearPeriodSkillType();
			overtimeRequestOpenPeriod.AddPeriodSkillType(
				new OvertimeRequestOpenPeriodSkillType(new SkillTypePhone(new Description("NotSupportedSkillType"),
					ForecastSource.Backoffice)));

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(1), StaffingPossibilityType.Overtime)
				.ToList();

			possibilities.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenOverstaffing()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(1), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 22d);
			}

			var workflowControlSet = _loggedOnUser.WorkflowControlSet;
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, StaffingInfoAvailableDaysProvider.GetDays(ToggleManager))
			});

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(1), StaffingPossibilityType.Overtime)
				.ToList();

			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber * 7);
			possibilities.ElementAt(0).Possibility.Should().Be.EqualTo(0);
			possibilities.ElementAt(_defaultSkillStaffingIntervalNumber * 7 - 1).Possibility.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetGoodPossibilitiesForOvertimeWhenNotOverstaffing()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(1), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 6d);
			}

			var workflowControlSet = _loggedOnUser.WorkflowControlSet;
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, StaffingInfoAvailableDaysProvider.GetDays(ToggleManager))
			});

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(1), StaffingPossibilityType.Overtime)
				.ToList();

			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber * 7);
			possibilities.ElementAt(0).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(_defaultSkillStaffingIntervalNumber * 7 - 1).Possibility.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetFairPossibilitiesAfterRoundStaffingDataWithTwoFractional()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(1), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 0.001d, 0d);
			}

			var workflowControlSet = _loggedOnUser.WorkflowControlSet;
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, StaffingInfoAvailableDaysProvider.GetDays(ToggleManager))
			});

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(1), StaffingPossibilityType.Overtime)
				.ToList();

			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber * 7);
			possibilities.ElementAt(0).Possibility.Should().Be.EqualTo(0);
			possibilities.ElementAt(_defaultSkillStaffingIntervalNumber * 7 - 1).Possibility.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotGetPossibilitiesForOvertimeWhenOverstaffingAndNoSkillTypeIsMatched()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(1), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 22d);
			}

			var workflowControlSet = new WorkflowControlSet();
			var emailSkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			_loggedOnUser.WorkflowControlSet = workflowControlSet;

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(1), StaffingPossibilityType.Overtime)
				.ToList();

			possibilities.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenOverstaffingAndSkillTypeIsMatched()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(1), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 22d);
			}

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			_loggedOnUser.WorkflowControlSet = workflowControlSet;

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(1), StaffingPossibilityType.Overtime)
				.ToList();
			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber * 7);
			possibilities.ElementAt(0).Possibility.Should().Be.EqualTo(0);
			possibilities.ElementAt(_defaultSkillStaffingIntervalNumber * 7 - 1).Possibility.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnNoPossibilitiesForOvertimeWhenOverstaffingAndSkillTypeIsNotSet()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = SkillFactory.CreateSkill("skill for test 1 without skill type").WithId();
			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, _defaultSkillOpenHour);
			SkillRepository.Has(skill);

			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today, CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 22d);
			}

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			_loggedOnUser.WorkflowControlSet = workflowControlSet;

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today, StaffingPossibilityType.Overtime)
				.ToList();
			possibilities.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetPossibilitiesForOvertimeBasedOnSkillTypeSetInOpenPeriod()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);

			var emailSkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();
			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();

			var person = User.CurrentUser();
			var activity1 = createActivity("activity 1");
			var skill1 = createSkill("skill 1", _defaultSkillOpenHour);
			skill1.SkillType = emailSkillType;
			var personSkill1 = createPersonSkill(activity1, skill1);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(1), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill1, dateOnly, 10d, 15d);
			}

			var activity2 = createActivity("activity 2");
			var skill2 = createSkill("skill 2", _defaultSkillOpenHour);
			skill2.SkillType = phoneSkillType;
			var personSkill2 = createPersonSkill(activity2, skill2);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill2, dateOnly, 10d, 5d);
			}

			createAssignment(person, _defaultAssignmentPeriod, activity1, activity2);

			addPersonSkillsToPersonPeriod(personPeriod, personSkill1, personSkill2);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(_today, _today.AddDays(13))
			});
			_loggedOnUser.WorkflowControlSet = workflowControlSet;

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(1), StaffingPossibilityType.Overtime).ToList();

			possibilities.Count.Should().Be(_defaultSkillStaffingIntervalNumber * 7);
			possibilities.ElementAt(0).Possibility.Should().Be(0);
			possibilities.ElementAt(1).Possibility.Should().Be(0);
		}

		[Test]
		public void ShouldGetPossibilitiesForOvertimeBasedOnSkillTypeOfEachDaySetInOpenPeriod()
		{
			Now.Is(new DateTime(2018, 2, 1, 8, 0, 0, DateTimeKind.Utc));
			var today = Now.UtcDateTime().ToDateOnly();

			var personPeriod = getOrAddPersonPeriod(today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);

			var phoneSkillType =
				new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
					.WithId();
			var chatSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat)
				.WithId();

			var activity1 = createActivity("activity 1");
			var phoneSkill = createSkill("phoneSkill", _defaultSkillOpenHour);
			phoneSkill.SkillType = phoneSkillType;
			var personPhoneSkill = createPersonSkill(activity1, phoneSkill);

			setupIntradayStaffingSkillFor24Hours(phoneSkill, today.AddDays(1), 10d, 15d);
			setupIntradayStaffingSkillFor24Hours(phoneSkill, today.AddDays(2), 10d, 15d);

			var activity2 = createActivity("activity 2");
			var chatSkill = createSkill("chatSkill", _defaultSkillOpenHour);
			chatSkill.SkillType = chatSkillType;
			var personChatSkill = createPersonSkill(activity2, chatSkill);

			setupIntradayStaffingSkillFor24Hours(chatSkill, today.AddDays(1), 10d, 5d);
			setupIntradayStaffingSkillFor24Hours(chatSkill, today.AddDays(2), 10d, 5d);

			addPersonSkillsToPersonPeriod(personPeriod, personPhoneSkill, personChatSkill);

			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity1, activity2);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 1),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 2),
				OrderIndex = 2
			});
			_loggedOnUser.WorkflowControlSet = workflowControlSet;

			var possibilities = Target.GetPossibilityViewModelsForWeek(today, StaffingPossibilityType.Overtime);

			var possibilitiesOn2nd = possibilities.Where(d => d.Date == today.AddDays(1).ToFixedClientDateOnlyFormat())
				.ToList();
			possibilitiesOn2nd.ElementAt(0).StartTime.Should().Be.EqualTo(new DateTime(2018, 2, 2, 8, 0, 0));
			possibilitiesOn2nd.ElementAt(0).Possibility.Should().Be.EqualTo(1);
			possibilitiesOn2nd.ElementAt(1).Possibility.Should().Be.EqualTo(1);

			var possibilitiesOn3rd = possibilities.Where(d => d.Date == today.AddDays(2).ToFixedClientDateOnlyFormat())
				.ToList();
			possibilitiesOn3rd.ElementAt(0).StartTime.Should().Be(new DateTime(2018, 2, 3, 8, 0, 0));
			possibilitiesOn3rd.ElementAt(0).Possibility.Should().Be.EqualTo(0);
			possibilitiesOn3rd.ElementAt(1).Possibility.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetPossibilitiesForOvertimeBasedOnSkillTypeOfOneDaySetInOpenPeriod()
		{
			Now.Is(new DateTime(2018, 2, 1, 8, 0, 0, DateTimeKind.Utc));
			var today = Now.UtcDateTime().ToDateOnly();

			var personPeriod = getOrAddPersonPeriod(today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);

			var phoneSkillType =
				new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
					.WithId();
			var chatSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat)
				.WithId();

			var activity1 = createActivity("activity 1");
			var phoneSkill = createSkill("phoneSkill", new TimePeriod(8, 0, 17, 0));
			phoneSkill.SkillType = phoneSkillType;
			var personPhoneSkill = createPersonSkill(activity1, phoneSkill);

			setupIntradayStaffingSkillFor24Hours(phoneSkill, today.AddDays(1), 10d, 15d);
			setupIntradayStaffingSkillFor24Hours(phoneSkill, today.AddDays(2), 10d, 15d);

			var activity2 = createActivity("activity 2");
			var chatSkill = createSkill("chatSkill", new TimePeriod(8, 0, 17, 0));
			chatSkill.SkillType = chatSkillType;
			var personChatSkill = createPersonSkill(activity2, chatSkill);

			setupIntradayStaffingSkillFor24Hours(chatSkill, today.AddDays(1), 10d, 5d);
			setupIntradayStaffingSkillFor24Hours(chatSkill, today.AddDays(2), 10d, 5d);

			addPersonSkillsToPersonPeriod(personPeriod, personPhoneSkill, personChatSkill);

			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity1, activity2);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 1),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 2),
				OrderIndex = 2
			});
			_loggedOnUser.WorkflowControlSet = workflowControlSet;

			var possibilities =
				Target.GetPossibilityViewModelsForMobileDay(today.AddDays(2), StaffingPossibilityType.Overtime);

			var possibilitiesOn3rd = possibilities.Where(d => d.Date == today.AddDays(2).ToFixedClientDateOnlyFormat())
				.ToList();
			possibilitiesOn3rd.ElementAt(0).StartTime.Should().Be.EqualTo(today.AddDays(2).Date.AddHours(8));
			possibilitiesOn3rd.Last().StartTime.Should().Be.EqualTo(today.AddDays(2).Date.AddHours(16).AddMinutes(45));
			possibilitiesOn3rd.ElementAt(0).Possibility.Should().Be.EqualTo(0);
			possibilitiesOn3rd.ElementAt(1).Possibility.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotGetPossibilitiesForOvertimeWhenOneOfThePeriodIsDeny()
		{
			Now.Is(new DateTime(2018, 1, 31, 8, 0, 0, DateTimeKind.Utc));
			var today = new DateOnly(new DateTime(2018, 1, 31, 8, 0, 0, DateTimeKind.Utc));

			var personPeriod = getOrAddPersonPeriod(today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);

			var chatSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat).WithId();

			var activity1 = createActivity("activity 1");
			var chatSkill = createSkill("phoneSkill", new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1)));
			chatSkill.SkillType = chatSkillType;
			var personChatSkill = createPersonSkill(activity1, chatSkill);
			addPersonSkillsToPersonPeriod(personPeriod, personChatSkill);

			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity1);
			setupIntradayStaffingSkillFor24Hours(chatSkill, today, 10d, 5d);
			setupIntradayStaffingSkillFor24Hours(chatSkill, today.AddDays(1), 10d, 5d);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				BetweenDays = new MinMax<int>(0, 7),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(1, 2),
				OrderIndex = 1
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var possibilities = Target.GetPossibilityViewModelsForWeek(today, StaffingPossibilityType.Overtime);

			var possibilitiesOn1st = possibilities.Where(d => d.Date == today.ToFixedClientDateOnlyFormat()).ToList();
			var possibilitiesOn2nd = possibilities.Where(d => d.Date == today.AddDays(1).ToFixedClientDateOnlyFormat()).ToList();
			var possibilitiesOn3rd = possibilities.Where(d => d.Date == today.AddDays(2).ToFixedClientDateOnlyFormat()).ToList();

			possibilitiesOn1st.Count.Should().Be.EqualTo(96);
			possibilitiesOn2nd.Count.Should().Be.EqualTo(0);
			possibilitiesOn3rd.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenNotOverstaffingWithoutShrinkage()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(1), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 8d);
			}

			var workflowControlSet = _loggedOnUser.WorkflowControlSet;
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, StaffingInfoAvailableDaysProvider.GetDays(ToggleManager))
			});
			User.CurrentUser().WorkflowControlSet.OvertimeRequestStaffingCheckMethod =
				OvertimeRequestStaffingCheckMethod.Intraday;

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(1), StaffingPossibilityType.Overtime)
				.ToList();

			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber * 7);
			possibilities.ElementAt(0).Possibility.Should().Be.EqualTo(0);
			possibilities.ElementAt(_defaultSkillStaffingIntervalNumber * 7 - 1).Possibility.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetGoodPossibilitiesForOvertimeWhenNotOverstaffingWithShrinkage()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(1), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 8d);
			}

			var workflowControlSet = _loggedOnUser.WorkflowControlSet;
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, StaffingInfoAvailableDaysProvider.GetDays(ToggleManager))
			});
			User.CurrentUser().WorkflowControlSet.OvertimeRequestStaffingCheckMethod =
				OvertimeRequestStaffingCheckMethod.IntradayWithShrinkage;

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(1), StaffingPossibilityType.Overtime)
				.ToList();

			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber * 7);
			possibilities.ElementAt(0).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(_defaultSkillStaffingIntervalNumber * 7 - 1).Possibility.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetFairAndGoodPossibilitiesForOvertime()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(1), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				var forecastedStaffingList = new List<double> { 10d, 10d };
				var scheduledStaffingList = new List<double> { 16d, 6d };
				var timePeriodsList = new List<TimePeriod> { new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30) };

				setupStaffingForSkill(skill, new List<SkillStaffingData>
				{
					new SkillStaffingData
					{
						Date = dateOnly,
						ForecastedStaffing = forecastedStaffingList,
						ScheduledStaffing = scheduledStaffingList,
						TimePeriods = timePeriodsList
					}
				});
			}

			var workflowControlSet = _loggedOnUser.WorkflowControlSet;
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, StaffingInfoAvailableDaysProvider.GetDays(ToggleManager))
			});

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(1), StaffingPossibilityType.Overtime)
				.ToList();

			possibilities.ElementAt(0).Possibility.Should().Be.EqualTo(0);
			possibilities.ElementAt(1).Possibility.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetGoodPossibilitiesForOvertimeWhenOneOfSkillIsNotCriticalUnderStaffing()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity1 = createActivity();
			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill1 = createPersonSkill(activity1, skill);

			var activity2 = createActivity("activity 2");
			var skill2 = createSkill("skill 2", _defaultSkillOpenHour);
			skill2.SkillType = phoneSkillType;
			var personSkill2 = createPersonSkill(activity2, skill2);


			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity1, activity2);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill1, personSkill2);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(1), CultureInfo.CurrentCulture);

			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill2, dateOnly, 10d, 15d);
			}

			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				var forecastedStaffingList = new List<double> { 10d, 10d };
				var scheduledStaffingList = new List<double> { 15d, 5d };
				var timePeriodsList = new List<TimePeriod> { new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30) };

				setupStaffingForSkill(skill, new List<SkillStaffingData>
				{
					new SkillStaffingData
					{
						Date = dateOnly,
						ForecastedStaffing = forecastedStaffingList,
						ScheduledStaffing = scheduledStaffingList,
						TimePeriods = timePeriodsList
					}
				});
			}

			var workflowControlSet = _loggedOnUser.WorkflowControlSet;
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, StaffingInfoAvailableDaysProvider.GetDays(ToggleManager))
			});

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(1), StaffingPossibilityType.Overtime)
				.ToList();

			possibilities.ElementAt(0).Possibility.Should().Be.EqualTo(0);
			possibilities.ElementAt(1).Possibility.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldMergePeriodsWithSameSkillType()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);

			var phoneSkillType =
				new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
					.WithId();
			SkillTypeRepository.Add(phoneSkillType);

			var chatSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat)
				.WithId();
			SkillTypeRepository.Add(chatSkillType);

			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, 7),
				AutoGrantType = OvertimeRequestAutoGrantType.No
			});
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(1, 4),
				AutoGrantType = OvertimeRequestAutoGrantType.Deny
			});
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { chatSkillType })
			{
				BetweenDays = new MinMax<int>(0, 48),
				AutoGrantType = OvertimeRequestAutoGrantType.No
			});
			_loggedOnUser.WorkflowControlSet = workFlowControlSet;

			var activity1 = createActivity();
			var channelSupportSkill = createSkill("channelSupportCriticalUnderStaffedSkill", _defaultSkillOpenHour);
			channelSupportSkill.SkillType = phoneSkillType;
			var personSkill1 = createPersonSkill(activity1, channelSupportSkill);
			for (var i = 0; i < 7; i++)
			{
				setupStaffingForSkill(channelSupportSkill, new List<SkillStaffingData>
				{
					new SkillStaffingData
					{
						Date = _today.AddDays(i),
						ForecastedStaffing = new List<double> {10d, 10d},
						ScheduledStaffing = new List<double> {5d, 5d},
						TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
					}
				});
			}

			var activity2 = createActivity();
			var webChatSkill = createSkill("webChatSkill not cricical understaffed at first", _defaultSkillOpenHour);
			webChatSkill.SkillType = chatSkillType;
			var personSkill2 = createPersonSkill(activity2, webChatSkill);
			for (var i = 0; i < 7; i++)
			{
				setupStaffingForSkill(webChatSkill, new List<SkillStaffingData>
				{
					new SkillStaffingData
					{
						Date = _today.AddDays(i),
						ForecastedStaffing = new List<double> {10d, 10d},
						ScheduledStaffing = new List<double> {15d, 5d},
						TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
					}
				});
			}

			addPersonSkillsToPersonPeriod(personPeriod, personSkill1, personSkill2);

			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity1, activity2);

			var possibilities = Target.GetPossibilityViewModelsForWeek(null, StaffingPossibilityType.Overtime)
				.Where(d => d.Date == _today.AddDays(1).ToFixedClientDateOnlyFormat()).ToList();

			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(0).Possibility.Should().Be.EqualTo(0);
			possibilities.ElementAt(1).Possibility.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetPossibilitiesForOvertimeWithDayOff()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var assignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(_loggedOnUser, Scenario.Current(),
				_today, new DayOffTemplate());
			PersonAssignmentRepository.Has(assignment);

			var activity = createActivity();
			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today, CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupStaffingForSkill(skill, new List<SkillStaffingData>
				{
					new SkillStaffingData
					{
						Date = dateOnly,
						ForecastedStaffing = new List<double> {10d, 10d},
						ScheduledStaffing = new List<double> {16d, 6d},
						TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
					}
				});
			}

			var possibilities = Target.GetPossibilityViewModelsForWeek(null, StaffingPossibilityType.Overtime)
				.Where(d => d.Date == _today.ToFixedClientDateOnlyFormat()).ToList();

			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber);
		}

		[Test]
		public void ShouldUsePrimarySkillsForOvertimeProbabilityWhenUsePrimarySkillValidationIsEnabled()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);

			var workflowControlSet = setupWorkFlowControlSet();
			workflowControlSet.OvertimeRequestUsePrimarySkill = true;

			var activity1 = createActivity();
			var primarySkill = createSkill("primary skill for test", _defaultSkillOpenHour);
			primarySkill.SetCascadingIndex(0);
			var personSkill = createPersonSkill(activity1, primarySkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today, CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupStaffingForSkill(primarySkill, new List<SkillStaffingData>
				{
					new SkillStaffingData
					{
						Date = dateOnly,
						ForecastedStaffing = new List<double> {5d, 5d},
						ScheduledStaffing = new List<double> {1d, 1d},
						TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
					}
				});
			}

			var activity2 = createActivity();
			var nonprimarySkill = createSkill("non primary skill for test", _defaultSkillOpenHour);
			var personSkill2 = createPersonSkill(activity2, nonprimarySkill);

			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupStaffingForSkill(nonprimarySkill, new List<SkillStaffingData>
				{
					new SkillStaffingData
					{
						Date = dateOnly,
						ForecastedStaffing = new List<double> {5d, 5d},
						ScheduledStaffing = new List<double> {6d, 6d},
						TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
					}
				});
			}

			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity1, activity2);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill, personSkill2);

			var possibilities = Target.GetPossibilityViewModelsForWeek(null, StaffingPossibilityType.Overtime)
				.Where(d => d.Date == _today.ToFixedClientDateOnlyFormat()).ToList();

			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(0).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(1).Possibility.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldUseAllSkillsForOvertimeProbabilityWhenUsePrimarySkillValidationIsDisabled()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);

			var workflowControlSet = setupWorkFlowControlSet();
			workflowControlSet.OvertimeRequestUsePrimarySkill = false;

			var activity1 = createActivity();
			var primarySkill = createSkill("primary skill for test", _defaultSkillOpenHour);
			primarySkill.SetCascadingIndex(1);
			var personSkill = createPersonSkill(activity1, primarySkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today, CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupStaffingForSkill(primarySkill, new List<SkillStaffingData>
				{
					new SkillStaffingData
					{
						Date = dateOnly,
						ForecastedStaffing = new List<double> {5d, 5d},
						ScheduledStaffing = new List<double> {10d, 10d},
						TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
					}
				});
			}

			var activity2 = createActivity();
			var nonprimarySkill = createSkill("non primary skill for test", _defaultSkillOpenHour);
			var personSkill2 = createPersonSkill(activity2, nonprimarySkill);

			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupStaffingForSkill(nonprimarySkill, new List<SkillStaffingData>
				{
					new SkillStaffingData
					{
						Date = dateOnly,
						ForecastedStaffing = new List<double> {5d, 5d},
						ScheduledStaffing = new List<double> {1d, 1d},
						TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
					}
				});
			}

			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity1, activity2);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill, personSkill2);

			var possibilities = Target.GetPossibilityViewModelsForWeek(null, StaffingPossibilityType.Overtime)
				.Where(d => d.Date == _today.ToFixedClientDateOnlyFormat()).ToList();

			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(0).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(1).Possibility.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldUsePrimarySkillsForOvertimeProbabilityWhenPrimarySkillIsNotLevel1InCascading()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);

			var workflowControlSet = setupWorkFlowControlSet();
			workflowControlSet.OvertimeRequestUsePrimarySkill = true;

			var primarySkill1 = createSkill("primary skill 1 for test", _defaultSkillOpenHour);
			primarySkill1.SetCascadingIndex(1);

			var weekPeriod = DateHelper.GetWeekPeriod(_today, CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupStaffingForSkill(primarySkill1, new List<SkillStaffingData>
				{
					new SkillStaffingData
					{
						Date = dateOnly,
						ForecastedStaffing = new List<double> {5d, 5d},
						ScheduledStaffing = new List<double> {10d, 10d},
						TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
					}
				});
			}

			var activity = createActivity();
			var primarySkill2 = createSkill("primary skill 2 for test", _defaultSkillOpenHour);
			primarySkill2.SetCascadingIndex(2);
			var personSkill2 = createPersonSkill(activity, primarySkill2);

			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupStaffingForSkill(primarySkill2, new List<SkillStaffingData>
				{
					new SkillStaffingData
					{
						Date = dateOnly,
						ForecastedStaffing = new List<double> {5d, 5d},
						ScheduledStaffing = new List<double> {10d, 10d},
						TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
					}
				});
			}

			var nonprimarySkill = createSkill("non primary skill for test", _defaultSkillOpenHour);
			var personSkill3 = createPersonSkill(activity, nonprimarySkill);

			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupStaffingForSkill(nonprimarySkill, new List<SkillStaffingData>
				{
					new SkillStaffingData
					{
						Date = dateOnly,
						ForecastedStaffing = new List<double> {5d, 5d},
						ScheduledStaffing = new List<double> {1d, 1d},
						TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
					}
				});
			}

			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill2, personSkill3);

			var possibilities = Target.GetPossibilityViewModelsForWeek(null, StaffingPossibilityType.Overtime)
				.Where(d => d.Date == _today.ToFixedClientDateOnlyFormat()).ToList();

			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(0).Possibility.Should().Be.EqualTo(0);
			possibilities.ElementAt(1).Possibility.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetGoodOvertimePossibilitiesWhenAllSkillsArePrimarySkillWithOneSkillCriticalUnderStaffing()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);

			var workflowControlSet = setupWorkFlowControlSet();
			workflowControlSet.OvertimeRequestUsePrimarySkill = true;

			var activity1 = createActivity();
			var primarySkill1 = createSkill("primary skill 1 for test", _defaultSkillOpenHour);
			var personSkill1 = createPersonSkill(activity1, primarySkill1);
			primarySkill1.SetCascadingIndex(1);

			var weekPeriod = DateHelper.GetWeekPeriod(_today, CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupStaffingForSkill(primarySkill1, new List<SkillStaffingData>
				{
					new SkillStaffingData
					{
						Date = dateOnly,
						ForecastedStaffing = new List<double> {5d, 5d},
						ScheduledStaffing = new List<double> {0d,0d},
						TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
					}
				});
			}

			var activity2 = createActivity();
			var primarySkill2 = createSkill("primary skill 2 for test", _defaultSkillOpenHour);
			primarySkill2.SetCascadingIndex(1);
			var personSkill2 = createPersonSkill(activity2, primarySkill2);

			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupStaffingForSkill(primarySkill2, new List<SkillStaffingData>
				{
					new SkillStaffingData
					{
						Date = dateOnly,
						ForecastedStaffing = new List<double> {5d, 5d},
						ScheduledStaffing = new List<double> {7d, 7d},
						TimePeriods = new List<TimePeriod> {new TimePeriod(8, 00, 8, 15), new TimePeriod(8, 15, 8, 30)}
					}
				});
			}

			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity1, activity2);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill1, personSkill2);

			var possibilities = Target.GetPossibilityViewModelsForWeek(null, StaffingPossibilityType.Overtime)
				.Where(d => d.Date == _today.ToFixedClientDateOnlyFormat()).ToList();

			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(0).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(1).Possibility.Should().Be.EqualTo(1);

		}

		[Test]
		public void ShouldReturnPossibilitiesWithoutOvertimeOpenPeriod()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test 1", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(1), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 10d);
			}

			var workFlowControlSet = new WorkflowControlSet()
			{
				OvertimeProbabilityEnabled = true
			};
			_loggedOnUser.WorkflowControlSet = workFlowControlSet;

			var possibilities = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(1), StaffingPossibilityType.Overtime)
				.ToList();

			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber * 7);
		}

		[Test]
		public void ShouldReturnCorrectEndTimeOnDSTDay()
		{
			TimeZone.Is(TimeZoneInfoFactory.CentralStandardTime());
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());

			Now.Is("2018-03-11 6:00");

			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			var timePeriod = new TimePeriod(1, 0, 4, 0);
			var team = personPeriod.Team;
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});

			var activity = createActivity();
			var skill = createSkill("test1", timePeriod);
			skill.TimeZone = TimeZone.TimeZone();
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);
			createAssignment(User.CurrentUser(), _defaultAssignmentPeriod, activity);

			var date = new DateOnly(2018, 3, 11);
			var staffingPeriodData = new List<StaffingPeriodData>();
			staffingPeriodData.Add(new StaffingPeriodData() { Period = date.ToDateTimePeriod(timePeriod, TimeZone.TimeZone()), ForecastedStaffing = 1, ScheduledStaffing = 2 });
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(skill, date, staffingPeriodData, TimeZone.TimeZone());

			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, 13)
			});
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.GetPossibilityViewModelsForMobileDay(date, StaffingPossibilityType.Overtime).ToList();
			result.Count.Should().Be.EqualTo(8);
			result[3].EndTime.TimeOfDay.TotalMinutes.Should().Be.EqualTo(180);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenSeriousUnderstaffingIsZeroAndForecastedAndScheduledAreEqual()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);

			TimeZone.Is(TimeZoneInfoFactory.DenverTimeZoneInfo());
			_loggedOnUser.PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());

			var workflowControlSet = setupWorkFlowControlSet();
			workflowControlSet.OvertimeRequestStaffingCheckMethod = OvertimeRequestStaffingCheckMethod.Intraday;

			var activity = createActivity();
			var skill = createSkill("skill for test", _defaultSkillOpenHour);
			skill.TimeZone = TimeZone.TimeZone();
			skill.StaffingThresholds = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0.1));
			var personSkill1 = createPersonSkill(activity, skill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today.AddWeeks(1), CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 6d, 6d);
			}

			addPersonSkillsToPersonPeriod(personPeriod, personSkill1);

			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var possibilitiesWeek = Target
				.GetPossibilityViewModelsForWeek(_today.AddWeeks(1), StaffingPossibilityType.Overtime).ToList();

			possibilitiesWeek.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber * 7);
			possibilitiesWeek.ElementAt(0).Possibility.Should().Be.EqualTo(0);
			possibilitiesWeek.ElementAt(1).Possibility.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnTwoDaysPossibilitiesOvertime()
		{
			var personPeriod = getOrAddPersonPeriod(_today, _loggedOnUser);
			setupSiteOpenHour(_defaultSiteOpenHour, personPeriod.Team.Site);
			setupWorkFlowControlSet();

			var activity = createActivity();
			createAssignment(_loggedOnUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test", _defaultSkillOpenHour);
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personPeriod, personSkill);

			var weekPeriod = DateHelper.GetWeekPeriod(_today, CultureInfo.CurrentCulture);
			foreach (var dateOnly in weekPeriod.DayCollection())
			{
				setupIntradayStaffingSkillFor24Hours(skill, dateOnly, 10d, 10d);
			}

			var possibilities = Target
				.GetPossibilityViewModelsForMobileDay(_today, StaffingPossibilityType.Overtime)
				.ToList();

			possibilities.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber * 2);
		}

		private WorkflowControlSet setupWorkFlowControlSet()
		{
			var absenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("absence for test"),
				Period = new DateOnlyPeriod(_today.AddDays(-20),
					_today.AddDays(20)),
				OpenForRequestsPeriod = new DateOnlyPeriod(_today.AddDays(-20),
					_today.AddDays(20)),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			var overtimeRequestOpenDatePeriod = new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 13)
			};

			var workFlowControlSet = new WorkflowControlSet();

			workFlowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenDatePeriod);
			workFlowControlSet.AddOpenOvertimeRequestPeriod(overtimeRequestOpenDatePeriod);

			_loggedOnUser.WorkflowControlSet = workFlowControlSet;
			return workFlowControlSet;
		}

		private PersonPeriod getOrAddPersonPeriod(DateOnly startDate, IPerson person)
		{
			var personPeriod =
				(PersonPeriod)person.PersonPeriods(startDate.ToDateOnlyPeriod())
					.FirstOrDefault();
			if (personPeriod != null) return personPeriod;
			var team = TeamFactory.CreateTeam("team1", "site1");
			personPeriod =
				(PersonPeriod)
				PersonPeriodFactory.CreatePersonPeriod(startDate,
					PersonContractFactory.CreatePersonContract(), team);
			person.AddPersonPeriod(personPeriod);
			return personPeriod;
		}

		private void setupSiteOpenHour(TimePeriod timePeriod, ISite site, bool isClosed = false)
		{
			site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = isClosed,
				WeekDay = DayOfWeek.Thursday
			});
		}

		private void addPersonSkillsToPersonPeriod(PersonPeriod personPeriod, params IPersonSkill[] personSkills)
		{
			foreach (var personSkill in personSkills)
			{
				personPeriod.AddPersonSkill(personSkill);
			}
		}

		private IPersonSkill createPersonSkill(IActivity activity, ISkill skill)
		{
			skill.Activity = activity;
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			return personSkill;
		}

		private ISkill createSkill(string name, TimePeriod openHour)
		{
			var skill = SkillFactory.CreateSkill(name).WithId();
			skill.SkillType = phoneSkillType;
			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHour);
			SkillRepository.Has(skill);
			return skill;
		}

		private static StaffingThresholds createStaffingThresholds()
		{
			return new StaffingThresholds(new Percent(-0.3), new Percent(-0.1), new Percent(0.1));
		}

		private static IActivity createActivity(string name = "activity1")
		{
			var activity = ActivityFactory.CreateActivity(name);
			activity.RequiresSkill = true;
			return activity;
		}

		private IPersonAssignment createAssignment(IPerson person, DateTimePeriod dateTimePeriod,
			params IActivity[] activities)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				Scenario.Current(),
				dateTimePeriod,
				ShiftCategoryFactory.CreateShiftCategory(), activities);
			PersonAssignmentRepository.Has(assignment);

			return assignment;
		}

		private void setupStaffingForSkill(ISkill skill, List<SkillStaffingData> staffingDataList)
		{
			var staffingPeriodDataList = new List<StaffingPeriodData>();
			staffingDataList.ForEach(staffingData =>
			{
				for (var i = 0; i < staffingData.ForecastedStaffing.Count; i++)
				{
					var staffingPeriodData = new StaffingPeriodData
					{
						ForecastedStaffing = staffingData.ForecastedStaffing[i],
						ScheduledStaffing = staffingData.ScheduledStaffing[i],
						Period = new DateTimePeriod(staffingData.Date.Utc().Add(staffingData.TimePeriods[i].StartTime),
							staffingData.Date.Utc().Date.Add(staffingData.TimePeriods[i].EndTime))
					};
					staffingPeriodDataList.Add(staffingPeriodData);
				}

				SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(skill, staffingData.Date,
					staffingPeriodDataList,
					_loggedOnUser.PermissionInformation.DefaultTimeZone());

				staffingPeriodDataList.Clear();
			});
		}

		private void setupIntradayStaffingSkillFor24Hours(ISkill skill, DateOnly date, double forecastedStaffing,
			double scheduledStaffing)
		{
			var forecastedStaffingList = new List<double>();
			var scheduledStaffingList = new List<double>();
			var timePeriodsList = new List<TimePeriod>();

			var start = TimeSpan.Zero;
			while (date.Date.Add(start) < date.Date.AddDays(1).Subtract(TimeSpan.FromSeconds(1)))
			{
				forecastedStaffingList.Add(forecastedStaffing);
				scheduledStaffingList.Add(scheduledStaffing);
				timePeriodsList.Add(new TimePeriod(start, start.Add(TimeSpan.FromMinutes(15))));

				start += TimeSpan.FromMinutes(15);
			}

			var skillStaffingDataList = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = date,
					ForecastedStaffing = forecastedStaffingList,
					ScheduledStaffing = scheduledStaffingList,
					TimePeriods = timePeriodsList
				}
			};

			setupStaffingForSkill(skill, skillStaffingDataList);
		}

		class SkillStaffingData
		{
			public DateOnly Date;
			public List<double> ForecastedStaffing { get; set; }
			public List<double> ScheduledStaffing { get; set; }
			public List<TimePeriod> TimePeriods { get; set; }
		}
	}
}
