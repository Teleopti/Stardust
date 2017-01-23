using System;
using System.Web;
using System.Web.Routing;

namespace Teleopti.Ccc.Web.Areas.MyTime
{
	public class GuidConstraint : IRouteConstraint
	{
		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			object value;
			if (values.TryGetValue(parameterName, out value))
			{
				var stringValue = value as string;
				if (!string.IsNullOrEmpty(stringValue))
				{
					Guid guidValue;
					return Guid.TryParse(stringValue, out guidValue) && (guidValue != Guid.Empty);
				}
			}
			return false;
		}
	}
}