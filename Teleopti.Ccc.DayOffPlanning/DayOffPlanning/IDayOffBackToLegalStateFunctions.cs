using System.Collections.Generic;
using System.Drawing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Secret.DayOffPlanning
{
    public interface IDayOffBackToLegalStateFunctions
    {
        ILockableBitArray WorkingArray { get; set; }
        IList<Point> WeekendList();
        int FindFirstIndexOfFirstWeekendWithNoDayOff(IList<Point> weekendList, bool considerLocks);
        bool IsWeekendDay(int indexToCheck, IList<Point> weekendList);
        int FindFirstNonWeekendDayWithNoDayOff(IList<Point> weekendList);
        int FindFirstNonWeekendDayWithDayOff(IList<Point> weekendList, bool considerLocks);
        int FindFirstWeekendDayWithNoDayOff(IList<Point> weekendList, bool considerLocks);
        int FindFirstWeekendDayDayOff(IList<Point> weekendList, bool considerLocks);
        int FindFirstNonLockedSingleDayWeekendDayOff(IList<Point> weekendList, bool considerLocks);
        int FindFirstNoneLockedAccompanyingSingleDayWeekendDayOff(IList<Point> weekendList);
        Point FindLongestConsecutiveWorkdayBlockWithAtLeastOneUnlockedBit();
        Point FindShortestConsecutiveWorkdayBlockWithAtLeastOneMovableBitBeforeOrAfter();
        int FindNextIndexWithNoDayOff(int indexToStartWith, bool considerLock);
        IDictionary<int, int> CreateWeeklyDaysOffsDictionary(bool arrayIncludesWeekBefore, bool arrayIncludesWeekAfter);
        int FindFirstWeekdayIndexWithDayOffButRatherNotWeekend(int weekIndex, IList<Point> weekendList, bool considerLocks);
        int FindFirstWeekdayIndexWithNoDayOffButRatherNotWeekend(int weekIndex, IList<Point> weekendList, bool considerLocks);
        int FindRandomNonLockedNonWeekendDayWithNoDayOff(IList<Point> weekendList, int maxIterations);
        int FindFirstIndexOfWeek(int index);
        Point FindFirstDayOffBlockWithUnlockedDayOff(int minBlockLength, int maxBlockLength);
        int FindFirstUnlockedIndex(int startIndex, int endIndex);
        int FindFirstBestIndexToMoveDaysOffFromLocksConsidered(IDictionary<int, int> weeklyList, IList<Point> weekendList);
        IEnumerable<int> PeriodWeekStartDayIndexes(bool arrayIncludesWeekBefore, bool arrayIncludesWeekAfter);
        int FindLastUnlockedIndexWithNoDayOffInBlock(Point block);
        int FindFirstUnlockedIndexWithNoDayOffInBlock(Point block);
        Point FindUnlockedDaysOffSurroundingBlock(Point block);
    }
}