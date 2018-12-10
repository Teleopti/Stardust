using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class MoveTimeDecisionMaker2
    {
	    private readonly IScheduleMatrixLockableBitArrayConverterEx _matrixConverter;

	    public MoveTimeDecisionMaker2(IScheduleMatrixLockableBitArrayConverterEx matrixConverter)
	    {
		    _matrixConverter = matrixConverter;
	    }

	    public IList<DateOnly> Execute(IScheduleMatrixPro matrix, IScheduleResultDataExtractor dataExtractor)
        {
            return makeDecision(_matrixConverter.Convert(matrix, false, false), matrix, dataExtractor);
        }

        private static IList<DateOnly> makeDecision(ILockableBitArray lockableBitArray, IScheduleMatrixPro matrix,
                                          IScheduleResultDataExtractor dataExtractor)
        {
            IList<DateOnly> result = new List<DateOnly>(2);
            var values = dataExtractor.Values();

            var indexesToMoveFrom = createIndexListSortByValueAscending(lockableBitArray, values);
            var indexesToMoveTo = createIndexListSortByValueDescending(lockableBitArray, values);
            foreach (int currentMoveFromIndex in indexesToMoveFrom)
            {
                foreach (int currentMoveToIndex in indexesToMoveTo)
                {
                    if (currentMoveToIndex == currentMoveFromIndex) continue;
                    if (values[currentMoveToIndex - lockableBitArray.PeriodArea.Minimum] <
                        values[currentMoveFromIndex - lockableBitArray.PeriodArea.Minimum])
                        break;
                    if (areFoundDaysValid(currentMoveFromIndex, currentMoveToIndex, matrix))
                    {
                        DateOnly mostUnderStaffedDay = matrix.FullWeeksPeriodDays[currentMoveFromIndex].Day;
                        DateOnly mostOverStaffedDay = matrix.FullWeeksPeriodDays[currentMoveToIndex].Day;
                        result.Add(mostUnderStaffedDay);
                        result.Add(mostOverStaffedDay);
                        return result;
                    }
                }
            }
            return result;
        }


        private static bool areFoundDaysValid(int currentMoveFromIndex, int currentMoveToIndex, IScheduleMatrixPro matrix)
        {
            IScheduleDayPro currentMoveFromDay = matrix.FullWeeksPeriodDays[currentMoveFromIndex];
            IScheduleDayPro currentMoveToDay = matrix.FullWeeksPeriodDays[currentMoveToIndex];

	        var moveFromDayPart = currentMoveFromDay.DaySchedulePart();
	        var moveToDayPart = currentMoveToDay.DaySchedulePart();
	        if (moveFromDayPart.SignificantPart() != SchedulePartView.MainShift
                || moveToDayPart.SignificantPart() != SchedulePartView.MainShift)
                return false;

            var moveFromWorkShiftLength = moveFromDayPart.ProjectionService().CreateProjection().ContractTime();
            var moveToWorkShiftLength = moveToDayPart.ProjectionService().CreateProjection().ContractTime();
            
			return moveFromWorkShiftLength <= moveToWorkShiftLength;
        }


        private static IList<int> createIndexListSortByValueAscending(ILockableBitArray lockableBitArray, IList<double?> values)
        {
            List<KeyValuePair<int, double>> test = new List<KeyValuePair<int, double>>();

            IList<int> ret = new List<int>();

            foreach (int index in lockableBitArray.UnlockedIndexes)
            {
                if (values[index - lockableBitArray.PeriodArea.Minimum].HasValue )
                    test.Add(new KeyValuePair<int, double>(index, values[index - lockableBitArray.PeriodArea.Minimum].Value));
            }

            test.Sort(sortByValueAscending);
            foreach (KeyValuePair<int, double> keyValuePair in test)
            {
                ret.Add(keyValuePair.Key);
            }

            return ret;
        }

        private static IList<int> createIndexListSortByValueDescending(ILockableBitArray lockableBitArray, IList<double?> values)
        {
            //should be an unlocked no day off
            List<KeyValuePair<int, double>> test = new List<KeyValuePair<int, double>>();

            IList<int> ret = new List<int>();

            //should be an unlocked day off
            foreach (int index in lockableBitArray.UnlockedIndexes)
            {
                if (values[index - lockableBitArray.PeriodArea.Minimum].HasValue)
                    test.Add(new KeyValuePair<int, double>(index, values[index - lockableBitArray.PeriodArea.Minimum].Value));
            }

            test.Sort(sortByValueDescending);
            foreach (KeyValuePair<int, double> keyValuePair in test)
            {
                ret.Add(keyValuePair.Key);
            }

            return ret;
        }

        private static int sortByValueAscending(KeyValuePair<int, double> x, KeyValuePair<int, double> y)
        {
            return x.Value.CompareTo(y.Value);
        }

        private static int sortByValueDescending(KeyValuePair<int, double> x, KeyValuePair<int, double> y)
        {
            return -1 * x.Value.CompareTo(y.Value);
        }
    }
}