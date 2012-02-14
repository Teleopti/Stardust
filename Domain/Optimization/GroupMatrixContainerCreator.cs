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
        /// <param name="ruleSet">The rule set.</param>
        /// <returns></returns>
        GroupMatrixContainer CreateGroupMatrixContainer(
            IList<DateOnly> daysOffToRemove,
            IList<DateOnly> daysOffToAdd,
            IScheduleMatrixPro scheduleMatrix,
            DayOffPlannerSessionRuleSet ruleSet);
    }

    public class GroupMatrixContainerCreator : IGroupMatrixContainerCreator
    {
        public GroupMatrixContainer CreateGroupMatrixContainer(
            IList<DateOnly> daysOffToRemove,
            IList<DateOnly> daysOffToAdd,
            IScheduleMatrixPro scheduleMatrix,
            DayOffPlannerSessionRuleSet ruleSet)
        {
            if (ruleSet == null) return null;
            IScheduleMatrixLockableBitArrayConverter bitArrayConverter = new ScheduleMatrixLockableBitArrayConverter(scheduleMatrix);
            ILockableBitArray personOriginalArray =
                bitArrayConverter.Convert(ruleSet.ConsiderWeekBefore,
                                          ruleSet.ConsiderWeekAfter);
            ILockableBitArray personWorkingArray = (LockableBitArray)personOriginalArray.Clone();
            if (!setDayOffBitsInBitArrays(daysOffToRemove, daysOffToAdd, scheduleMatrix, personWorkingArray, ruleSet))
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
            DayOffPlannerSessionRuleSet ruleSet)
        {
            foreach (var dateOnly in daysOffToRemove)
            {
                int index;
                if (!ruleSet.ConsiderWeekBefore)
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
                if (!ruleSet.ConsiderWeekBefore)
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