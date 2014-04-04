using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class FreeWeekendSolver : IDayOffBackToLegalStateSolver
    {
        private readonly ILockableBitArray _bitArray;
        private readonly IDaysOffPreferences _daysOffPreferences;
        private readonly IDayOffBackToLegalStateFunctions _functions;
        private readonly int _maxIterations;


        public FreeWeekendSolver(ILockableBitArray bitArray, IDayOffBackToLegalStateFunctions functions, IDaysOffPreferences daysOffPreferences, int maxIterations)
        {
            _maxIterations = maxIterations;
            _functions = functions;
            _bitArray = bitArray;
            _daysOffPreferences = daysOffPreferences;
        }


        public MinMaxNumberOfResult ResolvableState()
        {
            IList<Point> weekendList = _functions.WeekendList();
            int weekEndCounter = 0;

            foreach (var point in weekendList)
            {
                if(point.X >= _bitArray.PeriodArea.Minimum && point.Y <= _bitArray.PeriodArea.Maximum)
                {
                    if (_bitArray.Get(point.X) && _bitArray.Get(point.Y))
                        weekEndCounter++;
                }
            }

            if (weekEndCounter < _daysOffPreferences.FullWeekendsOffValue.Minimum)
                return MinMaxNumberOfResult.ToFew;
            if (weekEndCounter > _daysOffPreferences.FullWeekendsOffValue.Maximum)
                return MinMaxNumberOfResult.ToMany;

            return MinMaxNumberOfResult.Ok;
        }

        public bool SetToManyBackToLegalState()
        {
            if (ResolvableState() == MinMaxNumberOfResult.Ok)
                return false;

            int iterationCounter = 0;
            while (ResolvableState() == MinMaxNumberOfResult.ToMany)
            {
                int indexToMoveFrom = -1;
                //find first full weekend
                IList<Point> weekendList = _functions.WeekendList();
                for (int i = 0; i < weekendList.Count; i++)
                {
                    if (_bitArray[weekendList[i].X] && _bitArray[weekendList[i].Y])
                    {
                        if (!_bitArray.IsLocked(weekendList[i].Y, true))
                        {
                            indexToMoveFrom = weekendList[i].Y;
                            break;
                        }
                        if (!_bitArray.IsLocked(weekendList[i].X, true))
                        {
                            indexToMoveFrom = weekendList[i].X;
                            break;
                        }
                    }
                }

                int indexToMoveTo = -1;
                //find first none weekend day in this week
                int startIndex = _functions.FindFirstIndexOfWeek(indexToMoveFrom);
                for (int i = startIndex; i < startIndex + 7; i++)
                {
                    if (!_bitArray[i])
                    {
                        if (!_functions.IsWeekendDay(i, weekendList) && !_bitArray.IsLocked(i, true))
                            indexToMoveTo = i;
                    }
                    if (indexToMoveTo != -1)
                        break;
                }
                if (indexToMoveTo == -1)
                    indexToMoveTo = _functions.FindRandomNonLockedNonWeekendDayWithNoDayOff(weekendList, _maxIterations);

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
                //find first none full weekend
                int indexToMoveTo = _functions.FindFirstNoneLockedAccompanyingSingleDayWeekendDayOff(weekendList);
                //else find first empty weekend ? eller som här
                if (indexToMoveTo == -1)
                {
                    for (int i = 0; i < weekendList.Count; i++)
                    {
                        if (!_bitArray[weekendList[i].X] && !_bitArray.IsLocked(weekendList[i].X, true))
                        {
                            indexToMoveTo = weekendList[i].X;
                            break;
                        }
                    }
                }
                if (indexToMoveTo == -1)
                    return false;

                int indexToMoveFrom = _functions.FindFirstNonWeekendDayWithDayOff(weekendList, true);
               
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
            get { return "FreeWeekendRule"; }
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