using System.Collections.Generic;
using System.Drawing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Secret.DayOffPlanning
{
    public class ConsecutiveWorkdaysSolver : IDayOffBackToLegalStateSolver
    {
        private readonly ILockableBitArray _bitArray;
        private readonly IDaysOffPreferences _daysOffPreferences;
        private readonly IDayOffBackToLegalStateFunctions _functions;
        private readonly int _maxIterations;
        private int _setConsecutiveWorkDaysToFewBackToLegalStateFrom = -1;
        private int _setConsecutiveWorkDaysToFewBackToLegalStateTo = -1;

        public ConsecutiveWorkdaysSolver(ILockableBitArray bitArray, IDayOffBackToLegalStateFunctions functions, IDaysOffPreferences daysOffPreferences, int maxIterations)
        {
            _maxIterations = maxIterations;
            _functions = functions;
            _bitArray = bitArray;
            _daysOffPreferences = daysOffPreferences;
        }

        public MinMaxNumberOfResult ResolvableState()
        {
            Point block = _functions.FindShortestConsecutiveWorkdayBlockWithAtLeastOneMovableBitBeforeOrAfter();
            if(block.Y == -1 && block.X == -1)
                return MinMaxNumberOfResult.Ok;
            if (block.Y - block.X + 1 < _daysOffPreferences.ConsecutiveWorkdaysValue.Minimum)
                return MinMaxNumberOfResult.ToFew;
            block = _functions.FindLongestConsecutiveWorkdayBlockWithAtLeastOneUnlockedBit();
            if (block.Y - block.X + 1 > _daysOffPreferences.ConsecutiveWorkdaysValue.Maximum)
                return MinMaxNumberOfResult.ToMany;

            return MinMaxNumberOfResult.Ok;
        }

        public bool SetToManyBackToLegalState()
        {
            if (ResolvableState() == MinMaxNumberOfResult.Ok)
                return false;

            int lastIndexToMoveFrom = -1;
            int lastIndexToMoveTo = -1;
            int iterationCounter = 0;
            while (ResolvableState() == MinMaxNumberOfResult.ToMany)
            {
                IDictionary<int, int> weeklyList = _functions.CreateWeeklyDaysOffsDictionary(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter);
                IList<Point> weekEndList = _functions.WeekendList();

                int indexToMoveFrom = _functions.FindFirstBestIndexToMoveDaysOffFromLocksConsidered(weeklyList, weekEndList);
                Point longestBlock = _functions.FindLongestConsecutiveWorkdayBlockWithAtLeastOneUnlockedBit();
                
                int indexToMoveTo = longestBlock.X + ((longestBlock.Y - longestBlock.X) / 2);
                if(_bitArray.IsLocked(indexToMoveTo, true))
                    indexToMoveTo = _functions.FindNextIndexWithNoDayOff(longestBlock.X, true);

                if (lastIndexToMoveFrom == indexToMoveTo && lastIndexToMoveTo == indexToMoveFrom)
                    indexToMoveTo = _functions.FindRandomNonLockedNonWeekendDayWithNoDayOff(weekEndList, _maxIterations);

                lastIndexToMoveFrom = indexToMoveFrom;
                lastIndexToMoveTo = indexToMoveTo;
                if(!SwapBits(indexToMoveFrom, indexToMoveTo))
                    return false;

                iterationCounter++;
                if (iterationCounter > _maxIterations)
                    return true;
            }

            return true;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public bool SetToFewBackToLegalState()
        {
            if (ResolvableState() == MinMaxNumberOfResult.Ok)
            {
                _setConsecutiveWorkDaysToFewBackToLegalStateFrom = -1;
                _setConsecutiveWorkDaysToFewBackToLegalStateTo = -1;
                return false;
            }

            int iterationCounter = 0;
            while (ResolvableState() == MinMaxNumberOfResult.ToFew)
            {
                IList<Point> weekEndList = _functions.WeekendList();
                Point shortestBlock = _functions.FindShortestConsecutiveWorkdayBlockWithAtLeastOneMovableBitBeforeOrAfter();
                if (shortestBlock.X == -1 || shortestBlock.Y == -1)
                    return true;

                int indexToMoveFrom = -1;
                if (shortestBlock.X <= 0)
                {
                    indexToMoveFrom = shortestBlock.Y + 1;
                    if (_bitArray.IsLocked(shortestBlock.Y + 1, true))
                        return true;
                }
                if ((shortestBlock.Y + 1) >= _bitArray.Count)
                {
                    indexToMoveFrom = shortestBlock.Y - 1;
                    if (_bitArray.IsLocked(shortestBlock.Y - 1, true))
                        return true;
                }
                if (indexToMoveFrom == -1)
                {
                    if (_bitArray.IsLocked(shortestBlock.X - 1, true))
                    {
                        if (!_bitArray.IsLocked(shortestBlock.Y + 1, true))
                            indexToMoveFrom = shortestBlock.Y + 1;
                    }
                    else
                    {
                        if (_bitArray.IsLocked(shortestBlock.Y + 1, true))
                        {
                            if (!_bitArray.IsLocked(shortestBlock.Y - 1, true))
                                indexToMoveFrom = shortestBlock.Y - 1;
                        }
                    }
                }
                if(indexToMoveFrom == -1)
                {
					if(!_bitArray.IsLocked(shortestBlock.X - 1, true))
					{
						indexToMoveFrom = shortestBlock.X - 1;
						if (_functions.IsWeekendDay(indexToMoveFrom, weekEndList))
						{
							if (!_bitArray.IsLocked(shortestBlock.Y + 1, true))
								indexToMoveFrom = shortestBlock.Y + 1;
						}
							
					}
                    
                }

				if (indexToMoveFrom == -1)
					return true;

                _bitArray.Set(indexToMoveFrom, false);
                Point longestBlock = _functions.FindLongestConsecutiveWorkdayBlockWithAtLeastOneUnlockedBit();
                int indexToMoveTo = longestBlock.X + ((longestBlock.Y - longestBlock.X) / 2);
                if (_bitArray.IsLocked(indexToMoveTo, true))
                    indexToMoveTo = _functions.FindNextIndexWithNoDayOff(longestBlock.X + 1, true);


                if (indexToMoveFrom == _setConsecutiveWorkDaysToFewBackToLegalStateFrom && indexToMoveTo == _setConsecutiveWorkDaysToFewBackToLegalStateTo)
                    indexToMoveTo = _functions.FindRandomNonLockedNonWeekendDayWithNoDayOff(weekEndList, _maxIterations);
                if (indexToMoveFrom == _setConsecutiveWorkDaysToFewBackToLegalStateTo && indexToMoveTo == _setConsecutiveWorkDaysToFewBackToLegalStateFrom)
                    indexToMoveTo = _functions.FindRandomNonLockedNonWeekendDayWithNoDayOff(weekEndList, _maxIterations);

                if(indexToMoveTo == -1)
                    return true;

                _bitArray.Set(indexToMoveTo, true);

                _setConsecutiveWorkDaysToFewBackToLegalStateFrom = indexToMoveFrom;
                _setConsecutiveWorkDaysToFewBackToLegalStateTo = indexToMoveTo;
                iterationCounter++;
                if (iterationCounter > _maxIterations)
                    return true;
            }

            return true;
        }

        public string ResolverDescriptionKey
        {
            get { return "ConsecutiveWorkdaysRule"; }
        }

        public bool SwapBits(int indexToMoveFrom, int indexToMoveTo)
        {
            if (indexToMoveFrom == -1 || indexToMoveTo == -1)
                return false;
            _bitArray.Set(indexToMoveFrom, false);
            _bitArray.Set(indexToMoveTo, true);
            return true;
        }
    }
}