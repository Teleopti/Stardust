using System;
using System.Linq;
using System.Runtime;
using NUnit.Framework;
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

		[SetUp]
		public void Setup()
		{
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

			var person = PersonFactory.CreatePersonWithId();

			var bookingDate = new DateOnly(2015, 8, 7);
			var booking = SeatManagementProviderTestUtils.CreateSeatBooking(person,
				bookingDate,
				new DateTime(2015, 8, 7, 8, 0, 0),
				new DateTime(2015, 8, 7, 18, 0, 0));


			var bookingSeat2 = SeatManagementProviderTestUtils.CreateSeatBooking(person,
				bookingDate,
				new DateTime(2015, 8, 7, 8, 0, 0),
				new DateTime(2015, 8, 7, 18, 0, 0));

			booking.Book(seat);
			bookingSeat2.Book (seat2);

			_seatBookingRepository.Add(booking);
			_seatBookingRepository.Add(bookingSeat2);
			
			var seatOccupancyProvider = new SeatOccupancyProvider(_seatBookingRepository);
			var occupancyInformation = seatOccupancyProvider.Get (seat.Id.Value, bookingDate);

			var occupancyInfo = occupancyInformation.Single();

			Assert.IsTrue (occupancyInfo.StartDateTime == booking.StartDateTime);
			Assert.IsTrue(occupancyInfo.EndDateTime == booking.EndDateTime);
			Assert.IsTrue(occupancyInfo.PersonId == booking.Person.Id);
			Assert.IsTrue(occupancyInfo.SeatId == seat.Id);
			Assert.IsTrue(occupancyInfo.BookingId == booking.Id);
			
		}

	}
}