using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatLevelAllocator
	{
		private readonly Dictionary<Guid, List<ISeatOccupancyFrequency>> _seatFrequencies;
		private readonly IList<ISeat> _seats;

		public SeatLevelAllocator(IEnumerable<ISeat> seats)
		{
			_seats = seats.OrderBy(seat => seat.Priority).ToList();
		}

		public SeatLevelAllocator(IEnumerable<ISeat> seats, Dictionary<Guid, List<ISeatOccupancyFrequency>> seatFrequencies)
		{
			_seatFrequencies = seatFrequencies;
			_seats = seats.OrderBy(seat => seat.Priority).ToList();
		}

		public void AllocateSeats(params SeatBookingRequest[] seatBookingRequests)
		{
			var sortedSeatBookingRequests = seatBookingRequests.ToList(); 
			sortedSeatBookingRequests.Sort();

			var seatScores = SeatScorer.GetSeatScores(sortedSeatBookingRequests, _seats, _seatFrequencies);

			bookSeatsByGroup(sortedSeatBookingRequests, seatScores);
			bookSeatsThatHaventBeenAllocatedByGroup(sortedSeatBookingRequests, seatScores);
		}

		private void bookSeatsByGroup(IList<SeatBookingRequest> sortedSeatBookingRequests, List<SeatScore> seatScores)
		{
			var rankedScores = GroupSeatBookingRanker.GetRankedScoresForGroupBySeats(_seats, sortedSeatBookingRequests, seatScores).ToList();
			RankedSeatBookingProcessor.ProcessBookings(rankedScores);
		}

		private void bookSeatsThatHaventBeenAllocatedByGroup(IEnumerable<SeatBookingRequest> sortedSeatBookingRequests, List<SeatScore> seatScores)
		{
			var unallocatedBookings = sortedSeatBookingRequests
				.SelectMany(s => s.SeatBookings)
				.Where(s => s.Seat == null)
				.OrderBy(s => s.StartDateTime).ToArray();

			if (!unallocatedBookings.Any()) return;

			foreach (var booking in unallocatedBookings)
			{
				bookSeatIfAvailable(booking, seatScores);
			}
		}
		
		private void bookSeatIfAvailable(ISeatBooking seatBooking, List<SeatScore> seatScores)
		{
			var firstApplicableSeat = SeatBookingSeatFinder.TryToFindASeatForBooking(seatBooking, _seats, seatScores);
			if (firstApplicableSeat != null)
			{
				seatBooking.Book(firstApplicableSeat);
			}
		}
	}
}