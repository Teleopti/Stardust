using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class AgentShift
	{
		private readonly BookingPeriod _period;

		public AgentShift(BookingPeriod period, IPerson person, IScheduleDay scheduleDay)
		{
			_period = period;
			ScheduleDay = scheduleDay;
			Person = person;
		}

		public Guid Id { get; set; }
		public Seat Seat { get; set; }

		public IPerson Person { get; set; }
		public IScheduleDay ScheduleDay { get; set; }

		public BookingPeriod Period
		{
			get { return _period; }
		}

		public void Book(Seat seat)
		{
			Seat = seat;
			seat.Book(Period);
		}


	}
}
