using System;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	public class ShiftExchangeOfferFields
	{
		public DateTime OfferEndDate { get; set; }

		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public bool EndTimeNextDay { get; set; }
	}
}