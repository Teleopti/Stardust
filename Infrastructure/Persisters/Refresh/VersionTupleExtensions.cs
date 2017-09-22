using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
			var versionItem1 = versionTuple.Item1 as IVersioned;
			var versionItem2 = versionTuple.Item2 as IVersioned;
            return (versionItem1 != null && versionItem2 != null &&
                    versionItem1.Version < versionItem2.Version);
        }

		internal static bool HasSameVersion(this Tuple<IPersistableScheduleData, IPersistableScheduleData> versionTuple)
		{
			var versionItem1 = versionTuple.Item1 as IVersioned;
			var versionItem2 = versionTuple.Item2 as IVersioned;
			return (versionItem1 != null && versionItem2 != null &&
					versionItem1.Version == versionItem2.Version);
		}

		internal static bool IsInsertByMessage(this Tuple<IPersistableScheduleData, IPersistableScheduleData> versionTuple)
        {
            return versionTuple.Item1 == null;
        }
    }
}