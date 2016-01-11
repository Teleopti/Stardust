using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class LocationSeatBookingScore : IComparable<LocationSeatBookingScore>
	{
		public ISeatMapLocation Location { get; set; }
		public int RequestOrder { get; set; }
		public Guid GroupId { get; set; }
		public int GroupSize { get; set; }
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

				var comparisonGroupSize = locationScore1.GroupSize.CompareTo (locationScore2.GroupSize);
				if (comparisonGroupSize != 0)
					return -comparisonGroupSize;
				
				var locationScore1HighestPriority = locationScore1.ScoreList.Min(score => score.HighestPriority);
				var locationScore2HighestPriority = locationScore2.ScoreList.Min(score => score.HighestPriority);
				var comparisonPriority = locationScore1HighestPriority.CompareTo (locationScore2HighestPriority);
				if (comparisonPriority != 0)
					return comparisonPriority;

				var locationScore1EarliestStartTime = locationScore1.ScoreList.Min(score => score.EarliestStartTime);
				var locationScore2EarliestStartTime = locationScore2.ScoreList.Min(score => score.EarliestStartTime);
				var comparisonStartTime = locationScore1EarliestStartTime.CompareTo(locationScore2EarliestStartTime);
				if (comparisonStartTime != 0)
					return comparisonStartTime;


				var comparisonAddedRequestOrder = locationScore1.RequestOrder.CompareTo (locationScore2.RequestOrder);
				if (comparisonAddedRequestOrder != 0)
					return comparisonAddedRequestOrder;


				return 0;
			}
		}
	}
}