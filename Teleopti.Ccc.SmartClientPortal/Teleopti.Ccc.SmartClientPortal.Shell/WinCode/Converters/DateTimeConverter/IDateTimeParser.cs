using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters.DateTimeConverter
{
    public interface IDateTimeParser
    {
        DateTime Parse(string textToParse, DateTime dateTime, DateTimeParseMode mode);

        string ToGuiText(DateTime dateTime, DateTimeParseMode mode);

        DateTime BuildDateTimeFromTime(string time, DateTime dateTime);

        DateTime BuildDateTimeFromDate(string theDate, DateTime dateTime);

        DateTime BuildDateTimeFromDateTime(string dateTimeText, DateTime dateTime);
    }
}
