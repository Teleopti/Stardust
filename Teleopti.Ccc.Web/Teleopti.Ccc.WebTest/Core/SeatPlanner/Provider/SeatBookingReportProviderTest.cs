using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.SeatManagement;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Interfaces.Domain;

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

		[SetUp]
		public void Setup()
		{

			_seatBookingRepository = new FakeSeatBookingRepository();
			_seatMapLocationRepository = new FakeSeatMapRepository ();

			_seatBookingReportProvider = new SeatBookingReportProvider(_seatBookingRepository, _seatMapLocationRepository, new FakeTeamRepository());
			_team = createTeam("Team One");
			_location = new SeatMapLocation() { Name = "Location" };
			_location.SetId (Guid.NewGuid());

			_seatMapLocationRepository.Add (_location);
		}

		[Test]
		public void ShouldGetSingleSeatBooking()
		{
			var startDate = new DateOnly(2015, 03, 02);
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, _team);
			var seatBooking = createSeatBooking(person, startDate, new DateTime(2015, 03, 02, 8, 0, 0), new DateTime(2015, 03, 02, 13, 0, 0));

			seatBooking.Book(createSeat("Seat One", 1, _location));
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

		}

		[Test]
		public void ShouldGroupSeatBookingsByDate()
		{
			var startDate = new DateOnly(2015, 03, 02);
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, _team);
			var seatBooking = createSeatBooking(person, startDate, new DateTime(2015, 03, 02, 8, 0, 0), new DateTime(2015, 03, 02, 13, 0, 0));
			var seatBooking2 = createSeatBooking(person, startDate.AddDays(1), new DateTime(2015, 03, 03, 8, 0, 0), new DateTime(2015, 03, 03, 12, 0, 0));
			var seatBooking3 = createSeatBooking(person, startDate.AddDays(1), new DateTime(2015, 03, 03, 13, 0, 0), new DateTime(2015, 03, 03, 19, 0, 0));

			var seatOne = createSeat("Seat One", 1, _location);

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
			var team2 = createTeam("Team Two");

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


		private static Team createTeam(String name)
		{
			var team = new Team() { Description = new Description(name) };
			team.SetId(Guid.NewGuid());
			return team;
		}

		private static Seat createSeat(String name, int priority, SeatMapLocation location)
		{
			return location.AddSeat(name, priority);
		}

		private static SeatBooking createSeatBooking(IPerson person, DateOnly belongsToDate, DateTime startDateTime, DateTime endDateTime)
		{
			var seatBooking = new SeatBooking(person, belongsToDate, startDateTime, endDateTime);
			seatBooking.SetId(Guid.NewGuid());
			return seatBooking;
		}

		private void createBookingsForTwoTeams(DateOnly startDate, Team team2)
		{
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, _team);
			var person2 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, _team);
			var person3 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, team2);

			var seatBookings = new List<SeatBooking>()
			{
				createSeatBooking (person, startDate, new DateTime (2015, 03, 02, 8, 0, 0), new DateTime (2015, 03, 02, 13, 0, 0)),
				createSeatBooking (person2, startDate, new DateTime (2015, 03, 02, 8, 0, 0), new DateTime (2015, 03, 02, 13, 0, 0)),
				createSeatBooking (person3, startDate, new DateTime (2015, 03, 02, 8, 0, 0), new DateTime (2015, 03, 02, 13, 0, 0))
			};

			seatBookings[0].Book(createSeat("Seat One", 1, _location));
			seatBookings[1].Book(createSeat("Seat Two", 1, _location));
			seatBookings[2].Book(createSeat("Seat Three", 1, _location));

			_seatBookingRepository.AddRange(seatBookings);
		}
	}
	
}

