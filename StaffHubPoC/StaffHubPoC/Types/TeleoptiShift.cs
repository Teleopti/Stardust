using System;
using System.Collections.Generic;
using StaffHubPoC.Models;

namespace StaffHubPoC.Types
{
	public class TeleoptiShift
	{
		public string Email { get; set; }
		public string Label { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public List<Break> Breaks { get; set; }
		public bool Working { get; set; }

		public Shift ConvertToStaffHubShift(Member member)
		{
			return new Shift
			{
				memberId = member.id,
				title = Label,
				startTime = StartTime,
				endTime = EndTime,
				breaks = Breaks,
				shiftType = Working ? ShiftType.Working : ShiftType.Absence,
				notes = "Imported from TeleoptiWFM",
				groupIds = member.groupIds,
				isPublished = true
			};
		}
	}
}
