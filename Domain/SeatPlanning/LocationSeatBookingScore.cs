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
		
		private class sortRoleFrequencyPriorityComparer : IComparer <LocationSeatBookingScore>
		{
			public int Compare(LocationSeatBookingScore locationScore1, LocationSeatBookingScore locationScore2)
			{		
				var locationScore1MaxRoleMatches = locationScore1.ScoreList.Max(score => score.TotalRoleMatches);
				var locationScore2MaxRoleMatches = locationScore2.ScoreList.Max(score => score.TotalRoleMatches);

				if (locationScore1MaxRoleMatches > locationScore2MaxRoleMatches)
					return -1;
				if (locationScore1MaxRoleMatches < locationScore2MaxRoleMatches)
					return 1;

				var locationScore1MaxFrequency = locationScore1.ScoreList.Max(score => score.TotalFrequency);
				var locationScore2MaxFrequency = locationScore2.ScoreList.Max(score => score.TotalFrequency);

				if (locationScore1MaxFrequency > locationScore2MaxFrequency)
					return -1;
				if (locationScore1MaxFrequency < locationScore2MaxFrequency)
					return 1;

				if (locationScore1.Location.SeatCount > locationScore2.Location.SeatCount)
					return -1;
				if (locationScore1.Location.SeatCount < locationScore2.Location.SeatCount)
					return 1;

				var locationScore1HighestPriority = locationScore1.ScoreList.Min(score => score.HighestPriority);
				var locationScore2HighestPriority = locationScore2.ScoreList.Min(score => score.HighestPriority);

				if (locationScore1HighestPriority > locationScore2HighestPriority)
					return 1;
				if (locationScore1HighestPriority < locationScore2HighestPriority)
					return -1;

				return 0;
			}
		}
	}
}