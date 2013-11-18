﻿using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Refresh
{
    internal static class VersionTupleExtensions
    {
		internal static bool IsDeletedByMessage(this Tuple<INonversionedPersistableScheduleData, INonversionedPersistableScheduleData> versionTuple)
        {
            return versionTuple.Item2 == null;
        }

		internal static bool IsUpdateByMessage(this Tuple<INonversionedPersistableScheduleData, INonversionedPersistableScheduleData> versionTuple)
		{
			var versionItem1 = versionTuple.Item1 as IPersistableScheduleData;
			var versionItem2 = versionTuple.Item2 as IPersistableScheduleData;
            return (versionItem1 != null && versionItem2 != null &&
                    versionItem1.Version < versionItem2.Version);
        }

		internal static bool IsInsertByMessage(this Tuple<INonversionedPersistableScheduleData, INonversionedPersistableScheduleData> versionTuple)
        {
            return versionTuple.Item1 == null;
        }
    }
}