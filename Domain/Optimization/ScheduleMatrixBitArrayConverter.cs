using System.Collections;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Converts the scheduleMatrix to Bitarray
    /// </summary>
    public class ScheduleMatrixBitArrayConverter : IScheduleMatrixBitArrayConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public BitArray OuterWeekPeriodDayOffsBitArray(IScheduleMatrixPro matrix)
        {
            var result = new BitArray(matrix.OuterWeeksPeriodDays.Count);
            int index = 0;
            foreach (IScheduleDayPro day in matrix.OuterWeeksPeriodDays)
            {
				SchedulePartView significant = day.DaySchedulePart().SignificantPart();
				result.Set(index, ((significant == SchedulePartView.DayOff) || (significant == SchedulePartView.ContractDayOff)));
                index++;
            }
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public BitArray OuterWeekPeriodLockedDaysBitArray(IScheduleMatrixPro matrix)
        {
            var result = new BitArray(matrix.OuterWeeksPeriodDays.Count);
            int index = 0;
            foreach (IScheduleDayPro day in matrix.OuterWeeksPeriodDays)
            {
                result.Set(index, !matrix.UnlockedDays.Contains(day));
                index++;
            }
            return result;
        }

        public MinMax<int> PeriodIndexRange(IScheduleMatrixPro matrix)
        {
            return new MinMax<int>(periodStartIndex(matrix), periodLastIndex(matrix));
        }

        private static int periodStartIndex(IScheduleMatrixPro matrix)
        {
            int result = -1;
            DateOnly fistDay = matrix.EffectivePeriodDays[0].Day;
            for (int i = 0; i < matrix.FullWeeksPeriodDays.Count; i++)
            {
                if (matrix.FullWeeksPeriodDays[i].Day == fistDay)
                {
                    result = i + 7;
                    break;
                }
            }
            return result;
        }

        private static int periodLastIndex(IScheduleMatrixPro matrix)
        {
            int result = -1;
            DateOnly lastDay = matrix.EffectivePeriodDays[matrix.EffectivePeriodDays.Count-1].Day;
            for (int i = matrix.FullWeeksPeriodDays.Count - 1; i >= 0; i--)
            {
                if (matrix.FullWeeksPeriodDays[i].Day == lastDay)
                {
                    result = i + 7;
                    break;
                }
            }
            return result;
        }
    }
}
