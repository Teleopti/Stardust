using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    /// <summary>
    /// Distributes a number of day off in a period.
    /// </summary>
    public class DayOffInitialDistribution
    {

        private readonly IOfficialWeekendDays _officialWeekendDays;

        public DayOffInitialDistribution(IOfficialWeekendDays officialWeekendDays)
        {
            _officialWeekendDays = officialWeekendDays;
        }

        public void DistributeDayOffsEvenly(ILockableBitArray period)
        {
            int movableDayOffs = CountUnlockedDayOffs(period);
            ResetUnlockedDayOffsInPeriodArea(period);
            IList<int> priorityList = DayoffDistributionPriorityList();
            int priorityListCounter = 0;
            while (movableDayOffs>0 || priorityListCounter<priorityList.Count)
            {
                int nextPriorityIndex = priorityList[priorityListCounter];
                movableDayOffs = DistributeDayOffWithPriorityIndex(period, nextPriorityIndex, movableDayOffs);
                priorityListCounter++;
            }
        }

        private IList<int> DayoffDistributionPriorityList()
        {
            IList<int> resultList = new List<int> { 6, 5, 4, 3, 2, 1, 0 };
            for (int i = 0; i < _officialWeekendDays.WeekendDayIndexes().Count; i++)
            {
                int currentValue = _officialWeekendDays.WeekendDayIndexes()[i];
                resultList.Remove(currentValue);
                resultList.Insert(0, currentValue);
            }
            return resultList;
        }

        private static int CountUnlockedDayOffs(ILockableBitArray period)
        {
            int count = 0;
            for (int i = period.PeriodArea.Minimum; i < period.PeriodArea.Maximum; i++)
            {
                if(!period.IsLocked(i, true)
                    && period.DaysOffBitArray[i])
                {
                    count++;
                }
            }
            return count;
        }

        private static void ResetUnlockedDayOffsInPeriodArea(ILockableBitArray period)
        {
            for (int i = period.PeriodArea.Minimum; i < period.PeriodArea.Maximum; i++)
            {
                if(!period.IsLocked(i, true)
                    && period.DaysOffBitArray[i])
                {
                    period.Set(i, false);
                }
            }
        }

        private static int DistributeDayOffWithPriorityIndex(ILockableBitArray period, int priorityIndex, int daysToDistribute)
        {
            while(priorityIndex <= period.PeriodArea.Maximum
                  && daysToDistribute > 0)
            {
                if(priorityIndex >= period.PeriodArea.Minimum 
                   && !period.IsLocked(priorityIndex, true))
                {
                    period.Set(priorityIndex, true);
                    daysToDistribute--;
                }
                priorityIndex += 7;
            }
            return daysToDistribute;
        }
    }
}
