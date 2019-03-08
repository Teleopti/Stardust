﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;
using Teleopti.Ccc.WebTest.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture(true)]
	[TestFixture(false)]
	[MyTimeWebTest]
	[SetCulture("sv-SE")]
	public class ScheduleApiControllerWeekTimeLineTest:IIsolateSystem
	{
		public ScheduleApiController Target;
		public ICurrentScenario Scenario;
		public FakeLoggedOnUser User;
		public IScheduleStorage ScheduleData;
		public MutableNow Now;
		public IPushMessageDialogueRepository PushMessageDialogueRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeUserTimeZone UserTimeZone;
		public FakeBankHolidayCalendarSiteRepository BankHolidayCalendarSiteRepository;
		public FakeBankHolidayCalendarRepository BankHolidayCalendarRepository;
		public FakeBankHolidayDateRepository BankHolidayDateRepository;
		public FakePersonRepository PersonRepository;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;

		private readonly ISkillType skillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();
		private readonly Action<FakeToggleManager> _configure;

		public ScheduleApiControllerWeekTimeLineTest(bool optimizedEnabled)
		{
			_configure = t => t.Set(Toggles.WFM_ProbabilityView_ImproveResponseTime_80040, optimizedEnabled);
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			_configure.Invoke(toggleManager);
		}

		public void Isolate(IIsolate isolate)
		{
			var skill = new Skill("test1").WithId();
			skill.SkillType = skillType;
			var skillRepository = new FakeSkillRepository();
			skillRepository.Has(skill);
			isolate.UseTestDouble(skillRepository).For<ISkillRepository>();
			isolate.UseTestDouble(new FakeSkillTypeRepository(skillType)).For<ISkillTypeRepository>();

			isolate.UseTestDouble<FakeBankHolidayCalendarRepository>().For<IBankHolidayCalendarRepository>();
			isolate.UseTestDouble<FakeBankHolidayCalendarSiteRepository>().For<IBankHolidayCalendarSiteRepository>();
			isolate.UseTestDouble<FakeBankHolidayDateRepository>().For<IBankHolidayDateRepository>();
			isolate.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			isolate.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}

		[Test]
		public void ShouldAdjustTimelineForOverTimeWhenSiteOpenHourPeriodContainsSchedulePeriod()
		{
			setupLoggedOnUser();
			addSiteOpenHour();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(null, StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),7,45,17,15); 
		}

		[Test]
		public void ShouldNotAdjustTimelineForOverTimeWhenSchedulePeriodContainsSiteOpenHourPeriod()
		{
			setupLoggedOnUser();
			addSiteOpenHour();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 7, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 17, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),7,0,18,0);
		}

		[Test]
		public void ShouldNotAdjustTimelineBySiteOpenHourWhenAskForAbsence()
		{
			addSiteOpenHour();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(null, StaffingPossibilityType.Absence);

			AssertTimeLine(result.TimeLine.ToList(),9,0,10,0);
		}

		[Test]
		public void ShouldNotAdjustTimelineForOverTimeWhenNoSiteOpenHourAvailable()
		{
			setupLoggedOnUser();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
			   new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(null, StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),9,0,10,0);
		}

		[Test]
		public void ShouldAdjustTimelineAccordingSiteOpenHourInWeek()
		{
			setupLoggedOnUser();
			addSiteOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(8, 0, 17, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			}, new SiteOpenHour
			{
				TimePeriod = new TimePeriod(7, 0, 18, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Friday
			});

			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),6,45,18,15);
		}

		[Test]
		public void ShouldAdjustTimelineForWeekScheduleWithMultipleSkillTypesMatched()
		{
			Now.Is(new DateTime(2018, 2, 1, 8, 0, 0, DateTimeKind.Utc));

			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
				.WithId();
			var chatSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat).WithId();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 48),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(5))),
				OrderIndex = 2
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var skill1 = addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var skill2 = addSkill(TimeSpan.Zero, TimeSpan.FromDays(1));
			skill1.SkillType = phoneSkillType;
			skill2.SkillType = chatSkillType;

			var day = DateHelper.GetFirstDateInWeek(Now.UtcDateTime().Date, CultureInfo.CurrentCulture);
			for (var i = 0; i < 14; i++)
			{
				day = day.AddDays(i);
				var period1 = new DateTimePeriod(day.AddHours(6).AddMinutes(15),
					day.AddHours(9).AddMinutes(45));
				var period2 = new DateTimePeriod(day.AddHours(10).AddMinutes(15),
					day.AddHours(10).AddMinutes(45));
				addAssignment(new DateOnly(day), period1, skill1.Activity);
				addAssignment(new DateOnly(day), period2, skill2.Activity);
			}

			var result = Target.FetchWeekData(new DateOnly(2018, 2, 5), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 0, 0, 23, 59);
		}

		[Test]
		public void ShouldAdjustTimelineForWeekScheduleByNotDeniedSkillType()
		{
			Now.Is(new DateTime(2018, 2, 5, 8, 0, 0, DateTimeKind.Utc));

			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
				.WithId();
			var emailSkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new ISkillType[] { phoneSkillType , emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 7),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(0, 7),
				OrderIndex = 2
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var phone = addSkill(TimeSpan.FromHours(8), TimeSpan.FromHours(17));
			var email = addSkill(TimeSpan.Zero, TimeSpan.FromDays(1));
			phone.SkillType = phoneSkillType;
			email.SkillType = emailSkillType;

			var day = DateHelper.GetFirstDateInWeek(Now.UtcDateTime().Date, CultureInfo.CurrentCulture);
			for (var i = 0; i < 14; i++)
			{
				day = day.AddDays(i);
				var period1 = new DateTimePeriod(day.AddHours(6).AddMinutes(15),
					day.AddHours(9).AddMinutes(45));
				var period2 = new DateTimePeriod(day.AddHours(10).AddMinutes(15),
					day.AddHours(10).AddMinutes(45));
				addAssignment(new DateOnly(day), period1, phone.Activity);
				addAssignment(new DateOnly(day), period2, email.Activity);
			}

			var result = Target.FetchWeekData(new DateOnly(2018, 2, 5), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 6, 0, 17, 15);
		}

		[Test]
		public void ShouldAdjustTimelineAccordingOpenPeriodSkillTypeOpenHourWeekSchedule()
		{
			var skillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { skillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime().AddDays(-10)), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var skill1 = addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var skill2 = addSkill(TimeSpan.Zero, TimeSpan.FromDays(1));
			skill2.SkillType = skillType;

			var day = DateHelper.GetFirstDateInWeek(Now.UtcDateTime().Date, CultureInfo.CurrentCulture);
			for (var i = 0; i < 7; i++)
			{
				day = day.AddDays(i);
				var period1 = new DateTimePeriod(day.AddHours(6).AddMinutes(15),
					day.AddHours(9).AddMinutes(45));
				var period2 = new DateTimePeriod(day.AddHours(10).AddMinutes(15),
					day.AddHours(10).AddMinutes(45));
				addAssignment(period1, skill1.Activity);
				addAssignment(period2, skill2.Activity);
			}

			var result = Target.FetchWeekData(null, StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 0, 0, 23, 59);
		}

		[Test]
		public void ShouldAdjustTimelineAccordingCrossDaySiteOpenHourInWeek()
		{
			setupLoggedOnUser();
			addSiteOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(8, 0, 17, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			}, new SiteOpenHour
			{
				TimePeriod = new TimePeriod(7, 0, 18, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Friday
			}, new SiteOpenHour
			{
				TimePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(25)),
				IsClosed = false,
				WeekDay = DayOfWeek.Saturday
			});

			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 0, 45, 23, 59);
		}

		[Test]
		public void ShouldAdjustTimelineAccordingCrossWeekSiteOpenHourInWeek()
		{
			setupLoggedOnUser();
			addSiteOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(8, 0, 17, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			},new SiteOpenHour
			{
				TimePeriod = new TimePeriod(7, 0, 18, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Friday
			},new SiteOpenHour
			{
				TimePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(25)),
				IsClosed = false,
				WeekDay = DayOfWeek.Sunday
			});
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);


			var result = Target.FetchWeekData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),6,45,23,59);
		}

		[Test]
		public void ShouldNotAdjustTimelineWithSiteOpenHourWhenCurrentWeekOutOfRange()
		{
			setupLoggedOnUser();
			addSiteOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(8, 0, 17, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			}, new SiteOpenHour
			{
				TimePeriod = new TimePeriod(7, 0, 18, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Friday
			}, new SiteOpenHour
			{
				TimePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(25)),
				IsClosed = false,
				WeekDay = DayOfWeek.Sunday
			});

			var period = new DateTimePeriod(new DateTime(2015, 1, 5, 9, 0, 0, DateTimeKind.Utc),
				   new DateTime(2015, 1, 5, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period, new DateOnly(2015, 1, 5));

			var result = Target.FetchWeekData(new DateOnly(2015, 1, 5), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),8,45,10,0);
		}

		[Test]
		public void ShouldGetUnreadMessageCount()
		{
			PushMessageDialogueRepository.Add(new PushMessageDialogue(new PushMessage(), User.CurrentUser()));
			var result = Target.GetUnreadMessageCount();

			result.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAdjustTimeLineBySkillOpenHourWhenSiteOpenHourIsNotAvailableWeekSchedule()
		{
			setupLoggedOnUser();
			var skill = addSkill();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period, skill.Activity);

			var result = Target.FetchWeekData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),6,45,18,15);
		}

		[Test]
		public void ShouldAdjustTimeLineByFullDaySkillOpenHourWhenSiteOpenHourIsNotAvailableWeekSchedule()
		{
			var skill = addSkill(TimeSpan.Zero, TimeSpan.FromDays(1));
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period, skill.Activity);
			setupLoggedOnUser();

			var result = Target.FetchWeekData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),0,0,23,59);
		}

		[Test]
		public void ShouldAdjustTimeLineByMultipleDaySkillOpenHoursWhenSiteOpenHourIsNotAvailableWeekSchedule()
		{
			setupLoggedOnUser();
			var skill1 = addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var skill2 = addSkill(TimeSpan.FromHours(8), TimeSpan.FromHours(19));
			var period1 = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2014, 12, 18, 10, 00, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 11, 00, 0, DateTimeKind.Utc));
			addAssignment(null, new activityDto { Period = period1, Activity = skill1.Activity },
				new activityDto { Period = period2, Activity = skill2.Activity });

			var result = Target.FetchWeekData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),6,45,19,15);
		}

		[Test]
		public void ShouldNotAdjustTimeLineByNonInBoundPhoneSkillOpenHoursWhenSiteOpenHourIsNotAvailableWeekSchedule()
		{
			setupLoggedOnUser();
			var personPeriod = (PersonPeriod)User.CurrentUser().PersonPeriods(new DateOnly(Now.UtcDateTime()).ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.ResetPersonSkill();

			var skill = addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			skill.SkillType = new SkillTypeEmail(new Description("test"), ForecastSource.Email);
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),9,0,10,0);
		}

		[Test]
		public void ShouldNotAdjustTimeLineBySkillOpenHoursWhenNoSkillAreScheduled()
		{
			setupLoggedOnUser();
			addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),6,45,15,15);
		}

		[Test]
		public void ShouldInflateMinMaxTimeAfterAdjustBySkillOpenHourWeekSchedule()
		{
			setupLoggedOnUser();
			var skill = addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var day = DateHelper.GetFirstDateInWeek(Now.UtcDateTime().Date, CultureInfo.CurrentCulture);
			for (var i = 0; i < 7; i++)
			{
				day = day.AddDays(i);
				var period = new DateTimePeriod(day.AddHours(6).AddMinutes(15),
					day.AddHours(9).AddMinutes(45));
				addAssignment(period, skill.Activity);
			}

			var result = Target.FetchWeekData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),6,0,15,15);
		}

		[Test]
		public void ShouldAdjustTimelineBySiteOpenHourAndSkillOpenHourWeekSchedule()
		{
			setupLoggedOnUser();
			var skill = addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			addSiteOpenHour();

			var day = DateHelper.GetFirstDateInWeek(Now.UtcDateTime().Date, CultureInfo.CurrentCulture);
			for (var i = 0; i < 7; i++)
			{
				day = day.AddDays(i);
				var period = new DateTimePeriod(day.AddHours(8).AddMinutes(15),
					day.AddHours(9).AddMinutes(45));
				addAssignment(new DateOnly(day), new activityDto { Period = period, Activity = skill.Activity });
			}


			var result = Target.FetchWeekData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 6, 45, 17, 15);
		}

		[Test]
		public void ShouldAdjustTimeLineBySkillOpenHoursOnlyWithDayWhenStaffingDataIsAvailable()
		{
			setupLoggedOnUser();
			var skill1 = addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var skill2 = addSkill(TimeSpan.Zero, TimeSpan.FromDays(1));
			var period1 = new DateTimePeriod(new DateTime(2014, 12, 31, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 31, 9, 45, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2015, 1, 1, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2015, 1, 1, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(new DateOnly(2014, 12, 31), new activityDto {Activity = skill1.Activity, Period = period1});
			addAssignment(new DateOnly(2015, 1, 1), new activityDto {Activity = skill2.Activity, Period = period2});

			var result = Target.FetchWeekData(new DateOnly(2014, 12, 31), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 0, 0, 23, 59);
		}

		private void AssertTimeLine(IList<TimeLineViewModel> timeLine, int startHour, int startMinute, int endHour, int endMinute)
		{
			timeLine.First().Time.Hours.Should().Be.EqualTo(startHour);
			timeLine.First().Time.Minutes.Should().Be.EqualTo(startMinute);
			timeLine.Last().Time.Hours.Should().Be.EqualTo(endHour);
			timeLine.Last().Time.Minutes.Should().Be.EqualTo(endMinute); 
		}

		private ISkill createSkillWithOpenHours(TimeSpan start, TimeSpan end)
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			skill.Activity.InWorkTime = true;
			skill.Activity.RequiresSkill = true;
			skill.SkillType = skillType;

			foreach (var workload in skill.WorkloadCollection)
			{
				foreach (var templateWeek in workload.TemplateWeekCollection)
				{
					templateWeek.Value.ChangeOpenHours(new List<TimePeriod>
					{
						new TimePeriod(start, end)
					});
				}
			}
			return skill;
		}

		private ISkill addSkill()
		{
			var skill = createSkillWithOpenHours(TimeSpan.FromHours(7), TimeSpan.FromHours(18));
			var personPeriod = getOrAddPersonPeriod();
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			return skill;
		}

		private ISkill addSkill(TimeSpan openHourStart, TimeSpan openHourEnd)
		{
			var skill = createSkillWithOpenHours(openHourStart, openHourEnd);
			getOrAddPersonPeriod().AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			return skill;
		}

		private PersonPeriod getOrAddPersonPeriod()
		{
			var personPeriod = (PersonPeriod)User.CurrentUser().PersonPeriods(new DateOnly(2014, 1, 1).ToDateOnlyPeriod()).FirstOrDefault();
			if (personPeriod != null) return personPeriod;
			var team = TeamFactory.CreateTeam("team1", "site1");
			personPeriod = (PersonPeriod)PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2014, 1, 1), team);
			User.CurrentUser().AddPersonPeriod(personPeriod);
			return personPeriod;
		}

		private void addAssignment(DateTimePeriod period, DateOnly? belongsToDate = null)
		{
			var activity = new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue };
			addAssignment(belongsToDate, new activityDto { Period = period, Activity = activity });
		}

		private void addAssignment(DateTimePeriod period, IActivity activity)
		{
			addAssignment(null, new activityDto { Period = period, Activity = activity });
		}

		private void addAssignment(DateOnly belongsToDate, DateTimePeriod period, IActivity activity)
		{
			addAssignment(belongsToDate, new activityDto {Period = period, Activity = activity});
		}

		private class activityDto
		{
			public DateTimePeriod Period { get; set; }
			public IActivity Activity { get; set; }
		}

		private void addAssignment(DateOnly? belongsToDate = null, params activityDto[] activityDtos)
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), belongsToDate ?? new DateOnly(Now.UtcDateTime()));
			foreach (var activityDto in activityDtos)
			{
				assignment.AddActivity(activityDto.Activity, activityDto.Period);
			}
			ScheduleData.Add(assignment);
		}

		private void addSiteOpenHour()
		{
			var personPeriod = getOrAddPersonPeriod();
			var timePeriod = new TimePeriod(8, 0, 17, 0);
			var team = personPeriod.Team;
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
		}

		private void addSiteOpenHour(params SiteOpenHour[] siteOpenHours)
		{
			var personPeriod = getOrAddPersonPeriod();
			var team = personPeriod.Team;
			foreach (var siteOpenHour in siteOpenHours)
			{
				team.Site.AddOpenHour(siteOpenHour);
			}
		}

		private void setupLoggedOnUser()
		{
			var logonUser = PersonFactory.CreatePersonWithGuid("logon", "user");
			var workflowControlSet = new WorkflowControlSet("test");
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { skillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 13)
			});
			logonUser.WorkflowControlSet = workflowControlSet;

			var site = new Site("site1").WithId();
			SiteRepository.Add(site);
			var team = new Team { Site = site }.WithDescription(new Description("team1")).WithId();
			TeamRepository.Add(team);
			logonUser.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2014, 1, 1), team));

			User.SetFakeLoggedOnUser(logonUser);
			PersonRepository.Add(logonUser);
		}
	}
}
