using System.Collections.Generic;
using System.Drawing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Secret.DayOffPlanning
{
    public class DayOffBackToLegalStateFunctions : IDayOffBackToLegalStateFunctions
    {
        private ILockableBitArray _bitArray;
        
		public DayOffBackToLegalStateFunctions()
		{}

        public DayOffBackToLegalStateFunctions(ILockableBitArray bitArray)
        {
            _bitArray = bitArray;
        }

        public ILockableBitArray WorkingArray
        {
            get { return _bitArray; }
            set { _bitArray = value; }
        }

        public IList<Point> WeekendList()
        {
            IList<Point> weekEnds = new List<Point>();
            for (int i = 0; i < _bitArray.Count; i++)
            {
                if ((i + 1) % 7 == 0)
                {
                    weekEnds.Add(new Point(i - 1, i));
                }
            }

            return weekEnds;
        }

        public int FindFirstIndexOfFirstWeekendWithNoDayOff(IList<Point> weekendList, bool considerLocks)
        {
            for (int i = 0; i < weekendList.Count; i++)
            {
                if (!_bitArray[weekendList[i].X] && !_bitArray[weekendList[i].Y])
                {
                    if (!_bitArray.IsLocked(weekendList[i].X, considerLocks))
                        return weekendList[i].X;
                    if (!_bitArray.IsLocked(weekendList[i].Y, considerLocks))
                        return weekendList[i].Y;
                }

            }

            return -1;
        }

        public bool IsWeekendDay(int indexToCheck, IList<Point> weekendList)
        {
            foreach (var point in weekendList)
            {
                if (indexToCheck == point.X || indexToCheck == point.Y)
                {
                    return true;
                }
            }

            return false;
        }

        public int FindFirstNonWeekendDayWithNoDayOff(IList<Point> weekendList)
        {
            for (int i = 0; i < _bitArray.Count; i++)
            {
                if (!_bitArray[i] && !IsWeekendDay(i, weekendList))
                    return i;
            }

            return -1;
        }

        public int FindFirstNonWeekendDayWithDayOff(IList<Point> weekendList, bool considerLocks)
        {
            for (int i = 0; i < _bitArray.Count; i++)
            {
                if (_bitArray[i] && !IsWeekendDay(i, weekendList) && !_bitArray.IsLocked(i, considerLocks))
                    return i;
            }

            return -1;
        }

        public int FindFirstWeekendDayWithNoDayOff(IList<Point> weekendList, bool considerLocks)
        {
            for (int i = 0; i < weekendList.Count; i++)
            {
                if (!_bitArray[weekendList[i].X] && !_bitArray.IsLocked(weekendList[i].X, considerLocks))
                    return weekendList[i].X;
                if (!_bitArray[weekendList[i].Y] && !_bitArray.IsLocked(weekendList[i].Y, considerLocks))
                    return weekendList[i].Y;
            }

            return -1;
        }

        public int FindFirstWeekendDayDayOff(IList<Point> weekendList, bool considerLocks)
        {
            for (int i = 0; i < weekendList.Count; i++)
            {
                if (_bitArray[weekendList[i].X] && !_bitArray.IsLocked(weekendList[i].X, considerLocks))
                    return weekendList[i].X;
                if (_bitArray[weekendList[i].Y] && !_bitArray.IsLocked(weekendList[i].Y, considerLocks))
                    return weekendList[i].Y;
            }

            return -1;
        }

        public int FindFirstNonLockedSingleDayWeekendDayOff(IList<Point> weekendList, bool considerLocks)
        {
            int index = -1;
            for (int i = 0; i < weekendList.Count; i++)
            {
                if (_bitArray[weekendList[i].X] ^ _bitArray[weekendList[i].Y])
                {
                    if (_bitArray[weekendList[i].X] && !_bitArray.IsLocked(weekendList[i].X, considerLocks))
                        index = weekendList[i].X;
                    else
                    {
                        if (_bitArray[weekendList[i].Y] && !_bitArray.IsLocked(weekendList[i].Y, considerLocks))
                            index = weekendList[i].Y;
                    }

                    if (index != -1)
                        break;
                }
            }
            return index;
        }

        public int FindFirstNoneLockedAccompanyingSingleDayWeekendDayOff(IList<Point> weekendList)
        {
            int index = -1;
            for (int i = 0; i < weekendList.Count; i++)
            {
                if (_bitArray[weekendList[i].X] ^ _bitArray[weekendList[i].Y])
                {
                    if (_bitArray[weekendList[i].X] && !_bitArray.IsLocked(weekendList[i].Y, true))
                        index = weekendList[i].Y;
                    else
                        if (!_bitArray.IsLocked(weekendList[i].X, true))
                            index = weekendList[i].X;

                    if (index != -1)
                        break;
                }
            }
            return index;
        }

        public Point FindLongestConsecutiveWorkdayBlockWithAtLeastOneUnlockedBit()
        {
            Point ret = new Point(0, 0);
            int blockStartIndex = -1;
            int blockEndIndex = -1;
	        int startIndex = 0;
	        if (_bitArray.PeriodArea.Minimum >= 6)
	        {
		        bool dayOffFound = false;
		        for (int i = 0; i < 7; i++)
		        {
			        if (!_bitArray.DaysOffBitArray[i]) 
						continue;

			        dayOffFound = true;
			        break;
		        }

		        if (!dayOffFound)
		        {
					startIndex = 7;
		        }
	        }

	        int endIndex = _bitArray.Count;
	        if (_bitArray.PeriodArea.Maximum < _bitArray.Count - 7)
			{
				bool dayOffFound = false;
				for (int i = _bitArray.Count - 1; i >= _bitArray.Count - 7; i--)
				{
					if (!_bitArray.DaysOffBitArray[i])
						continue;

					dayOffFound = true;
					break;
				}

				if (!dayOffFound)
				{
					endIndex = _bitArray.Count - 8;
				}
			}

			for (int i = startIndex; i < endIndex; i++)
            {
                if (!_bitArray[i] && blockStartIndex == -1)
                    blockStartIndex = i;
                if (_bitArray[i] && blockStartIndex != -1)
                {
                    blockEndIndex = i - 1;
                    if (blockEndIndex - blockStartIndex > ret.Y - ret.X)
                    {
                        if (FindFirstUnlockedIndex(blockStartIndex, blockEndIndex) != -1)
                            ret = new Point(blockStartIndex, blockEndIndex);
                    }
                    blockStartIndex = -1;
                    blockEndIndex = -1;
                }
            }
            if (blockEndIndex == -1 && blockStartIndex != -1)
                blockEndIndex = endIndex;
	        if (blockEndIndex == _bitArray.Count)
		        blockEndIndex--;
            if (blockEndIndex - blockStartIndex > ret.Y - ret.X)
            {
                if (FindFirstUnlockedIndex(blockStartIndex, blockEndIndex) != -1)
					ret = new Point(blockStartIndex, blockEndIndex);
            }

            return ret;
        }

        public Point FindShortestConsecutiveWorkdayBlockWithAtLeastOneMovableBitBeforeOrAfter()
        {
            
            Point ret = new Point(0, int.MaxValue);
            int blockStartIndex = -1;
            int blockEndIndex = -1;
            for (int i = 0; i < _bitArray.Count; i++)
            {
                if (!_bitArray[i] && blockStartIndex == -1)
                    blockStartIndex = i;
                if (_bitArray[i] && blockStartIndex != -1)
                {
                    blockEndIndex = i - 1;
                    if (blockEndIndex - blockStartIndex < ret.Y - ret.X && blockEndIndex != _bitArray.Count)
                    {
                        if (blockStartIndex > 0 && blockEndIndex < (_bitArray.Count - 1) && !_bitArray.IsLocked(blockEndIndex + 1, true))
                            ret = new Point(blockStartIndex, blockEndIndex);
                        if (blockStartIndex > 0 && !_bitArray.IsLocked(blockStartIndex - 1, true))
                            ret = new Point(blockStartIndex, blockEndIndex);
                    }

                    blockStartIndex = -1;
                    blockEndIndex = -1;
                }
            }
            if (blockEndIndex == -1 && blockStartIndex > 0 && !_bitArray.IsLocked(blockStartIndex - 1, true))
                blockEndIndex = _bitArray.Count - 1;
            if (blockEndIndex != -1 && blockStartIndex != -1 && (blockEndIndex - blockStartIndex < ret.Y - ret.X))
            {
                if (!_bitArray.IsLocked(blockStartIndex - 1, true) || !_bitArray.IsLocked(blockEndIndex + 1, true))
                {
                    if (blockEndIndex < _bitArray.Count - 1)
                        ret = new Point(blockStartIndex, blockEndIndex);
                }
            }

            if (ret == new Point(0, int.MaxValue))
                ret = new Point(-1, -1);

            return ret;
        }

        public int FindNextIndexWithNoDayOff(int indexToStartWith, bool considerLock)
        {
            int indexToMoveTo = -1;
            //find next free
            for (int i = indexToStartWith; i < _bitArray.Count; i++)
            {
                if (!_bitArray[i] && !_bitArray.IsLocked(i, considerLock))
                {
                    indexToMoveTo = i;
                    break;
                }
            }
            if (indexToMoveTo == -1)
            {
                for (int i = 0; i < _bitArray.Count; i++)
                {
                    if (!_bitArray[i] && !_bitArray.IsLocked(i, considerLock))
                    {
                        indexToMoveTo = i;
                        break;
                    }
                }
            }

            return indexToMoveTo;
        }

        public IDictionary<int, int> CreateWeeklyDaysOffsDictionary(bool arrayIncludesWeekBefore, bool arrayIncludesWeekAfter)
        {
            IDictionary<int, int> retList = new Dictionary<int, int>();
            int key = 0;
            foreach (int weekStartDayIndex in PeriodWeekStartDayIndexes(arrayIncludesWeekBefore, arrayIncludesWeekAfter))
            {
                int dayOffCounter = 0;
                for (int i = 0; i < 7; i++)
                {
                    if (_bitArray[i + weekStartDayIndex])
                        dayOffCounter++;
                }
                retList.Add(key, dayOffCounter);
                key++;
            }
            return retList;
        }

        /// <summary>
        /// Periods the week start day indexes. It comes back with the same set of weekstart indexes regardles the arrayIncludesWeekAfter
        /// arrayIncludesWeekBefore settings
        /// </summary>
        /// <param name="arrayIncludesWeekBefore">if set to <c>true</c> [array includes week before].</param>
        /// <param name="arrayIncludesWeekAfter">if set to <c>true</c> [array includes week after].</param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public IEnumerable<int> PeriodWeekStartDayIndexes(bool arrayIncludesWeekBefore, bool arrayIncludesWeekAfter)
        {
            int startDayIndex = 0;
            int endDayIndex = _bitArray.Count - 1;
            if (arrayIncludesWeekBefore)
                startDayIndex = 7;
            if (arrayIncludesWeekAfter)
                endDayIndex = endDayIndex - 7;
            for (int w = startDayIndex; w < endDayIndex; w += 7)
            {
                yield return w;
            }
        }

        public int FindFirstWeekdayIndexWithDayOffButRatherNotWeekend(int weekIndex, IList<Point> weekendList, bool considerLocks)
        {
            int indexToMove = -1;
            int alternateIndexToMove = -1;
            int baseIndex = ((short)weekIndex * 7);
            for (int i = 0; i < 7; i++)
            {
                if (_bitArray[i + baseIndex])
                {
                    if (!_bitArray.IsLocked(i + baseIndex, considerLocks))
                        alternateIndexToMove = i + baseIndex;
                    if (!IsWeekendDay(i + baseIndex, weekendList))
                    {
                        if (!_bitArray.IsLocked(i + baseIndex, considerLocks))
                        {
                            indexToMove = i + baseIndex;
                            break;
                        }
                    }
                }
            }
            if (indexToMove == -1)
                indexToMove = alternateIndexToMove;

            return indexToMove;
        }

        public int FindFirstWeekdayIndexWithNoDayOffButRatherNotWeekend(int weekIndex, IList<Point> weekendList, bool considerLocks)
        {
            int indexToMove = -1;
            int alternateIndexToMove = -1;
            int baseIndex = ((short)weekIndex * 7);
            for (int i = 0; i < 7; i++)
            {
                if (!_bitArray[i + baseIndex])
                {
                    if (!_bitArray.IsLocked(i + baseIndex, considerLocks))
                        alternateIndexToMove = i + baseIndex;
                    if (!IsWeekendDay(i + baseIndex, weekendList))
                    {
                        if (!_bitArray.IsLocked(i + baseIndex, considerLocks))
                        {
                            indexToMove = i + baseIndex;
                            break;
                        }
                    }
                }
            }
            if (indexToMove == -1)
                indexToMove = alternateIndexToMove;

            return indexToMove;
        }

        public static int FindWeekWithLeastNumberOfDaysOff(IDictionary<int, int> weeklyList)
        {
            int minDays = int.MaxValue;
            int weekIndex = -1;
            foreach (KeyValuePair<int, int> keyValuePair in weeklyList)
            {
                if (weeklyList[keyValuePair.Key] < minDays)
                {
                    minDays = weeklyList[keyValuePair.Key];
                    weekIndex = keyValuePair.Key;
                }
            }

            return weekIndex;
        }

        public static int FindWeekWithMostNumberOfDays(IDictionary<int, int> weeklyList)
        {
            int maxDays = int.MinValue;
            int weekIndex = -1;
            foreach (KeyValuePair<int, int> keyValuePair in weeklyList)
            {
                if (weeklyList[keyValuePair.Key] > maxDays)
                {
                    maxDays = weeklyList[keyValuePair.Key];
                    weekIndex = keyValuePair.Key;
                }
            }

            return weekIndex;
        }

        public int FindRandomNonLockedNonWeekendDayWithNoDayOff(IList<Point> weekendList, int maxIterations)
        {
            int iterationCount = 0;

            while (iterationCount < maxIterations)
            {
                iterationCount++;
                int randomIndex = _bitArray.FindRandomUnlockedIndex();
                if (randomIndex == -1)
                    return -1;

                if (!_bitArray[randomIndex] && !IsWeekendDay(randomIndex, weekendList))
                    return randomIndex;
            }
            return -1;
        }

        public int FindFirstIndexOfWeek(int index)
        {
            int week = index / 7;
            return week * 7;
        }

        public Point FindFirstDayOffBlockWithUnlockedDayOff(int minBlockLength, int maxBlockLength)
        {
            int startIndex = -1;
            int endIndex = -1;
            for (int i = 0; i < _bitArray.Count; i++)
            {
                if (_bitArray[i])
                {
                    if (startIndex == -1)
                        startIndex = i;
                }
                else
                {
                    if (startIndex != -1)
                    {
                        endIndex = i - 1;
                        int length = endIndex - startIndex + 1;
                        if (length >= minBlockLength && length <= maxBlockLength)
                        {
                            if (FindFirstUnlockedIndex(startIndex, endIndex) != -1)
                                return new Point(startIndex, endIndex);
                        }
                        startIndex = -1;
                        endIndex = -1;
                    }
                }
            }
            if (endIndex == -1 && startIndex != -1)
            {
                endIndex = _bitArray.Count - 1;
                int length = endIndex - startIndex + 1;
                if (length >= minBlockLength && length <= maxBlockLength)
                {
                    if (FindFirstUnlockedIndex(startIndex, endIndex) != -1)
                        return new Point(startIndex, endIndex);
                }
            }

            return new Point(-1, -1);
        }

        public int FindFirstUnlockedIndex(int startIndex, int endIndex)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (!_bitArray.IsLocked(i, true))
                    return i;
            }
            return -1;
        }

        public int FindFirstBestIndexToMoveDaysOffFromLocksConsidered(IDictionary<int, int> weeklyList, IList<Point> weekendList)
        {
            int indexToMoveFrom = -1;
            do
            {
                if (weeklyList.Count == 0)
                    break;
                int weekToMoveFrom = FindWeekWithMostNumberOfDays(weeklyList);
                indexToMoveFrom = FindFirstWeekdayIndexWithDayOffButRatherNotWeekend(weekToMoveFrom, weekendList, true);
                weeklyList.Remove(weekToMoveFrom);

            } while (indexToMoveFrom == -1);

            return indexToMoveFrom;
        }

        public int FindLastUnlockedIndexWithNoDayOffInBlock(Point block)
        {
            int indexToMoveTo = -1;
            //find next free
            for (int i = block.Y; i >= block.X; i--)
            {
                if (!_bitArray[i] && !_bitArray.IsLocked(i, true))
                {
                    indexToMoveTo = i;
                    break;
                }
            }

            return indexToMoveTo;
        }

        public int FindFirstUnlockedIndexWithNoDayOffInBlock(Point block)
        {
            int indexToMoveTo = -1;
            //find next free
            for (int i = block.X; i <= block.Y; i++)
            {
                if (!_bitArray[i] && !_bitArray.IsLocked(i, true))
                {
                    indexToMoveTo = i;
                    break;
                }
            }

            return indexToMoveTo;
        }

        public Point FindUnlockedDaysOffSurroundingBlock(Point block)
        {
            Point result = new Point(-1, -1);
            if (block.X > 0)
            {
                if (!_bitArray.IsLocked(block.X - 1, true))
                    result.X = block.X - 1;
            }

            if (block.Y < _bitArray.Count - 1)
            {
                if (!_bitArray.IsLocked(block.Y + 1, true))
                    result.Y = block.Y + 1;
            }

            return result;
        }
    }
}