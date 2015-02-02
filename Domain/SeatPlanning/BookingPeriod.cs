using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public struct BookingPeriod
	{
		private DateTime _start;
		private DateTime _end;

		public BookingPeriod(DateTime start, DateTime end)
		{
			_start = start;
			_end = end;
		}

		public DateTime Start
		{
			get { return _start; }
		}

		public DateTime End
		{
			get { return _end; }
		}

		public bool Intersects(BookingPeriod period)
		{
			return !((period.End < Start) || (period.Start > End));
		}

		public override string ToString()
		{
			return Start.ToShortTimeString() + " - " + End.ToShortTimeString();
		}
	}
}
