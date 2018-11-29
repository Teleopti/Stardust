using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings
{
	public static class PageHelper
	{
		public static Description CreateNewName<T>(IList<T> sourceList, string property, string defaultText)
		{

			int nextId = 0;
			if (sourceList != null && sourceList.Count > 0)
			{
				InParameter.NotNull("property", property);
				InParameter.NotNull("defaultText", defaultText);

				IEnumerable<string> queryResult = QueryByName(sourceList, property, defaultText);

				nextId = GetNextId(queryResult);
			}

			string name;
			if (nextId == 0)
			{
				name = string.Format(CultureInfo.InvariantCulture, "<{0}>", defaultText);
			}
			else
			{
				name = string.Format(CultureInfo.InvariantCulture, "<{0} {1}>", defaultText, nextId);
			}

			return new Description(name);
		}

		private static IEnumerable<string> QueryByName<T>(IEnumerable<T> sourceList, string property, string defaultText)
		{
			var propertyReflector = new PropertyReflector();

			var r = sourceList.Select(s => (string)propertyReflector.GetValue(s, property));
			return r.Select(s => s.Replace("<", string.Empty).Replace(">", string.Empty).Replace(defaultText, string.Empty));
		}

		private static int GetNextId(IEnumerable<string> numberList)
		{
			var result = 0;
			foreach (var n in numberList)
			{
				int tryNumber; // to get rid of TryParse error.
				if (!string.IsNullOrEmpty(n) && int.TryParse(n, out tryNumber) && tryNumber > result)
					result = tryNumber;
			}

			return result+1;
		}
	}
}
