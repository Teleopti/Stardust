using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class AffectedDayOffs
	{
		private readonly ILockableBitArrayChangesTracker _lockableBitArrayChangesTracker;

		public AffectedDayOffs(ILockableBitArrayChangesTracker lockableBitArrayChangesTracker)
		{
			_lockableBitArrayChangesTracker = lockableBitArrayChangesTracker;
		}

		public MovedDaysOff Execute(IScheduleMatrixPro matrix, IDaysOffPreferences daysOffPreferences, ILockableBitArray originalArray, ILockableBitArray resultingArray)
		{
			if (originalArray.HasSameDayOffs(resultingArray))
				return null;

			bool considerWeekBefore = daysOffPreferences.ConsiderWeekBefore;
			// find out what have changed, Does the predictor beleve in this? depends on how many members
			IList<DateOnly> addedDaysOff = _lockableBitArrayChangesTracker.DaysOffAdded(resultingArray, originalArray, matrix,
				considerWeekBefore);
			IList<DateOnly> removedDaysOff = _lockableBitArrayChangesTracker.DaysOffRemoved(resultingArray, originalArray, matrix,
				considerWeekBefore);

			return new MovedDaysOff
			{
				AddedDaysOff = addedDaysOff,
				RemovedDaysOff = removedDaysOff
			};
		}
	}
}