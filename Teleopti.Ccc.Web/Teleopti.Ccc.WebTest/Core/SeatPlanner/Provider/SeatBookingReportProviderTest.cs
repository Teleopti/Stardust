using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.SeatManagement;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;


namespace Teleopti.Ccc.WebTest.Core.SeatPlanner.Provider
{
	[TestFixture]
	class SeatBookingReportProviderTest
	{
		private FakeSeatBookingRepository _seatBookingRepository;
		private FakeSeatMapRepository _seatMapLocationRepository;
		private SeatBookingReportProvider _seatBookingReportProvider;
		private Team _team;
		private SeatMapLocation _location;
		private IUserTimeZone _userTimeZone;
		private TimeZoneInfo _timeZone;

		[SetUp]
		public void Setup()
		{

			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			_userTimeZone.Stub(x => x.TimeZone()).Return(_timeZone);
			
			_seatBookingRepository = new FakeSeatBookingRepository();
			_seatMapLocationRepository = new FakeSeatMapRepository ();
			_seatBookingReportProvider = new SeatBookingReportProvider(_seatBookingRepository, _seatMapLocationRepository, new FakeTeamRepository(null), _userTimeZone);
			_team = SeatManagementProviderTestUtils.CreateTeam ("Team One");
			_location = new SeatMapLocation() { Name = "Location", LocationPrefix = "Prefix", LocationSuffix = "Suffix"};
			_location.SetId (Guid.NewGuid());

			_seatMapLocationRepository.Add (_location);
		}

		[Test]
		public void ShouldGetSingleSeatBooking()
		{
			var startDate = new DateOnly(2015, 03, 02);
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, _team);
			var seatBooking = SeatManagementProviderTestUtils.CreateSeatBooking(person, startDate, new DateTime(2015, 03, 02, 8, 0, 0, DateTimeKind.Utc), new DateTime(2015, 03, 02, 13, 0, 0, DateTimeKind.Utc));

			seatBooking.Book(_location.AddSeat("Seat One", 1));
			_seatBookingRepository.Add(seatBooking);

			var criteria = new SeatBookingReportCriteria()
			{
				Locations = new List<SeatMapLocation>() { _location },
				Teams = new List<Team>() { _team },
				Period = new DateOnlyPeriod(new DateOnly(2015, 03, 02), new DateOnly(2015, 03, 02))
			};

			var result = _seatBookingReportProvider.Get(criteria);
			var seatBookingDateGroup = result.SeatBookingsByDate.Single();

			Assert.IsTrue(seatBookingDateGroup.Date == startDate);
			Assert.IsTrue(result.SeatBookingsByDate.Count == 1);
			Assert.IsTrue(seatBookingDateGroup.Teams[0].SeatBookings.Single().LocationPrefix == "Prefix");
			Assert.IsTrue(seatBookingDateGroup.Teams[0].SeatBookings.Single().LocationSuffix == "Suffix");
		}

		[Test]
		public void ShouldGroupSeatBookingsByDate()
		{
			var startDate = new DateOnly(2015, 03, 02);
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, _team);
			var seatBooking = SeatManagementProviderTestUtils.CreateSeatBooking(person, startDate, new DateTime(2015, 03, 02, 8, 0, 0, DateTimeKind.Utc), new DateTime(2015, 03, 02, 13, 0, 0, DateTimeKind.Utc));
			var seatBooking2 = SeatManagementProviderTestUtils.CreateSeatBooking(person, startDate.AddDays(1), new DateTime(2015, 03, 03, 8, 0, 0, DateTimeKind.Utc), new DateTime(2015, 03, 03, 12, 0, 0, DateTimeKind.Utc));
			var seatBooking3 = SeatManagementProviderTestUtils.CreateSeatBooking(person, startDate.AddDays(1), new DateTime(2015, 03, 03, 13, 0, 0, DateTimeKind.Utc), new DateTime(2015, 03, 03, 19, 0, 0, DateTimeKind.Utc));

			var seatOne = _location.AddSeat ("Seat One", 1);

			seatBooking.Book(seatOne);
			seatBooking2.Book(seatOne);
			seatBooking3.Book(seatOne);

			_seatBookingRepository.Add(seatBooking);
			_seatBookingRepository.Add(seatBooking2);
			_seatBookingRepository.Add(seatBooking3);


			var criteria = new SeatBookingReportCriteria()
			{
				Locations = new List<SeatMapLocation>() { _location },
				Teams = new List<Team>() { _team },
				Period = new DateOnlyPeriod(new DateOnly(2015, 03, 02), new DateOnly(2015, 03, 03))
			};

			var result = _seatBookingReportProvider.Get(criteria);

			Assert.IsTrue(result.SeatBookingsByDate[0].Date == startDate);
			Assert.IsTrue(result.SeatBookingsByDate[1].Date == startDate.AddDays(1));
		}

		[Test]
		public void ShouldGroupBookingsByTeam()
		{
			var startDate = new DateOnly(2015, 03, 02);
			var team2 = SeatManagementProviderTestUtils.CreateTeam("Team Two");

			createBookingsForTwoTeams(startDate, team2);

			var criteria = new SeatBookingReportCriteria()
			{
				Locations = new List<SeatMapLocation>() { _location },
				Teams = new List<Team>() { _team, team2 },
				Period = new DateOnlyPeriod(new DateOnly(2015, 03, 02), new DateOnly(2015, 03, 02))
			};

			var result = _seatBookingReportProvider.Get(criteria);

			var seatBookingDateGroup = result.SeatBookingsByDate.Single();
			var seatBookingTeamGroups = seatBookingDateGroup.Teams;

			Assert.IsTrue(seatBookingTeamGroups.Count == 2);
		}


		private void createBookingsForTwoTeams(DateOnly startDate, Team team2)
		{
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, _team);
			var person2 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, _team);
			var person3 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, team2);

			var seatBookings = new List<SeatBooking>()
			{
				SeatManagementProviderTestUtils.CreateSeatBooking (person, startDate, new DateTime (2015, 03, 02, 8, 0, 0, DateTimeKind.Utc), new DateTime (2015, 03, 02, 13, 0, 0, DateTimeKind.Utc)),
				SeatManagementProviderTestUtils.CreateSeatBooking (person2, startDate, new DateTime (2015, 03, 02, 8, 0, 0, DateTimeKind.Utc), new DateTime (2015, 03, 02, 13, 0, 0, DateTimeKind.Utc)),
				SeatManagementProviderTestUtils.CreateSeatBooking (person3, startDate, new DateTime (2015, 03, 02, 8, 0, 0, DateTimeKind.Utc), new DateTime (2015, 03, 02, 13, 0, 0, DateTimeKind.Utc))
			};

			seatBookings[0].Book(_location.AddSeat("Seat One", 1 ));
			seatBookings[1].Book(_location.AddSeat("Seat Two", 1 ));
			seatBookings[2].Book(_location.AddSeat("Seat Three", 1));

			_seatBookingRepository.AddRange(seatBookings);
		}
	}
	
}

