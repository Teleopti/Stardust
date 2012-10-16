﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Time;

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

        public static TimeZoneInfo UtcTimeZoneInfo()
        {
            return TimeZoneInfo.Utc;
        }

        public static TimeZoneInfo HawaiiTimeZoneInfo()
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time");
        }

        public static TimeZoneInfo SingaporeTimeZoneInfo()
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
        }
    }
}
