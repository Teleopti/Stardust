using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	internal class DeleteSeatBookingCommandHandlerTest
	{
		private FakeSeatBookingRepository _seatBookingRepository;
		private FakeSeatPlanRepository _seatPlanRepository;

		[SetUp]
		public void SetUp()
		{
			new FakeCurrentScenario();
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

		private void executeDeleteCommand(Guid seatBookingId)
		{
			var deleteSeatBookingCommandHandler =
				new DeleteSeatBookingCommandHandler(new SeatPlanPersister(_seatBookingRepository, _seatPlanRepository));
			deleteSeatBookingCommandHandler.Handle(new DeleteSeatBookingCommand()
			{
				SeatBookingId = seatBookingId
			});
		}

		#endregion

		[Test]
		public void ShouldDeleteSeatBooking()
		{
			var seatBookingId = setupSeatBooking();

			executeDeleteCommand(seatBookingId);

			Assert.IsFalse(_seatBookingRepository.Any());

		}

		private Guid setupSeatBooking()
		{
			var seat = new Seat ("Seat One", 1);
			var seatBookingId = Guid.NewGuid();

			var startDateTime = new DateTime (2015, 03, 02, 08, 00, 00);
			var endDateTime = new DateTime (2015, 03, 02, 17, 00, 00);
			addSeatBooking (new Person(), startDateTime, endDateTime, seat, seatBookingId);
			return seatBookingId;
		}

		[Test]
		public void ShouldDeleteSeatPlanWhenAllSeatBookingsAreDeleted()
		{
			var seat = new Seat("Seat One", 1);
			var seatBookingId = Guid.NewGuid();

			var startDateTime = new DateTime(2015, 03, 02, 08, 00, 00);
			var endDateTime = new DateTime(2015, 03, 02, 17, 00, 00);
			addSeatBooking(new Person(), startDateTime, endDateTime, seat, seatBookingId);

			_seatPlanRepository.Add(new SeatPlan() { Date = new DateOnly(startDateTime), Status = SeatPlanStatus.Ok });

			executeDeleteCommand(seatBookingId);

			Assert.IsFalse(_seatPlanRepository.Any());

		}

		
	}
}