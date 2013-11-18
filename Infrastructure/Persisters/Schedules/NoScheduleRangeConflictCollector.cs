﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class NoScheduleRangeConflictCollector : IScheduleRangeConflictCollector
	{
		public IEnumerable<PersistConflict> GetConflicts(IDifferenceCollection<INonversionedPersistableScheduleData> differences, IScheduleParameters scheduleParameters)
		{
			return Enumerable.Empty<PersistConflict>();
		}
	}
}