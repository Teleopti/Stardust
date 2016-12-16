﻿using System;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// 
    /// </summary>
    /// <note type="note">
    /// 	http://msdn.microsoft.com/en-us/library/ms912391%28v=winembedded.11%29.aspx
    /// </note>
    public static class TimeZoneInfoFactory
    {
        public static TimeZoneInfo StockholmTimeZoneInfo()
        {
            return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        }

        public static TimeZoneInfo HelsinkiTimeZoneInfo()
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
        }

		public static TimeZoneInfo GmtTimeZoneInfo()
		{
			return TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
		}
		
        public static TimeZoneInfo UtcTimeZoneInfo()
        {
			return TimeZoneInfo.Utc;
        }

		public static TimeZoneInfo MoskowTimeZoneInfo()
		{
			return TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
		}

		public static TimeZoneInfo AbuDhabiTimeZoneInfo()
		{
			return TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
		}

		public static TimeZoneInfo NewYorkTimeZoneInfo()
		{
			return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
		}

	    public static TimeZoneInfo DenverTimeZoneInfo()
	    {
		    return TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
		}

        public static TimeZoneInfo HawaiiTimeZoneInfo()
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time");
        }

        public static TimeZoneInfo SingaporeTimeZoneInfo()
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
        }

        public static TimeZoneInfo BrazilTimeZoneInfo()
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }

		public static TimeZoneInfo AustralianTimeZoneInfo()
		{
			return TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
		}

		public static TimeZoneInfo NewZealandTimeZoneInfo()
		{
			return TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
		}

		public static TimeZoneInfo IstanbulTimeZoneInfo()
		{
			return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
		}

	    public static TimeZoneInfo ChinaTimeZoneInfo()
	    {
		    return TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
	    }

	    public static TimeZoneInfo RussiaTz7ZoneInfo()
	    {
			return TimeZoneInfo.FindSystemTimeZoneById("North Asia East Standard Time");
		}

	    public static TimeZoneInfo TaipeiTimeZoneInfo()
	    {
		    return TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
		}

	    public static TimeZoneInfo MountainTimeZoneInfo()
	    {
			return TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
		}

	    public static TimeZoneInfo IranTimeZoneInfo()
	    {
		    return TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
	    }

	    public static TimeZoneInfo Kathmandu()
	    {
		    return TimeZoneInfo.FindSystemTimeZoneById("Nepal Standard Time");
	    }
    }
}
