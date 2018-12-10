using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class ExtendReduceTimeDecisionMaker
    {
	    private readonly IScheduleMatrixLockableBitArrayConverterEx _matrixConverter;

	    public ExtendReduceTimeDecisionMaker(IScheduleMatrixLockableBitArrayConverterEx matrixConverter)
		{
			_matrixConverter = matrixConverter;
		}

	    public ExtendReduceTimeDecisionMakerResult Execute(IScheduleMatrixPro matrix, IScheduleResultDataExtractor dataExtractor)
        {
            var result = new ExtendReduceTimeDecisionMakerResult();

            ILockableBitArray bitArray = _matrixConverter.Convert(matrix, false, false);
            IList<double?> values = dataExtractor.Values();

            int? dayToLengthen = findDayToLenghten(bitArray, values);
            int? dayToShorten = findDayToShorten(bitArray, values);

            if(dayToLengthen.HasValue)
                result.DayToLengthen = matrix.FullWeeksPeriodDays[dayToLengthen.Value].Day;

            if (dayToShorten.HasValue)
                result.DayToShorten = matrix.FullWeeksPeriodDays[dayToShorten.Value].Day;

            return result;
        }

        private static int? findDayToLenghten(ILockableBitArray lockableBitArray, IList<double?> values)
        {
            List<KeyValuePair<int, double>> test = new List<KeyValuePair<int, double>>();

            foreach (int index in lockableBitArray.UnlockedIndexes)
            {
                if (lockableBitArray.DaysOffBitArray[index])
                    continue;

                if (values[index - lockableBitArray.PeriodArea.Minimum].HasValue)
                    test.Add(new KeyValuePair<int, double>(index, values[index - lockableBitArray.PeriodArea.Minimum].Value));
            }

            test.Sort(sortByValueAscending);
            
            if (test.Count == 0)
                return null;

            if (test[0].Value < 0)
                return test[0].Key;

            return null;
        }

        private static int? findDayToShorten(ILockableBitArray lockableBitArray, IList<double?> values)
        {
            List<KeyValuePair<int, double>> test = new List<KeyValuePair<int, double>>();

            foreach (int index in lockableBitArray.UnlockedIndexes)
            {
                if (lockableBitArray.DaysOffBitArray[index])
                    continue;

                if (values[index - lockableBitArray.PeriodArea.Minimum].HasValue)
                    test.Add(new KeyValuePair<int, double>(index, values[index - lockableBitArray.PeriodArea.Minimum].Value));
            }

            test.Sort(sortByValueDescending);

            if (test.Count == 0)
                return null;

            if (test[0].Value > 0)
                return test[0].Key;

            return null;
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

    public class ExtendReduceTimeDecisionMakerResult
    {
        public DateOnly? DayToLengthen { get; set; }
        public DateOnly? DayToShorten { get; set; }
    }
}