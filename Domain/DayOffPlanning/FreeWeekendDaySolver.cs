using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class FreeWeekendDaySolver : IDayOffBackToLegalStateSolver
    {
        private readonly ILockableBitArray _bitArray;
        private readonly IDaysOffPreferences _daysOffPreferences;
        private readonly IDayOffBackToLegalStateFunctions _functions;
        private readonly int _maxIterations;

        public FreeWeekendDaySolver(ILockableBitArray bitArray, IDayOffBackToLegalStateFunctions functions, IDaysOffPreferences daysOffPreferences, int maxIterations)
        {
            _maxIterations = maxIterations;
            _functions = functions;
            _bitArray = bitArray;
            _daysOffPreferences = daysOffPreferences;
        }

        public MinMaxNumberOfResult ResolvableState()
        {
            IList<Point> weekendList = _functions.WeekendList();
            int weekendDayCounter = 0;
            for (int i = _bitArray.PeriodArea.Minimum; i < _bitArray.PeriodArea.Maximum + 1; i++)
            {
                if(_functions.IsWeekendDay(i, weekendList) && _bitArray[i])
                    weekendDayCounter++;
            }

            if (weekendDayCounter > _daysOffPreferences.WeekEndDaysOffValue.Maximum)
                return MinMaxNumberOfResult.ToMany;
            if (weekendDayCounter < _daysOffPreferences.WeekEndDaysOffValue.Minimum)
                return MinMaxNumberOfResult.ToFew;

            return MinMaxNumberOfResult.Ok;
        }

        public bool SetToManyBackToLegalState()
        {
            if (ResolvableState() == MinMaxNumberOfResult.Ok)
                return false;

            int iterationCounter = 0;
            while (ResolvableState() == MinMaxNumberOfResult.ToMany)
            {
                IList<Point> weekendList = _functions.WeekendList();
                int indexToMoveFrom = _functions.FindFirstNonLockedSingleDayWeekendDayOff(weekendList, true);
                if (indexToMoveFrom == -1)
                    indexToMoveFrom = _functions.FindFirstWeekendDayDayOff(weekendList, true);

                int indexToMoveTo = _functions.FindRandomNonLockedNonWeekendDayWithNoDayOff(weekendList, 20);

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
            if (ResolvableState() == MinMaxNumberOfResult.Ok)
                return false;

            int iterationCounter = 0;
            while (ResolvableState() == MinMaxNumberOfResult.ToFew)
            {
                IList<Point> weekendList = _functions.WeekendList();
                int indexToMoveFrom = _functions.FindFirstNonWeekendDayWithDayOff(weekendList, true);
                int indexToMoveTo = _functions.FindFirstIndexOfFirstWeekendWithNoDayOff(weekendList, true);
                if (indexToMoveTo == -1)
                    indexToMoveTo = _functions.FindFirstWeekendDayWithNoDayOff(weekendList, true);

                if (!SwapBits(indexToMoveFrom, indexToMoveTo))
                    return false;

                iterationCounter++;
                if (iterationCounter > _maxIterations)
                    return true;
            }

            return true;
        }

        public string ResolverDescriptionKey
        {
            get { return "FreeWeekendDayRule"; }
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