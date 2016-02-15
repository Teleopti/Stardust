using System;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GroupScheduleProjectionViewModel
	{
		public string Color { get; set; }
		public string Description { get; set; }
		public string Start { get; set; }
		public int Minutes { get; set; }
		public bool IsOvertime { get; set; }
	}
}