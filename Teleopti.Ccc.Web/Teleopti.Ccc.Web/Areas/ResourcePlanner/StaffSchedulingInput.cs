using System;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class StaffSchedulingInput
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}

	public class AgentGroupStaffSchedulingInput : StaffSchedulingInput
	{
		public Guid AgentGroupId { get; set; }
	}
}