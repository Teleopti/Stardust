using System;
using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class HttpPostOrPutAttribute : ActionMethodSelectorAttribute
	{
		private static readonly AcceptVerbsAttribute _innerPostAttribute = new AcceptVerbsAttribute(HttpVerbs.Post);
		private static readonly AcceptVerbsAttribute _innerPutAttribute = new AcceptVerbsAttribute(HttpVerbs.Put);

		public override bool IsValidForRequest(ControllerContext controllerContext, System.Reflection.MethodInfo methodInfo)
		{
			return _innerPutAttribute.IsValidForRequest(controllerContext, methodInfo)
			       || _innerPostAttribute.IsValidForRequest(controllerContext, methodInfo);
		}
	}
}