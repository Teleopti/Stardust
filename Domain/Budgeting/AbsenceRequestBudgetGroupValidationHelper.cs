using System;
using System.Globalization;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Budgeting
{
	public class AbsenceRequestBudgetGroupValidationHelper
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(AbsenceRequestBudgetGroupValidationHelper));

		public static IValidatedRequest PersonPeriodOrBudgetGroupIsNull(CultureInfo langauage, Guid? personId)
		{
			var logInfo = string.Format("There is no budget group for person: {0}.", personId);
			var errorInfo = Resources.ResourceManager.GetString(nameof(Resources.BudgetGroupMissing),
                langauage) ?? Resources.BudgetGroupMissing;

			return logAndReturnValidatedRequest(logInfo, errorInfo);
		}

		public static IValidatedRequest BudgetDaysAreNull(CultureInfo langauage, CultureInfo culture, DateOnlyPeriod requestedPeriod)
		{
			var logInfo = string.Format("There is no budget for this period {0}.", requestedPeriod);
			var errorInfo = string.Format(Resources.ResourceManager.GetString(nameof(Resources.NoBudgetForThisPeriod),
                langauage) ?? Resources.NoBudgetForThisPeriod, requestedPeriod.ToShortDateString(culture));
			return logAndReturnValidatedRequest(logInfo, errorInfo);
		}

		public static IValidatedRequest BudgetDaysAreNotEqualToRequestedPeriodDays(CultureInfo langauage, CultureInfo culture, DateOnlyPeriod requestedPeriod)
		{
			var logInfo = string.Format("One or more days during this requested period {0} has no budget.", requestedPeriod);
			var errorInfo = string.Format(Resources.ResourceManager.GetString(nameof(Resources.NoBudgetDefineForSomeRequestedDays),
                langauage) ?? Resources.NoBudgetDefineForSomeRequestedDays, requestedPeriod.ToShortDateString(culture));
			return logAndReturnValidatedRequest(logInfo, errorInfo);
		}

		public static IValidatedRequest InvalidDaysInBudgetDays(string invalidDays, string scenarioValidationError)
		{
			var logInfo = string.Format(scenarioValidationError, invalidDays);
			var errorInfo = logInfo;
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
