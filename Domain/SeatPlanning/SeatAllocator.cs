using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatAllocator
	{
		private readonly Dictionary<Guid, List<ISeatOccupancyFrequency>> _seatFrequencies;
		private readonly SeatMapLocation[] _seatMapLocations;

		public SeatAllocator(params SeatMapLocation[] seatMapLocations)
		{
			_seatMapLocations = seatMapLocations;
		}

		public SeatAllocator(Dictionary<Guid, List<ISeatOccupancyFrequency>> seatFrequencies, params SeatMapLocation[] seatMapLocations)
		{
			_seatFrequencies = seatFrequencies;
			_seatMapLocations = seatMapLocations;
		}

		public void AllocateSeats(params SeatBookingRequest[] seatBookingRequests)
		{
			var sortedSeatBookingRequests =  seatBookingRequests.ToList();
			sortedSeatBookingRequests.Sort();

			//ROBTODO: remove debug code
			var count = 0;
			foreach (var seatBookingRequest in sortedSeatBookingRequests)
			{
				count++;
				Console.WriteLine("Group " + count + " - Number of bookings " + seatBookingRequest.SeatBookings.Count());
				seatBookingRequest.SeatBookings.ForEach(seatBooking => Console.WriteLine(seatBooking.Person.Name));
			}

			var allLocationsUnsorted = getAllLocationsUnsorted();
			var seatScores = SeatScorer.GetSeatScores(sortedSeatBookingRequests, allLocationsUnsorted, _seatFrequencies);

			bookSeatsByGroup(sortedSeatBookingRequests, allLocationsUnsorted, seatScores);
			bookSeatsThatHaventBeenAllocatedByGroup(sortedSeatBookingRequests, allLocationsUnsorted, seatScores);
		}

		private List<SeatMapLocation> getAllLocationsUnsorted()
		{
			var allLocationsUnsorted = new List<SeatMapLocation>();
			foreach (var location in _seatMapLocations)
			{
				allLocationsUnsorted.AddRange(location.GetFullLocationHierarchyAsList());
			}
			return allLocationsUnsorted;
		}

		private void bookSeatsByGroup(IEnumerable<SeatBookingRequest> sortedSeatBookingRequests, List<SeatMapLocation> allLocationsUnsorted, List<SeatScore> seatScores)
		{
			var rankedScores = GroupSeatBookingRanker.GetRankedScoresForGroupsAndLocations(sortedSeatBookingRequests, allLocationsUnsorted, seatScores, _seatFrequencies).ToList();

			//ROBTODO: remove debug code
			Console.WriteLine ("Ranked Scores for Groups and Locations");
			foreach (var rankedScore in rankedScores)
			{
			
					Console.WriteLine(" -> " + rankedScore.Location.Name );
				foreach (var transientSeatBooking in rankedScore.ScoreList.First().TransientSeatBookingsForASeat)
				{
					Console.WriteLine(" ->   " + transientSeatBooking.AlmostAllocatedBookings.First().Person.Name);
				}
			}
			
			RankedSeatBookingProcessor.ProcessBookings(rankedScores);
		}

		private void bookSeatsThatHaventBeenAllocatedByGroup(IEnumerable<SeatBookingRequest> sortedSeatBookingRequests, List<SeatMapLocation> allLocationsUnsorted, List<SeatScore> seatScores)
		{
			var unallocatedBookings = sortedSeatBookingRequests
				.SelectMany(s => s.SeatBookings)
				.Where(s => s.Seat == null)
				.OrderBy(s => s.StartDateTime).ToArray();

			if (!unallocatedBookings.Any()) return;

			Console.WriteLine("There were " + unallocatedBookings.Count() + " Unallocated Bookings after booking groups");
			
			foreach (var booking in unallocatedBookings)
			{
				findAndBookSeatForLocation(allLocationsUnsorted, booking, seatScores);
			}
		}

		private void findAndBookSeatForLocation(IList<SeatMapLocation> allLocationsUnsorted, ISeatBooking seatBooking, List<SeatScore> seatScores)
		{
			var locationForBooking = SeatBookingLocationFinder.GetLocationForBooking(allLocationsUnsorted, seatBooking, seatScores);
			if (locationForBooking != null)
			{
				bookSeatForLocation(locationForBooking, seatBooking, seatScores);
			}
		}

		private void bookSeatForLocation(ISeatMapLocation targetSeatMapLocation, ISeatBooking booking, List<SeatScore> seatScores)
		{
			var firstUnallocatedSeat = SeatBookingSeatFinder.TryToFindASeatForBooking(booking, targetSeatMapLocation.Seats, seatScores);
			if (firstUnallocatedSeat != null)
			{
				booking.Book(firstUnallocatedSeat);
			}
		}


	}
}