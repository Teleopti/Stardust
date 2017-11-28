using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class SchedulingHintError
	{
		public Guid ResourceId { get; set; }
		public string ResourceName { get; set; }
		[JsonIgnore]
		public ICollection<Type> ValidationTypes { get; set; }
		public ICollection<ValidationError> ValidationErrors { get; set; }
	}

	public class ValidationError
	{
		public string ErrorResource { get; set; }
		public List<object> ErrorResourceData { get; set; }
		public string ErrorMessageLocalized { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public ValidationResourceType ResourceType { get; set; }
	}

	public enum ValidationResourceType
	{
		BlockScheduling,
		Basic,
		Skill
	}
}