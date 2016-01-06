using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public static class SeatBookingSeatFinder
	{
		public static ISeat TryToFindASeatForBookingCheckingTransientBookings(ISeatBooking seatBooking, IEnumerable<ISeat> seats, List<TransientSeatBooking> transientBookings, List<SeatScore> seatScores)
		{
			var foundSeats = getSeatsThatCanBeBookedRankedByScore(seatBooking, seats, transientBookings, seatScores);
			return foundSeats.FirstOrDefault();
		}

		private static IEnumerable<ISeat> getSeatsThatCanBeBookedRankedByScore(ISeatBooking seatBooking, IEnumerable<ISeat> seats, IEnumerable<TransientSeatBooking> transientBookings, IEnumerable<SeatScore> seatScores)
		{
			var seatsByPersonScore = getSeatsByBookingScore(seatBooking, seats, seatScores);

			var foundSeats = from seat in seatsByPersonScore
							 let transientSeatBooking = transientBookings.SingleOrDefault(booking => booking.Seat == seat)
							 where transientSeatBooking != null &&
								   !transientSeatBooking.IsAllocated(seatBooking) 
							 select seat;
			return foundSeats;
		}

		public static ISeat TryToFindASeatForBooking(ISeatBooking seatBooking, IEnumerable<ISeat> seats, List<SeatScore> seatScores)
		{
			var seatsByPersonScore = getSeatsByBookingScore(seatBooking, seats, seatScores);
			return seatsByPersonScore.FirstOrDefault((seat) => !seat.IsAllocated(seatBooking));
		}

		private static IEnumerable<ISeat> getSeatsByBookingScore (ISeatBooking seatBooking, IEnumerable<ISeat> seats, IEnumerable<SeatScore> seatScores)
		{
			return from score in seatScores
				where score.SeatBooking == seatBooking && seats.Contains (score.Seat)
				select score.Seat;
		}
	}
}