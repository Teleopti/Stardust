using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupMatrixContainerCreator
    {
        /// <summary>
        /// Factory class to create a group matrix container.
        /// </summary>
        /// <param name="daysOffToRemove">The days off to remove.</param>
        /// <param name="daysOffToAdd">The days off to add.</param>
        /// <param name="scheduleMatrix">The schedule matrix.</param>
        /// <param name="daysOffPreferences">The days off preferences.</param>
        /// <returns></returns>
        GroupMatrixContainer CreateGroupMatrixContainer(
            IList<DateOnly> daysOffToRemove,
            IList<DateOnly> daysOffToAdd,
            IScheduleMatrixPro scheduleMatrix,
            IDaysOffPreferences daysOffPreferences);
    }

    public class GroupMatrixContainerCreator : IGroupMatrixContainerCreator
    {
        public GroupMatrixContainer CreateGroupMatrixContainer(
            IList<DateOnly> daysOffToRemove,
            IList<DateOnly> daysOffToAdd,
            IScheduleMatrixPro scheduleMatrix,
            IDaysOffPreferences daysOffPreferences)
        {
            if (daysOffPreferences == null) return null;
            IScheduleMatrixLockableBitArrayConverter bitArrayConverter = new ScheduleMatrixLockableBitArrayConverter(scheduleMatrix);
            ILockableBitArray personOriginalArray =
                bitArrayConverter.Convert(daysOffPreferences.ConsiderWeekBefore,
                                          daysOffPreferences.ConsiderWeekAfter);
            ILockableBitArray personWorkingArray = (LockableBitArray)personOriginalArray.Clone();
            if (!setDayOffBitsInBitArrays(daysOffToRemove, daysOffToAdd, scheduleMatrix, personWorkingArray, daysOffPreferences))
                return null;

            var matrixContainer = new GroupMatrixContainer
                                      {
                                          Matrix = scheduleMatrix,
                                          OriginalArray = personOriginalArray,
                                          WorkingArray = personWorkingArray
                                      };
            return matrixContainer;
        }

        private static bool setDayOffBitsInBitArrays(
            IEnumerable<DateOnly> daysOffToRemove,
            IEnumerable<DateOnly> daysOffToAdd,
            IScheduleMatrixPro scheduleMatrix,
            ILockableBitArray personWorkingArray,
            IDaysOffPreferences daysOffPreferences)
        {
            foreach (var dateOnly in daysOffToRemove)
            {
                int index;
                if (!daysOffPreferences.ConsiderWeekBefore)
                {
                    index =
                        scheduleMatrix.FullWeeksPeriodDays.IndexOf(
                            scheduleMatrix.GetScheduleDayByKey(dateOnly));
                }
                else
                {
                    index =
                        scheduleMatrix.OuterWeeksPeriodDays.IndexOf(
                            scheduleMatrix.GetScheduleDayByKey(dateOnly));
                }

                if (personWorkingArray.IsLocked(index, true))
                    return false;
                personWorkingArray.Set(index, false);

            }
            foreach (var dateOnly in daysOffToAdd)
            {
                int index;
                if (!daysOffPreferences.ConsiderWeekBefore)
                {
                    index =
                        scheduleMatrix.FullWeeksPeriodDays.IndexOf(
                            scheduleMatrix.GetScheduleDayByKey(dateOnly));
                }
                else
                {
                    index =
                        scheduleMatrix.OuterWeeksPeriodDays.IndexOf(
                            scheduleMatrix.GetScheduleDayByKey(dateOnly));
                }

                if (personWorkingArray.IsLocked(index, true))
                    return false;
                personWorkingArray.Set(index, true);

            }
            return true;
        }
    }
}