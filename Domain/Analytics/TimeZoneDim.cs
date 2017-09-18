using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Analytics
{
    public class TimeZoneDim : ITimeZoneDim
    {
		private TimeZoneDim()
        {
        }

        public TimeZoneDim(TimeZoneInfo timeZone, bool isEtlDefaultTimeZone, bool isUtcInUse) : this()
        {
			IsUtcInUse = isUtcInUse;
			MartId = -1;
            TimeZoneCode = timeZone.Id;
            TimeZoneName = timeZone.DisplayName;
            IsDefaultTimeZone = isEtlDefaultTimeZone;
            UtcConversion = Convert.ToInt32(timeZone.BaseUtcOffset.TotalMinutes);
            UtcConversionDst = getUtcConversionIncludedDaylightSaving(timeZone);
        }

        public TimeZoneDim(
			int martId, 
			string timeZoneCode, 
			string timeZoneName, 
			bool isEtlDefaultTimeZone, 
			int utcConversion, 
			int utcConversionDst,
			bool isUtcInUse
		)
            : this()
        {
            MartId = martId;
            TimeZoneCode = timeZoneCode;
            TimeZoneName = timeZoneName;
            IsDefaultTimeZone = isEtlDefaultTimeZone;
            UtcConversion = utcConversion;
            UtcConversionDst = utcConversionDst;
			IsUtcInUse = isUtcInUse;
		}

        public int MartId { get; }
        
        public string TimeZoneCode { get; }

		public string TimeZoneName { get; }

        public bool IsDefaultTimeZone { get; }

		public int UtcConversion { get; }

        public int UtcConversionDst { get; }
		public bool IsUtcInUse { get; }

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