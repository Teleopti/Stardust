using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Helper to check inparameters to methods and constructors
    /// </summary>
    public static class InParameter
    {

        /// <summary>
        /// Value the must be larger than zero.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="valueToCheck">The value to check.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-31
        /// </remarks>
        public static void ValueMustBeLargerThanZero(string parameterName, double valueToCheck)
        {
            if (valueToCheck <= 0)
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    String.Format(CultureInfo.CurrentCulture,
                                  "Value must be larger than zero."));
        }

        /// <summary>
        /// Values the must be larger than or equal to zero.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="valueToCheck">The value to check.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-01
        /// </remarks>
        public static void ValueMustBePositive(string parameterName, double valueToCheck)
        {
            if (valueToCheck < 0)
                throw new ArgumentOutOfRangeException(parameterName,
                                                      String.Format(CultureInfo.CurrentCulture,
                                                                    "Value must be larger than or equal to zero."));
        }

        /// <summary>
        /// Checks the time limit min one sec.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="timeToCheck">The time to check.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 19.12.2007
        /// </remarks>
        public static void CheckTimeSpanAtLeastOneTick(string parameterName, TimeSpan timeToCheck)
        {
            if (timeToCheck <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(
                    String.Format(CultureInfo.CurrentCulture,
                                  "TimePeriod must ba at least one tick. {0}",
                                  parameterName));
        }

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
        /// Verifies that a string (e.g. a value) is not string.Empty or null
        /// </summary>
        /// <param name="parameterName">Name of the param.</param>
        /// <param name="value">The value.</param>
        public static void NotStringEmptyOrNull(string parameterName, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                var errMess = string.Format(CultureInfo.CurrentCulture,
                                               "Parameter '{0}' must not be empty.",
                                               parameterName);
                throw new ArgumentException(parameterName, errMess);
            }
        }

				/// <summary>
				/// Verifies that a string (e.g. a value) is not string.Empty, null or only contains whitespace(s)
				/// </summary>
				/// <param name="parameterName">Name of the param.</param>
				/// <param name="value">The value.</param>
				public static void NotStringEmptyOrWhiteSpace(string parameterName, string value)
				{
					if (string.IsNullOrWhiteSpace(value))
					{
						var errMess = string.Format(CultureInfo.CurrentCulture,
																					 "Parameter '{0}' must not be empty or contain only whitespaces.",
																					 parameterName);
						throw new ArgumentException(parameterName, errMess);
					}
				}

        /// <summary>
        /// Shared methed to check if timespan is within limits
        /// </summary>
        /// <param name="parameterName">Name of parameter</param>
        /// <param name="timeToCheck">Time to check</param>
        /// <param name="limitation">Limit (as hours)</param>
        public static void CheckTimeLimit(string parameterName, TimeSpan timeToCheck, int limitation)
        {
            if (timeToCheck.TotalHours > limitation)
                throw new ArgumentOutOfRangeException(
                    String.Format(CultureInfo.CurrentCulture,
                                  "{0} has exceeded the limit of {1} hours.",
                                  parameterName,
                                  limitation));
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

        /// <summary>
        /// Verifies that a percentage not is above 100% or lower than 1%
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="value">Percentage to check.</param>
        public static void BetweenOneAndHundredPercent(string parameterName, Percent value)
        {
            if (value.Value <= 0 || value.Value > 1)
            {
                var errMess = string.Format(CultureInfo.CurrentCulture,
                                               "Parameter '{0}'' must have a value between one and hundred.",
                                               parameterName);
                throw new ArgumentOutOfRangeException(parameterName, errMess);
            }
        }

        /// <summary>
        /// Verifies that a percentage not is above 100% or lower than 0%
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-08
        /// </remarks>
        public static void BetweenZeroAndHundredPercent(string parameterName, Percent value)
        {
            if (value.Value < 0 || value.Value > 1)
            {
                var errMess = string.Format(CultureInfo.CurrentCulture,
                                               "Parameter '{0}' must have a value between one and hundred.",
                                               parameterName);
                throw new ArgumentOutOfRangeException(parameterName, errMess);
            }
        }

        /// <summary>
        /// Checks if a string is too long.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="valueToCheck">The value to check.</param>
        /// <param name="maxLength">Maximum length of string.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-13
        /// </remarks>
        public static void StringTooLong(string parameterName, string valueToCheck, int maxLength)
        {
            if (valueToCheck.Length > maxLength)
            {
                var errMess = string.Format(CultureInfo.CurrentCulture,
                                               "String parameter {0} may not be longer than {1} characters.",
                                               parameterName, maxLength);
                throw new ArgumentException(parameterName, errMess);
            }

        }

        /// <summary>
        /// TimesSpan can not be negativ.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-22    
        /// /// </remarks>
        public static void TimeSpanCannotBeNegative(string parameterName, TimeSpan value)
        {
            var errMess = string.Format(CultureInfo.CurrentCulture,
                                               "Parameter '{0}' must not have a negativ value.",
                                               parameterName);
            if (value < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(errMess, parameterName);
        }

        /// <summary>
        /// Throws Exception  if not true
        /// </summary>
        public static void MustBeTrue(string parameterName, bool value)
        {
            var errMess = string.Format(CultureInfo.CurrentCulture,
                                               "Parameter '{0}' must be valid.",
                                               parameterName);
            if (!value)
                throw new ArgumentException(errMess, parameterName);
        }

        /// <summary>
        /// Lists the cannot be empty.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-09-01    
        /// </remarks>
        public static void ListCannotBeEmpty<T>(string parameterName, IEnumerable<T> value)
        {
            var errMess = string.Format(CultureInfo.CurrentCulture,
                                               "Parameter '{0}' must not be an empty list.",
                                               parameterName);
            if (!value.Any())
                throw new ArgumentOutOfRangeException(errMess, parameterName);
        }

        /// <summary>
        /// Ensure that there are no seconds specified in start or end time of the given period.
        /// </summary>
        /// <param name="period">The period to check.</param>
        public static void EnsureNoSecondsInPeriod(DateTimePeriod period)
        {
            if (period.StartDateTime.Second != 0 || period.EndDateTime.Second != 0)
            {
                throw new ArgumentException(@"The seconds part is not allowed for this period.", nameof(period));
            }
        }
		
		public static void EnsurePeriodHasLength(DateTimePeriod period)
		{
			if (period.StartDateTime >= period.EndDateTime)
			{
				throw new ArgumentException(@"StartDateTime have to be less than EndDateTime.", nameof(period));
			}
		}

		/// <summary>
		/// Ensure that the two lists has the same size
		/// </summary>
		/// <typeparam name="T1">type the the first list</typeparam>
		/// <typeparam name="T2">type of the second list</typeparam>
		/// <param name="list1">first list</param>
		/// <param name="list2">second list</param>
		/// <remarks>
		/// Does NOT check for null
		/// </remarks>
	    public static void ListsHaveSameSize<T1, T2>(IEnumerable<T1> list1, IEnumerable<T2> list2)
	    {
		    if (list1.Count() != list2.Count())
			    throw new ArgumentException(@"Lists must have the same size");
	    }
    }
}