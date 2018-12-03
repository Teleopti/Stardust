using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Secrets.DayOffPlanning;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class DaysOffPerWeekSolver : IDayOffBackToLegalStateSolver
    {
        private readonly ILockableBitArray _bitArray;
        private readonly IDaysOffPreferences _daysOffPreferences;
        private readonly IDayOffBackToLegalStateFunctions _functions;
        private readonly int _maxIterations;


        public DaysOffPerWeekSolver(ILockableBitArray bitArray, IDayOffBackToLegalStateFunctions functions, IDaysOffPreferences daysOffPreferences, int maxIterations)
        {
            _maxIterations = maxIterations;
            _functions = functions;
            _bitArray = bitArray;
            _daysOffPreferences = daysOffPreferences;
        }

        public MinMaxNumberOfResult ResolvableState()
        {
            IDictionary<int, int> weeklyDaysOffsDictionary = _functions.CreateWeeklyDaysOffsDictionary(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter);
            IDictionary<int, int> weeklyNumberOfNonPeriodDays = createWeeklyNumberOfNonPeriodDaysDictionary(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter);
            int weeklyMinDayOff = _daysOffPreferences.DaysOffPerWeekValue.Minimum;

            foreach (int key in weeklyDaysOffsDictionary.Keys)
            {
                if (weeklyDaysOffsDictionary[key] > _daysOffPreferences.DaysOffPerWeekValue.Maximum)
                   return MinMaxNumberOfResult.ToMany;
                int currentMinDaysOff = weeklyMinDayOff - weeklyNumberOfNonPeriodDays[key];
                if (weeklyDaysOffsDictionary[key] < currentMinDaysOff)
                    return MinMaxNumberOfResult.ToFew;
            }
            return MinMaxNumberOfResult.Ok;
        }

        public bool SetToManyBackToLegalState()
        {
            if (ResolvableState() == MinMaxNumberOfResult.Ok)
                return false;

            int iterationCounter = 0;
            while (ResolvableState() == MinMaxNumberOfResult.ToMany)
            {
                Point result = calculateSwap();

                if (!SwapBits(result.X, result.Y))
                    return false;

                iterationCounter++;
                if (iterationCounter > _maxIterations)
                    return true;
            }

            return true;
        }

        public bool SetToFewBackToLegalState()
        {
            if (ResolvableState() == MinMaxNumberOfResult.Ok)
                return false;

            int iterationCounter = 0;
            while (ResolvableState() == MinMaxNumberOfResult.ToFew)
            {
                Point result = calculateSwap();

                if (!SwapBits(result.X, result.Y))
                    return false;

                iterationCounter++;
                if (iterationCounter > _maxIterations)
                    return true;
            }

            return true;
        }

        public bool SwapBits(int indexToMoveFrom, int indexToMoveTo)
        {
            if (indexToMoveFrom == -1 || indexToMoveTo == -1)
                return false;
            _bitArray.Set(indexToMoveFrom, false);
            _bitArray.Set(indexToMoveTo, true);
            return true;
        }

        private Point calculateSwap()
        {
            IDictionary<int, int> weeklyList = _functions.CreateWeeklyDaysOffsDictionary(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter);
            IList<Point> weekEndList = _functions.WeekendList();
            int indexToMoveFrom = _functions.FindFirstBestIndexToMoveDaysOffFromLocksConsidered(weeklyList, weekEndList);
            int indexToMoveTo;
            do
            {
                int weekIndexLeast = DayOffBackToLegalStateFunctions.FindWeekWithLeastNumberOfDaysOff(weeklyList);
                if (weekIndexLeast == -1)
                    weekIndexLeast = 0;
                indexToMoveTo = _functions.FindFirstWeekdayIndexWithNoDayOffButRatherNotWeekend(weekIndexLeast, weekEndList, true);
                if (indexToMoveTo == -1)
                    weeklyList.Remove(weekIndexLeast);
                if(weeklyList.Count == 0)
                    break;

            } while (indexToMoveTo == -1);
            

            return new Point(indexToMoveFrom, indexToMoveTo);
        }

        /// <summary>
        /// Calculates the non period days on a week after the terminal date. This can happen, if the person starts or leaves in the middle of a week. 
        /// In that case we do not have to check for the day off numbers.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private IDictionary<int, int> createWeeklyNumberOfNonPeriodDaysDictionary(bool considerWeekBefore, bool considerWeekAfter)
        {
            IDictionary<int, int> result = new Dictionary<int, int>();
            MinMax<int> minMaxPeriodArea = _bitArray.PeriodArea;

            int weekIndex = -1;
            foreach (int weekStartDayIndex in _functions.PeriodWeekStartDayIndexes(considerWeekBefore, considerWeekAfter))
            {
                weekIndex++;
                int numberOfNonPeriodDays = 0;
                for (int dayIndex = weekStartDayIndex; dayIndex < weekStartDayIndex + 7; dayIndex++)
                {
                    if (!minMaxPeriodArea.Contains(dayIndex))
                        numberOfNonPeriodDays++;

                }
                result.Add(weekIndex, numberOfNonPeriodDays);
            }
            return result;
        }
    }
}