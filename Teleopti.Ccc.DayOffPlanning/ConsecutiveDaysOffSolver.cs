using System.Drawing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    public class ConsecutiveDaysOffSolver : IDayOffBackToLegalStateSolver
    {
        private readonly ILockableBitArray _bitArray;
        private readonly DayOffPlannerSessionRuleSet _sessionRuleSet;
        private readonly IDayOffBackToLegalStateFunctions _functions;
        private readonly int _maxIterations;

        public ConsecutiveDaysOffSolver(ILockableBitArray bitArray, IDayOffBackToLegalStateFunctions functions, DayOffPlannerSessionRuleSet sessionRuleSet, int maxIterations)
        {
            _maxIterations = maxIterations;
            _functions = functions;
            _bitArray = bitArray;
            _sessionRuleSet = sessionRuleSet;
        }

        public MinMaxNumberOfResult ResolvableState()
        {
            Point block = _functions.FindFirstDayOffBlockWithUnlockedDayOff(int.MinValue, _sessionRuleSet.ConsecutiveDaysOff.Minimum - 1);
            if (block.X != -1)
                return MinMaxNumberOfResult.ToFew;

            block = _functions.FindFirstDayOffBlockWithUnlockedDayOff(_sessionRuleSet.ConsecutiveDaysOff.Maximum + 1, int.MaxValue);
            if (block.X != -1)
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
                //find it
                Point block = _functions.FindFirstDayOffBlockWithUnlockedDayOff(_sessionRuleSet.ConsecutiveDaysOff.Maximum + 1, int.MaxValue);
                if(block.X == -1)
                    return false;
                int indexToMoveFrom = _functions.FindFirstUnlockedIndex(block.X, block.Y);

                //find next free
                int indexToMoveTo = _functions.FindNextIndexWithNoDayOff(block.Y + _sessionRuleSet.ConsecutiveWorkdays.Minimum, true);

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
                //find it
                Point block = _functions.FindFirstDayOffBlockWithUnlockedDayOff(int.MinValue, _sessionRuleSet.ConsecutiveDaysOff.Minimum - 1);
                if (block.X == -1)
                    return false;
                int indexToMoveFrom = _functions.FindFirstUnlockedIndex(block.X, block.Y);

                int indexToMoveTo;
                //find next free
                indexToMoveTo = _functions.FindNextIndexWithNoDayOff(indexToMoveFrom + 1, true);

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
            get { return "ConsecutiveDaysOffRule"; }
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