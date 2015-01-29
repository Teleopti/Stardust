using System;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleDayViewModel
	{
		public DateTime Date { get; set; }
		public Guid Person { get; set; }
		public DateTime? StarTime { get; set; }
		public DateTime? EndTime { get; set; }
		public bool IsDayOff { get; set; }
	}
}