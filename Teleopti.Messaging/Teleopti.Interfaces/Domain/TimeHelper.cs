﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

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
            timeValue = span.Value;
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
                timeValue = new TimeSpan(hours, 0, 0);
                return true;
            }

            if (int.TryParse(timeAsText, out hours) && hours == 24)
            {
                timeValue = new TimeSpan(1, 0, 0, 0);
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
                timeValue = new TimeSpan();
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
            int pos = timeAsText.IndexOf('+');
            if (pos > -1)
            {
                int days;
                if (int.TryParse(timeAsText.Substring(pos + 1, timeAsText.Length - pos - 1), out days))
                {
                    TimeSpan timeSpannet;
                    if (TryParse(timeAsText.Substring(0, pos - 1), out timeSpannet))
                    {
                        timeValue = timeSpannet.Add(new TimeSpan(days, 0, 0, 0));
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
                        timeValue = new TimeSpan(0, minutes, 0);
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
            bool pmDesignatorExists = false;
            timeAsText = timeAsText.ToLower(CultureInfo.CurrentCulture);
            string[] pmSigns = new[]
                                   {
                                       CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator,
                                       "pm"
                                   };
            for (int i = 0; i < pmSigns.Length; i++)
            {
                if (!string.IsNullOrEmpty(pmSigns[i]) && timeAsText.Contains(pmSigns[i]))
                {
                    pmDesignatorExists = true;
                    break;
                }
            }

            //Strip everything but numbers and do a recursive call
            string strippedTime = Regex.Replace(timeAsText, @"[^0-9]+", string.Empty);
            if (strippedTime != timeAsText)
            {
                if (TryParse(strippedTime, out timeValue))
                {
                    //Add twelve more hours if PM exists and the time is less than 12
                    if (pmDesignatorExists &&
                        timeValue.Value.Hours < 12)
                    {
                        timeValue = timeValue.Value.Add(new TimeSpan(12, 0, 0));
                    }

                    return true;
                }
            }

            timeValue = TimeSpan.Zero;
            return false;
        }

        /// <summary>
        /// Fits the entered time to default resolution.
        /// </summary>
        /// <param name="timeAsTimeSpan">The time as time span.</param>
        /// <param name="defaultResolution">The default resolution.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-01-29
        /// </remarks>
        public static TimeSpan FitToDefaultResolution(TimeSpan timeAsTimeSpan, int defaultResolution)
        {
            //Skip seconds, and milliseconds, and additional tics
            TimeSpan cleanedTime = TimeSpan.FromMinutes((int)timeAsTimeSpan.TotalMinutes);
            int remainder;
            int numberOfIntervals = Math.DivRem(Convert.ToInt32(cleanedTime.TotalMinutes), defaultResolution, out remainder);
            if (remainder == 0) return cleanedTime;

            if (remainder >= defaultResolution / 2)
                numberOfIntervals++;

            return TimeSpan.FromMinutes(numberOfIntervals * defaultResolution);
        }

        /// <summary>
        /// Tries the parse time periods.
        /// </summary>
        /// <param name="timePeriods">The time periods (separated by ;).</param>
        /// <param name="result">The result.</param>
        /// <returns>True if parse succeeds.</returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-12
        /// </remarks>
        public static bool TryParseTimePeriods(string timePeriods, out IList<TimePeriod> result)
        {
            bool dayAfter = false;
            string[] timePeriodCollection = timePeriods.Split(
                new[] { ';' },
                StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < timePeriodCollection.Length; i++)
            {
                if (timePeriodCollection[i].Contains("+"))
                {
                    timePeriodCollection[i] = timePeriodCollection[i].Substring(0, timePeriodCollection[i].IndexOf('+'));
                    dayAfter = true;
                }
            }

            result = new List<TimePeriod>();
            foreach (string timePeriodItem in timePeriodCollection)
            {

                if (timePeriodItem.Trim().Length < 1) continue;

                TimePeriod timePeriod;
                if (TimePeriod.TryParse(timePeriodItem.Trim(), out timePeriod))
                {
                    if (dayAfter)
                        timePeriod = new TimePeriod(timePeriod.StartTime, timePeriod.EndTime.Add(new TimeSpan(1, 0, 0, 0)));
                    result.Add(timePeriod);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to parse long hour minute or empty to a TimeSpan.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ci">The ci.</param>
        /// <param name="timeSpan">The time span.</param>
        /// <returns></returns>
        public static bool TryParseLongHourMinuteOrEmptyToTimeSpan(string text, CultureInfo ci, out TimeSpan timeSpan)
        {
            timeSpan = TimeSpan.Zero;

            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

           
            text = text.Trim();
            bool isNegativSign = text.StartsWith("-", StringComparison.Ordinal);

            char separator = Char.Parse(ci.DateTimeFormat.TimeSeparator);
            String[] split = text.Split(separator);

            if (split.Length > 2)
                return false;

            int minutes = 0;
            int hours;
            if (split.Length == 2)
            {
                if (!int.TryParse(split[1], out minutes))
                    return false;
                if (minutes > 59)
                    return false;
            }

            if (!int.TryParse(split[0], out hours))
                return false;

            if (hours < 0 || isNegativSign)
                minutes *= -1;

            timeSpan = TimeSpan.FromHours(hours).Add(TimeSpan.FromMinutes(minutes));
            return true;
        }

        /// <summary>
        /// Tries the parse long hour string (hh:mm) eg. 25:40.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="timeValue">The time value.</param>
        /// <param name="timeFormatsType">Type of the time formats.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-11
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        public static bool TryParseLongHourString(string text, out TimeSpan timeValue, TimeFormatsType timeFormatsType)
        {
            timeValue = new TimeSpan();
            CultureInfo ci = CultureInfo.CurrentCulture;

            char separator = Char.Parse(ci.DateTimeFormat.TimeSeparator);
            String[] ret = text.Split(separator);

            if (ret.Length > 3)
                return false;

            int seconds = 0;
            int minutes = 0;
            int hours = 0;
            if (timeFormatsType == TimeFormatsType.HoursMinutesSeconds)
            {
                if (ret.Length == 3)
                {
                    if (!int.TryParse(ret[0], out hours))
                        return false;
                    if (!int.TryParse(ret[2], out seconds))
                        return false;
                    if (!int.TryParse(ret[1], out minutes))
                        return false;
                    if (!CheckMinutesSeconds(minutes, seconds))
                        return false;
                }

                else if (ret.Length == 2)
                {
                    if (!int.TryParse(ret[0], out hours))
                        return false;
                    if (!int.TryParse(ret[1], out minutes))
                        return false;
                    if (!CheckMinutesSeconds(minutes, 0))
                        return false;
                }
                else if (ret.Length == 1)
                {
                    if (!int.TryParse(ret[0], out hours))
                        return false;
                }
            }
            else
            {
                if (ret.Length == 2)
                {
                    if (!int.TryParse(ret[0], out hours))
                        return false;
                    if (!int.TryParse(ret[1], out minutes))
                        return false;
                    if (!CheckMinutesSeconds(minutes, 0))
                        return false;
                }
                else if (ret.Length == 1)
                {
                    if (!int.TryParse(ret[0], out hours))
                        return false;
                }
                else if (ret.Length == 3)
                    return false;
            }

            if(hours < 0)
                timeValue = TimeSpan.FromHours(hours).Add(TimeSpan.FromMinutes(-minutes)).Add(TimeSpan.FromSeconds(-seconds));
            else
                timeValue = TimeSpan.FromHours(hours).Add(TimeSpan.FromMinutes(minutes)).Add(TimeSpan.FromSeconds(seconds));
            return true;
        }

        /// <summary>
        /// Gets the long hour, minute and seconds string from a timespan eg. 123:23:50.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="cultureInfo">The culture info.</param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public static string GetLongHourMinuteSecondTimeString(TimeSpan timeSpan, CultureInfo cultureInfo)
        {
            string separator = cultureInfo.DateTimeFormat.TimeSeparator;
            int sign = Math.Sign(timeSpan.Ticks);

            TimeSpan absValue = TimeSpan.FromTicks(Math.Abs(timeSpan.Ticks));

            int hour = (int)absValue.TotalHours;
            int minutes = absValue.Minutes;
            int seconds = absValue.Seconds;

            if (absValue.Milliseconds >= 500)
                seconds += 1;
            if (seconds == 60)
            {
                minutes++;
                seconds = 0;
            }
            if (minutes == 60)
            {
                hour++;
                minutes = 0;
            }

            string hours = Convert.ToString(hour, CultureInfo.CurrentCulture);
            string min = Convert.ToString(minutes, CultureInfo.CurrentCulture);
            string sec = Convert.ToString(seconds, CultureInfo.CurrentCulture);

            string signChar = string.Empty;
            if (sign == -1)
                signChar = "-";

            if (min.Length == 1)
                min = "0" + min;

            if (sec.Length == 1)
                sec = "0" + sec;

            return string.Concat(signChar, hours, separator, min, separator, sec);
        }

        /// <summary>
        /// Gets the long hour and minutes tring from a timespan eg. 123:23.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="cultureInfo">The culture info.</param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public static string GetLongHourMinuteTimeString(TimeSpan timeSpan, CultureInfo cultureInfo)
        {
            string separator = cultureInfo.DateTimeFormat.TimeSeparator;
            int sign = Math.Sign(timeSpan.Ticks);

            TimeSpan absValue = TimeSpan.FromTicks(Math.Abs(timeSpan.Ticks));

            int hour = (int)absValue.TotalHours;
            int minutes = absValue.Minutes;

            if (absValue.Seconds >= 30)
                minutes += 1;
            if (minutes == 60)
            {
                hour++;
                minutes = 0;
            }

            string min = Convert.ToString(minutes, CultureInfo.CurrentCulture);
            string hours = Convert.ToString(hour, CultureInfo.CurrentCulture);

            string signChar = string.Empty;
            if (sign == -1)
                signChar = "-";

            if (min.Length == 1)
                min = "0" + min;

            return string.Concat(signChar, hours, separator, min);
        }

        /// <summary>
        /// Gets the long hour and minutes tring from a timespan eg. 123:23.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="previousPreposition">The previous preposition "-" or "+".</param>
        /// <param name="cultureInfo">The culture info.</param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public static string GetLongHourMinuteTimeString(TimeSpan timeSpan, char previousPreposition, CultureInfo cultureInfo)
        {
            if (previousPreposition != '-' && previousPreposition != '+')
                throw new ArgumentException("previousPreposition must be either '-' or '+'", "previousPreposition");
            int prepositionSign = previousPreposition == '+' ? 1 : -1;

            string separator = cultureInfo.DateTimeFormat.TimeSeparator;
            int valueSign = Math.Sign(timeSpan.Ticks);

            TimeSpan absValue = TimeSpan.FromTicks(Math.Abs(timeSpan.Ticks));

            int hour = (int)absValue.TotalHours;
            int minutes = absValue.Minutes;

            if (absValue.Seconds >= 30)
                minutes += 1;
            if (minutes == 60)
            {
                hour++;
                minutes = 0;
            }

            string min = Convert.ToString(minutes, CultureInfo.CurrentCulture);
            string hours = Convert.ToString(hour, CultureInfo.CurrentCulture);

            int sign = valueSign * prepositionSign;
            string signChar = sign >= 0 ? "+" : "-";

            if (min.Length == 1)
                min = "0" + min;

            return string.Concat(signChar, " ", hours, separator, min);
        }


        private static bool CheckMinutesSeconds(int minutes, int seconds)
        {
            if (minutes > 59 || seconds > 59)
                return false;
            return true;
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static string TimeOfDayFromTimeSpan(TimeSpan timeSpan, CultureInfo cultureInfo)
    	{
    		var parsed = ParseTimeOfDayFromTimeSpan(timeSpan);

			var nextDay = "";
			if (parsed.Days > 0)
				nextDay = " +" + parsed.Days;

    		var baseTime = DateTime.Now.Date;
			var timeOfDayAsDateTime = baseTime.Add(parsed.TimeOfDay);
			//var timeOfDayAsDateTime = DateTime.MinValue.Add(parsed.TimeOfDay);
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

		/// <summary>
		/// Creates a time of day timespan from a timespan and a next day flag and
		/// returns a new timespan containing the time of day and
		/// the number of days in the future, ie nextDay = true will result in 1 day
		/// A helper function to handle the "08:00 +1"/"time of next day" scenario
		/// </summary>
		/// <param name="timeOfDay">Time of day</param>
		/// <param name="nextDay">Time of day is next day</param>
		/// <returns>Timespan including next day of next day is true</returns>
		public static TimeSpan ParseTimeSpanFromTimeOfDay(TimeSpan timeOfDay, bool nextDay)
		{
			return nextDay ? timeOfDay.Add(TimeSpan.FromDays(1)) : timeOfDay;
		}

    	/// <summary>
        /// Tries the parse long hour string default interpretation.
        /// E.g input == 12, result in 0:12 if default
        /// interpret as minutes == true. If false
        /// 12:00.
        /// </summary>
        /// <param name="timeAsText">The time as text.</param>
        /// <param name="timeValue">The time value.</param>
        /// <param name="timeFormatsType">Type of the time formats.</param>
        /// <param name="defaultInterpretAsMinutes">if set to <c>true</c> [default interprete as minutes].</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        public static bool TryParseLongHourStringDefaultInterpretation(string timeAsText, out TimeSpan timeValue, TimeFormatsType timeFormatsType, bool defaultInterpretAsMinutes)
        {
            CultureInfo ci = CultureInfo.CurrentCulture;

            char separator = Char.Parse(ci.DateTimeFormat.TimeSeparator);
            String[] ret = timeAsText.Split(separator);

            if (defaultInterpretAsMinutes)
            {
                if (ret.Length == 1)
                {
                    int minutes;
                    if (int.TryParse(ret[0], out minutes))
                    {
                        //timeAsText = string.Concat("0", separator, minutes, separator, "0");
                        timeValue = TimeSpan.FromMinutes(minutes);
                        return true;
                    }
                }
            }
            else
            {
                if (ret.Length == 1)
                {
                    int hours;
                    if (int.TryParse(ret[0], out hours))
                    {
                        //timeAsText = string.Concat(hours, separator, "0", separator, "0");
                        timeValue = TimeSpan.FromHours(hours);
                        return true;
                    }
                }
            }
            
            // Try to parse as TimeSpan
            TimeSpan timeSpan;
            if (TimeSpan.TryParse(timeAsText, out timeSpan))
            {
                timeValue = timeSpan;
                return true;
            }
            return TryParseLongHourString(timeAsText, out timeValue, timeFormatsType);
        }

        ///<summary>
        /// Fits the entered time to lower default resolution.
        ///</summary>
        ///<param name="value">The time as time span.</param>
        ///<param name="resolution">The default resolution.</param>
        ///<returns></returns>
        /// <remarks>
        /// Created by: marias
        /// Created date: 2010-06-04
        /// </remarks>
        public static TimeSpan FitToDefaultResolutionRoundDown(TimeSpan value, int resolution)
        {
            TimeSpan cleanedTime = TimeSpan.FromMinutes((int)value.TotalMinutes);
            int remainder;
            int numberOfIntervals = Math.DivRem(Convert.ToInt32(cleanedTime.TotalMinutes), resolution, out remainder);
            if (remainder == 0) return cleanedTime;

            return TimeSpan.FromMinutes(numberOfIntervals * resolution);

        }

        /// <summary>
        /// Returns true/false whether or not Current Culture is using a 24 hour clock
        /// </summary>
        /// <returns></returns>
        public static bool CurrentCultureUsing24HourClock()
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains("H");
        }
    }


	/// <summary>
	/// Return value of ParseTimeOfDayFromTimeSpan method
	/// </summary>
	public class ParsedTimeOfDay
	{
		/// <summary>
		/// Days that the time of day timespan included. 1 = Next day
		/// </summary>
		public int Days { get; set; }
		/// <summary>
		/// The time of day the timespan included
		/// </summary>
		public TimeSpan TimeOfDay { get; set; }
	}

}