using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeRequestsPreparationViewModel
	{
		public bool HasWorkflowControlSet { get; set; }

		public IEnumerable<ShiftTradeScheduleLayer> MySchedulelayers { get; set; }

		public int TimeLineLengthInMinutes
		{
			get
			{
				// Temporary. Later this will prob be done in mapping and calculated from both my schedule and all other schedules.
				return MySchedulelayers.Sum(layer => layer.LengthInMinutes);
			}
		}
	}
}