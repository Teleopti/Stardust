using System;
using Teleopti.Ccc.Web.Areas.Messages.Models;
using Teleopti.Interfaces.Domain;

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
		public String SeatName { get; set; }
		public int SeatPriority { get; set; }
		public Guid PersonId { get; set; }
		public DateOnly BelongsToDate { get; set; }
		public Guid LocationId { get; set; }
		public Guid SeatId { get; set; }
	}
}