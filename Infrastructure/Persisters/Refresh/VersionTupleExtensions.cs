using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Refresh
{
    internal static class VersionTupleExtensions
    {
        internal static bool IsDeletedByMessage(this Tuple<IPersistableScheduleData, IPersistableScheduleData> versionTuple)
        {
            return versionTuple.Item2 == null;
        }

        internal static bool IsUpdateByMessage(this Tuple<IPersistableScheduleData, IPersistableScheduleData> versionTuple)
        {
            return (versionTuple.Item1 != null && versionTuple.Item2 != null &&
                    versionTuple.Item1.Version < versionTuple.Item2.Version);
        }

        internal static bool IsInsertByMessage(this Tuple<IPersistableScheduleData, IPersistableScheduleData> versionTuple)
        {
            return versionTuple.Item1 == null;
        }
    }
}