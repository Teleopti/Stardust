using System;
using System.Linq;
using System.Reflection;
using NodaTime.TimeZones;

namespace Teleopti.Ccc.Web.Core
{
    public class IanaTimeZoneProvider : IIanaTimeZoneProvider
    {
        private readonly TzdbDateTimeZoneSource _tzdbSource;

        public IanaTimeZoneProvider()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var tzdbStream = assembly.GetManifestResourceStream("Teleopti.Ccc.Web.tzdb2016c.nzd");
            _tzdbSource = tzdbStream == null ? TzdbDateTimeZoneSource.Default : TzdbDateTimeZoneSource.FromStream(tzdbStream);
        }

        // This will return the Windows zone that matches the IANA zone, if one exists.
        public string IanaToWindows(string ianaZoneId)
        {
            var utcZones = new[] { "Etc/UTC", "Etc/UCT" };
            if (utcZones.Contains(ianaZoneId, StringComparer.OrdinalIgnoreCase))
                return "UTC";

           
            // resolve any link, since the CLDR doesn't necessarily use canonical IDs
            var links = _tzdbSource.CanonicalIdMap
              .Where(x => x.Value.Equals(ianaZoneId, StringComparison.OrdinalIgnoreCase))
              .Select(x => x.Key);

            var mappings = _tzdbSource.WindowsMapping.MapZones;
            var item = mappings.FirstOrDefault(x => x.TzdbIds.Any(links.Contains));
            if (item == null) return null;
            return item.WindowsId;
        }

        // This will return the "primary" IANA zone that matches the given windows zone.
        // If the primary zone is a link, it then resolves it to the canonical ID.
        public string WindowsToIana(string windowsZoneId)
        {
            if (windowsZoneId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
                return "Etc/UTC";
       var tzi = TimeZoneInfo.FindSystemTimeZoneById(windowsZoneId);
            var tzid = _tzdbSource.MapTimeZoneId(tzi);
            return _tzdbSource.CanonicalIdMap[tzid];
        }
    }
}