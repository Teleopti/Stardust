using System.Collections.Generic;
using System.Drawing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
	public class FiveConsecutiveWorkdaysSolver : IDayOffBackToLegalStateSolver
    {
        private readonly ILockableBitArray _bitArray;
        private readonly IDaysOffPreferences _daysOffPreferences;
        private readonly IDayOffBackToLegalStateFunctions _functions;

		public FiveConsecutiveWorkdaysSolver(ILockableBitArray bitArray, IDayOffBackToLegalStateFunctions functions, IDaysOffPreferences daysOffPreferences)
        {
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

        	IList<int> weekStartIndexes =
        		new List<int>(_functions.PeriodWeekStartDayIndexes(_daysOffPreferences.ConsiderWeekBefore,
        		                                                   _daysOffPreferences.ConsiderWeekAfter));
			IList<int> weekLastIndexes = new List<int>();
			foreach (var weekStartIndex in weekStartIndexes)
			{
				weekLastIndexes.Add(weekStartIndex + 6);
			}

        	foreach (var weekStartIndex in weekStartIndexes)
        	{
        		int weekLastIndex = weekStartIndex + 6;
				bool firstDayFixed = _bitArray.DaysOffBitArray[weekStartIndex];
				bool lastDayFixed = _bitArray.DaysOffBitArray[weekLastIndex];

				if (!firstDayFixed && !_bitArray.IsLocked(weekStartIndex, true))
				{
					for (int index = weekStartIndexes[0]; index < weekLastIndexes[weekLastIndexes.Count - 1]; index++)
					{
						if (findIndexToSwap(weekLastIndexes, weekStartIndexes, index))
							continue;

						SwapBits(index, weekStartIndex);
						break;
					}
				}

				if (!lastDayFixed && !_bitArray.IsLocked(weekLastIndex, true))
				{
					for (int index = 0; index < _bitArray.Count; index++)
					{
						if (findIndexToSwap(weekLastIndexes, weekStartIndexes, index)) 
							continue;

						SwapBits(index, weekLastIndex);
						break;
					}
				}
        	}

			foreach (var weekStartIndex in weekStartIndexes)
			{
				int weekLastIndex = weekStartIndex + 6;
				bool firstDayFixed = _bitArray.DaysOffBitArray[weekStartIndex];
				bool lastDayFixed = _bitArray.DaysOffBitArray[weekLastIndex];

				if (!firstDayFixed || !lastDayFixed)
					return false;
			}

        	return true;
        }

		private bool findIndexToSwap(IList<int> weekLastIndexes, IList<int> weekStartIndexes, int index)
		{
			if (_bitArray.IsLocked(index, true))
				return true;

			if (!_bitArray.DaysOffBitArray[index])
				return true;

			if (weekStartIndexes.Contains(index) || weekLastIndexes.Contains(index))
				return true;
			return false;
		}

		public bool SetToFewBackToLegalState()
        {
			return ResolvableState() != MinMaxNumberOfResult.Ok;
        }

        public string ResolverDescriptionKey
        {
            get { return "FiveConsecutiveWorkdaysRule"; }
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