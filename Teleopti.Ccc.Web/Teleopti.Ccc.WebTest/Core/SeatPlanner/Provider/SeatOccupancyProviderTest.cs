using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.SeatPlanner.Provider
{
	[TestFixture]
	internal class SeatOccupancyProviderTest
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
			_timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			_userTimeZone.Stub(x => x.TimeZone()).Return(_timeZone);

			_person = PersonFactory.CreatePersonWithId();

			_seatBookingRepository = new FakeSeatBookingRepository();
			_seatMapLocationRepository = new FakeSeatMapRepository();
		}

		[Test]
		public void ShouldGetOccupancyInformation()
		{
			var location = new SeatMapLocation() { Name = "Location" };
			location.SetId(Guid.NewGuid());

			var seat = location.AddSeat("Seat", 1);
			var seat2 = location.AddSeat("Seat", 1);
			_seatMapLocationRepository.Add(location);

			var bookingDate = new DateOnly(2015, 8, 7);
			var booking = SeatManagementProviderTestUtils.CreateSeatBooking(
				_person,
				bookingDate,
				new DateTime(2015, 8, 7, 8, 0, 0),
				new DateTime(2015, 8, 7, 18, 0, 0));


			var bookingSeat2 = SeatManagementProviderTestUtils.CreateSeatBooking(
				_person,
				bookingDate,
				new DateTime(2015, 8, 7, 8, 0, 0),
				new DateTime(2015, 8, 7, 18, 0, 0));

			booking.Book(seat);
			bookingSeat2.Book(seat2);

			_seatBookingRepository.Add(booking);
			_seatBookingRepository.Add(bookingSeat2);

			var seatOccupancyProvider = new SeatOccupancyProvider(_seatBookingRepository, _seatMapLocationRepository, _userTimeZone);
			var occupancyInformation = seatOccupancyProvider.Get(seat.Id.Value, bookingDate);
			var occupancyInfo = occupancyInformation.Single();

			occupancyInfo.StartDateTime.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(booking.StartDateTime, _timeZone));
			occupancyInfo.EndDateTime.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(booking.EndDateTime, _timeZone));
			occupancyInfo.PersonId.Should().Be.EqualTo(booking.Person.Id);
			occupancyInfo.SeatId.Should().Be.EqualTo(seat.Id);
			occupancyInfo.BookingId.Should().Be.EqualTo(booking.Id);

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

			var assignment1Person1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				new Activity("Play Guitar"),
				_person,
				new DateTimePeriod(booking1ForPerson1.StartDateTime, booking1ForPerson1.EndDateTime),
				shiftCategory,
				scenario);

			var assignment2Person1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				new Activity("Play Guitar"),
				_person,
				new DateTimePeriod(booking2ForPerson1.StartDateTime, booking2ForPerson1.EndDateTime),
				shiftCategory,
				scenario);

			var assignment1Person2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				new Activity("Play Guitar"),
				person2,
				new DateTimePeriod(bookingForPerson2.StartDateTime, bookingForPerson2.EndDateTime),
				shiftCategory,
				scenario);

			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(bookingDateStart, _person, scenario);
			scheduleDayOnePerson1.Add(assignment1Person1);

			var scheduleDayOnePerson2 = ScheduleDayFactory.Create(bookingDateStart, person2, scenario);
			scheduleDayOnePerson2.Add(assignment1Person2);

			var scheduleDayTwoPerson1 = ScheduleDayFactory.Create(bookingDateEnd, _person, scenario);
			scheduleDayTwoPerson1.Add(assignment2Person1);

			var seatOccupancyProvider = new SeatOccupancyProvider(_seatBookingRepository, _seatMapLocationRepository, _userTimeZone);
			var occupancyInformation = seatOccupancyProvider.GetSeatBookingsForScheduleDays(new List<IScheduleDay>() { scheduleDayOnePerson1, scheduleDayOnePerson2 });
			
			Assert.AreEqual(2, occupancyInformation.Count);

			Assert.AreEqual(_person.Id, occupancyInformation[0].PersonId);
			Assert.AreEqual(person2.Id, occupancyInformation[1].PersonId);
		}

		[Test]
		public void ShouldGetSeatOccupancyInformationForScheduleDayWithoutPersonAssignment()
		{
			var location = new SeatMapLocation { Name = "Location" };
			location.SetId(Guid.NewGuid());

			var seat = location.AddSeat("Seat", 1);
			_seatMapLocationRepository.Add(location);

			var bookingDateStart = new DateOnly(2015, 8, 7);
			
			var booking1ForPerson1 = SeatManagementProviderTestUtils.CreateSeatBooking(
				_person,
				bookingDateStart,
				new DateTime(2015, 8, 7, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 8, 7, 18, 0, 0, DateTimeKind.Utc));

			booking1ForPerson1.Book(seat);

			_seatBookingRepository.Add(booking1ForPerson1);

			var scenario = new Scenario("d");

			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(bookingDateStart, _person, scenario);
			
			var seatOccupancyProvider = new SeatOccupancyProvider(_seatBookingRepository, _seatMapLocationRepository, _userTimeZone);
			var occupancyInformation = seatOccupancyProvider.GetSeatBookingsForScheduleDays(new List<IScheduleDay> { scheduleDayOnePerson1 });

			Assert.AreEqual(0, occupancyInformation.Count);
		}
	}
}
