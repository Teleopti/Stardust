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
			var canAllocateAllBookings = false;
			foreach (var transientSeatBookingInfo in topScore.TransientSeatBookingsForASeat)
			{
				var almostAllocatedBookings = transientSeatBookingInfo.AlmostAllocatedBookings;
				if (almostAllocatedBookings != null && !almostAllocatedBookings.Any(booking => transientSeatBookingInfo.Seat.IsAllocated(booking)))
				{
					canAllocateAllBookings = true;
				}
				else
				{
					canAllocateAllBookings = false;
					break;
				}
			}

			if (canAllocateAllBookings)
			{
				foreach (var transientSeatBookingInfo in topScore.TransientSeatBookingsForASeat)
				{
					var almostAllocatedBookings = transientSeatBookingInfo.AlmostAllocatedBookings;
					almostAllocatedBookings.ForEach (booking => booking.Book (transientSeatBookingInfo.Seat));
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

		private static void removeUnsuccessfulScore (SeatByIndexScore topScore, List<LocationSeatBookingScore> scoresRankedFromBestToWorst,LocationSeatBookingScore bestRankedGroupScoreForLocation)
		{
			bestRankedGroupScoreForLocation.ScoreList.Remove (topScore);

			if (!bestRankedGroupScoreForLocation.ScoreList.Any())
			{
				scoresRankedFromBestToWorst.Remove (bestRankedGroupScoreForLocation);
			}
		}
	}
}