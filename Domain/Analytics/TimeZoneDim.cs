using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Analytics
{
    public class TimeZoneDim : ITimeZoneDim
    {
        private readonly string _timeZoneCode;

        private TimeZoneDim()
        {
        }

        public TimeZoneDim(TimeZoneInfo timeZone, TimeZoneInfo defaultTimeZone) : this()
        {
            MartId = -1;
            _timeZoneCode = timeZone.Id;
            TimeZoneName = timeZone.DisplayName;
            if (timeZone.Id == defaultTimeZone.Id)
                IsDefaultTimeZone = true;
            else 
                IsDefaultTimeZone = false;
            UtcConversion = Convert.ToInt32(timeZone.BaseUtcOffset.TotalMinutes);
            UtcConversionDst = getUtcConversionIncludedDaylightSaving(timeZone);
        }

        public TimeZoneDim(int martId, string timeZoneCode, string timeZoneName, bool isDefault, int utcConversion, int utcConversionDst)
            : this()
        {
            MartId = martId;
            _timeZoneCode = timeZoneCode;
            TimeZoneName = timeZoneName;
            IsDefaultTimeZone = isDefault;
            UtcConversion = utcConversion;
            UtcConversionDst = utcConversionDst;
        }

        public int MartId { get; private set; }
        
        public string TimeZoneCode
        {
            get { return _timeZoneCode; }
        }

        public string TimeZoneName { get; private set; }

        public bool IsDefaultTimeZone { get; private set; }

        public int UtcConversion { get; private set; }

        public int UtcConversionDst { get; private set; }

        private int getUtcConversionIncludedDaylightSaving(TimeZoneInfo timeZone)
        {
            int retVal = 0;

            TimeZoneInfo.AdjustmentRule[] adjustmentRules = timeZone.GetAdjustmentRules();
            if (adjustmentRules.Length > 0)
            {
                //Get daylight saving minutes from the first adjustment rule (should be the same for all rules of a time zone)
                retVal = Convert.ToInt32(adjustmentRules[0].DaylightDelta.TotalMinutes);
            }

            return retVal + UtcConversion;
        }
    }
}