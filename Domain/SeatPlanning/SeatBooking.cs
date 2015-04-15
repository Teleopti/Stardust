using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	[Serializable]
	public class SeatBooking : NonversionedAggregateRootWithBusinessUnit, ISeatBooking
	{

		private DateTime _startDateTime;
		private DateTime _endDateTime;

		protected SeatBooking ()
		{
		}
		
		public SeatBooking(IPerson person, DateTime startDateTime, DateTime endDateTime)
		{
			StartDateTime = startDateTime;
			EndDateTime = endDateTime;
			Person = person;
		}
		public virtual ISeat Seat { get; set; }
		public virtual IPerson Person { get; set; }
		
		public virtual DateTime StartDateTime
		{
			get { return _startDateTime; }
			set { _startDateTime = value; }
		}

		public virtual  DateTime EndDateTime
		{
			get { return _endDateTime; }
			set { _endDateTime = value; }
		}

		public virtual void Book(ISeat seat)
		{
			Seat = seat;
			seat.AddSeatBooking(this);
		}
		
		public virtual bool Intersects(ISeatBooking booking)
		{
			return !((booking.EndDateTime < StartDateTime) || (booking.StartDateTime > EndDateTime));
		}

		public virtual bool Intersects (DateTimePeriod period)
		{
			return !((period.EndDateTime < StartDateTime) || (period.StartDateTime > EndDateTime));
		}
		
	}
}
