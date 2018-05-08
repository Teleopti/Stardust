using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Model
{
	public class BaseResultModel
	{
		public BaseResultModel()
		{
			Result = new List<object>();
			Errors = new List<object>();
		}

		[JsonProperty(PropertyName = "success")]
		public bool Success => !Errors.Any();

		[JsonProperty(PropertyName = "result")]
		public List<object> Result { get; set; }

		[JsonProperty(PropertyName = "errors")]
		public List<object> Errors { get; set; }
	}
}