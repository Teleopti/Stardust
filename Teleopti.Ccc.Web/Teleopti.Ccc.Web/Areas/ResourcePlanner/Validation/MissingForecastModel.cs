using System;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public class MissingForecastModel
	{
		public string SkillName { get; set; }
		public MissingForecastRange[] MissingRanges { get; set; }
		public Guid SkillId { get; set; }
	}
}