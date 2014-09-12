using System;
using System.ComponentModel.DataAnnotations;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public static class EnumerationExtensions
	{
		public static string GetDisplayName(this Enum value)
		{
			var type = value.GetType();
			var members = type.GetMember(value.ToString());
			var member = members[0];
			// attempt to get display attribute, throwing an exception if the display attribute does not exist.
			var attributes = member.GetCustomAttributes(typeof(DisplayAttribute), false);
			if (attributes.Length == 0)
			{
				throw new ArgumentException(String.Format("'{0}.{1}' doesn't have DisplayAttribute", type.Name, value));
			}

			var attribute = (DisplayAttribute)attributes[0];
			return attribute.GetName();
		}
	}
}