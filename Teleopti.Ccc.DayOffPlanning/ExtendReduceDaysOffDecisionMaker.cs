using System.Collections;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Secret
{
    public interface IExtendReduceDaysOffDecisionMaker
    {
        ExtendReduceTimeDecisionMakerResult Execute(IScheduleMatrixLockableBitArrayConverter matrixConverter,
                                                    IScheduleResultDataExtractor dataExtractor, IList<IDayOffLegalStateValidator> validatorList);

        bool ValidateArray(ILockableBitArray array, IList<IDayOffLegalStateValidator> validatorList);
    }

    public class ExtendReduceDaysOffDecisionMaker : IExtendReduceDaysOffDecisionMaker
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ExtendReduceTimeDecisionMakerResult Execute(IScheduleMatrixLockableBitArrayConverter matrixConverter,
                                                           IScheduleResultDataExtractor dataExtractor, IList<IDayOffLegalStateValidator> validatorList)
        {
            var result = new ExtendReduceTimeDecisionMakerResult();

            IScheduleMatrixPro matrix = matrixConverter.SourceMatrix;
            ILockableBitArray bitArray = matrixConverter.Convert(false, false);
            IList<double?> values = dataExtractor.Values();

            int? dayToLengthen = findDayToLenghten(bitArray, values, validatorList);
            if(dayToLengthen.HasValue)
            {
                bitArray.Set(dayToLengthen.Value, false);
                result.DayToLengthen = matrix.FullWeeksPeriodDays[dayToLengthen.Value].Day;
            }

            int? dayToShorten = findDayToShorten(bitArray, values, validatorList);
            if (dayToShorten.HasValue)
            {
                bitArray.Set(dayToShorten.Value, true);
                result.DayToShorten = matrix.FullWeeksPeriodDays[dayToShorten.Value].Day;
            }
                
            if(!dayToLengthen.HasValue && dayToShorten.HasValue)
            {
                dayToLengthen = findDayToLenghten(bitArray, values, validatorList);
                if (dayToLengthen.HasValue)
                {
                    bitArray.Set(dayToLengthen.Value, false);
                    result.DayToLengthen = matrix.FullWeeksPeriodDays[dayToLengthen.Value].Day;
                }
            }
            return result;
        }

        private int? findDayToLenghten(ILockableBitArray lockableBitArray, IList<double?> values, IList<IDayOffLegalStateValidator> validatorList)
        {
            List<KeyValuePair<int, double>> test = new List<KeyValuePair<int, double>>();

            foreach (int index in lockableBitArray.UnlockedIndexes)
            {
                if (!lockableBitArray.DaysOffBitArray[index])
                    continue;

                ILockableBitArray clone = (LockableBitArray)lockableBitArray.Clone();
                clone.Set(index, false);

                bool valid = ValidateArray(clone, validatorList);
                if (valid && values[index - lockableBitArray.PeriodArea.Minimum].HasValue)
                    test.Add(new KeyValuePair<int, double>(index,
                                                           values[index - lockableBitArray.PeriodArea.Minimum].Value));
            }

            test.Sort(sortByValueAscending);

            if (test.Count == 0)
                return null;

            if (test[0].Value < 0)
                return test[0].Key;

            return null;
        }

        private int? findDayToShorten(ILockableBitArray lockableBitArray, IList<double?> values, IList<IDayOffLegalStateValidator> validatorList)
        {
            List<KeyValuePair<int, double>> test = new List<KeyValuePair<int, double>>();

            foreach (int index in lockableBitArray.UnlockedIndexes)
            {
                if (lockableBitArray.DaysOffBitArray[index])
                    continue;

                ILockableBitArray clone = (LockableBitArray)lockableBitArray.Clone();
                clone.Set(index, true);

                bool valid = ValidateArray(clone, validatorList);
                if (valid && values[index - lockableBitArray.PeriodArea.Minimum].HasValue)
                    test.Add(new KeyValuePair<int, double>(index,
                                                           values[index - lockableBitArray.PeriodArea.Minimum].Value));
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
            return -1*x.Value.CompareTo(y.Value);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool ValidateArray(ILockableBitArray array, IList<IDayOffLegalStateValidator> validatorList)
        {
            BitArray longBitArray = array.ToLongBitArray();
            int offset = 0;
            if (array.PeriodArea.Minimum < 7)
                offset = 7;
            for (int i = array.PeriodArea.Minimum; i <= array.PeriodArea.Maximum; i++)
            {
                if (longBitArray[i + offset])
                {
                    if (!validateIndex(longBitArray, i + offset, validatorList))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool validateIndex(BitArray daysOffArray, int index, IList<IDayOffLegalStateValidator> validatorList)
        {
            foreach (IDayOffLegalStateValidator validator in validatorList)
            {
                if (!validator.IsValid(daysOffArray, index))
                    return false;
            }
            return true;
        }
    }

}