using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
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
	public class TeamScheduleControllerTest
	{
		public TeamScheduleApiController Target;
		public FakeTeamRepository TeamRepository;
		public FakePersonRepository PersonRepository;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public IScheduleStorage ScheduleData;
		public FakeUserTimeZone UserTimeZone;

		[Test]
		public void ShouldGiveSuccessResponseButWithRightBusinessReasonWhenThereIsNoDefaultTeam()
		{
			var person = PersonFactory.CreatePerson("test");
			person.TerminatePerson(new DateOnly(2018, 2, 5), new PersonAccountUpdaterDummy());
			PersonRepository.Has(person);

			var response = Target.DefaultTeam(new DateOnly(2018, 2, 6));

			dynamic content = response;
			Type typeOfContent = content.GetType();
			var exist = typeOfContent.GetProperties().Where(p => p.Name.Equals("DefaultTeam")).Any();
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

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First() as TeamScheduleTimeLineViewModel;
			firstTeamScheduleTimeLineViewModel.Time.Should().Be(TimeSpan.FromHours(8));
			firstTeamScheduleTimeLineViewModel.TimeLineDisplay.Should().Be(today.Date.AddMinutes(480).ToLocalizedTimeFormat());

			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last() as TeamScheduleTimeLineViewModel;
			lastTeamScheduleTimeLineViewModel.Time.Should().Be(TimeSpan.FromHours(17));
			lastTeamScheduleTimeLineViewModel.TimeLineDisplay.Should().Be(today.Date.AddMinutes(1020).ToLocalizedTimeFormat());
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnDisplayTimeBasedOnLocalTimezoneInTimeLine()
		{
			UserTimeZone.IsSweden();
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

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First() as TeamScheduleTimeLineViewModel;
			firstTeamScheduleTimeLineViewModel.Time.Should().Be(TimeSpan.FromHours(8));
			firstTeamScheduleTimeLineViewModel.TimeLineDisplay.Should().Be(today.Date.AddMinutes(480).ToLocalizedTimeFormat());

			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last() as TeamScheduleTimeLineViewModel;
			lastTeamScheduleTimeLineViewModel.Time.Should().Be(TimeSpan.FromHours(17));
			lastTeamScheduleTimeLineViewModel.TimeLineDisplay.Should().Be(today.Date.AddMinutes(1020).ToLocalizedTimeFormat());
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

			var diff = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(15)) - TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(45));

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First() as TeamScheduleTimeLineViewModel;
			firstTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(Math.Round((decimal)TimeSpan.FromMinutes(15).Ticks / diff.Ticks, 4));

			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last() as TeamScheduleTimeLineViewModel;
			lastTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(Math.Round((decimal)TimeSpan.FromMinutes(555).Ticks / diff.Ticks, 4));
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

			var diff = TimeSpan.FromHours(16).Add(TimeSpan.FromMinutes(15)) - TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(45));

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First() as TeamScheduleTimeLineViewModel;
			firstTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(Math.Round((decimal)TimeSpan.FromMinutes(15).Ticks / diff.Ticks, 4));

			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last() as TeamScheduleTimeLineViewModel;
			lastTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(Math.Round((decimal)TimeSpan.FromMinutes(7 * 60 + 15).Ticks / diff.Ticks, 4));
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

			var diff = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(12)) - TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(02));

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First() as TeamScheduleTimeLineViewModel;
			firstTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(Math.Round((decimal)TimeSpan.FromMinutes(58).Ticks / diff.Ticks, 4));

			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last() as TeamScheduleTimeLineViewModel;
			lastTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(Math.Round((decimal)TimeSpan.FromMinutes(7 * 60 + 58).Ticks / diff.Ticks, 4));
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
			((TeamScheduleTimeLineViewModel)teamScheduleViewModel.TimeLine.Last()).Time.Should().Be(TimeSpan.FromDays(1).Add(TimeSpan.FromHours(2)));

			var diff = TimeSpan.FromHours(26).Add(TimeSpan.FromMinutes(15)) - TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(45));

			var firstTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.First() as TeamScheduleTimeLineViewModel;
			firstTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(Math.Round((decimal)TimeSpan.FromMinutes(15).Ticks / diff.Ticks, 4));

			var lastTeamScheduleTimeLineViewModel = teamScheduleViewModel.TimeLine.Last() as TeamScheduleTimeLineViewModel;
			lastTeamScheduleTimeLineViewModel.PositionPercentage.Should().Be(Math.Round((decimal)TimeSpan.FromMinutes(17 * 60 + 15).Ticks / diff.Ticks, 4));
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

			firstPeriod.Color.Should().Be(Color.Green.Name);
			firstPeriod.Title.Should().Be("Phone");
			firstPeriod.StartTime.Should().Be(period.StartDateTime);
			firstPeriod.EndTime.Should().Be(period.EndDateTime);
			firstPeriod.TimeSpan.Should().Be("9:00 AM - 4:00 PM");
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

			var diff = TimeSpan.FromHours(16).Add(TimeSpan.FromMinutes(15)) - TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(45));

			var startPosition = TimeSpan.FromMinutes(15).Ticks / (decimal)diff.Ticks;
			var endPosition = TimeSpan.FromMinutes(7 * 60 + 15).Ticks / (decimal)diff.Ticks;
			assertPeriodPosition(firstPeriod, startPosition, endPosition);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnPositionPercentageInPeriod()
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

			var diff = TimeSpan.FromHours(16).Add(TimeSpan.FromMinutes(15)) - TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(45));

			var startPosition = TimeSpan.FromMinutes(15).Ticks / (decimal)diff.Ticks;
			var endPosition = TimeSpan.FromMinutes(7 * 60 + 15).Ticks / (decimal)diff.Ticks;
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

			var diff = TimeSpan.FromHours(26).Add(TimeSpan.FromMinutes(15)) - TimeSpan.FromHours(21).Add(TimeSpan.FromMinutes(45));

			var startPosition = TimeSpan.FromMinutes(15).Ticks / (decimal)diff.Ticks;
			var endPosition = TimeSpan.FromMinutes(4*60+15).Ticks / (decimal)diff.Ticks;
			assertPeriodPosition(firstPeriod, startPosition, endPosition);
		}

		private void assertPeriodPosition(TeamScheduleAgentScheduleLayerViewModel period, decimal expectedStartPosition, decimal expectedEndPosition)
		{
			period.StartPositionPercentage.Should().Be(Math.Round(expectedStartPosition, 4));
			period.EndPositionPercentage.Should().Be(Math.Round(expectedEndPosition, 4));
		}
	}
}