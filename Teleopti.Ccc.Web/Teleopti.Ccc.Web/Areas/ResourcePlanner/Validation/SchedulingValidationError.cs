using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public class SchedulingValidationError
	{
		public Guid ResourceId { get; set; }
		public string ResourceName { get; set; }
		public ICollection<string> ValidationErrors { get; set; }
		public ValidationResourceType ResourceType { get; set; }
	}

	public enum ValidationResourceType
	{
		Agent,
		Skill
	}
}