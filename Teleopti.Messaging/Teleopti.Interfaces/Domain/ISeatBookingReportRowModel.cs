using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeatBookingReportModel
	{
		IEnumerable<IPersonScheduleWithSeatBooking> SeatBookings { get; set; }
		int RecordCount { get; set; }
	}

	public interface IPersonScheduleWithSeatBooking
	{

		DateTime PersonScheduleStart { get; set; }
		DateTime PersonScheduleEnd { get; set; }
		DateTime? SeatBookingStart { get; set; }
		DateTime? SeatBookingEnd { get; set; }
		DateOnly BelongsToDate { get; set; }
		DateTime BelongsToDateTime { get; set; }
		Guid SeatId { get; set; }
		String SeatName { get; set; }
		Guid PersonId { get; set; }
		String FirstName { get; set; }
		String LastName { get; set; }
		Guid LocationId { get; set; }
		String LocationName { get; set; }
		Guid TeamId { get; set; }
		String TeamName { get; set; }
		int NumberOfRecords { get; set; }
	}

	
}