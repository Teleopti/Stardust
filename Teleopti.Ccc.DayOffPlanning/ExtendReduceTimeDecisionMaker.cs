using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Secret
{
    public interface IExtendReduceTimeDecisionMaker
    {
        ExtendReduceTimeDecisionMakerResult Execute(IScheduleMatrixLockableBitArrayConverter matrixConverter, IScheduleResultDataExtractor dataExtractor);
    }

    public class ExtendReduceTimeDecisionMaker : IExtendReduceTimeDecisionMaker
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ExtendReduceTimeDecisionMakerResult Execute(IScheduleMatrixLockableBitArrayConverter matrixConverter, IScheduleResultDataExtractor dataExtractor)
        {
            var result = new ExtendReduceTimeDecisionMakerResult();

            IScheduleMatrixPro matrix = matrixConverter.SourceMatrix;
            ILockableBitArray bitArray = matrixConverter.Convert(false, false);
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