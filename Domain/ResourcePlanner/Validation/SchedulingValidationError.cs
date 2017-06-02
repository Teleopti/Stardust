using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class SchedulingValidationError
	{
		public Guid ResourceId { get; set; }
		public string ResourceName { get; set; }
		public ICollection<string> ValidationErrors { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public ValidationResourceType ResourceType { get; set; }
		[JsonIgnore]
		public ICollection<Type> ValidationTypes { get; set; }
	}

	public enum ValidationResourceType
	{
		Agent,
		Skill
	}
}