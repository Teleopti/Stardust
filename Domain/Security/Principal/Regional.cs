using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public class Regional : IRegional
    {
        public ICccTimeZoneInfo TimeZone { get; set; }
        public CultureInfo Culture { get; set; }
        public CultureInfo UICulture { get; set; }

        public Regional(ICccTimeZoneInfo defaultTimeZone, CultureInfo culture, CultureInfo uiCulture)
        {
            TimeZone = defaultTimeZone;
            Culture = culture;
            UICulture = uiCulture;
        }
    }
}
