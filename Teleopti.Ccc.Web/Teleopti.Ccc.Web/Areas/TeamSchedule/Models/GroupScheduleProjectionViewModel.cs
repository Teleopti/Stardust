﻿using System;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class GroupScheduleProjectionViewModel
	{
		/// <summary>
		/// Id of the person absence current projection mapped from, will be used in client side for remove absence.
		/// </summary>
		public Guid[] ShiftLayerIds { get; set; }
		public Guid[] ParentPersonAbsences { get; set; }
		public string Color { get; set; }
		public string Description { get; set; }
		public string Start { get; set; }
		public string End { get; set; }
		public int Minutes { get; set; }
		public bool IsOvertime { get; set; }
		public Guid ActivityId { get; set; }
		public Guid? TopShiftLayerId { get; set; }
		public bool FloatOnTop { get; set; }
		public bool IsPersonalActivity { get; set; }
		public bool IsMeeting { get; set; }
	}
}