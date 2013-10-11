using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class ScheduleDictionaryModifiedCallback : IScheduleDictionaryModifiedCallback
	{
		// todo: do "UpdateCurrent" and not snapshot. 
		// The TakeSnapshot call after this actually solves the problem because current and snapshot will be the same
		// but its waaay unclear how and why it works
		// man, this is ugly!

		public void Callback(
			IScheduleDictionary scheduleDictionary,
		    IEnumerable<IPersistableScheduleData> modifiedEntities,
		    IEnumerable<IPersistableScheduleData> addedEntities,
		    IEnumerable<IPersistableScheduleData> deletedEntities
			)
		{
			DoUnsafeSnapshotUpdate(e => (ScheduleRange) scheduleDictionary[e.Person], modifiedEntities, addedEntities, deletedEntities);
			scheduleDictionary.TakeSnapshot();
		}

		public void Callback(
			IScheduleRange scheduleRange,
			IEnumerable<IPersistableScheduleData> modifiedEntities,
			IEnumerable<IPersistableScheduleData> addedEntities,
			IEnumerable<IPersistableScheduleData> deletedEntities
			)
		{
			DoUnsafeSnapshotUpdate(e => (ScheduleRange) scheduleRange, modifiedEntities, addedEntities, deletedEntities);
			scheduleRange.TakeSnapshot();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "deletedEntities")]
		private static void DoUnsafeSnapshotUpdate(Func<IPersistableScheduleData, ScheduleRange> whichRange,
		                                    IEnumerable<IPersistableScheduleData> modifiedEntities,
		                                    IEnumerable<IPersistableScheduleData> addedEntities,
		                                    IEnumerable<IPersistableScheduleData> deletedEntities
			)
		{
			modifiedEntities.ForEach(e => whichRange(e).UnsafeSnapshotUpdate(e, true));
			addedEntities.ForEach(e => whichRange(e).UnsafeSnapshotUpdate(e, true));
			deletedEntities.ForEach(e => whichRange(e).UnsafeSnapshotDelete(e.Id.Value, true));
		}
	}
}