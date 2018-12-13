using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;

namespace Teleopti.Ccc.WebTest.Core.SeatPlanner.Provider
{
	[TestFixture]
	public class SeatOccupancyProviderTest
	{
		private FakeSeatBookingRepository _seatBookingRepository;
		private FakeSeatMapRepository _seatMapLocationRepository;
		private IUserTimeZone _userTimeZone;
		private TimeZoneInfo _timeZone;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{

			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));  //GMT +1
			_userTimeZone.Stub(x => x.TimeZone()).Return(_timeZone);

			_person = PersonFactory.CreatePersonWithId();

			_seatBookingRepository = new FakeSeatBookingRepository();
			_seatMapLocationRepository = new FakeSeatMapRepository();
		}

		[Test]
		public void ShouldGetOccupancyInformation()
		{

			var location = new SeatMapLocation() { Name = "Location",  LocationPrefix = "Prefix", LocationSuffix = "Suffix" };
			location.SetId(Guid.NewGuid());

			var seat = location.AddSeat("Seat", 1);
			var seat2 = location.AddSeat("Seat", 1);
			_seatMapLocationRepository.Add(location);

			var bookingDate = new DateOnly(2015, 8, 7);
			var booking = SeatManagementProviderTestUtils.CreateSeatBooking(
				_person,
				bookingDate,
				new DateTime(2015, 8, 7, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 8, 7, 12, 0, 0, DateTimeKind.Utc));


			var bookingSeat2 = SeatManagementProviderTestUtils.CreateSeatBooking(
				_person,
				bookingDate,
				new DateTime(2015, 8, 7, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 8, 7, 18, 0, 0, DateTimeKind.Utc));

			booking.Book(seat);
			bookingSeat2.Book(seat2);

			_seatBookingRepository.Add(booking);
			_seatBookingRepository.Add(bookingSeat2);

			var seatOccupancyProvider = new SeatOccupancyProvider(_seatBookingRepository, _seatMapLocationRepository, _userTimeZone);
			var occupancyInformation = seatOccupancyProvider.Get(new List<Guid>{ seat.Id.GetValueOrDefault()}, bookingDate);
			var occupancyInfo = occupancyInformation.Single().Occupancies.Single();

			occupancyInfo.StartDateTime.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(booking.StartDateTime, _timeZone));
			occupancyInfo.EndDateTime.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(booking.EndDateTime, _timeZone));
			occupancyInfo.PersonId.Should().Be.EqualTo(booking.Person.Id);
			occupancyInfo.SeatId.Should().Be.EqualTo(seat.Id);
			occupancyInfo.BookingId.Should().Be.EqualTo(booking.Id);
			occupancyInfo.LocationPrefix.Should().Be.EqualTo ("Prefix");
			occupancyInfo.LocationSuffix.Should().Be.EqualTo ("Suffix");
		}


		[Test]
		public void ShouldIncludeOccupancyOnTimeZoneBoundary()
		{
			var occupancyInformation = createBookingForDateTimeAndReturnBookingsForFullDay(
				new DateTime (2015, 8, 6, 20, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 8, 6, 22, 0, 0, DateTimeKind.Utc),  // 2015-08-06:22:0:0 utc = 2015-08-07:00:00:00 west so should include occupancy
				new DateOnly (2015, 8, 7)
			);

			Assert.AreEqual(1, occupancyInformation.Count);
		}

		[Test]
		public void ShouldExcludeOccupancyOnTimeZoneBoundary()
		{
			var occupancyInformation = createBookingForDateTimeAndReturnBookingsForFullDay( 
				new DateTime(2015, 8, 6, 20, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 8, 6, 21, 59, 59, DateTimeKind.Utc),  // 2015-08-06:21:59:59 utc = 2015-08-06:23:59:59 west so should exclude occupancy
				new DateOnly(2015, 8, 7)
			);

			Assert.AreEqual(0, occupancyInformation.Count);
		}


		private IList<GroupedOccupancyViewModel> createBookingForDateTimeAndReturnBookingsForFullDay(DateTime startDateTime, DateTime endDateTime, DateOnly dateToGet)
		{
			var location = new SeatMapLocation() {Name = "Location"};
			location.SetId (Guid.NewGuid());

			var seat = location.AddSeat ("Seat", 1);
			_seatMapLocationRepository.Add (location);

			var booking = SeatManagementProviderTestUtils.CreateSeatBooking (
				_person,
				new DateOnly(startDateTime.Date),
				startDateTime,
				endDateTime);

			booking.Book (seat);
			_seatBookingRepository.Add (booking);

			var seatOccupancyProvider = new SeatOccupancyProvider (_seatBookingRepository, _seatMapLocationRepository, _userTimeZone);
			var occupancyInformation = seatOccupancyProvider.Get (new List<Guid>{seat.Id.GetValueOrDefault()}, dateToGet);
			return occupancyInformation;
		}


		[Test]
		public void ShouldGetOccupancyInformationForMultipleSeats()
		{
			var location = new SeatMapLocation() { Name = "Location" };
			location.SetId(Guid.NewGuid());

			var person2 = PersonFactory.CreatePersonWithId();

			var seat = location.AddSeat("Seat", 1);
			var seat2 = location.AddSeat("Seat 2", 2);
			var seat3 = location.AddSeat("Seat 3", 3);
			_seatMapLocationRepository.Add(location);

			var bookingDate = new DateOnly(2015, 8, 7);
			var booking = SeatManagementProviderTestUtils.CreateSeatBooking(_person, bookingDate,
				new DateTime(2015, 8, 7, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 8, 7, 10, 0, 0, DateTimeKind.Utc));

			var bookingSeat2 = SeatManagementProviderTestUtils.CreateSeatBooking(_person, bookingDate,
				new DateTime(2015, 8, 7, 10, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 8, 7, 13, 0, 0, DateTimeKind.Utc));

			var bookingSeat3 = SeatManagementProviderTestUtils.CreateSeatBooking(_person, bookingDate,
				new DateTime(2015, 8, 7, 14, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 8, 7, 16, 0, 0, DateTimeKind.Utc));

			var bookingPerson2 = SeatManagementProviderTestUtils.CreateSeatBooking(person2, bookingDate,
				new DateTime(2015, 8, 7, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 8, 7, 18, 0, 0, DateTimeKind.Utc));
			
			booking.Book(seat);
			bookingSeat2.Book(seat2);
			bookingSeat3.Book(seat3);
			bookingPerson2.Book (seat);

			_seatBookingRepository.Add(booking);
			_seatBookingRepository.Add(bookingSeat2);
			_seatBookingRepository.Add(bookingSeat3);
			_seatBookingRepository.Add(bookingPerson2);

			var seatOccupancyProvider = new SeatOccupancyProvider(_seatBookingRepository, _seatMapLocationRepository, _userTimeZone);
			var occupancyGroupedBySeat = seatOccupancyProvider.Get(new Guid[]
			{
				seat.Id.GetValueOrDefault(), seat2.Id.GetValueOrDefault()
			}, bookingDate);

			Assert.AreEqual (2, occupancyGroupedBySeat.Count);

			occupancyGroupedBySeat[0].SeatId.Should().Be.EqualTo(seat.Id);
			occupancyGroupedBySeat[1].SeatId.Should().Be.EqualTo(seat2.Id);

			Assert.AreEqual(2, occupancyGroupedBySeat[0].Occupancies.Count);
			occupancyGroupedBySeat[0].Occupancies[0].PersonId.Should().Be.EqualTo (_person.Id);
			occupancyGroupedBySeat[0].Occupancies[1].PersonId.Should().Be.EqualTo(person2.Id);
		}

		[Test]
		public void ShouldGetSeatOccupancyInformationForScheduleDays()
		{
			var location = new SeatMapLocation { Name = "Location" };
			location.SetId(Guid.NewGuid());

			var seat = location.AddSeat("Seat", 1);
			var seat2 = location.AddSeat("Seat", 1);
			_seatMapLocationRepository.Add(location);

			var person2 = PersonFactory.CreatePersonWithId();

			var bookingDateStart = new DateOnly(2015, 8, 7);
			var bookingDateEnd = new DateOnly(2015, 8, 8);

			var booking1ForPerson1 = SeatManagementProviderTestUtils.CreateSeatBooking(
				_person,
				bookingDateStart,
				new DateTime(2015, 8, 7, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 8, 7, 18, 0, 0, DateTimeKind.Utc));

			var booking2ForPerson1 = SeatManagementProviderTestUtils.CreateSeatBooking(
				_person,
				bookingDateEnd,
				new DateTime(2015, 8, 8, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 8, 8, 18, 0, 0, DateTimeKind.Utc));


			var bookingForPerson2 = SeatManagementProviderTestUtils.CreateSeatBooking(
				person2,
				bookingDateStart,
				new DateTime(2015, 8, 7, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 8, 7, 18, 0, 0, DateTimeKind.Utc));

			booking1ForPerson1.Book(seat);
			booking2ForPerson1.Book(seat);
			bookingForPerson2.Book(seat2);

			_seatBookingRepository.Add(booking1ForPerson1);
			_seatBookingRepository.Add(booking2ForPerson1);
			_seatBookingRepository.Add(bookingForPerson2);


			var shiftCategory = new ShiftCategory("a");
			var scenario = new Scenario("d");

			var assignment1Person1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person,
				scenario, new Activity("Play Guitar"), new DateTimePeriod(booking1ForPerson1.StartDateTime, booking1ForPerson1.EndDateTime), shiftCategory);

			var assignment2Person1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person,
				scenario, new Activity("Play Guitar"), new DateTimePeriod(booking2ForPerson1.StartDateTime, booking2ForPerson1.EndDateTime), shiftCategory);

			var assignment1Person2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
				scenario, new Activity("Play Guitar"), new DateTimePeriod(bookingForPerson2.StartDateTime, bookingForPerson2.EndDateTime), shiftCategory);

			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(bookingDateStart, _person, scenario);
			scheduleDayOnePerson1.Add(assignment1Person1);

			var scheduleDayOnePerson2 = ScheduleDayFactory.Create(bookingDateStart, person2, scenario);
			scheduleDayOnePerson2.Add(assignment1Person2);

			var scheduleDayTwoPerson1 = ScheduleDayFactory.Create(bookingDateEnd, _person, scenario);
			scheduleDayTwoPerson1.Add(assignment2Person1);

			var seatOccupancyProvider = new SeatOccupancyProvider(_seatBookingRepository, _seatMapLocationRepository, _userTimeZone);
			var occupancyInformation = seatOccupancyProvider.GetSeatBookingsForScheduleDays(new DateOnlyPeriod(bookingDateStart,bookingDateEnd), _person);

			Assert.AreEqual(2, occupancyInformation.Count);

			Assert.AreEqual(_person.Id, occupancyInformation[0].PersonId);
			Assert.AreEqual(_person.Id, occupancyInformation[1].PersonId);
		}
	}
}
