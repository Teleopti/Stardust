using System;
using System.Linq;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class HintsHelper
	{
		public static string BuildErrorMessage(ValidationError validationError)
		{
			var localizedString = Resources.ResourceManager.GetString(validationError.ErrorResource ?? string.Empty) ?? validationError.ErrorResource;
			try
			{
				var argumentsArray = validationError.ErrorResourceData?.Any() ?? false
					? validationError.ErrorResourceData.ToArray()
					: new object[] { string.Empty };
				return string.Format(localizedString, argumentsArray);
			}
			catch (Exception)
			{
				return localizedString;
			}
		}
	}
}
