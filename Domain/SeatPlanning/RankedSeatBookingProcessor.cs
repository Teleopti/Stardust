using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class RankedSeatBookingProcessor
	{
		public static void ProcessBookings(List<LocationSeatBookingScore> rankedScores)
		{
			var bestRankedGroupScore = rankedScores.FirstOrDefault();
			
			while (bestRankedGroupScore != null)
			{
				var topScore = bestRankedGroupScore.ScoreList.FirstOrDefault();

				if (topScore != null)
				{
					processScore(topScore, rankedScores, bestRankedGroupScore);
				}

				bestRankedGroupScore = rankedScores.FirstOrDefault();
			}
		}

		private static void processScore(SeatByIndexScore topScore, List<LocationSeatBookingScore> scoresRankedFromBestToWorst, LocationSeatBookingScore bestRankedGroupScoreForLocation)
		{
			var canAllocateAllBookings = !topScore.TransientSeatBookingsForASeat.Any(transientSeatBookingInfo => transientSeatBookingInfo.AlmostAllocatedBookings.Any (booking => transientSeatBookingInfo.Seat.IsAllocated (booking)));
			if (canAllocateAllBookings)
			{
				foreach (var transientSeatBookingInfo in topScore.TransientSeatBookingsForASeat)
				{
					var seat = transientSeatBookingInfo.Seat;
					var almostAllocatedBookings = transientSeatBookingInfo.AlmostAllocatedBookings;
					almostAllocatedBookings.ForEach (booking => booking.Book (seat));
				}

				// remove all other potential bookings for this group....
				scoresRankedFromBestToWorst.RemoveAll (groupScore => groupScore.GroupId == bestRankedGroupScoreForLocation.GroupId);
			}
			else
			{
				removeUnsuccessfulScore(topScore, scoresRankedFromBestToWorst, bestRankedGroupScoreForLocation);
				scoresRankedFromBestToWorst.Sort();
			}
		}

		private static void removeUnsuccessfulScore (SeatByIndexScore topScore, List<LocationSeatBookingScore> scoresRankedFromBestToWorst, LocationSeatBookingScore bestRankedGroupScoreForLocation)
		{
			bestRankedGroupScoreForLocation.ScoreList.Remove (topScore);

			if (!bestRankedGroupScoreForLocation.ScoreList.Any())
			{
				scoresRankedFromBestToWorst.Remove (bestRankedGroupScoreForLocation);
			}
		}
	}
}