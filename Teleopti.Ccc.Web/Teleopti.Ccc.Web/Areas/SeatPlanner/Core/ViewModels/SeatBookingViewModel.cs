using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels
{
	public class SeatBookingViewModel
	{
		public String FirstName { get; set; }
		public String LastName{ get; set; }
		public String LocationName { get; set; }
		public String LocationPath { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public Guid PersonId { get; set; }
		public DateOnly BelongsToDate { get; set; }
		public Guid LocationId { get; set; }
		public Guid SiteId { get; set; }
		public String SiteName { get; set; }
		public Guid SeatId { get; set; }
		public String SeatName { get; set; }
		public string LocationPrefix { get; set; }
		public string LocationSuffix { get; set; }
		public int SeatPriority { get; set; }
		public bool IsDayOff { get; set; }
		public bool IsFullDayAbsence { get; set; }
	}
}