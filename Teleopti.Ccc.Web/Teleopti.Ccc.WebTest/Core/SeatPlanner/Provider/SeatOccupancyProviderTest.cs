using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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
		private ILoggedOnUser _loggedOnUser;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{

			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			_userTimeZone.Stub(x => x.TimeZone()).Return(_timeZone);

			_person = PersonFactory.CreatePersonWithId();
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(_person);


			_seatBookingRepository = new FakeSeatBookingRepository();
			_seatMapLocationRepository = new FakeSeatMapRepository();
		}

		[Test]
		public void  ShouldGetOccupancyInformation()
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
			bookingSeat2.Book (seat2);

			_seatBookingRepository.Add(booking);
			_seatBookingRepository.Add(bookingSeat2);

			var seatOccupancyProvider = new SeatOccupancyProvider(_seatBookingRepository, _userTimeZone, _loggedOnUser);
			var occupancyInformation = seatOccupancyProvider.Get (seat.Id.Value, bookingDate);
			var occupancyInfo = occupancyInformation.Single();

			occupancyInfo.StartDateTime.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(booking.StartDateTime, _timeZone));
			occupancyInfo.EndDateTime.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(booking.EndDateTime, _timeZone));
			occupancyInfo.PersonId.Should().Be.EqualTo (booking.Person.Id);
			occupancyInfo.SeatId.Should().Be.EqualTo (seat.Id);
			occupancyInfo.BookingId.Should().Be.EqualTo (booking.Id);
			
		}

		[Test]
		public void ShouldGetOccupancyInformationForCurrentUser()
		{
			var location = new SeatMapLocation() { Name = "Location" };
			location.SetId(Guid.NewGuid());

			var seat = location.AddSeat("Seat", 1);
			var seat2 = location.AddSeat("Seat", 1);
			_seatMapLocationRepository.Add(location);

			var person2 = PersonFactory.CreatePersonWithId();

			var bookingDateStart = new DateOnly(2015, 8, 7);
			var bookingDateEnd= new DateOnly(2015, 8, 8);

			var booking1ForPerson1 = SeatManagementProviderTestUtils.CreateSeatBooking(
				_person,
				bookingDateStart,
				new DateTime(2015, 8, 7, 8, 0, 0),
				new DateTime(2015, 8, 7, 18, 0, 0));
			
			var booking2ForPerson1 = SeatManagementProviderTestUtils.CreateSeatBooking(
				_person,
				bookingDateStart,
				new DateTime(2015, 8, 8, 8, 0, 0),
				new DateTime(2015, 8, 8, 18, 0, 0));


			var bookingForPerson2 = SeatManagementProviderTestUtils.CreateSeatBooking(
				person2,
				bookingDateStart,
				new DateTime(2015, 8, 7, 8, 0, 0),
				new DateTime(2015, 8, 7, 18, 0, 0));

			booking1ForPerson1.Book(seat);
			booking2ForPerson1.Book (seat);
			bookingForPerson2.Book(seat2);

			_seatBookingRepository.Add(booking1ForPerson1);
			_seatBookingRepository.Add(booking2ForPerson1);
			_seatBookingRepository.Add(bookingForPerson2);
			
			var seatOccupancyProvider = new SeatOccupancyProvider(_seatBookingRepository, _userTimeZone, _loggedOnUser);
			var occupancyInformation = seatOccupancyProvider.GetSeatBookingsForCurrentUser (new DateOnlyPeriod(bookingDateStart, bookingDateEnd));
			Assert.IsTrue (occupancyInformation.Count == 2);
			occupancyInformation[0].PersonId.Should().Be (_person.Id);
			occupancyInformation[1].PersonId.Should().Be(_person.Id);
		}

	}
}
