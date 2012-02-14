using System;
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
    public static class CccTimeZoneInfoFactory
    {
        public static CccTimeZoneInfo StockholmTimeZoneInfo()
        {
            return new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
        }

        public static CccTimeZoneInfo HelsinkiTimeZoneInfo()
        {
            return new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time"));
        }

        public static CccTimeZoneInfo UtcTimeZoneInfo()
        {
            return new CccTimeZoneInfo(TimeZoneInfo.Utc);
        }

        public static CccTimeZoneInfo HawaiiTimeZoneInfo()
        {
            return new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"));
        }

        public static CccTimeZoneInfo SingaporeTimeZoneInfo()
        {
            return new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"));
        }
    }
}
