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
		private DateOnly _belongsToDate;
		private IPerson _person;

		protected SeatBooking ()
		{
		}
		
		public SeatBooking(IPerson person, DateOnly belongsToDate, DateTime startDateTime, DateTime endDateTime)
		{
			_startDateTime = startDateTime;
			_endDateTime = endDateTime;
			_belongsToDate = belongsToDate;
			_person = person;
		}
		public virtual ISeat Seat { get; set; }

		public virtual IPerson Person
		{
			get { return _person; }
			set { _person = value; }
		}

		public virtual DateOnly BelongsToDate
		{
			get { return _belongsToDate; }
			set { _belongsToDate = value; }
		}

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


		public virtual void RemoveBooking()
		{
			if (Seat != null)
			{
				Seat.RemoveSeatBooking (this);
				Seat = null;
			}		
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
