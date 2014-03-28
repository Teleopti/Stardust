using System;
using System.Drawing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Secret.DayOffPlanning
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tui")]
    public class TuiCaseSolver : IDayOffBackToLegalStateSolver
    {
        private readonly ILockableBitArray _bitArray;
        private readonly IDaysOffPreferences _daysOffPreferences;
        private readonly IDayOffBackToLegalStateFunctions _functions;
        private readonly int _maxIterations;
        private readonly Random _random;

        public TuiCaseSolver(ILockableBitArray bitArray, IDayOffBackToLegalStateFunctions functions, IDaysOffPreferences daysOffPreferences, int maxIterations, int randomSeed)
        {
            _maxIterations = maxIterations;
            _functions = functions;
            _bitArray = bitArray;
            _daysOffPreferences = daysOffPreferences;
            _random = new Random(randomSeed);
        }

        public MinMaxNumberOfResult ResolvableState()
        {
            Point block = _functions.FindLongestConsecutiveWorkdayBlockWithAtLeastOneUnlockedBit();
            if (block.Y - block.X + 1 > _daysOffPreferences.ConsecutiveWorkdaysValue.Maximum)
                return MinMaxNumberOfResult.ToMany;

            return MinMaxNumberOfResult.Ok;
        }

        public bool SetToManyBackToLegalState()
        {
            if (ResolvableState() == MinMaxNumberOfResult.Ok)
            {
                return false;
            }

            int iterationCounter = 0;
            while (ResolvableState() == MinMaxNumberOfResult.ToMany)
            {
                Point longestBlock = _functions.FindLongestConsecutiveWorkdayBlockWithAtLeastOneUnlockedBit();
                Point indexesToMoveFrom = _functions.FindUnlockedDaysOffSurroundingBlock(longestBlock);
                int indexToMoveFrom = -1;
                int indexToMoveTo = -1;
                if (indexesToMoveFrom.X != -1 && indexesToMoveFrom.Y != -1)
                {
                    int y = _random.Next(0, 2);
                    if (y == 0)
                    {
                        indexToMoveFrom = indexesToMoveFrom.X;
                        indexToMoveTo = _functions.FindFirstUnlockedIndexWithNoDayOffInBlock(longestBlock);
                    }
                    else
                    {
                        indexToMoveFrom = indexesToMoveFrom.Y;
                        indexToMoveTo = _functions.FindLastUnlockedIndexWithNoDayOffInBlock(longestBlock);
                    }
                }
                else
                {
                    if (indexesToMoveFrom.X != -1)
                    {
                        indexToMoveFrom = indexesToMoveFrom.X;
                        indexToMoveTo = _functions.FindFirstUnlockedIndexWithNoDayOffInBlock(longestBlock);
                    }
                    else
                    {
                        if (indexesToMoveFrom.Y != -1)
                        {
                            indexToMoveFrom = indexesToMoveFrom.Y;
                            indexToMoveTo = _functions.FindLastUnlockedIndexWithNoDayOffInBlock(longestBlock);
                        }
                    }

                }

                if (indexToMoveFrom == -1 || indexToMoveTo == -1)
                    return true;

                if (!SwapBits(indexToMoveFrom, indexToMoveTo))
                    return false;

                iterationCounter++;
                if (iterationCounter > _maxIterations)
                    return true;
            }

            return true;
        }

        public bool SetToFewBackToLegalState()
        {
            return ResolvableState() != MinMaxNumberOfResult.Ok;
        }

        public string ResolverDescriptionKey
        {
            get { return "TUI Case solver"; }
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