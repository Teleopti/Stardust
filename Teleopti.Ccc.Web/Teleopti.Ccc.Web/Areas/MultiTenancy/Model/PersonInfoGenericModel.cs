using System;
using Newtonsoft.Json;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Model
{
	public class PersonInfoGenericModel
	{
		[JsonProperty(PropertyName = "personId")]
		public Guid PersonId { get; set; }
		[JsonProperty(PropertyName = "message")]
		public string Message { get; set; }
	}
}