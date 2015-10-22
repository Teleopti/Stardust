using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SeatPlanning
{
	public class SeatLevelAllocatorTests
	{
		[Test]
		public void ShouldAllocateAnAgentToASeat()
		{
			var bookingDate = new DateOnly(2014, 01, 01);
			var booking = new SeatBooking(null, bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(17));
			var seatBookingRequest = new SeatBookingRequest(booking);
			var seats = new List<Seat> { new Seat("Seat1", 1) };

			new SeatLevelAllocator(seats).AllocateSeats(seatBookingRequest);

			Assert.That(booking.Seat.Name == seats.Single().Name);
		}

		[Test]
		public void ShouldAllocateTwoAgentsToOneSeatEach()
		{

			var bookingDate = new DateOnly(2014, 01, 01);

			var booking1 = new SeatBooking(null, bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(17));
			var booking2 = new SeatBooking(null, bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(17));
			var seatBookingRequest = new SeatBookingRequest(booking1, booking2);

			var seats = new List<Seat>()
			{
				new Seat("seat1", 1), 
				new Seat("seat2", 2)
			};

			new SeatLevelAllocator(seats).AllocateSeats(seatBookingRequest);

			seatBookingRequest.SeatBookings.Count(booking => booking.Seat != null).Should().Be(2);
			seatBookingRequest.SeatBookings.Count(booking => booking.Seat.Name == seats[0].Name).Should().Be(1);
			seatBookingRequest.SeatBookings.Count(booking => booking.Seat.Name == seats[1].Name).Should().Be(1);
		}

		[Test]
		public void ShouldAllocateTwoAgentsToOneSeat()
		{

			var bookingDate = new DateOnly(2014, 01, 01);

			var booking1 = new SeatBooking(null, bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(12));
			var booking2 = new SeatBooking(null, bookingDate, bookingDate.Date.AddHours(13), bookingDate.Date.AddHours(17));
			var seatBookingRequest = new SeatBookingRequest(booking1, booking2);

			var seats = new List<Seat>()
			{
				new Seat("seat1", 1)
			};

			new SeatLevelAllocator(seats).AllocateSeats(seatBookingRequest);

			seatBookingRequest.SeatBookings.Count(booking => booking.Seat != null).Should().Be(2);
			seatBookingRequest.SeatBookings.Count(booking => booking.Seat.Name == seats[0].Name).Should().Be(2);
		}

		[Test]
		public void ShouldAllocateAccordingToPriority()
		{
			var bookingDate = DateOnly.Today;

			var agentShift1 = new SeatBooking(null, bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(17));
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var agentShift2 = new SeatBooking(null, bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(17));
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2);

			var seats = new List<Seat>()
			{
				new Seat("Seat1", 2),
				new Seat("Seat2", 1)
			};

			new SeatLevelAllocator(seats).AllocateSeats(seatBookingRequest1, seatBookingRequest2);
			Assert.That(agentShift2.Seat.Name == "Seat1");
			Assert.That(agentShift1.Seat.Name == "Seat2");
		}


		[Test]
		public void ShouldAllocateAccordingToStartTime()
		{
			var bookingDate = DateOnly.Today;

			var agentShift1 = new SeatBooking(null, bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(17));
			var agentShift2 = new SeatBooking(null, bookingDate, bookingDate.Date.AddHours(7), bookingDate.Date.AddHours(16));
			var seatBookingRequest = new SeatBookingRequest(agentShift1, agentShift2);

			var seats = new List<Seat>()
			{
				new Seat("Seat1", 1)
				
			};

			new SeatLevelAllocator(seats).AllocateSeats(seatBookingRequest);
			Assert.That(agentShift2.Seat.Name == "Seat1");

		}


		[Test]
		public void ShouldAllocateToGroupFirst()
		{
			var bookingDate = DateOnly.Today;

			var agentShift1 = new SeatBooking(null, bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(17));
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var agentShift2 = new SeatBooking(null, bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(17));
			var agentShift3 = new SeatBooking(null, bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(17));
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2, agentShift3);

			var seats = new List<Seat>()
			{
				new Seat("Seat1", 1),
				new Seat("Seat2", 2)
			};

			new SeatLevelAllocator(seats).AllocateSeats(seatBookingRequest1, seatBookingRequest2);
			Assert.That(agentShift2.Seat.Name == "Seat1");
			Assert.That(agentShift3.Seat.Name == "Seat2");
			Assert.That(agentShift1.Seat == null);

		}

		[Test]
		public void ShouldNotAllocateAnAgentToAnAlreadyBookedSeat()
		{
			var bookingDate = new DateOnly(2014, 01, 01);
			var booking = new SeatBooking(null, bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(17));
			var seatBookingRequest = new SeatBookingRequest(booking);

			var seat = new Seat("Seat1", 1);
			seat.AddSeatBooking(new SeatBooking(new Person(), bookingDate, bookingDate.Date, bookingDate.Date.AddHours(9)));
			var seats = new List<Seat> { seat };

			new SeatLevelAllocator(seats).AllocateSeats(seatBookingRequest);
			Assert.That(booking.Seat == null);

		}

		[Test]
		public void ShouldAllocateToAvailableSeat()
		{
			var bookingDate = DateOnly.Today;

			var agentShift1 = new SeatBooking(null, bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(12));
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var agentShift2 = new SeatBooking(null, bookingDate, bookingDate.Date.AddHours(13), bookingDate.Date.AddHours(17));
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2);

			var seat1 = new Seat("Seat1", 1);
			seat1.AddSeatBooking(new SeatBooking(new Person(), bookingDate, bookingDate.Date, bookingDate.Date.AddHours(10)));
			var seat2 = new Seat("Seat2", 2);
			seat2.AddSeatBooking(new SeatBooking(new Person(), bookingDate, bookingDate.Date.AddHours(15), bookingDate.Date.AddHours(20)));

			var seats = new List<Seat>() { seat1, seat2 };

			new SeatLevelAllocator(seats).AllocateSeats(seatBookingRequest1, seatBookingRequest2);
			Assert.That(agentShift2.Seat.Name == "Seat1");
			Assert.That(agentShift1.Seat.Name == "Seat2");

		}

	}
}