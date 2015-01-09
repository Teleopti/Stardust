using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftExchangeOfferForm
	{
		public DateTime Date { get; set; }
		public TimeSpan? StartTime { get; set; }
		public TimeSpan? EndTime { get; set; }
		public DateTime OfferValidTo { get; set; }
		public bool EndTimeNextDay { get; set; }
		public Guid? Id { get; set; }
	}
}