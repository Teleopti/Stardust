using System;
using System.Globalization;

// ReSharper disable once CheckNamespace
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Helper to check inparameters to methods and constructors
    /// </summary>
    public static class InParameter
    {
        /// <summary>
        /// Verifies that parameterValue is not null
        /// </summary>
        /// <param name="parameterValue">The parameter value.</param>
        /// <param name="parameterName">Name of the param.</param>
        public static void NotNull(string parameterName, object parameterValue)
        {
            if (parameterValue == null)
            {
                var errMess = string.Format(CultureInfo.CurrentCulture,
                                               "Parameter '{0}' must not be null.",
                                               parameterName);
                throw new ArgumentNullException(parameterName, errMess);
            }
        }
		
        /// <summary>
        /// Verifies the date is UTC.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="dateToCheck">The date to check.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-18
        /// </remarks>
        public static void VerifyDateIsUtc(string parameterName, DateTime dateToCheck)
        {
            if (dateToCheck.Kind != DateTimeKind.Utc)
                throw new ArgumentException("DateTime must be passed as UTC.", parameterName);
        }
    }
}