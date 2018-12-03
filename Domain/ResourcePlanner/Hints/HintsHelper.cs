using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class HintsHelper
	{
		public static string BuildErrorMessage(ValidationError validationError, IUserTimeZone userTimeZone)
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
					if (argument is DateTime dateTimeArgument)
					{
						try
						{
							localizedArguments.Add(TimeZoneHelper.ConvertFromUtc(dateTimeArgument, userTimeZone.TimeZone()));
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

		public static void BuildErrorMessages(ICollection<ValidationError> validationErrors, IUserTimeZone userTimeZone)
		{
			foreach (var validationError in validationErrors)
			{
				validationError.ErrorMessageLocalized = BuildErrorMessage(validationError, userTimeZone);
			}
		}

		public static ICollection<SchedulingHintError> BuildBusinessRulesValidationResults(
			IEnumerable<SchedulingHintError> businessRulesValidationResults, IUserTimeZone userTimeZone)
		{
			foreach (var schedulingHintError in businessRulesValidationResults)
			{
				BuildErrorMessages(schedulingHintError.ValidationErrors, userTimeZone);
			}

			var groups = businessRulesValidationResults.GroupBy(x =>
				x.ValidationErrors.First().ResourceType);

			var schedulingHintErrors = new List<SchedulingHintError>(); 
			foreach (var group in groups)
			{
				schedulingHintErrors.AddRange(group.OrderBy(x => x.ResourceName));
			}
			return schedulingHintErrors;
		}

	}
}
