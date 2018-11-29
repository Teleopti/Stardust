using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;


namespace Teleopti.Ccc.WebTest.Core.SeatPlanner.Provider
{
	[TestFixture]
	internal class SeatMapProviderTest
	{
		private FakeSeatBookingRepository _seatBookingRepository;
		private FakeSeatMapRepository _seatMapLocationRepository;
		private IUserTimeZone _userTimeZone;
		private TimeZoneInfo _timeZone;

		[SetUp]
		public void Setup()
		{

			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));  //GMT +1
			_userTimeZone.Stub(x => x.TimeZone()).Return(_timeZone);

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
				new DateTime (2015, 8, 7, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2015, 8, 7, 18, 0, 0, DateTimeKind.Utc));
			
			booking.Book (seat);
			_seatBookingRepository.Add (booking);

			var provider = new SeatMapProvider(_seatMapLocationRepository, _seatBookingRepository, _userTimeZone);
			var locationViewModel = provider.Get (location.Id, bookingDate);
			
			Assert.IsTrue(locationViewModel.Seats[0].IsOccupied);
			Assert.IsFalse(locationViewModel.Seats[1].IsOccupied);
		}

		[Test]
		public void ShouldLoadAvailiableRolesInTheSeats()
		{
			var role1 = ApplicationRoleFactory.CreateRole("role1", "Role 1");
			role1.SetId(Guid.NewGuid());
			var role2 = ApplicationRoleFactory.CreateRole("role2", "Role 2");
			role2.SetId(Guid.NewGuid());
			
			var location = new SeatMapLocation() { Name = "Location" };
			location.SetId(Guid.NewGuid());
			location.AddSeat("Seat1", 1);
			var seat2 = location.AddSeat("Seat2", 2);
			seat2.SetRoles(new IApplicationRole[] {role1, role2});

			_seatMapLocationRepository.Add(location);
			role2.SetDeleted();

			var provider = new SeatMapProvider(_seatMapLocationRepository, _seatBookingRepository, _userTimeZone);
			var locationViewModel = provider.Get(null);

			Assert.IsTrue(locationViewModel.Seats.Count == 2);
			Assert.IsTrue(locationViewModel.Seats[0].RoleIdList.Count == 0);
			Assert.IsTrue(locationViewModel.Seats[1].RoleIdList.Count == 1);
			Assert.AreEqual(role1.Id, locationViewModel.Seats[1].RoleIdList.Single());
		}

		[Test]
		public void ShouldLoadPropertiesInTheSeats()
		{
			var role1 = ApplicationRoleFactory.CreateRole("role1", "Role 1");
			role1.SetId(Guid.NewGuid());

			var location = new SeatMapLocation() { Name = "Location" };
			location.SetId(Guid.NewGuid());
			var seat1 = location.AddSeat("Seat1", 1);
			seat1.SetRoles(new IApplicationRole[] {role1});

			_seatMapLocationRepository.Add(location);

			var provider = new SeatMapProvider(_seatMapLocationRepository, _seatBookingRepository, _userTimeZone);
			var locationViewModel = provider.Get(null);

			Assert.IsTrue(locationViewModel.Seats.Count == 1);
			Assert.IsTrue(locationViewModel.Seats.Single().Id == seat1.Id);
			Assert.IsTrue(locationViewModel.Seats[0].RoleIdList.Count == 1);
			Assert.AreEqual(role1.Id, locationViewModel.Seats[0].RoleIdList.Single());
			Assert.AreEqual(seat1.Priority, locationViewModel.Seats[0].Priority);
			Assert.AreEqual(seat1.Name, locationViewModel.Seats[0].Name);
		}
	}
}