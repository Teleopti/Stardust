﻿using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Helper
{
    /// <summary>
    /// Various String functions to make your life easier.
    /// </summary>
    /// <remarks>
    /// Created by: henryg
    /// Created date: 2007-12-13
    /// </remarks>
    public static class StringHelper
    {
        /// <summary>
        /// Capitalizes the specified string paragraph.
        /// </summary>
        /// <param name="paragraph">The string paragraph.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2007-12-13
        /// </remarks>
        public static string Capitalize(string paragraph)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(paragraph);
        }

        /// <summary>
        /// Splits the specified to be splitted.
        /// </summary>
        /// <param name="toBeSplit">To be splitted.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-01-15
        /// </remarks>
        public static string[] Split(this string toBeSplit, string separator)
        {
            return toBeSplit.Split(new[] {separator}, StringSplitOptions.None);
        }
    }

    public static class DateExtensions
    {
        public static DateTime LimitMin(this DateTime dateTime)
        {
            if (DateHelper.MinSmallDateTime > dateTime)
                return DateHelper.MinSmallDateTime;

            return dateTime;
        }
    }
}
