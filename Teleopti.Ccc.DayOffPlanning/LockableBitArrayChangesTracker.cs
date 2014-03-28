﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Secret
{
    public interface ILockableBitArrayChangesTracker
    {
        IList<DateOnly> DaysOffRemoved(ILockableBitArray workingBitArray, ILockableBitArray originalBitArray, IScheduleMatrixPro matrix, bool considerWeekBefore);
        IList<DateOnly> DaysOffAdded(ILockableBitArray workingBitArray, ILockableBitArray originalBitArray, IScheduleMatrixPro matrix, bool considerWeekBefore);
        IList<DateOnly> DayOffChanges(ILockableBitArray workingBitArray, ILockableBitArray originalBitArray, IScheduleMatrixPro matrix, bool considerWeekBefore);
    }

    public class LockableBitArrayChangesTracker : ILockableBitArrayChangesTracker
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public IList<DateOnly> DaysOffRemoved(ILockableBitArray workingBitArray, ILockableBitArray originalBitArray, IScheduleMatrixPro matrix, bool considerWeekBefore)
        {
            var movedDays = new List<DateOnly>();

            int bitArrayToMatrixOffset = 0;
            if (!considerWeekBefore)
                bitArrayToMatrixOffset = 7;

            IEnumerable<int> changedIndexes = DayOffRemovedIndexChanges(workingBitArray, originalBitArray);

            foreach (int index in changedIndexes)
            {
                IScheduleDayPro scheduleDayPro =
                    matrix.OuterWeeksPeriodDays[index + bitArrayToMatrixOffset];
                movedDays.Add(scheduleDayPro.Day);
            }

            return movedDays;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public IList<DateOnly> DaysOffAdded(ILockableBitArray workingBitArray, ILockableBitArray originalBitArray, IScheduleMatrixPro matrix, bool considerWeekBefore)
        {
            var movedDays = new List<DateOnly>();

            int bitArrayToMatrixOffset = 0;
            if (!considerWeekBefore)
                bitArrayToMatrixOffset = 7;

            IEnumerable<int> changedIndexes = DayOffAddedIndexChanges(workingBitArray, originalBitArray);

            foreach (int index in changedIndexes)
            {
                IScheduleDayPro scheduleDayPro =
                    matrix.OuterWeeksPeriodDays[index + bitArrayToMatrixOffset];
                movedDays.Add(scheduleDayPro.Day);
            }

            return movedDays;
        }

        public IList<DateOnly> DayOffChanges(ILockableBitArray workingBitArray, ILockableBitArray originalBitArray, IScheduleMatrixPro matrix, bool considerWeekBefore)
        {
            var added = DaysOffAdded(workingBitArray, originalBitArray, matrix, considerWeekBefore);
            var removed = DaysOffRemoved(workingBitArray, originalBitArray, matrix, considerWeekBefore);

            foreach (var dateOnly in added)
            {
              removed.Add(dateOnly);  
            }
            
            return removed;
        }

        //public static IList<int> DayOffIndexChanges(ILockableBitArray workingBitArray, ILockableBitArray originalBitArray)
        //{
        //    IList<int> movedDays = new List<int>();

        //    for (int i = 0; i < workingBitArray.Count; i++)
        //    {
        //        if (!workingBitArray[i] && originalBitArray[i]
        //            || workingBitArray[i] && !originalBitArray[i])
        //        {
        //            movedDays.Add(i);
        //        }
        //    }
        //    return movedDays;
        //}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static IList<int> DayOffRemovedIndexChanges(ILockableBitArray workingBitArray, ILockableBitArray originalBitArray)
        {
            IList<int> movedDays = new List<int>();

            for (int i = 0; i < workingBitArray.Count; i++)
            {
                if (!workingBitArray[i] && originalBitArray[i])
                {
                    movedDays.Add(i);
                }
            }
            return movedDays;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static IList<int> DayOffAddedIndexChanges(ILockableBitArray workingBitArray, ILockableBitArray originalBitArray)
        {
            IList<int> movedDays = new List<int>();

            for (int i = 0; i < workingBitArray.Count; i++)
            {
                if (workingBitArray[i] && !originalBitArray[i])
                {
                    movedDays.Add(i);
                }
            }
            return movedDays;
        }
    }
}
