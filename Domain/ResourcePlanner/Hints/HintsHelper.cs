using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

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

				var localizedArguments = new List<Object>();
				foreach (var argument in argumentsArray)
				{
					if (argument is DateTime)
					{
						var dateTimeArgument = (DateTime)argument;
						try
						{
							localizedArguments.Add(TimeZoneHelper.ConvertFromUtc(dateTimeArgument, TimeZoneHelper.CurrentSessionTimeZone));
						}
						catch (Exception)
						{
							localizedArguments.Add(dateTimeArgument);
						}
					}
					else
					{
						localizedArguments.Add(argument);
					}
				}
				return string.Format(localizedString, localizedArguments.ToArray());
			}
			catch (Exception)
			{
				return localizedString;
			}
		}

		public static void BuildErrorMessages(ICollection<ValidationError> validationErrors)
		{
			foreach (var validationError in validationErrors)
			{
				validationError.ErrorMessageLocalized = HintsHelper.BuildErrorMessage(validationError);
			}
		}
	}
}
