using System;
using System.Globalization;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common
{
	public class DaylightSavingsTimeAdjustmentViewModel
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double AdjustmentOffsetInMinutes { get; set; }
		public int LocalDSTStartTimeInMinutes { get; set; }
		public bool EnteringDST { get; set; }

		public DaylightSavingsTimeAdjustmentViewModel(DaylightTime daylightTime)
		{
			StartDateTime = daylightTime.Start;
			EndDateTime = daylightTime.End;
			AdjustmentOffsetInMinutes = daylightTime.Delta.TotalMinutes;
		}
	}
}