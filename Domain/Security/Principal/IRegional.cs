using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public interface IRegional
    {
        ICccTimeZoneInfo TimeZone { get; }
        CultureInfo UICulture { get; }
        CultureInfo Culture { get; }
    }
}