using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture, MyTimeWebTest]
	public class ShiftTradeScheduleSiteOpenHourFilterTest
	{
		public FakeLoggedOnUser LoggedOnUser;
		public FakeToggleManager ToggleManager;
		public IShiftTradeScheduleSiteOpenHourFilter Target;

		private DateOnly _shiftTradeDate = new DateOnly(2016, 8, 8);

		[Test]
		public void ShouldFilterByCurrentUserSiteOpenHour()
		{
			prepareData(createPersonWithSiteOpenHours(8, 15));

			var person1 = createPersonWithSiteOpenHours(8, 19);
			var person2 = createPersonWithSiteOpenHours(8, 19);

			var shiftTradeAddPersonScheduleViews = new[]
			{
				createShiftTradeAddPersonScheduleViewModel(person1, _shiftTradeDate, new[]
				{
					new TimePeriod(8, 30, 10, 30),
					new TimePeriod(11, 30, 14, 30),
				}),
				createShiftTradeAddPersonScheduleViewModel(person2, _shiftTradeDate, new[]
				{
					new TimePeriod(8, 30, 10, 30),
					new TimePeriod(11, 30, 14, 30),
					new TimePeriod(16, 00, 18, 30),
				})
			};

			var datePersons = new DatePersons {Date = _shiftTradeDate, Persons = new[] {person1, person2}};
			var filteredShiftTradeAddPersonScheduleViews = Target.Filter(shiftTradeAddPersonScheduleViews, datePersons).ToList();

			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(1);
			filteredShiftTradeAddPersonScheduleViews.FirstOrDefault().PersonId.Should().Be(person1.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldFilterByOtherAgentSiteOpenHour()
		{
			prepareData(createPersonWithSiteOpenHours(8, 15));

			var person1 = createPersonWithSiteOpenHours(8, 15);
			var person2 = createPersonWithSiteOpenHours(8, 11);

			var shiftTradeAddPersonScheduleViews = new[]
			{
				createShiftTradeAddPersonScheduleViewModel(person1, _shiftTradeDate, new[]
				{
					new TimePeriod(8, 30, 10, 30),
					new TimePeriod(11, 30, 14, 30),
				}),
				createShiftTradeAddPersonScheduleViewModel(person2, _shiftTradeDate, new[]
				{
					new TimePeriod(8, 30, 10, 30),
					new TimePeriod(11, 30, 14, 30),
				})
			};

			var datePersons = new DatePersons { Date = _shiftTradeDate, Persons = new[] { person1, person2 } };
			var filteredShiftTradeAddPersonScheduleViews = Target.Filter(shiftTradeAddPersonScheduleViews, datePersons).ToList();

			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(1);
			filteredShiftTradeAddPersonScheduleViews.FirstOrDefault().PersonId.Should().Be(person1.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldReturnOriginalScheduleViewsWithoutSiteOpenHourSetting()
		{
			var date = new DateOnly(2016, 1, 1);

			prepareData(PersonFactory.CreatePersonWithPersonPeriodTeamSite(date));

			var person1 = PersonFactory.CreatePersonWithPersonPeriodTeamSite(date);
			var person2 = PersonFactory.CreatePersonWithPersonPeriodTeamSite(date);

			var shiftTradeAddPersonScheduleViews = new[]
			{
				createShiftTradeAddPersonScheduleViewModel(person1, _shiftTradeDate, new[]
				{
					new TimePeriod(8, 30, 10, 30),
					new TimePeriod(11, 30, 14, 30),
				}),
				createShiftTradeAddPersonScheduleViewModel(person2, _shiftTradeDate, new[]
				{
					new TimePeriod(8, 30, 10, 30),
					new TimePeriod(11, 30, 14, 30),
				})
			};

			var datePersons = new DatePersons { Date = _shiftTradeDate, Persons = new[] { person1, person2 } };
			var filteredShiftTradeAddPersonScheduleViews = Target.Filter(shiftTradeAddPersonScheduleViews, datePersons).ToList();
			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(2);
		}

		private void prepareData(IPerson person)
		{
			ToggleManager.Enable(Toggles.Wfm_Requests_Site_Open_Hours_39936);
			LoggedOnUser.SetFakeLoggedOnUser(person);
		}

		private static ShiftTradeAddPersonScheduleViewModel createShiftTradeAddPersonScheduleViewModel(IPerson person,
			DateOnly date, TimePeriod[] timePeriods)
		{
			var teamScheduleLayerViewModels = new List<TeamScheduleLayerViewModel>();

			foreach (var timePeriod in timePeriods)
			{
				teamScheduleLayerViewModels.Add(createTeamScheduleLayerViewModel(date.Date.Add(timePeriod.StartTime),
					date.Date.Add(timePeriod.EndTime)));
			}

			return new ShiftTradeAddPersonScheduleViewModel
			{
				PersonId = person.Id.GetValueOrDefault(),
				ScheduleLayers = teamScheduleLayerViewModels.ToArray(),
				Name = "test",
				Total = teamScheduleLayerViewModels.Count,
			};
		}

		private static TeamScheduleLayerViewModel createTeamScheduleLayerViewModel(DateTime start, DateTime end)
		{
			return new TeamScheduleLayerViewModel
			{
				LengthInMinutes = (int) (end - start).TotalMinutes,
				Start = start,
				End = end
			};
		}

		private static IPerson createPersonWithSiteOpenHours(int startHour, int endHour)
		{
			var team = TeamFactory.CreateTeam("team", "site");
			team.Site.OpenHours.Add(System.DayOfWeek.Monday, new TimePeriod(startHour, 0, endHour, 0));
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(2016, 1, 1), team);
			return person;
		}
	}
}
