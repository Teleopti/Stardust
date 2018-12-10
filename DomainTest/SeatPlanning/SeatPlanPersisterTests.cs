using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.DomainTest.SeatPlanning
{
	[TestFixture]
	internal class SeatPlanPersisterTests
	{
		private FakeSeatBookingRepository _seatBookingRepository;
		private FakeSeatPlanRepository _seatPlanRepository;

		[SetUp]
		public void SetUp()
		{
			_seatBookingRepository = new FakeSeatBookingRepository();
			_seatPlanRepository = new FakeSeatPlanRepository();
		}

		#region helper methods

		private void addSeatBooking (IPerson person, DateTime startDateTime, DateTime endDateTime,
			ISeat seat, Guid seatBookingId)
		{
			var existingSeatBooking = new SeatBooking(person, new DateOnly(startDateTime), startDateTime, endDateTime)
			{
				Seat = seat
			};

			existingSeatBooking.SetId(seatBookingId);
			_seatBookingRepository.Add (existingSeatBooking);
		}

		private Guid setupSeatBooking()
		{
			var seat = new Seat("Seat One", 1);
			var seatBookingId = Guid.NewGuid();

			var startDateTime = new DateTime(2015, 03, 02, 08, 00, 00, DateTimeKind.Utc);
			var endDateTime = new DateTime(2015, 03, 02, 17, 00, 00, DateTimeKind.Utc);
			addSeatBooking(new Person(), startDateTime, endDateTime, seat, seatBookingId);
			return seatBookingId;
		}


		private void removeSeatBooking(Guid seatBookingId)
		{
			new SeatPlanPersister(_seatBookingRepository, _seatPlanRepository).RemoveSeatBooking (seatBookingId);
		}

		#endregion

		[Test]
		public void ShouldDeleteSeatBooking()
		{
			var seatBookingId = setupSeatBooking();

			removeSeatBooking(seatBookingId);

			Assert.IsFalse(_seatBookingRepository.Any());

		}

		[Test]
		public void ShouldDeleteSeatPlanWhenAllSeatBookingsAreDeleted()
		{
			var seat = new Seat("Seat One", 1);
			var seatBookingId = Guid.NewGuid();

			var startDateTime = new DateTime(2015, 03, 02, 08, 00, 00, DateTimeKind.Utc);
			var endDateTime = new DateTime(2015, 03, 02, 17, 00, 00, DateTimeKind.Utc);
			addSeatBooking(new Person(), startDateTime, endDateTime, seat, seatBookingId);

			_seatPlanRepository.Add(new SeatPlan() { Date = new DateOnly(startDateTime), Status = SeatPlanStatus.Ok });

			removeSeatBooking(seatBookingId);

			Assert.IsFalse(_seatPlanRepository.Any());
		}
	}
}