using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeRequestsScheduleViewModel
	{
		//public IScheduleDay MyScheduleDay { get; set; }
		public IEnumerable<ShiftTradeScheduleLayer> MyScheduleLayers { get; set; }

		public int TimeLineLengthInMinutes
		{
			get
			{
				// Temporary. Later this will prob be done in mapping and calculated from both my schedule and all other schedules.
				return MyScheduleLayers.Sum(layer => layer.LengthInMinutes);
			}
		}
	}
}