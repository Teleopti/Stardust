using System;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GroupScheduleProjectionViewModel
	{
		/// <summary>
		/// Id of the person absence current projection mapped from, will be used in client side for remove absence.
		/// </summary>
		public Guid? ParentPersonAbsence { get; set; }
		public string Color { get; set; }
		public string Description { get; set; }
		public string Start { get; set; }
		public int Minutes { get; set; }
		public bool IsOvertime { get; set; }
	}
}