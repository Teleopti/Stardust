using System;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class MissingForecastModel
	{
		public string SkillName { get; set; }
		public MissingForecastRange[] MissingRanges { get; set; }
	}
}