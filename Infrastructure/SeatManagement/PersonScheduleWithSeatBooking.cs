using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.SeatManagement
{
	public class PersonScheduleWithSeatBooking : IPersonScheduleWithSeatBooking
	{
		private DateTime _belongsToDateTime;
		public DateTime PersonScheduleStart { get; set; }
		public DateTime PersonScheduleEnd { get; set; }
		public string PersonScheduleModelSerialized { get; set; }
		public DateTime? SeatBookingStart { get; set; }
		public DateTime? SeatBookingEnd { get; set; }
		public DateOnly BelongsToDate { get; set; }
		public Guid SeatId { get; set; }
		public String SeatName { get; set; }
		public Guid PersonId { get; set; }
		public String FirstName { get; set; }
		public String LastName { get; set; }
		public Guid LocationId { get; set; }
		public String LocationName { get; set; }
		public String LocationPrefix { get; set; }
		public String LocationSuffix { get; set; }
		public Guid TeamId { get; set; }
		public String TeamName { get; set; }
		public Guid SiteId { get; set; }
		public String SiteName { get; set; }
		public int NumberOfRecords { get; set; }
		public bool IsDayOff { get; set; }
		public DateTime BelongsToDateTime
		{
			get { return _belongsToDateTime; }
			set
			{
				_belongsToDateTime = value;
				BelongsToDate = new DateOnly(value);
			}
		}

	}
}