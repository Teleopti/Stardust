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

using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture, MyTimeWebTest]
	public class ShiftTradeSiteOpenHourFilterTest
	{
		public FakeLoggedOnUser LoggedOnUser;
		public FakeToggleManager ToggleManager;
		public IShiftTradeSiteOpenHourFilter Target;
		public ICurrentScenario CurrentScenario;

		private readonly DateOnly _periodStartDate = new DateOnly(2016, 1, 1);
		private readonly DateOnly _shiftTradeDate = new DateOnly(2016, 8, 8);

		private ShiftTradeAddPersonScheduleViewModel _personFromScheduleView;
		private IPerson _personFrom;

		[Test]
		public void ShouldFilterScheduleByCurrentUserSiteOpenHour()
		{
			prepareData();

			var person1 = createPersonWithSiteOpenHours(8, 19);
			var person2 = createPersonWithSiteOpenHours(8, 19);

			var scheduleDays = new[]
			{
				createScheduleDay(person1, new TimePeriod(8, 30, 10, 30), new TimePeriod(11, 30, 14, 30)),
				createScheduleDay(person2, new TimePeriod(8, 30, 10, 30), new TimePeriod(11, 30, 14, 30), new TimePeriod(16, 00, 18, 30))
			};

			var filteredScheduleDays =
				scheduleDays.Where(scheduleDay => Target.FilterSchedule(scheduleDay, _personFromScheduleView)).ToList();

			filteredScheduleDays.Count.Should().Be(1);
			filteredScheduleDays.First().Person.Should().Be(person1);
		}

		[Test]
		public void ShouldFilterScheduleByOtherAgentSiteOpenHour()
		{
			prepareData();

			var person1 = createPersonWithSiteOpenHours(8, 15);
			var person2 = createPersonWithSiteOpenHours(8, 11);

			var scheduleDays = new[]
			{
				createScheduleDay(person1, new TimePeriod(8, 30, 10, 30), new TimePeriod(11, 30, 14, 30)),
				createScheduleDay(person2, new TimePeriod(8, 30, 10, 30), new TimePeriod(11, 30, 14, 30))
			};

			var filteredScheduleDays =
				scheduleDays.Where(scheduleDay => Target.FilterSchedule(scheduleDay, _personFromScheduleView)).ToList();

			filteredScheduleDays.Count.Should().Be(1);
			filteredScheduleDays.First().Person.Should().Be(person1);
		}

		[Test]
		public void ShouldFilterScheduleWhenSiteOpenHoursIsClosed()
		{
			prepareData();
			_personFrom.SiteOpenHour(_shiftTradeDate).IsClosed = true;

			var person1 = createPersonWithSiteOpenHours(8, 15);
			var person2 = createPersonWithSiteOpenHours(8, 11);

			var scheduleDays = new[]
			{
				createScheduleDay(person1, new TimePeriod(8, 30, 10, 30), new TimePeriod(11, 30, 14, 30)),
				createScheduleDay(person2, new TimePeriod(8, 30, 10, 30), new TimePeriod(11, 30, 14, 30))
			};

			var filteredScheduleDays =
				scheduleDays.Where(scheduleDay => Target.FilterSchedule(scheduleDay, _personFromScheduleView));

			filteredScheduleDays.Count().Should().Be(0);
		}

		[Test]
		public void ShouldFilterScheduleWithNightShiftSchedule()
		{
			var personFrom = createPersonWithSiteOpenHours(new Dictionary<DayOfWeek, TimePeriod>
			{
				{ DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(9), TimeSpan.FromHours(33)) }
			});
			prepareData(personFrom);

			var person1 = createPersonWithSiteOpenHours(8, 15);

			var scheduleDays = new[]
			{
				createScheduleDay(person1, new TimePeriod(TimeSpan.FromHours(22), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(5))))
			};

			var filteredScheduleDays =
				scheduleDays.Where(scheduleDay => Target.FilterSchedule(scheduleDay, _personFromScheduleView)).ToList();

			filteredScheduleDays.Count.Should().Be(1);
			filteredScheduleDays.First().Person.Should().Be(person1);
		}

		[Test]
		public void ShouldFilterScheduleWithNullOrEmptyScheduleDay()
		{
			prepareData();
			Assert.True(Target.FilterSchedule(null, _personFromScheduleView));
			Assert.True(Target.FilterSchedule(ScheduleDayFactory.Create(_shiftTradeDate), _personFromScheduleView));
		}

		[Test]
		public void ShouldFilterScheduleViewModelWhenPersonToScheduleIsNull()
		{
			prepareData();
			Assert.True(Target.FilterSchedule(null, _personFromScheduleView));
		}

		[Test]
		public void ShouldFilterPersonToScheduleEvenPersonFromHasADayOff()
		{
			prepareData();

			var person1 = createPersonWithSiteOpenHours(8, 15);
			var person2 = createPersonWithSiteOpenHours(8, 11);

			var scheduleDays = new[]
			{
				createScheduleDay(person1, new TimePeriod(8, 30, 10, 30), new TimePeriod(11, 30, 14, 30)),
				createScheduleDay(person2, new TimePeriod(8, 30, 10, 30), new TimePeriod(11, 30, 15, 30))
			};
			_personFromScheduleView = createShiftTradeAddPersonScheduleViewModelWithDayOff(person1);

			var filteredScheduleDays =
				scheduleDays.Where(scheduleDay => Target.FilterSchedule(scheduleDay, _personFromScheduleView)).ToList();

			filteredScheduleDays.Count.Should().Be(1);
			filteredScheduleDays.First().Person.Should().Be(person1);
		}

		[Test]
		public void ShouldFilterScheduleViewByCurrentUserSiteOpenHour()
		{
			prepareData();

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

			var datePersons = new DatePersons { Date = _shiftTradeDate, Persons = new[] { person1, person2 } };
			var filteredShiftTradeAddPersonScheduleViews =
				Target.FilterScheduleView(shiftTradeAddPersonScheduleViews, _personFromScheduleView, datePersons).ToList();

			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(1);
			filteredShiftTradeAddPersonScheduleViews.First().PersonId.Should().Be(person1.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldNotFilterScheduleViewWhenFromPersonScheduleIsNull()
		{
			prepareData();

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
			_personFromScheduleView = null;
			var datePersons = new DatePersons {Date = _shiftTradeDate, Persons = new[] { person1, person2 } };
			var filteredShiftTradeAddPersonScheduleViews =
				Target.FilterScheduleView(shiftTradeAddPersonScheduleViews, _personFromScheduleView, datePersons).ToList();

			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(2);
		}

		[Test]
		public void ShouldFilterScheduleViewByOtherAgentSiteOpenHour()
		{
			prepareData();

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
				Target.FilterScheduleView(shiftTradeAddPersonScheduleViews, _personFromScheduleView, datePersons).ToList();

			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(1);
			filteredShiftTradeAddPersonScheduleViews.First().PersonId.Should().Be(person1.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldFilterScheduleViewWhenSiteOpenHoursIsClosed()
		{
			prepareData();
			_personFrom.SiteOpenHour(_shiftTradeDate).IsClosed = true;

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
				Target.FilterScheduleView(shiftTradeAddPersonScheduleViews, _personFromScheduleView, datePersons).ToList();

			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(0);
		}

		[Test]
		public void ShouldFilterScheduleViewWithNightShiftSchedule()
		{
			var personFrom = createPersonWithSiteOpenHours(new Dictionary<DayOfWeek, TimePeriod>
			{
				{ DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(19), TimeSpan.FromHours(37)) },
				{ DayOfWeek.Tuesday, new TimePeriod(TimeSpan.FromHours(19), TimeSpan.FromHours(37)) }
			});
			prepareData(personFrom);

			_personFromScheduleView = createShiftTradeAddPersonScheduleViewModel(_personFrom, _shiftTradeDate, new[]
			{
				new TimePeriod(
					TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(45))
					, TimeSpan.FromDays(1).Add(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(45))))
			});

			var person1 = createPersonWithSiteOpenHours(new Dictionary<DayOfWeek, TimePeriod>
			{
				{ DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(19), TimeSpan.FromHours(37)) },
				{ DayOfWeek.Tuesday, new TimePeriod(TimeSpan.FromHours(19), TimeSpan.FromHours(37)) }
			});

			var shiftTradeAddPersonScheduleViews = new[]
			{
				createShiftTradeAddPersonScheduleViewModel(person1, _shiftTradeDate, new[]
				{
					new TimePeriod(
					TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(45))
					, TimeSpan.FromDays(1).Add(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(45))))
				})
			};

			var datePersons = new DatePersons { Date = _shiftTradeDate, Persons = new[] { person1 } };
			var filteredShiftTradeAddPersonScheduleViews =
				Target.FilterScheduleView(shiftTradeAddPersonScheduleViews, _personFromScheduleView, datePersons).ToList();

			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(1);
			filteredShiftTradeAddPersonScheduleViews.First().PersonId.Should().Be(person1.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldFilterScheduleViewWithNightShiftScheduleWhenSiteOpenHoursIs24Hours()
		{
			var fullDayPeriod = new TimePeriod(TimeSpan.FromHours(0), TimeSpan.FromHours(24));
			var openHours = new Dictionary<DayOfWeek, TimePeriod>
			{
				{DayOfWeek.Monday, fullDayPeriod},
				{DayOfWeek.Tuesday, fullDayPeriod},
				{DayOfWeek.Wednesday, fullDayPeriod},
				{DayOfWeek.Thursday, fullDayPeriod},
				{DayOfWeek.Friday, fullDayPeriod},
				{DayOfWeek.Saturday, fullDayPeriod},
				{DayOfWeek.Sunday, fullDayPeriod}
			};

			var personFrom = createPersonWithSiteOpenHours(openHours);
			prepareData(personFrom);
			_personFromScheduleView = createShiftTradeAddPersonScheduleViewModel(_personFrom, _shiftTradeDate, new[]
			{
				new TimePeriod(
					TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(45))
					, TimeSpan.FromDays(1).Add(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(45))))
			});

			var person1 = createPersonWithSiteOpenHours(openHours);

			var shiftTradeAddPersonScheduleViews = new[]
			{
				createShiftTradeAddPersonScheduleViewModel(person1, _shiftTradeDate, new[]
				{
					new TimePeriod(
					TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(45))
					, TimeSpan.FromDays(1).Add(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(45))))
				})
			};

			var datePersons = new DatePersons { Date = _shiftTradeDate, Persons = new[] { person1 } };
			var filteredShiftTradeAddPersonScheduleViews =
				Target.FilterScheduleView(shiftTradeAddPersonScheduleViews, _personFromScheduleView, datePersons).ToList();

			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(1);
			filteredShiftTradeAddPersonScheduleViews.First().PersonId.Should().Be(person1.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldReturnNightShiftScheduleViewForEmptySiteOpenHour()
		{
			var personFrom = createPersonWithSiteOpenHours(new Dictionary<DayOfWeek, TimePeriod>
			{
				{ DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(9), TimeSpan.FromHours(33)) }
			});
			prepareData(personFrom);

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
				Target.FilterScheduleView(shiftTradeAddPersonScheduleViews, _personFromScheduleView, datePersons).ToList();

			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(1);
			filteredShiftTradeAddPersonScheduleViews.First().PersonId.Should().Be(person1.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldReturnScheduleViewWithDayOff()
		{
			var today = DateTime.Today;
			var dayOfWeek = today.DayOfWeek;
			var personFrom = createPersonWithSiteOpenHours(new Dictionary<DayOfWeek, TimePeriod>
			{
				{ dayOfWeek, new TimePeriod(TimeSpan.FromHours(9), TimeSpan.FromHours(33)) }
			});
			prepareData(personFrom);

			var person1 = createPersonWithSiteOpenHours(new Dictionary<DayOfWeek, TimePeriod>());

			var tomorrow = today.AddDays(1);
			var dayoffLayerViewModel = new[]
			{
				new TeamScheduleLayerViewModel
				{
					TitleHeader = "Dayoff Layer",
					LengthInMinutes = (int) (tomorrow - today).TotalMinutes,
					Start = today,
					End = tomorrow
				}
			};
			var shiftTradeAddPersonScheduleViews = new[]
			{
				new ShiftTradeAddPersonScheduleViewModel
				{
					PersonId = person1.Id.GetValueOrDefault(),
					ScheduleLayers = dayoffLayerViewModel,
					IsDayOff = true,
					Name = "test",
					Total = dayoffLayerViewModel.Length
				}
			};

			var datePersons = new DatePersons { Date = _shiftTradeDate, Persons = new[] { person1 } };
			var filteredShiftTradeAddPersonScheduleViews =
				Target.FilterScheduleView(shiftTradeAddPersonScheduleViews, _personFromScheduleView, datePersons).ToList();

			filteredShiftTradeAddPersonScheduleViews.Count.Should().Be(1);
			filteredShiftTradeAddPersonScheduleViews.First().PersonId.Should().Be(person1.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldFilterShiftExchangeOfferByCurrentUserSiteOpenHour()
		{
			prepareData();

			var person1 = createPersonWithSiteOpenHours(8, 19);
			var person2 = createPersonWithSiteOpenHours(8, 19);

			var shiftExchangeOffers = new[]
			{
				createShiftExchangeOffer(person1, _shiftTradeDate, 8, 11),
				createShiftExchangeOffer(person2, _shiftTradeDate, 8, 16),
			};

			var filteredShiftExchangeOffers = shiftExchangeOffers
				.Where(shiftExchangeOffer => Target.FilterShiftExchangeOffer(shiftExchangeOffer, _personFromScheduleView)).ToList();
			filteredShiftExchangeOffers.Count.Should().Be(1);
			filteredShiftExchangeOffers.First().Person.Should().Be(person1);
		}

		[Test]
		public void ShouldFilterShiftExchangeOfferByOtherAgentSiteOpenHour()
		{
			prepareData();

			var person1 = createPersonWithSiteOpenHours(8, 19);
			var person2 = createPersonWithSiteOpenHours(8, 11);

			var shiftExchangeOffers = new[]
			{
				createShiftExchangeOffer(person1, _shiftTradeDate, 8, 15),
				createShiftExchangeOffer(person2, _shiftTradeDate, 8, 15)
			};

			var filteredShiftExchangeOffers = shiftExchangeOffers
				.Where(shiftExchangeOffer => Target.FilterShiftExchangeOffer(shiftExchangeOffer, _personFromScheduleView)).ToList();
			filteredShiftExchangeOffers.Count.Should().Be(1);
			filteredShiftExchangeOffers.First().Person.Should().Be(person1);
		}

		[Test]
		public void ShouldReturnShiftExchangeOffersWithEmptyDay()
		{
			prepareData();

			var person1 = createPersonWithSiteOpenHours(8, 19);

			var shiftExchangeOffers = new[]
			{
				createEmptyDayShiftExchangeOffer(person1, _shiftTradeDate),
			};

			var filteredShiftExchangeOffers = shiftExchangeOffers
				.Where(shiftExchangeOffer => Target.FilterShiftExchangeOffer(shiftExchangeOffer, _personFromScheduleView)).ToList();
			filteredShiftExchangeOffers.Count.Should().Be(1);
		}

		[Test]
		public void ShouldFilterShiftExchangeOfferWithNightShiftSchedule()
		{
			var personFrom = createPersonWithSiteOpenHours(new Dictionary<DayOfWeek, TimePeriod>
			{
				{ DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(9), TimeSpan.FromHours(33)) }
			});
			prepareData(personFrom);

			var person1 = createPersonWithSiteOpenHours(8, 17);
			var person2 = createPersonWithSiteOpenHours(8, 19);

			var shiftExchangeOffers = new[]
			{
				createShiftExchangeOffer(person1, _shiftTradeDate, new TimePeriod(TimeSpan.FromHours(22), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(5)))),
				createShiftExchangeOffer(person2, _shiftTradeDate, 8, 16)
			};

			var filteredShiftExchangeOffers = shiftExchangeOffers
				.Where(shiftExchangeOffer => Target.FilterShiftExchangeOffer(shiftExchangeOffer, _personFromScheduleView)).ToList();
			filteredShiftExchangeOffers.Count.Should().Be(1);
			filteredShiftExchangeOffers.First().Person.Should().Be(person1);
		}

		[Test]
		public void ShouldFilterScheduleBySiteOpenHourWithOvernightShift()
		{
			var saturday = new DateOnly(2018, 6, 23);
			var siteOpenHourDic = new Dictionary<DayOfWeek, TimePeriod>
			{
				{DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(24))},
				{DayOfWeek.Tuesday, new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(24))},
				{DayOfWeek.Wednesday, new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(24))},
				{DayOfWeek.Thursday, new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(24))},
				{DayOfWeek.Friday, new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(24))},
				{DayOfWeek.Saturday, new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(24))},
				{DayOfWeek.Sunday, new TimePeriod(TimeSpan.FromHours(9), TimeSpan.FromHours(23))}
			};
			var personFrom = createPersonWithSiteOpenHours(siteOpenHourDic);
			var timezone = TimeZoneInfoFactory.NewYorkTimeZoneInfo();
			personFrom.PermissionInformation.SetDefaultTimeZone(timezone);

			LoggedOnUser.SetFakeLoggedOnUser(personFrom);

			var timePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromDays(1));
			var personFromScheduleView = createShiftTradeAddPersonScheduleViewModel(personFrom, saturday, timePeriod);

			var person1 = createPersonWithSiteOpenHours(siteOpenHourDic);
			person1.PermissionInformation.SetDefaultTimeZone(timezone);
			var person2 = createPersonWithSiteOpenHours(siteOpenHourDic);
			person2.PermissionInformation.SetDefaultTimeZone(timezone);

			var scheduleDays = new[]
			{
				createScheduleDay(saturday, person1, timePeriod),
				createScheduleDay(saturday, person2, timePeriod)
			};

			var filteredScheduleDays =
				scheduleDays.Where(scheduleDay => Target.FilterSchedule(scheduleDay, personFromScheduleView)).ToList();

			filteredScheduleDays.Count.Should().Be(2);
			filteredScheduleDays.First().Person.Should().Be(person1);
		}

		private void prepareData(IPerson person = null)
		{
			_personFrom = person ?? createPersonWithSiteOpenHours(8, 15);
			LoggedOnUser.SetFakeLoggedOnUser(_personFrom);
			_personFromScheduleView = createShiftTradeAddPersonScheduleViewModel(_personFrom, _shiftTradeDate, new[]
			{
				new TimePeriod(8, 30, 10, 30),
				new TimePeriod(11, 30, 14, 30),
			});
		}

		private static ShiftTradeAddPersonScheduleViewModel createShiftTradeAddPersonScheduleViewModel(IPerson person,
			DateOnly date, params TimePeriod[] timePeriods)
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

		private static ShiftTradeAddPersonScheduleViewModel createShiftTradeAddPersonScheduleViewModelWithDayOff(IPerson person)
		{
			var teamScheduleLayerViewModels = new List<TeamScheduleLayerViewModel>();

			return new ShiftTradeAddPersonScheduleViewModel
			{
				PersonId = person.Id.GetValueOrDefault(),
				ScheduleLayers = teamScheduleLayerViewModels.ToArray(),
				Name = "test",
				Total = teamScheduleLayerViewModels.Count,
				IsDayOff = true,
				IsFullDayAbsence = true,
				DayOffName = "dayOff"
			};
		}

		private static TeamScheduleLayerViewModel createTeamScheduleLayerViewModel(DateTime start, DateTime end)
		{
			return new TeamScheduleLayerViewModel
			{
				LengthInMinutes = (int)(end - start).TotalMinutes,
				Start = start,
				End = end
			};
		}

		private IPerson createPersonWithSiteOpenHours(int startHour, int endHour, bool isOpenHoursClosed = false)
		{
			var team = TeamFactory.CreateTeam("team", "site");
			var siteOpenHour = new SiteOpenHour
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
				var siteOpenHour = new SiteOpenHour
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
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, dateTimePeriod);
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
				new ShiftExchangeCriteria { DayType = ShiftExchangeLookingForDay.EmptyDay },
				ShiftExchangeOfferStatus.Pending);
			return shiftExchangeOffer;
		}

		private IScheduleDay createScheduleDay(IPerson person, params TimePeriod[] timePeriods)
		{
			return createScheduleDay(_shiftTradeDate, person, timePeriods);
		}

		private IScheduleDay createScheduleDay(DateOnly shiftDate, IPerson person, params TimePeriod[] timePeriods)
		{
			var scenario = CurrentScenario.Current();
			var scheduleDay = ScheduleDayFactory.Create(shiftDate, person, scenario);
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, shiftDate);
			foreach (var timePeriod in timePeriods)
			{
				assignment.AddActivity(new Activity("d"), timePeriod);
			}
			scheduleDay.Add(assignment);
			return scheduleDay;
		}
	}
}