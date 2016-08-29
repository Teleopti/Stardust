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
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture, MyTimeWebTest]
	public class ShiftTradeSiteOpenHourFilterTest
	{
		public FakeLoggedOnUser LoggedOnUser;
		public FakeToggleManager ToggleManager;
		public IShiftTradeSiteOpenHourFilter Target;
		public FakeCurrentScenario CurrentScenario;

		private readonly DateOnly _periodStartDate = new DateOnly(2016, 1, 1);
		private readonly DateOnly _shiftTradeDate = new DateOnly(2016, 8, 8);

		[Test]
		[Ignore]
		public void ShouldFilterScheduleViewByCurrentUserSiteOpenHour()
		{
			var personFrom = createPersonWithSiteOpenHours(8, 15);
			prepareData(personFrom);
			var personFromScheduleView = createShiftTradeAddPersonScheduleViewModel(personFrom, _shiftTradeDate, new[]
			{
				new TimePeriod(8, 30, 10, 30),
				new TimePeriod(11, 30, 14, 30),
			});

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
			var filteredShiftTradeAddPersonScheduleViews =
				Target.FilterScheduleView(shiftTradeAddPersonScheduleViews, personFromScheduleView, datePersons).ToList();

			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(1);
			filteredShiftTradeAddPersonScheduleViews.FirstOrDefault().PersonId.Should().Be(person1.Id.GetValueOrDefault());
		}

		[Test]
		[Ignore]
		public void ShouldFilterScheduleViewByOtherAgentSiteOpenHour()
		{
			var personFrom = createPersonWithSiteOpenHours(8, 15);
			prepareData(personFrom);
			var personFromScheduleView = createShiftTradeAddPersonScheduleViewModel(personFrom, _shiftTradeDate, new[]
			{
				new TimePeriod(8, 30, 10, 30),
				new TimePeriod(11, 30, 14, 30),
			});

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

			var datePersons = new DatePersons {Date = _shiftTradeDate, Persons = new[] {person1, person2}};
			var filteredShiftTradeAddPersonScheduleViews =
				Target.FilterScheduleView(shiftTradeAddPersonScheduleViews, personFromScheduleView, datePersons).ToList();

			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(1);
			filteredShiftTradeAddPersonScheduleViews.FirstOrDefault().PersonId.Should().Be(person1.Id.GetValueOrDefault());
		}

		[Test]
		[Ignore]
		public void ShouldFilterScheduleViewWhenSiteOpenHoursIsClosed()
		{
			var personFrom = createPersonWithSiteOpenHours(8, 15, true);
			prepareData(personFrom);
			var personFromScheduleView = createShiftTradeAddPersonScheduleViewModel(personFrom, _shiftTradeDate, new[]
			{
				new TimePeriod(8, 30, 10, 30),
				new TimePeriod(11, 30, 14, 30),
			});

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
			var filteredShiftTradeAddPersonScheduleViews =
				Target.FilterScheduleView(shiftTradeAddPersonScheduleViews, personFromScheduleView, datePersons).ToList();

			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(0);
		}

		[Test]
		[Ignore]
		public void ShouldReturnScheduleViewsWithoutSiteOpenHourSetting()
		{
			var personFrom = PersonFactory.CreatePersonWithPersonPeriodTeamSite(_periodStartDate);
			prepareData(personFrom);
			var personFromScheduleView = createShiftTradeAddPersonScheduleViewModel(personFrom, _shiftTradeDate, new[]
			{
				new TimePeriod(8, 30, 10, 30),
				new TimePeriod(11, 30, 14, 30),
			});

			var person1 = PersonFactory.CreatePersonWithPersonPeriodTeamSite(_periodStartDate);
			var person2 = PersonFactory.CreatePersonWithPersonPeriodTeamSite(_periodStartDate);

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

			var datePersons = new DatePersons {Date = _shiftTradeDate, Persons = new[] {person1, person2}};
			var filteredShiftTradeAddPersonScheduleViews =
				Target.FilterScheduleView(shiftTradeAddPersonScheduleViews, personFromScheduleView, datePersons).ToList();
			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(2);
		}

		[Test]
		[Ignore]
		public void ShouldFilterScheduleViewWithNightShiftSchedule()
		{
			var personFrom = createPersonWithSiteOpenHours(new Dictionary<DayOfWeek, TimePeriod>
			{
				{ DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(9), TimeSpan.FromHours(24).Subtract(new TimeSpan(1))) },
				{ DayOfWeek.Tuesday, new TimePeriod(TimeSpan.Zero, TimeSpan.FromHours(9)) },
			});
			prepareData(personFrom);
			var personFromScheduleView = createShiftTradeAddPersonScheduleViewModel(personFrom, _shiftTradeDate, new[]
			{
				new TimePeriod(8, 30, 14, 30)
			});

			var person1 = createPersonWithSiteOpenHours(8, 15);

			var shiftTradeAddPersonScheduleViews = new[]
			{
				createShiftTradeAddPersonScheduleViewModel(person1, _shiftTradeDate, new[]
				{
					new TimePeriod(TimeSpan.FromHours(22), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(5)))
				})
			};

			var datePersons = new DatePersons { Date = _shiftTradeDate, Persons = new[] { person1 } };
			var filteredShiftTradeAddPersonScheduleViews =
				Target.FilterScheduleView(shiftTradeAddPersonScheduleViews, personFromScheduleView, datePersons).ToList();

			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(1);
			filteredShiftTradeAddPersonScheduleViews.FirstOrDefault().PersonId.Should().Be(person1.Id.GetValueOrDefault());
		}

		[Test]
		[Ignore]
		public void ShouldFilterShiftExchangeOfferByCurrentUserSiteOpenHour()
		{
			prepareData(createPersonWithSiteOpenHours(8, 15));

			var person1 = createPersonWithSiteOpenHours(8, 19);
			var person2 = createPersonWithSiteOpenHours(8, 19);

			var shiftExchangeOffers = new[]
			{
				createShiftExchangeOffer(person1, _shiftTradeDate, 8, 11),
				createShiftExchangeOffer(person2, _shiftTradeDate, 8, 16),
			};

			var filteredShiftExchangeOffers =
				Target.FilterShiftExchangeOffer(shiftExchangeOffers,
					createShiftTradeAddPersonScheduleViewModel(person1, _shiftTradeDate, new[]
					{
						new TimePeriod(8, 0, 17, 0),
					})).ToList();
			filteredShiftExchangeOffers.Count.Should().Be(1);
			filteredShiftExchangeOffers.FirstOrDefault().Person.Should().Be(person1);
		}

		[Test]
		[Ignore]
		public void ShouldFilterShiftExchangeOfferByOtherAgentSiteOpenHour()
		{
			prepareData(createPersonWithSiteOpenHours(8, 15));

			var person1 = createPersonWithSiteOpenHours(8, 19);
			var person2 = createPersonWithSiteOpenHours(8, 11);

			var shiftExchangeOffers = new[]
			{
				createShiftExchangeOffer(person1, _shiftTradeDate, 8, 15),
				createShiftExchangeOffer(person2, _shiftTradeDate, 8, 15),
			};

			var filteredShiftExchangeOffers =
				Target.FilterShiftExchangeOffer(shiftExchangeOffers,
					createShiftTradeAddPersonScheduleViewModel(person1, _shiftTradeDate, new[]
					{
						new TimePeriod(8, 0, 17, 0),
					})).ToList();
			filteredShiftExchangeOffers.Count.Should().Be(1);
			filteredShiftExchangeOffers.FirstOrDefault().Person.Should().Be(person1);
		}

		[Test]
		[Ignore]
		public void ShouldReturnShiftExchangeOffersWithoutSiteOpenHourSetting()
		{
			prepareData(PersonFactory.CreatePersonWithPersonPeriodTeamSite(_periodStartDate));

			var person1 = PersonFactory.CreatePersonWithPersonPeriodTeamSite(_periodStartDate);
			var person2 = PersonFactory.CreatePersonWithPersonPeriodTeamSite(_periodStartDate);

			var shiftExchangeOffers = new[]
			{
				createShiftExchangeOffer(person1, _shiftTradeDate, 8, 15),
				createShiftExchangeOffer(person2, _shiftTradeDate, 8, 15),
			};

			var filteredShiftExchangeOffers =
				Target.FilterShiftExchangeOffer(shiftExchangeOffers,
					createShiftTradeAddPersonScheduleViewModel(person1, _shiftTradeDate, new TimePeriod[] {})).ToList();
			filteredShiftExchangeOffers.Count.Should().Be(2);
		}

		[Test]
		[Ignore]
		public void ShouldReturnShiftExchangeOffersWithEmptyDay()
		{
			prepareData(createPersonWithSiteOpenHours(8, 15));

			var person1 = createPersonWithSiteOpenHours(8, 19);

			var shiftExchangeOffers = new[]
			{
				createEmptyDayShiftExchangeOffer(person1, _shiftTradeDate),
			};

			var filteredShiftExchangeOffers =
				Target.FilterShiftExchangeOffer(shiftExchangeOffers,
					createShiftTradeAddPersonScheduleViewModel(person1, _shiftTradeDate, new TimePeriod[] {})).ToList();
			filteredShiftExchangeOffers.Count.Should().Be(1);
		}

		[Test]
		[Ignore]
		public void ShouldFilterShiftExchangeOfferWithNightShiftSchedule()
		{
			var personFrom = createPersonWithSiteOpenHours(new Dictionary<DayOfWeek, TimePeriod>
			{
				{ DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(9), TimeSpan.FromHours(24).Subtract(new TimeSpan(1))) },
				{ DayOfWeek.Tuesday, new TimePeriod(TimeSpan.Zero, TimeSpan.FromHours(9)) },
			});
			prepareData(personFrom);

			var person1 = createPersonWithSiteOpenHours(8, 17);
			var person2 = createPersonWithSiteOpenHours(8, 19);

			var shiftExchangeOffers = new[]
			{
				createShiftExchangeOffer(person1, _shiftTradeDate, new TimePeriod(TimeSpan.FromHours(22), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(5)))),
				createShiftExchangeOffer(person2, _shiftTradeDate, 8, 16)
			};

			var filteredShiftExchangeOffers =
				Target.FilterShiftExchangeOffer(shiftExchangeOffers,
					createShiftTradeAddPersonScheduleViewModel(person1, _shiftTradeDate, new[]
					{
						new TimePeriod(8, 0, 17, 0),
					})).ToList();
			filteredShiftExchangeOffers.Count.Should().Be(1);
			filteredShiftExchangeOffers.FirstOrDefault().Person.Should().Be(person1);
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

		private IPerson createPersonWithSiteOpenHours(int startHour, int endHour, bool isOpenHoursClosed = false)
		{
			var team = TeamFactory.CreateTeam("team", "site");
			var siteOpenHour = new SiteOpenHour()
			{
				IsClosed = isOpenHoursClosed,
				Parent = team.Site,
				TimePeriod = new TimePeriod(startHour, 0, endHour, 0),
				WeekDay = DayOfWeek.Monday
			};
			team.Site.AddOpenHour(siteOpenHour);
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(_periodStartDate, team);
			return person;
		}

		private IPerson createPersonWithSiteOpenHours(Dictionary<DayOfWeek, TimePeriod> openHours)
		{
			var team = TeamFactory.CreateTeam("team", "site");
			foreach (var openHour in openHours)
			{
				var siteOpenHour = new SiteOpenHour()
				{
					Parent = team.Site,
					TimePeriod = openHour.Value,
					WeekDay = openHour.Key
				};
				team.Site.AddOpenHour(siteOpenHour);
			}
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(_periodStartDate, team);
			return person;
		}

		private IShiftExchangeOffer createShiftExchangeOffer(IPerson person, DateOnly date, int startHour, int endHour)
		{
			return createShiftExchangeOffer(person, date, new TimePeriod(startHour, 0, endHour, 0));
		}

		private IShiftExchangeOffer createShiftExchangeOffer(IPerson person, DateOnly date, TimePeriod timePeriod)
		{
			var scenario = CurrentScenario.Current();
			var dateTimePeriod =
				date.ToDateTimePeriod(timePeriod, person.PermissionInformation.DefaultTimeZone());
			var scheduleDay = ScheduleDayFactory.Create(date, person, scenario);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person,
				dateTimePeriod);
			scheduleDay.Add(assignment);
			var shiftExchangeOffer = new ShiftExchangeOffer(scheduleDay, new ShiftExchangeCriteria(),
				ShiftExchangeOfferStatus.Pending);

			var personRequest = new PersonRequest(person);
			personRequest.Request = shiftExchangeOffer;

			return shiftExchangeOffer;
		}

		private IShiftExchangeOffer createEmptyDayShiftExchangeOffer(IPerson person, DateOnly date)
		{
			var scenario = CurrentScenario.Current();
			var scheduleDay = ScheduleDayFactory.Create(date, person, scenario);
			var shiftExchangeOffer = new ShiftExchangeOffer(scheduleDay,
				new ShiftExchangeCriteria() {DayType = ShiftExchangeLookingForDay.EmptyDay},
				ShiftExchangeOfferStatus.Pending);
			return shiftExchangeOffer;
		}
	}
}
