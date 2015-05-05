using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public struct PlanningPeriodChangeRangeModel
	{
		public int Number { get; set; }
		public SchedulePeriodType PeriodType { get; set; }
		public DateTime DateFrom { get; set; }
	}
}