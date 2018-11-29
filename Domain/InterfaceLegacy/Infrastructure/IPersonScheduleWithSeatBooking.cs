using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IPersonScheduleWithSeatBooking
	{
		DateTime PersonScheduleStart { get; set; }
		DateTime PersonScheduleEnd { get; set; }
		string PersonScheduleModelSerialized { get; set; }
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
		String LocationPrefix { get; set; }
		String LocationSuffix { get; set; }
		Guid TeamId { get; set; }
		String TeamName { get; set; }
		Guid SiteId { get; set; }
		String SiteName { get; set; }
		bool IsDayOff { get; set; }
		int NumberOfRecords { get; set; }
	}
}