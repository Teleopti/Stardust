using System;
using System.Collections.Generic;
using System.Globalization;
using log4net;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class AbsenceRequestBudgetGroupValidationHelper
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(BudgetGroupAllowanceSpecification));

		public static IValidatedRequest PersonPeriodOrBudgetGroupIsNull(CultureInfo culture, Guid? personId)
		{
			var logInfo = string.Format("There is no budget group for person: {0}.", personId);
			var errorInfo = string.Format(culture, Resources.BudgetGroupMissing);
			return logAndReturnValidatedRequest(logInfo, errorInfo);
		}

		public static IValidatedRequest BudgetDaysAreNull(CultureInfo culture, DateOnlyPeriod requestedPeriod)
		{

			var logInfo = string.Format("There is no budget for this period {0}.", requestedPeriod);
			var errorInfo = string.Format(culture, Resources.NoBudgetForThisPeriod, requestedPeriod);
			return logAndReturnValidatedRequest(logInfo, errorInfo);
		}

		public static IValidatedRequest BudgetDaysAreNotEqualToRequestedPeriodDays(CultureInfo culture, DateOnlyPeriod requestedPeriod)
		{
			var logInfo = string.Format("One or more days during this requested period {0} has no budget.", requestedPeriod);
			var errorInfo = string.Format(culture, Resources.NoBudgetDefineForSomeRequestedDays, requestedPeriod);
			return logAndReturnValidatedRequest(logInfo, errorInfo);
		}

		public static IValidatedRequest InvalidDaysInBudgetDays(string invalidDays, string scenarioValidationError)
		{
			var logInfo = string.Format("There is not enough allowance for day {0}.", invalidDays);
			var errorInfo = scenarioValidationError + invalidDays;
			return logAndReturnValidatedRequest(logInfo, errorInfo);
		}

		private static IValidatedRequest logAndReturnValidatedRequest(string logInfo, string validationError)
		{
			Logger.DebugFormat(logInfo);
			return new ValidatedRequest
			{
				IsValid = false,
				ValidationErrors = validationError
			};
		}
	}
}
