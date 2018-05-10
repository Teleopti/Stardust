using System;
using System.Collections.Generic;

namespace StaffHubPoC.Models
{
	public class Break
	{
		public int duration { get; set; }
		public string breakType { get; set; }
	}

	public class LastModifiedBy
	{
		public string id { get; set; }
		public DateTime at { get; set; }
	}

	public class Shift
	{
		public string memberId { get; set; }
		public string id { get; set; }
		public string title { get; set; }
		public string shiftType { get; set; }
		public DateTime startTime { get; set; }
		public DateTime endTime { get; set; }
		public string notes { get; set; }
		public string state { get; set; }
		public string eTag { get; set; }
		public List<Break> breaks { get; set; }
		public List<string> groupIds { get; set; }
		public bool isPublished { get; set; }
		public object sharedChanges { get; set; }
		public LastModifiedBy lastModifiedBy { get; set; }
	}

	public class ShiftRoot
	{
		public Shift shift { get; set; }
		public bool isShiftPublished { get; set; }
	}

	public class ShiftCollection
	{
		public List<Shift> Shifts { get; set; }
		public List<Shift> PublishedShifts { get; set; }
		public List<Shift> UnpublishedShifts { get; set; }
	}
}
