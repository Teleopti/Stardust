using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	class SeatScorer
	{

		public static List<SeatScore> GetSeatScores(IEnumerable<SeatBookingRequest> sortedSeatBookingRequests, List<SeatMapLocation> allLocationsUnsorted, Dictionary<Guid, List<ISeatOccupancyFrequency>> seatFrequencies)
		{
			var seatScores = new List<SeatScore>();

			var scoresForGroupSeatBookings = from seatBookingRequest in sortedSeatBookingRequests
											 from seatMapLocation in allLocationsUnsorted
											 where seatMapLocation.IncludeInSeatPlan
											 let seatArray = seatMapLocation.Seats.OrderBy(seat => seat.Priority).ToList()
											 let groupSeatBookings = seatBookingRequest.SeatBookings.OrderBy(booking => booking.StartDateTime)
											 select groupSeatBookings.SelectMany(seatBooking => getSeatScores(seatBooking, seatArray, seatFrequencies)).ToList();

			foreach (var scoresForGroup in scoresForGroupSeatBookings)
			{
				seatScores.AddRange(scoresForGroup);
			}

			return seatScores;
		}
		
		public static List<SeatScore> GetSeatScores(IEnumerable<SeatBookingRequest> sortedSeatBookingRequests, IList<ISeat> seats, Dictionary<Guid, List<ISeatOccupancyFrequency>> seatFrequencies)
		{
			var seatScores = new List<SeatScore>();

			var scoresForGroupSeatBookings = from seatBookingRequest in sortedSeatBookingRequests
											 let seatArray = seats.OrderBy(seat => seat.Priority).ToList()
											 let groupSeatBookings = seatBookingRequest.SeatBookings.OrderBy(booking => booking.StartDateTime)
											 select groupSeatBookings.SelectMany(seatBooking => getSeatScores(seatBooking, seatArray, seatFrequencies)).ToList();

			foreach (var scoresForGroup in scoresForGroupSeatBookings)
			{
				seatScores.AddRange(scoresForGroup);
			}

			return seatScores;
		}
		
		private static IEnumerable<ISeatOccupancyFrequency> getSeatFrequenciesForPerson(ISeatBooking seatBooking, IReadOnlyDictionary<Guid, List<ISeatOccupancyFrequency>> seatFrequencies)
		{
			List<ISeatOccupancyFrequency> seatFrequenciesForPerson;

			if (seatFrequencies == null ||
				!seatFrequencies.TryGetValue(seatBooking.Person.Id.GetValueOrDefault(), out seatFrequenciesForPerson))
			{
				seatFrequenciesForPerson = new List<ISeatOccupancyFrequency>();
			}
			return seatFrequenciesForPerson;
		}

		private static IEnumerable<SeatScore> getSeatScores(ISeatBooking seatBooking, IEnumerable<ISeat> seats, IReadOnlyDictionary<Guid, List<ISeatOccupancyFrequency>> seatFrequencies)
		{
			var seatFrequenciesForPerson = getSeatFrequenciesForPerson(seatBooking, seatFrequencies);

			return from seat in seats
				   let personApplicationRoles = seatBooking.Person.PermissionInformation.ApplicationRoleCollection
				   let personRoleAndSeatRoleIntersectionCount = seat.Roles.Intersect(personApplicationRoles).Count()
				   let seatFrequency = seatFrequenciesForPerson.SingleOrDefault(seatFreq => seatFreq.Seat == seat)
				   let seatFrequencyValue = seatFrequency != null ? seatFrequency.Frequency : 0
				   where seat.Roles.Count == personRoleAndSeatRoleIntersectionCount
				   orderby personRoleAndSeatRoleIntersectionCount descending,
					   seatFrequencyValue descending,
					   seat.Priority ascending 
				   select new SeatScore()
				   {
					   Seat = seat,
					   Location = seat.Parent as SeatMapLocation,
					   SeatBooking = seatBooking,
					   RoleMatches = personRoleAndSeatRoleIntersectionCount,
					   Frequency = seatFrequencyValue
				   };
		}


	}
}
