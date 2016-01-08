using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class LocationSeatBookingScore : IComparable<LocationSeatBookingScore>
	{
		public ISeatMapLocation Location { get; set; }
		public Guid GroupId { get; set; }
		public List<SeatByIndexScore> ScoreList { get; set; }
		public int CompareTo(LocationSeatBookingScore other)
		{
			return new sortRoleFrequencyPriorityComparer().Compare(this, other);
		}
		
		private class sortRoleFrequencyPriorityComparer : Comparer <LocationSeatBookingScore>
		{
			public override int Compare(LocationSeatBookingScore locationScore1, LocationSeatBookingScore locationScore2)
			{		
				var locationScore1MaxRoleMatches = locationScore1.ScoreList.Max(score => score.TotalRoleMatches);
				var locationScore2MaxRoleMatches = locationScore2.ScoreList.Max(score => score.TotalRoleMatches);

				var comparisonOfTotalRoleMatches = locationScore1MaxRoleMatches.CompareTo (locationScore2MaxRoleMatches);
				if (comparisonOfTotalRoleMatches != 0)
					return -comparisonOfTotalRoleMatches;
				
				var locationScore1MaxFrequency = locationScore1.ScoreList.Max(score => score.TotalFrequency);
				var locationScore2MaxFrequency = locationScore2.ScoreList.Max(score => score.TotalFrequency);
				var comparisonOfMaxFrequency = locationScore1MaxFrequency.CompareTo (locationScore2MaxFrequency);
				if (comparisonOfMaxFrequency != 0)
					return -comparisonOfMaxFrequency;

				var comparisonSeatCount = locationScore1.Location.SeatCount.CompareTo (locationScore2.Location.SeatCount);
				if (comparisonSeatCount != 0)
					return -comparisonSeatCount;

				var locationScore1HighestPriority = locationScore1.ScoreList.Min(score => score.HighestPriority);
				var locationScore2HighestPriority = locationScore2.ScoreList.Min(score => score.HighestPriority);
				var comparisonPriority = locationScore1HighestPriority.CompareTo (locationScore2HighestPriority);
				if (comparisonPriority != 0)
					return comparisonPriority;
				
				return 0;
			}
		}
	}
}