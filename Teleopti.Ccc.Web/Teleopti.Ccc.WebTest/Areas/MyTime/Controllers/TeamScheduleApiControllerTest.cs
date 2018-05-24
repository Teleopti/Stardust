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
	[TestFixture, MyTimeWebTest]
	public class TeamScheduleControllerTest
	{
		public TeamScheduleApiController Target;
		public FakeTeamRepository TeamRepository;
		public FakePersonRepository PersonRepository;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public IScheduleStorage ScheduleData;

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
		public void ShouldReturnTeamScheduleTimeLineViewModel()
		{
			var today = DateOnly.Today;
			var teamScheduleRequest = new TeamScheduleApiController.TeamScheduleRequest
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
			(teamScheduleViewModel.TimeLine[0] is TeamScheduleTimeLineViewModel).Should().Be(true);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleView_75989)]
		public void ShouldReturnTimeOfTeamScheduleTimeLineViewModel()
		{
			var today = DateOnly.Today;
			var teamScheduleRequest = new TeamScheduleApiController.TeamScheduleRequest
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
		public void ShouldReturnPositionPercentageOfTimeInTeamScheduleTimeLineViewModel()
		{
			var today = DateOnly.Today;
			var teamScheduleRequest = new TeamScheduleApiController.TeamScheduleRequest
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
		public void ShouldReturnPositionPercentageOfTimeInTeamScheduleTimeLineViewModelWithRoundedHourSchedules()
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

			var teamScheduleRequest = new TeamScheduleApiController.TeamScheduleRequest
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
		public void ShouldReturnPositionPercentageOfTimeInTeamScheduleTimeLineViewModelWithNotRoundedHourSchedules()
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

			var teamScheduleRequest = new TeamScheduleApiController.TeamScheduleRequest
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
	}
}