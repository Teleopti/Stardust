using System;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public struct SuggestedPlanningPeriodRangeModel
	{
		public int Number { get; set; }
		public string PeriodType { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}