using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftExchangeOfferForm
	{
		public ShiftExchangeOfferForm()
		{
			WishShiftType = ShiftExchangeLookingForDay.WorkingShift;
		}

		public DateTime Date { get; set; }
		public TimeSpan? StartTime { get; set; }
		public TimeSpan? EndTime { get; set; }
		public DateTime OfferValidTo { get; set; }
		public bool EndTimeNextDay { get; set; }
		public Guid? Id { get; set; }
		public ShiftExchangeLookingForDay WishShiftType { get; set; }
	}
}