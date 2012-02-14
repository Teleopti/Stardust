using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Core
{
	public static class ModelStateExtensions
	{
		public static JsonResult ToJson(this ModelStateDictionary instance)
		{
			var errors = from v in instance.Values
			             from e in v.Errors
			             select e.ErrorMessage;
			var data = new ModelStateResult { Errors = errors.ToArray() };
			return new JsonResult
			       	{
			       		Data = data, 
			       		JsonRequestBehavior = JsonRequestBehavior.AllowGet
			       	};
		}

	}

	public class ModelStateResult
	{
		public IEnumerable<string> Errors { get; set; }
	}

}