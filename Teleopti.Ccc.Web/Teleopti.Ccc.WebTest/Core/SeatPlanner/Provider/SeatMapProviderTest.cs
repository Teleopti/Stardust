using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.SeatPlanner.Provider
{
	[TestFixture]
	internal class SeatMapProviderTest
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
		public void ShouldSetOccupancyFlagForBookedSeats()
		{
			var location = new SeatMapLocation() { Name = "Location" };
			location.SetId (Guid.NewGuid());
			var seat = location.AddSeat("Seat", 1);
			location.AddSeat("Seat", 2);

			_seatMapLocationRepository.Add (location);

			var person = PersonFactory.CreatePerson();

			var bookingDate = new DateOnly (2015, 8, 7);
			var booking = SeatManagementProviderTestUtils.CreateSeatBooking (person,
				bookingDate,
				new DateTime (2015, 8, 7, 8, 0, 0),
				new DateTime (2015, 8, 7, 18, 0, 0));
			
			booking.Book (seat);
			_seatBookingRepository.Add (booking);

			var provider = new SeatMapProvider(_seatMapLocationRepository, _seatBookingRepository);
			var locationViewModel = provider.Get (location.Id, bookingDate);
			
			Assert.IsTrue(locationViewModel.Seats[0].IsOccupied);
			Assert.IsFalse(locationViewModel.Seats[1].IsOccupied);
		}

	}
}