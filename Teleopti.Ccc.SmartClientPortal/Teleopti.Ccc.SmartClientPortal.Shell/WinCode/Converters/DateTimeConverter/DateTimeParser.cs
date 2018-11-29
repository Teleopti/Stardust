using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters.DateTimeConverter
{
    /// <summary>
    /// Responsible for deciding what part of the datetime to rebuild and for parsing the supplied text
    /// If not able to parse, it will return the original datetime
    /// </summary>
    /// <remarks>
    /// This class will not do anything regarding Utc/Local
    /// Created by: henrika
    /// Created dateOnly: 2009-09-08
    /// </remarks>
    public class DateTimeParser : IDateTimeParser
    {
        public DateTime Parse(string textToParse, DateTime dateTime, DateTimeParseMode mode)
        {
            switch (mode)
            {
                case DateTimeParseMode.Time:
                    return BuildDateTimeFromTime(textToParse, dateTime);
                case DateTimeParseMode.Date:
                    return BuildDateTimeFromDate(textToParse, dateTime);
                default:
                    return BuildDateTimeFromDateTime(textToParse, dateTime);
            }
        }

        public string ToGuiText(DateTime dateTime, DateTimeParseMode mode)
        {
            switch (mode)
            {
                case DateTimeParseMode.Time:
                    return TimeText(dateTime);

                case DateTimeParseMode.Date:
                    return DateText(dateTime);

                default:
                    return DateTimeText(dateTime);
            }
        }

        public  DateTime BuildDateTimeFromTime(string time, DateTime dateTime)
        {
            //If we can parse the string to a time of day
            //Return the dateOnly plus that time, otherwise return the datetime:
            TimeSpan timeOfDay;
            if (TimeHelper.TryParse(time, out timeOfDay))
            {
                var t = Truncate(timeOfDay);
                return dateTime.Date.Add(t);
            }
            return dateTime;
        }

        private static TimeSpan Truncate(TimeSpan timeOfDay)
        {
            return new TimeSpan(timeOfDay.Days, timeOfDay.Hours, timeOfDay.Minutes, 0); //Cuts the seconds away
        }

        public  DateTime BuildDateTimeFromDate(string theDate, DateTime dateTime)
        {
            DateTime ret;
            if (DateTime.TryParse(theDate, out ret))
            {
                var t = Truncate(dateTime.TimeOfDay);
                return ret.Date.Add(t);
            }
            return dateTime;
        }

        public  DateTime BuildDateTimeFromDateTime(string dateTimeText, DateTime dateTime)
        {
            DateTime ret;
            if (DateTime.TryParse(dateTimeText, out ret))
            {
                return ret;
            }
            return dateTime;
        }

        public static string TimeText(DateTime dateTime)
        {
            return dateTime.ToShortTimeString();
        }

        public static string DateText(DateTime dateTime)
        {
            return dateTime.ToShortDateString();
        }

        public static string DateTimeText(DateTime dateTime)
        {
            return DateText(dateTime) + " " + TimeText(dateTime);
        }
    }
}
