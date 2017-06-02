using System;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class MissingForecastModel
	{
		public string SkillName { get; set; }
		public MissingForecastRange[] MissingRanges { get; set; }
		public Guid SkillId { get; set; }
	}
}