using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.FileImport
{

    public class FileImportDateTimeParser : IFileImportDateTimeParser
    {
        //Henrik 2009-03-31: The CultureInfo was hardcoded, is this by design?
        private TimeZoneInfo _timeZone = TimeZoneInfo.Utc; //Utc as default
        private readonly CultureInfo _cultureInfo = new CultureInfo("en-US");

        private DateTime ParseToUtc(string dateValue, string timeValue)
        {
            DateTime dateTime = DateTime.ParseExact(dateValue, "yyyyMMdd", _cultureInfo);
			TimeSpan timeSpan = TimeSpan.Parse(timeValue, CultureInfo.InvariantCulture);
            DateTime local = dateTime.Add(timeSpan);
            return _timeZone.SafeConvertTimeToUtc(local);
        }

        public bool DateTimeIsValid(string dateValue, string timeValue)
        {
            DateTime dateTime = DateTime.ParseExact(dateValue, "yyyyMMdd", _cultureInfo);
            TimeSpan timeSpan = TimeSpan.Parse(timeValue,CultureInfo.InvariantCulture);
            DateTime local = dateTime.Add(timeSpan);
            return !_timeZone.IsInvalidTime(local);
        }


        #region IFileImportDateTimeParser Members
        public DateTime UtcDateTime(string dateValue, string timeValue)
        {
            return ParseToUtc(dateValue, timeValue);
        }

        public string UtcTime(string dateValue, string timeValue)
        {
            DateTime dateTime = ParseToUtc(dateValue, timeValue);
            return string.Format(_cultureInfo, "{0:HH:mm}", dateTime);
        }

        public void TimeZone(TimeZoneInfo timeZoneForConverting)
        {
            _timeZone = timeZoneForConverting;
        }

        #endregion
    }
}