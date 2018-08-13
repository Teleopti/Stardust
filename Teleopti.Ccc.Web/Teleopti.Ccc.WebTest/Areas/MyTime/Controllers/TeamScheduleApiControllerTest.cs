using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Ccc.Web.Core.Extensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[MyTimeWebTest]
	[SetCulture("en-US")]
	public class TeamScheduleControllerTest : IIsolateSystem
	{
		public TeamScheduleApiController Target;
		public FakeTeamRepository TeamRepository;
		public FakePersonRepository PersonRepository;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public IScheduleStorage ScheduleData;
		public FakeUserTimeZone UserTimeZone;
		public FakeMeetingRepository MeetingRepository;
		public FullPermission Permission;
		public MutableNow Now;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<PermissionProvider>().For<IPermissionProvider>();
		}

		[Test]
		public void ShouldGiveSuccessResponseButWithRightBusinessReasonWhenThereIsNoDefaultTeam()
		{
			var person = PersonFactory.CreatePerson("test");
			person.TerminatePerson(new DateOnly(2018, 2, 5), new PersonAccountUpdaterDummy());
			PersonRepository.Has(person);

			var response = Target.DefaultTeam(new DateOnly(2018, 2, 6));

			dynamic content = response;
			Type typeOfContent = content.GetType();
			var exist = typeOfContent.GetProperties().Any(p => p.Name.Equals("DefaultTeam"));
			Assert.That(exist, Is.EqualTo(false));
			Assert.That((object)content.Message, Is.EqualTo(Resources.NoTeamsAvailable));
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnViewModel()
		{
			var today = DateOnly.Today;
			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = Guid.NewGuid().ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);

			(teamScheduleViewModel != null).Should().Be(true);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnTimeLine()
		{
			var today = DateOnly.Today;
			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = Guid.NewGuid().ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);

			(teamScheduleViewModel.TimeLine == null).Should().Be(false);
			(teamScheduleViewModel.TimeLine.Length > 0).Should().Be(true);
			(teamScheduleViewModel.TimeLine[0] is TeamScheduleTimeLineViewModel).Should().Be(true);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnTimeInTimeLine()
		{
			var today = DateOnly.Today;
			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = Guid.NewGuid().ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First();
			firstTeamScheduleTimeLineViewModel.Time.Should().Be(today.Date.AddHours(8));
			firstTeamScheduleTimeLineViewModel.TimeLineDisplay.Should().Be(today.Date.AddMinutes(480).ToLocalizedDateTimeFormatWithTSpliting());

			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last();
			lastTeamScheduleTimeLineViewModel.Time.Should().Be(today.Date.AddHours(17));
			lastTeamScheduleTimeLineViewModel.TimeLineDisplay.Should().Be(today.Date.AddMinutes(1020).ToLocalizedDateTimeFormatWithTSpliting());
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnDisplayTimeBasedOnLocalTimezoneInTimeLine()
		{
			UserTimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			var today = new DateOnly(2018, 5, 23);
			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = Guid.NewGuid().ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);

			(teamScheduleViewModel == null).Should().Be(false);
			(teamScheduleViewModel.TimeLine == null).Should().Be(false);
			(teamScheduleViewModel.TimeLine.Length > 0).Should().Be(true);

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First();
			firstTeamScheduleTimeLineViewModel.Time.Should().Be(today.Date.AddHours(8));
			firstTeamScheduleTimeLineViewModel.TimeLineDisplay.Should().Be(today.Date.AddMinutes(480).ToLocalizedDateTimeFormatWithTSpliting());

			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last();
			lastTeamScheduleTimeLineViewModel.Time.Should().Be(today.Date.AddHours(17));
			lastTeamScheduleTimeLineViewModel.TimeLineDisplay.Should().Be(today.Date.AddMinutes(1020).ToLocalizedDateTimeFormatWithTSpliting());
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnExtendedTimeLine()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = PersonFactory.CreatePersonWithGuid("test", "agent");
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 10);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);

			(teamScheduleViewModel != null).Should().Be(true);

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First();
			firstTeamScheduleTimeLineViewModel.Time.Should().Be(today.Date.AddHours(8));
			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last();
			lastTeamScheduleTimeLineViewModel.Time.Should().Be(today.Date.AddHours(17));
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnPositionPercentageInTimeLine()
		{
			var today = DateOnly.Today;
			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = Guid.NewGuid().ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);

			(teamScheduleViewModel == null).Should().Be(false);
			(teamScheduleViewModel.TimeLine == null).Should().Be(false);
			(teamScheduleViewModel.TimeLine.Length > 0).Should().Be(true);

			var diff = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(15)) - TimeSpan.FromHours(8);

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First();
			firstTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(0);

			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last();
			lastTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(Math.Round((decimal)TimeSpan.FromHours(9).Ticks / diff.Ticks, 4));
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnPositionPercentageInTimeLineWithRoundedHourSchedules()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = PersonFactory.CreatePersonWithGuid("test", "agent");
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 16);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);

			(teamScheduleViewModel == null).Should().Be(false);
			(teamScheduleViewModel.TimeLine == null).Should().Be(false);
			(teamScheduleViewModel.TimeLine.Length > 0).Should().Be(true);

			var diff = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(15)) - TimeSpan.FromHours(8);

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First();
			firstTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(0);

			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last();
			lastTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(Math.Round((decimal)TimeSpan.FromMinutes(9 * 60).Ticks / diff.Ticks, 4));
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnPositionPercentageInTimeLineWithNotRoundedHourSchedules()
		{
			var today = new DateOnly(2014, 12, 15);

			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = PersonFactory.CreatePersonWithGuid("test", "agent");
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(new DateTime(2014, 12, 15, 9, 17, 0, 0, DateTimeKind.Utc), new DateTime(2014, 12, 15, 16, 57, 0, 0, DateTimeKind.Utc));
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);

			(teamScheduleViewModel == null).Should().Be(false);
			(teamScheduleViewModel.TimeLine == null).Should().Be(false);
			(teamScheduleViewModel.TimeLine.Length > 0).Should().Be(true);

			var diff = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(15)) - TimeSpan.FromHours(8);

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First();
			firstTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(0);

			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last();
			lastTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(Math.Round((decimal)TimeSpan.FromMinutes(9 * 60).Ticks / diff.Ticks, 4));
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnPositionPercentageInTimeLineWithRoundedHourAndOverNightSchedule()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = PersonFactory.CreatePersonWithGuid("test", "agent");
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 16, 2);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);

			(teamScheduleViewModel == null).Should().Be(false);
			(teamScheduleViewModel.TimeLine == null).Should().Be(false);
			(teamScheduleViewModel.TimeLine.Length > 0).Should().Be(true);
			teamScheduleViewModel.TimeLine.Last().Time.Should().Be(today.Date.AddDays(1).AddHours(2));

			var diff = TimeSpan.FromHours(26).Add(TimeSpan.FromMinutes(15)) - TimeSpan.FromHours(8);

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First();
			firstTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(0);

			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last();
			lastTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(Math.Round((decimal)TimeSpan.FromMinutes(18 * 60).Ticks / diff.Ticks, 4));
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnTimeLineHoursCorrectlyWithOverNightSchedules()
		{
			UserTimeZone.IsHawaii();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.HawaiiTimeZoneInfo());

			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = PersonFactory.CreatePersonWithGuid("test", "agent");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 4, 2014, 12, 15, 12);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);

			teamScheduleViewModel.TimeLine.Length.Should().Be(24);
			teamScheduleViewModel.TimeLine.First().Time.Should().Be(TimeZoneHelper.ConvertFromUtc(new DateTime(2014, 12, 15, 4, 0, 0, DateTimeKind.Utc), TimeZoneInfoFactory.HawaiiTimeZoneInfo()));
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnAgentSchedules()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = PersonFactory.CreatePersonWithGuid("test", "agent");
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 16);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);

			(teamScheduleViewModel != null).Should().Be(true);
			(teamScheduleViewModel.AgentSchedules != null).Should().Be(true);
			teamScheduleViewModel.AgentSchedules.Length.Should().Be(1);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnPeriodsInAgentSchedules()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = PersonFactory.CreatePersonWithGuid("test", "agent");
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 16);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			var periods = teamScheduleViewModel.AgentSchedules[0].Periods;

			(periods != null).Should().Be(true);
			periods.Count().Should().Be(1);
		}

		[Test]
		[SetCulture("en-US")]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnPeriodInPeriods()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = PersonFactory.CreatePersonWithGuid("test", "agent");
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 16);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			var firstPeriod = teamScheduleViewModel.AgentSchedules[0].Periods.ElementAt(0);

			firstPeriod.Color.Should().Be("0,128,0");
			firstPeriod.Title.Should().Be("Phone");
			firstPeriod.StartTime.Should().Be(period.StartDateTime);
			firstPeriod.EndTime.Should().Be(period.EndDateTime);
			firstPeriod.TimeSpan.Should().Be(TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.FromHours(9), CultureInfo.CurrentCulture) + " - " + TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.FromHours(16), CultureInfo.CurrentCulture));
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnTimeLineHoursCorrectlyWithDayOffFilterWhereMyScheduleIsNotDayOff()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = User.CurrentUser();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 10);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var person2 = PersonFactory.CreatePersonWithGuid("test", "agent");
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			PersonRepository.Add(person2);
			person2.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var dayOffAssignment2 = PersonAssignmentFactory.CreateAssignmentWithDayOff(person2, Scenario.Current(), today, new DayOffTemplate(new Description("dayoff")));
			ScheduleData.Add(dayOffAssignment2);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString(),
					IsDayOff = true,
					FilteredStartTimes = "00:00-23:59",
					FilteredEndTimes = "00:00-23:59"
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First();
			firstTeamScheduleTimeLineViewModel.Time.Should().Be(today.Date.AddHours(8));
			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last();
			lastTeamScheduleTimeLineViewModel.Time.Should().Be(today.Date.AddHours(17));
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnTimeLineHoursCorrectlyWithDayOffFilterWhereMyScheduleIsLonger()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = User.CurrentUser();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 6, 2014, 12, 15, 19);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var person2 = PersonFactory.CreatePersonWithGuid("test", "agent");
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			PersonRepository.Add(person2);
			person2.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var dayOffAssignment2 = PersonAssignmentFactory.CreateAssignmentWithDayOff(person2, Scenario.Current(), today, new DayOffTemplate(new Description("dayoff")));
			ScheduleData.Add(dayOffAssignment2);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString(),
					IsDayOff = true,
					FilteredStartTimes = "00:00-23:59",
					FilteredEndTimes = "00:00-23:59"
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First();
			firstTeamScheduleTimeLineViewModel.Time.Should().Be(today.Date.AddHours(6));
			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last();
			lastTeamScheduleTimeLineViewModel.Time.Should().Be(today.Date.AddHours(19));
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnTimeLineHoursCorrectlyWithDayOffFilter()
		{
			UserTimeZone.IsSweden();

			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = User.CurrentUser();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var dayOffAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, Scenario.Current(), today, new DayOffTemplate(new Description("dayoff")));
			ScheduleData.Add(dayOffAssignment);

			var person2 = PersonFactory.CreatePersonWithGuid("test", "agent");
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			PersonRepository.Add(person2);
			person2.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var dayOffAssignment2 = PersonAssignmentFactory.CreateAssignmentWithDayOff(person2, Scenario.Current(), today, new DayOffTemplate(new Description("dayoff")));
			ScheduleData.Add(dayOffAssignment2);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString(),
					IsDayOff = true,
					FilteredStartTimes = "00:00-23:59",
					FilteredEndTimes = "00:00-23:59"
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First();
			firstTeamScheduleTimeLineViewModel.Time.Should().Be(today.Date.AddHours(8));
			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last();
			lastTeamScheduleTimeLineViewModel.Time.Should().Be(today.Date.AddHours(17));
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldShiftCategoryWithDayOffFilter()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = User.CurrentUser();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 10);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var person2 = PersonFactory.CreatePersonWithGuid("test", "agent");
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			PersonRepository.Add(person2);
			person2.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var dayOffAssignment2 = PersonAssignmentFactory.CreateAssignmentWithDayOff(person2, Scenario.Current(), today, new DayOffTemplate(new Description("dayoff")));
			ScheduleData.Add(dayOffAssignment2);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString(),
					IsDayOff = true,
					FilteredStartTimes = "00:00-23:59",
					FilteredEndTimes = "00:00-23:59"
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			teamScheduleViewModel.AgentSchedules.Length.Should().Be(1);
			teamScheduleViewModel.AgentSchedules[0].ShiftCategory.Should().Not.Be(null);
			teamScheduleViewModel.AgentSchedules[0].ShiftCategory.Name.Should().Be("dayoff");
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnMySchedule()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = User.CurrentUser();
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 16);
			assignment.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"),
				period, new MultiplicatorDefinitionSet("a", MultiplicatorType.Overtime));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			var mySchedule = teamScheduleViewModel.MySchedule;
			(mySchedule != null).Should().Be(true);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldNotReturnMyScheduleWhenTodayIsAfterPublishScheduledPeriod()
		{
			Permission.AddToBlackList(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			Now.Is(new DateTime(2015,12,25,0,0,0,DateTimeKind.Utc));
			var today = new DateOnly(2015, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = User.CurrentUser();
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			person.WorkflowControlSet = new WorkflowControlSet("test")
			{
				SchedulePublishedToDate = new DateTime(2015, 12, 14),
				PreferencePeriod = new DateOnlyPeriod(2015, 12, 12, 2015, 12, 13)
			};

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			var mySchedule = teamScheduleViewModel.MySchedule;
			
			mySchedule.Periods.Count().Should().Be(0);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnFalseForDayOff()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = User.CurrentUser();
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 16);
			assignment.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"),
				period, new MultiplicatorDefinitionSet("a", MultiplicatorType.Overtime));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			var mySchedule = teamScheduleViewModel.MySchedule;
			mySchedule.IsDayOff.Should().Be(false);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnTrueForDayOff()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = User.CurrentUser();
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, Scenario.Current(), today, new DayOffTemplate(new Description("dayoff")));
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 16);
			assignment.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"),
				period, new MultiplicatorDefinitionSet("a", MultiplicatorType.Overtime));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			var mySchedule = teamScheduleViewModel.MySchedule;
			mySchedule.IsDayOff.Should().Be(true);
			mySchedule.DayOffName.Should().Be("dayoff");
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnFalseForIsNotScheduled()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = User.CurrentUser();
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 16);
			assignment.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"),
				period, new MultiplicatorDefinitionSet("a", MultiplicatorType.Overtime));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			var mySchedule = teamScheduleViewModel.MySchedule;
			mySchedule.IsNotScheduled.Should().Be(false);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnTrueForIsNotScheduled()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = User.CurrentUser();
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			var mySchedule = teamScheduleViewModel.MySchedule;
			mySchedule.IsNotScheduled.Should().Be(true);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnTrueForIsOvertimeInPeriod()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = User.CurrentUser();
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 16);
			assignment.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"),
				period, new MultiplicatorDefinitionSet("a", MultiplicatorType.Overtime));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			var firstPeriod = teamScheduleViewModel.MySchedule.Periods.ElementAt(0);

			firstPeriod.IsOvertime.Should().Be(true);
		}
		
		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnPositionPercentageInMySchedule()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = User.CurrentUser();
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 16);
			assignment.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"),
				period, new MultiplicatorDefinitionSet("a", MultiplicatorType.Overtime));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			var firstPeriod = teamScheduleViewModel.MySchedule.Periods.ElementAt(0);

			var diff = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(15)) - TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(45));

			var startPosition = (TimeSpan.FromHours(1).Ticks + TimeSpan.FromMinutes(15).Ticks) / (decimal)diff.Ticks;
			var endPosition = TimeSpan.FromMinutes(8 * 60 + 15).Ticks / (decimal)diff.Ticks;
			assertPeriodPosition(firstPeriod, startPosition, endPosition);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnPositionPercentageWhenStartTimeIsHalfPast5()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = User.CurrentUser();
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 5, 2014, 12, 16, 1);
			period = period.ChangeStartTime(TimeSpan.FromMinutes(30));
			period = period.ChangeEndTime(TimeSpan.FromMinutes(30));
			assignment.AddActivity(ActivityFactory.CreateActivity("test"), period);
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			var firstPeriod = teamScheduleViewModel.MySchedule.Periods.ElementAt(0);

			var diff = TimeSpan.FromHours(25).Add(TimeSpan.FromMinutes(45)) - TimeSpan.FromHours(5).Add(TimeSpan.FromMinutes(15));

			var startPosition = TimeSpan.FromMinutes(15).Ticks / (decimal)diff.Ticks;
			var endPosition = TimeSpan.FromMinutes(20 * 60 + 15).Ticks / (decimal)diff.Ticks;
			assertPeriodPosition(firstPeriod, startPosition, endPosition);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnPositionPercentageInAgentScheulePeriod()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = PersonFactory.CreatePersonWithGuid("test", "agent");
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 16);
			assignment.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"),
				period, new MultiplicatorDefinitionSet("a", MultiplicatorType.Overtime));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			var firstPeriod = teamScheduleViewModel.AgentSchedules[0].Periods.ElementAt(0);

			var diff = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(15)) - TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(45));

			var startPosition = (TimeSpan.FromHours(1).Ticks + TimeSpan.FromMinutes(15).Ticks) / (decimal)diff.Ticks;
			var endPosition = TimeSpan.FromMinutes(8 * 60 + 15).Ticks / (decimal)diff.Ticks;
			assertPeriodPosition(firstPeriod, startPosition, endPosition);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnPositionPercentageInPeriodWithOverNightShift()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = PersonFactory.CreatePersonWithGuid("test", "agent");
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 22, 2014, 12, 16, 2);
			assignment.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"),
				period, new MultiplicatorDefinitionSet("a", MultiplicatorType.Overtime));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			var firstPeriod = teamScheduleViewModel.AgentSchedules[0].Periods.ElementAt(0);

			var diff = TimeSpan.FromHours(26).Add(TimeSpan.FromMinutes(15)) - TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(45));

			var startPosition = (TimeSpan.FromHours(14).Ticks + TimeSpan.FromMinutes(15).Ticks) / (decimal) diff.Ticks;
			var endPosition = TimeSpan.FromMinutes(18 * 60 + 15).Ticks / (decimal) diff.Ticks;
			assertPeriodPosition(firstPeriod, startPosition, endPosition);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnAgentNameInAgentSchedules()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = PersonFactory.CreatePersonWithGuid("test", "agent");
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 16);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			var agentName = teamScheduleViewModel.AgentSchedules[0].Name;

			agentName.Should().Be("test agent");;
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnMeetingInPeriod()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = PersonFactory.CreatePersonWithGuid("test", "agent");
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 16);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var meeting = new Meeting(person, new[] { new MeetingPerson(person, false) }, "subj", "loc",
				"desc", phoneActivity, null);
			meeting.SetScenario(Scenario.Current());
			meeting.StartDate = meeting.EndDate = today;
			meeting.StartTime = TimeSpan.FromHours(15);
			meeting.EndTime = TimeSpan.FromHours(16);
			MeetingRepository.Has(meeting);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			var meetingViewModel = teamScheduleViewModel.AgentSchedules[0].Periods.ElementAt(1).Meeting;
			(meetingViewModel != null).Should().Be(true);
			meetingViewModel.Title.Should().Be("subj");
			meetingViewModel.Location.Should().Be("loc");
			meetingViewModel.Description.Should().Be("desc");
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnTotalAgentCount()
		{
			var today = new DateOnly(2014, 12, 15);
			var team = TeamFactory.CreateSimpleTeam("test team").WithId();
			TeamRepository.Add(team);

			var person = PersonFactory.CreatePersonWithGuid("test", "agent");
			PersonRepository.Add(person);
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today, team));

			var assignment = new PersonAssignment(person, Scenario.Current(), today);
			var period = new DateTimePeriod(2014, 12, 15, 9, 2014, 12, 15, 16);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var teamScheduleRequest = new TeamScheduleRequest
			{
				SelectedDate = today.Date,
				Paging = new Paging
				{
					Take = 10
				},
				ScheduleFilter = new Domain.Repositories.ScheduleFilter
				{
					TeamIds = team.Id.ToString()
				}
			};
			var teamScheduleViewModel = Target.TeamSchedule(teamScheduleRequest);
			teamScheduleViewModel.TotalAgentCount.Should().Be(1);
		}

		private void assertPeriodPosition(TeamScheduleAgentScheduleLayerViewModel period, decimal expectedStartPosition, decimal expectedEndPosition)
		{
			period.StartPositionPercentage.Should().Be(Math.Round(expectedStartPosition, 4));
			period.EndPositionPercentage.Should().Be(Math.Round(expectedEndPosition, 4));
		}
	}
}