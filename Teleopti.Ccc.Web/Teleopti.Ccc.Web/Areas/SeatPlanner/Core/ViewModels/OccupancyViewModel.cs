using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels
{
	public class OccupancyViewModel
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public Guid PersonId { get; set; }
		public String FirstName { get; set; }
		public String LastName{ get; set; }
		public Guid SeatId { get; set; }
		public Guid BookingId { get; set; }
		public string SeatName { get; set; }
		public DateOnly BelongsToDate { get; set; }
		public string LocationPath { get; set; }
		public string LocationPrefix { get; set; }
		public string LocationSuffix { get; set; }
	}
}