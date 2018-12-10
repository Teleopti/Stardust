using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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
            var cleanedTime = TimeSpan.FromMinutes((int)timeAsTimeSpan.TotalMinutes);
			var numberOfIntervals = Math.DivRem(Convert.ToInt32(cleanedTime.TotalMinutes), defaultResolution, out var remainder);
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
            var dayAfter = false;
            var timePeriodCollection = timePeriods.Split(
                new[] { ';' },
                StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < timePeriodCollection.Length; i++)
            {
                if (timePeriodCollection[i].Contains("+"))
                {
                    timePeriodCollection[i] = timePeriodCollection[i].Substring(0, timePeriodCollection[i].IndexOf('+'));
                    dayAfter = true;
                }
            }

            result = new List<TimePeriod>();
            foreach (var timePeriodItem in timePeriodCollection)
            {

                if (timePeriodItem.Trim().Length < 1) continue;

                TimePeriod timePeriod;
                if (TimePeriod.TryParse(timePeriodItem.Trim(), out timePeriod))
                {
                    if (dayAfter)
                        timePeriod = new TimePeriod(timePeriod.StartTime, timePeriod.EndTime.Add(TimeSpan.FromDays(1)));
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
            var ci = CultureInfo.CurrentCulture;

            var separator = Char.Parse(ci.DateTimeFormat.TimeSeparator);
            var ret = text.Split(separator);

            if (ret.Length > 3)
                return false;

            double seconds = 0;
            var minutes = 0;
            var hours = 0;
            if (timeFormatsType == TimeFormatsType.HoursMinutesSeconds)
            {
                if (ret.Length == 3)
                {
                    if (!int.TryParse(ret[0], out hours))
                        return false;
                    if (!int.TryParse(ret[1], out minutes))
                        return false;
					if (!double.TryParse(ret[2], out seconds))
						return false;
                    if (!CheckMinutesSeconds(minutes, (int)Math.Abs(seconds)))
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
			{
				if (text.StartsWith("-") && hours == 0)
					timeValue = TimeSpan.FromHours(0).Add(TimeSpan.FromMinutes(-minutes)).Add(TimeSpan.FromSeconds(seconds));
				else
					timeValue = TimeSpan.FromHours(hours).Add(TimeSpan.FromMinutes(minutes)).Add(TimeSpan.FromSeconds(seconds));

			}
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
            var separator = cultureInfo.DateTimeFormat.TimeSeparator;

            var signChar = string.Empty;
            if (timeSpan < TimeSpan.Zero)
            {
                signChar = "-";
                timeSpan = timeSpan.Negate();
            }

            var hour = (int)timeSpan.TotalHours;
            var minutes = timeSpan.Minutes;
            var seconds = timeSpan.Seconds;

            if (timeSpan.Milliseconds >= 500)
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

            var hours = Convert.ToString(hour, CultureInfo.CurrentCulture);
            var min = Convert.ToString(minutes, CultureInfo.CurrentCulture);
            var sec = Convert.ToString(seconds, CultureInfo.CurrentCulture);

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
			
			string signChar = string.Empty;
			if (timeSpan < TimeSpan.Zero)
			{
				signChar = "-";
				timeSpan = timeSpan.Negate();
			}

			int hour = (int)timeSpan.TotalHours;
			int minutes = timeSpan.Minutes;

			if (timeSpan.Seconds >= 30)
				minutes += 1;
			if (minutes == 60)
			{
				hour++;
				minutes = 0;
			}

			string min = Convert.ToString(minutes, CultureInfo.CurrentCulture);
			string hours = Convert.ToString(hour, CultureInfo.CurrentCulture);

			if (min.Length == 1)
				min = "0" + min;

			return string.Concat(signChar, hours, separator, min);
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
        /// <param name="maximumValue">max timespan value</param>
        /// <param name="timeValue">The time value.</param>
        /// <param name="timeFormatsType">Type of the time formats.</param>
        /// <param name="defaultInterpretAsMinutes">if set to <c>true</c> [default interprete as minutes].</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        public static bool TryParseLongHourStringDefaultInterpretation(string timeAsText, TimeSpan maximumValue, out TimeSpan timeValue, TimeFormatsType timeFormatsType, bool defaultInterpretAsMinutes)
        {
            if (timeAsText == null) throw new ArgumentNullException(nameof(timeAsText));

            var ci = CultureInfo.CurrentCulture;

            var separator = Char.Parse(ci.DateTimeFormat.TimeSeparator);
            var ret = timeAsText.Split(separator);

            if (defaultInterpretAsMinutes)
            {
                if (ret.Length == 1)
                {
                    int minutes;
                    if (int.TryParse(ret[0], out minutes))
                    {
                        timeValue = TimeSpan.FromMinutes(minutes);
                        if (timeValue > maximumValue)
                        {
                            timeValue = maximumValue;
                            return false;
                        }
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
                        hours = verifySaneAmountOfHours(hours);
                        timeValue = TimeSpan.FromHours(hours);
                        if (timeValue > maximumValue)
                        {
                            timeValue = maximumValue;
                            return false;
                        }
                        
                        return true;
                    }
                }
            }
            
            return TryParseLongHourString(timeAsText, out timeValue, timeFormatsType);
        }

        private static int verifySaneAmountOfHours(int hours)
        {
            return Math.Min(hours, 1000000);
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
            var cleanedTime = TimeSpan.FromMinutes((int)value.TotalMinutes);
            int remainder;
            var numberOfIntervals = Math.DivRem(Convert.ToInt32(cleanedTime.TotalMinutes), resolution, out remainder);
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
}