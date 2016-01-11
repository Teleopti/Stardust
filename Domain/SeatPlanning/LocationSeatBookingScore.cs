using System;
using System.Collections.Generic;
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
			return new LocationSeatBookingScoreComparer().Compare(this, other);
		}
	}
}