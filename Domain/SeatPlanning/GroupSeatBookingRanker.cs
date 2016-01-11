using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	class GroupSeatBookingRanker
	{
		public static IEnumerable<LocationSeatBookingScore> GetRankedScoresForGroupsAndLocations(IEnumerable<SeatBookingRequest> sortedSeatBookingRequests, List<SeatMapLocation> allLocationsUnsorted, List<SeatScore> seatScores, Dictionary<Guid, List<ISeatOccupancyFrequency>> seatFrequencies)
		{
			var scoresByGrouping = new List<LocationSeatBookingScore>();

			var orderSeatBookingRequestWasAdded = 0;
			foreach (var seatBookingRequest in sortedSeatBookingRequests)
			{
				orderSeatBookingRequestWasAdded ++;
				var seatScoresForCurrentRequest = seatScores.Where(seatScore => seatBookingRequest.SeatBookings.Contains(seatScore.SeatBooking)).ToList();
				var scoresForGroupByLocation = getScoresForGroupByLocation(allLocationsUnsorted, seatBookingRequest.SeatBookings, seatScoresForCurrentRequest, orderSeatBookingRequestWasAdded);
				scoresByGrouping.AddRange(scoresForGroupByLocation);
			}

			scoresByGrouping.Sort();

			return scoresByGrouping;
		}

		public static IList<LocationSeatBookingScore> GetRankedScoresForGroupBySeats(IList<ISeat> seats, IList<SeatBookingRequest> sortedSeatBookingRequests, List<SeatScore> seatScores)
		{
			if (!seats.Any())
			{
				return null;
			}

			var location = seats.First().Parent as SeatMapLocation;
			var locationSeatBookingScores = new List<LocationSeatBookingScore>();

			var orderSeatBookingRequestWasAdded = 0;

			foreach (var seatBookingRequest in sortedSeatBookingRequests)
			{
				orderSeatBookingRequestWasAdded ++;
				var groupId = Guid.NewGuid();
				var seatScoresForCurrentRequest = seatScores.Where(seatScore => seatBookingRequest.SeatBookings.Contains(seatScore.SeatBooking)).ToList();
				var scoreListByIndexForThisLocation = getScoreListByIndex(seatBookingRequest.SeatBookings, location, seatScoresForCurrentRequest, groupId, seats, orderSeatBookingRequestWasAdded);
				if (scoreListByIndexForThisLocation != null)
				{
					locationSeatBookingScores.Add(scoreListByIndexForThisLocation);
				}

			}

			locationSeatBookingScores.Sort();

			return locationSeatBookingScores;
		}


		public static List<SeatByIndexScore> TryToFindBestSeatsForGroup(IEnumerable<ISeatBooking> groupSeatBookings, IEnumerable<ISeat> possibleSeats, List<SeatScore> seatScoresForCurrentLocation)
		{
			// we have a teams worth of bookings.  We are trying to find the best seats in this location for the team.
			// we want to keep the teams bookings together, but choose the best position and rearrange their order to get the agents into
			// the best seats for them.

			var groupSeatBookingsInOrder = groupSeatBookings.OrderBy(booking => booking.StartDateTime).ToList();

			var seatArray = possibleSeats.OrderBy(seat => seat.Priority).ToList();
			var seatScoresByIndex = new List<SeatByIndexScore>();

			// handle next batch of seats at position...
			for (var startIdx = 0; startIdx <= seatArray.Count(); startIdx++)
			{
				// get the seats that can be used by the team at this position.
				var seats = seatArray.Skip(startIdx).Take(groupSeatBookingsInOrder.Count());
				if (seats.Any())
				{
					seatScoresByIndex.Add(getSeatBatchScore(seatScoresForCurrentLocation, seats, startIdx));
				}
			}
			
            var seatScoresInOrder = from score in seatScoresByIndex
				orderby score.TotalRoleMatches descending,
					score.TotalFrequency descending,
					score.TotalGroupNeighbourCount descending,
					score.FilledSeatCount,  // Billions Finance may not require this!
					score.HighestPriority
				where allBookingsHaveBeenAllocatedASeat(groupSeatBookingsInOrder, score)
				select score;

			return seatScoresInOrder.ToList();
		}

		private static IEnumerable<LocationSeatBookingScore> getScoresForGroupByLocation(IEnumerable<ISeatMapLocation> locations, IEnumerable<ISeatBooking> groupSeatBookings, List<SeatScore> seatScores, int orderSeatBookingRequestWasAdded)
		{
			var locationSeatBookingScores = new List<LocationSeatBookingScore>();
			var groupId = Guid.NewGuid();

			foreach (var location in locations.Where(location => location.IncludeInSeatPlan))
			{
				var seatScoresForCurrentLocation = seatScores.Where(seatScore => seatScore.Location == location).ToList();
				var scoreListByIndexForThisLocation = getScoreListByIndex(groupSeatBookings, location, seatScoresForCurrentLocation, groupId, location.Seats, orderSeatBookingRequestWasAdded);

				if (scoreListByIndexForThisLocation != null)
				{
					locationSeatBookingScores.Add(scoreListByIndexForThisLocation);
				}

			}

			return locationSeatBookingScores;
		}

		private static LocationSeatBookingScore getScoreListByIndex(IEnumerable<ISeatBooking> groupSeatBookings, ISeatMapLocation location, List<SeatScore> seatScoresForCurrentLocation, Guid groupId, IEnumerable<ISeat> seats, int orderSeatBookingRequestWasAdded)
		{
			var scoreListByIndexForThisLocation = TryToFindBestSeatsForGroup(groupSeatBookings, seats, seatScoresForCurrentLocation);
			if (scoreListByIndexForThisLocation.Any())
			{
				return new LocationSeatBookingScore()
				{
					Location = location,
					RequestOrder = orderSeatBookingRequestWasAdded,
					ScoreList = scoreListByIndexForThisLocation,
					GroupId = groupId,
					GroupSize = groupSeatBookings.Count()
				};
			}

			return null;
		}

		private static bool allBookingsHaveBeenAllocatedASeat(IEnumerable<ISeatBooking> groupSeatBookings, SeatByIndexScore score)
		{
			return score.TransientSeatBookingsForASeat.SelectMany(booking => booking.AlmostAllocatedBookings).Count() == groupSeatBookings.Count();
		}

		private static SeatByIndexScore getSeatBatchScore(IEnumerable<SeatScore> seatScores, IEnumerable<ISeat> seats, int startIdx)
		{
			var allScoresForChosenSeats = getAllScoresForChosenSeats(seatScores, seats);
			var transientSeatBookings = seats.Select(s => new TransientSeatBooking(s)).ToList();

			var seatByIndexScore = new SeatByIndexScore()
			{
				Index = startIdx,
				TransientSeatBookingsForASeat = transientSeatBookings,
				HighestPriority = seats.Min(seat => seat.Priority)
			};

			temporarilyAllocateSeatAndCalculateRoleMatchAndFrequencyScores(allScoresForChosenSeats, transientSeatBookings, seatByIndexScore);

			var transientSeatBookingsWithBookings = transientSeatBookings.Where(booking => booking.AlmostAllocatedBookings.Any()).ToArray();

			seatByIndexScore.FilledSeatCount = transientSeatBookingsWithBookings.Count();
			seatByIndexScore.TotalGroupNeighbourCount = calculateGroupNeighbourCount(transientSeatBookingsWithBookings);
			if (transientSeatBookingsWithBookings.Any())
			{
				seatByIndexScore.EarliestStartTime = 
					transientSeatBookingsWithBookings.Min(transientBooking => 
						transientBooking.AlmostAllocatedBookings.Min(booking => booking.StartDateTime));
			}
		
			return seatByIndexScore;
		}

		private static IEnumerable<SeatScore> getAllScoresForChosenSeats(IEnumerable<SeatScore> seatScores, IEnumerable<ISeat> seats)
		{
			return from score in seatScores
				orderby score.RoleMatches descending,
					score.Frequency descending,
					score.Seat.Priority
				where seats.Contains(score.Seat)
				select score;
		}

		private static int calculateGroupNeighbourCount(TransientSeatBooking[] transientSeatBookings)
		{
			if (!transientSeatBookings.Any())
			{
				return 0;
			}

			var totalGroupNeighbourCount = 0;
			var location = transientSeatBookings[0].Seat.Parent as SeatMapLocation;

			foreach (var transientSeatBooking in transientSeatBookings)
			{
				var numberOfBookingsOnThisSeat = transientSeatBooking.AlmostAllocatedBookings.Count;
				totalGroupNeighbourCount += numberOfBookingsOnThisSeat;

				var nextHighestPrioritySeat =
					location.Seats.Where (seat => seat.Priority > transientSeatBooking.Seat.Priority)
						.OrderBy (seat => seat.Priority)
						.FirstOrDefault();
				if (nextHighestPrioritySeat != null)
				{
					totalGroupNeighbourCount += transientSeatBookings.Count(booking => booking.Seat.Priority == nextHighestPrioritySeat.Priority) * numberOfBookingsOnThisSeat;
				}

				
			}
			return totalGroupNeighbourCount;
		}

		private static void temporarilyAllocateSeatAndCalculateRoleMatchAndFrequencyScores(IEnumerable<SeatScore> allScoresForChosenSeats, List<TransientSeatBooking> transientSeatBookings, SeatByIndexScore seatByIndexScore)
		{
			var totalRoleMatch = 0;
			var totalFrequency = 0;

			var bookingsAlreadyProcessed = new List<ISeatBooking>();

			foreach (var score in allScoresForChosenSeats.Where(score => !bookingsAlreadyProcessed.Contains(score.SeatBooking)))
			{
				var transientSeatBookingForSeat = transientSeatBookings.Single(booking => booking.Seat == score.Seat);

				if (transientSeatBookingForSeat.IsAllocated(score.SeatBooking))
				{
					continue;
				}

				transientSeatBookingForSeat.TemporarilyAllocate(score.SeatBooking);
				bookingsAlreadyProcessed.Add(score.SeatBooking);

				totalRoleMatch += score.RoleMatches;
				totalFrequency += score.Frequency;
			}

			seatByIndexScore.TotalRoleMatches = totalRoleMatch;
			seatByIndexScore.TotalFrequency = totalFrequency;
		}

	}
}