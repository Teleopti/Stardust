using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Teleopti.Interfaces.Domain
{
	/// <summary>
    /// Time helper class with utilities for time handling
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-14
    /// </remarks>
    public static class TimeHelper
    {
        /// <summary>
        /// Tries to parse time from text as time span.
        /// </summary>
        /// <param name="timeAsText">The time as text.</param>
        /// <param name="timeValue">The time value.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-14
        /// </remarks>

        public static bool TryParse(string timeAsText, out TimeSpan timeValue)
        {
            TimeSpan? span;
            var result = TryParse(timeAsText, out span);
            timeValue = span.GetValueOrDefault();
            return result;
        }

        /// <summary>
        /// Parses the time text an returns a result 
        /// and a nullable timespan.
        /// </summary>
        /// <param name="timeAsText">The time as text.</param>
        /// <param name="timeValue">The time value.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static bool TryParse(string timeAsText, out TimeSpan? timeValue)
        {
            //Try to parse as numeric hours
            int hours;
            if (int.TryParse(timeAsText, out hours) &&
                hours < 24 && hours >= 0)
            {
                timeValue = TimeSpan.FromHours(hours);
                return true;
            }

            if (int.TryParse(timeAsText, out hours) && hours == 24)
            {
                timeValue = TimeSpan.FromDays(1);
                return true;
            }

            //Try to parse as numeric hours and minutes
            if (hours > 24 && hours < 100)
            {
                timeValue = new TimeSpan((hours / 10), hours % 10, 0);
                return true;
            }
            if (hours >= 100 && hours < 2400)
            {
                timeValue = new TimeSpan((hours / 100), hours % 100, 0);
                return true;
            }
            if (hours > 2399)
            {
                timeValue = TimeSpan.Zero;
                return false;
            }


            // Try to parse as TimeSpan
            TimeSpan timeSpan;
            if (TimeSpan.TryParse(timeAsText, out timeSpan))
            {
                timeValue = timeSpan;
                return true;
            }

            // Try to find a + sign, if one exists we want to add the number of DAYS behind the sign
            var pos = timeAsText.IndexOf('+');
            if (pos > -1)
            {
                int days;
                if (int.TryParse(timeAsText.Substring(pos + 1, timeAsText.Length - pos - 1), out days))
                {
                    TimeSpan timeSpannet;
                    if (TryParse(timeAsText.Substring(0, pos).TrimEnd(), out timeSpannet))
                    {
                        timeValue = timeSpannet.Add(TimeSpan.FromDays(days));
                        return true;
                    }
                }
            }

            pos = timeAsText.IndexOf(CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator, StringComparison.Ordinal);
            if (pos > -1)
            {
                if (pos == 0)
                {
                    int minutes;
                    if (int.TryParse(timeAsText.Substring(pos + 1, timeAsText.Length - pos - 1), out minutes))
                    {
                        timeValue = TimeSpan.FromMinutes(minutes);
                        return true;
                    }
                }
            }

            //Try to parse as datetime
            DateTime time;
            if (DateTime.TryParse(timeAsText, out time))
            {
                if (time.TimeOfDay.TotalHours >= 0d)
                {
                    timeValue = time.TimeOfDay;
                    return true;
                }
            }

            //Try to find "PM" occurencies
            var pmDesignatorExists = false;
            timeAsText = timeAsText.ToLower(CultureInfo.CurrentCulture);
            var pmSigns = new[]
                                   {
                                       CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator,
                                       "pm"
                                   };
            for (var i = 0; i < pmSigns.Length; i++)
            {
                if (!string.IsNullOrEmpty(pmSigns[i]) && timeAsText.Contains(pmSigns[i]))
                {
                    pmDesignatorExists = true;
                    break;
                }
            }

            //Strip everything but numbers and do a recursive call
            var strippedTime = Regex.Replace(timeAsText, @"[^0-9]+", string.Empty);
            if (strippedTime != timeAsText)
            {
                if (TryParse(strippedTime, out timeValue))
                {
                    //Add twelve more hours if PM exists and the time is less than 12
                    if (pmDesignatorExists &&
                        timeValue.GetValueOrDefault().Hours < 12)
                    {
                        timeValue = timeValue.GetValueOrDefault().Add(TimeSpan.FromHours(12));
                    }

                    return true;
                }
            }

            timeValue = TimeSpan.Zero;
            return false;
        }
		
        /// <summary>
        /// Return a string in short time format representing the time of day from a TimeSpan.
        /// If the TimeSpan is longer than one day a + and the number of days is added on the end for example "08:00 +1"
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns></returns>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-20    
        /// /// </remarks>
        public static string TimeOfDayFromTimeSpan(TimeSpan timeSpan)
        {
        	return TimeOfDayFromTimeSpan(timeSpan, CultureInfo.CurrentCulture);
        }

    	/// <summary>
    	/// Return a string in short time format representing the time of day from a TimeSpan.
    	/// If the TimeSpan is longer than one day a + and the number of days is added on the end for example "08:00 +1"
    	/// </summary>
    	/// <param name="timeSpan">The time span.</param>
    	/// <param name="cultureInfo">The culture</param>
    	/// <returns></returns>
    	/// /// 
    	/// <remarks>
    	///  Created by: Ola
    	///  Created date: 2008-10-20    
    	/// /// </remarks>
		public static string TimeOfDayFromTimeSpan(TimeSpan timeSpan, IFormatProvider cultureInfo)
    	{
    		var parsed = ParseTimeOfDayFromTimeSpan(timeSpan);

			var nextDay = "";
			if (parsed.Days > 0)
				nextDay = " +" + parsed.Days;

    		var baseTime = new DateTime(2001,1,1);
			var timeOfDayAsDateTime = baseTime.Add(parsed.TimeOfDay);
			var formattedDateTime = timeOfDayAsDateTime.ToString("t", cultureInfo);
			formattedDateTime += nextDay;
			return formattedDateTime;
		}

		/// <summary>
		/// Extract the number of days from a "time of day" timespan includes and
		/// returns a new time of day time span and
		/// the number of days the original time span included.
		/// A helper function to handle the "08:00 +1"/"time of next day" scenario
		/// </summary>
		/// <param name="timeSpan">The timespan that may include</param>
		public static ParsedTimeOfDay ParseTimeOfDayFromTimeSpan(TimeSpan timeSpan)
		{
			return new ParsedTimeOfDay { Days = timeSpan.Days, TimeOfDay = timeSpan.Subtract(TimeSpan.FromDays(timeSpan.Days)) };
		}
    }
}