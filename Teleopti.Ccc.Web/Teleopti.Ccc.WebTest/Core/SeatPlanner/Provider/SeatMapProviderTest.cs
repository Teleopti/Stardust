using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeRepositories;

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




		}
	}
}