using System;
using System.Globalization;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public interface IRegional
    {
        TimeZoneInfo TimeZone { get; }
        CultureInfo UICulture { get; }
        CultureInfo Culture { get; }
    }
}