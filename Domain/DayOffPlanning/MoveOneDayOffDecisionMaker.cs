using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    /// <summary>
    /// Moves one day off to the best possible place without loosing legal state according to the provided validators
    /// </summary>
    public class MoveOneDayOffDecisionMaker : IDayOffDecisionMaker
    {
        private readonly IList<IDayOffLegalStateValidator> _validatorList;
        private readonly ILogWriter _logWriter;


        public MoveOneDayOffDecisionMaker(
            IList<IDayOffLegalStateValidator> validatorList, 
            ILogWriter logWriter)
        {
            _validatorList = validatorList;
            _logWriter = logWriter;
        }

        public bool Execute(ILockableBitArray lockableBitArray, IList<double?> values)
        {
            _logWriter.LogInfo(()=>$"Execute of {nameof(MoveOneDayOffDecisionMaker)}");

            IEnumerable<int> indexesToMoveFrom = CreatePreferredIndexesToMoveFrom(lockableBitArray, values);
            IEnumerable<int> indexesToMoveTo = CreatePreferredIndexesToMoveTo(lockableBitArray, values);

            _logWriter.LogInfo(()=>$"Move from preference index: {string.Join(",", indexesToMoveFrom)}");
            _logWriter.LogInfo(()=>$"Move to preference index: {string.Join(",", indexesToMoveTo)}");

            foreach (int moveFrom in indexesToMoveFrom)
            {
                foreach (int moveTo in indexesToMoveTo)
                {
                    if (values[moveTo - lockableBitArray.PeriodArea.Minimum] <= values[moveFrom - lockableBitArray.PeriodArea.Minimum])
                        break;

                    ILockableBitArray clone = (LockableBitArray)lockableBitArray.Clone();
                    clone.Set(moveFrom, false);
                    clone.Set(moveTo, true);

                    bool valid = ValidateArray(clone);
                    if (valid)
                    {
                        lockableBitArray.Set(moveFrom, false);
                        lockableBitArray.Set(moveTo, true);

                        _logWriter.LogInfo(()=>$"{nameof(MoveOneDayOffDecisionMaker)} has moved the following day index: from {moveFrom}, to {moveTo}");

                        return true;
                    }
                }
            }

            _logWriter.LogInfo(()=>$"{nameof(MoveOneDayOffDecisionMaker)} has not found any legal day to move");

            return false;
        }

        /// <summary>
        /// Validates the array agains the set of validators.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        private bool ValidateArray(ILockableBitArray array)
        {
            BitArray longBitArray = array.ToLongBitArray();
            int offset = 0;
            if (array.PeriodArea.Minimum < 7)
                offset = 7;
            for (int i = array.PeriodArea.Minimum; i <= array.PeriodArea.Maximum; i++)
            {
                if (longBitArray[i + offset])
                {
                    if (!ValidateIndex(longBitArray, i + offset))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Validates an index agains the set of validators.
        /// </summary>
        /// <param name="daysOffArray">The days off array.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private bool ValidateIndex(BitArray daysOffArray, int index)
        {
            foreach (IDayOffLegalStateValidator validator in _validatorList)
            {
                if (!validator.IsValid(daysOffArray, index))
                    return false;
            }
            return true;
        }

        private static IEnumerable<int> CreatePreferredIndexesToMoveFrom(ILockableBitArray lockableBitArray, IList<double?> values)
        {
            List<KeyValuePair<int, double>> test = new List<KeyValuePair<int, double>>();

            IList<int> ret = new List<int>();

            //should be an unlocked day off
            foreach (int index in lockableBitArray.UnlockedIndexes)
            {
                if (lockableBitArray[index] && values[index - lockableBitArray.PeriodArea.Minimum].HasValue)
                    test.Add(new KeyValuePair<int, double>(index, values[index - lockableBitArray.PeriodArea.Minimum].Value));
            }

            test.Sort(SortByValueAscending);
            foreach (KeyValuePair<int, double> keyValuePair in test)
            {
                ret.Add(keyValuePair.Key);
            }

            return ret;
        }

        private static IEnumerable<int> CreatePreferredIndexesToMoveTo(ILockableBitArray lockableBitArray, IList<double?> values)
        {
            //should be an unlocked no day off
            List<KeyValuePair<int, double>> test = new List<KeyValuePair<int, double>>();
			
            //should be an unlocked day off
            foreach (int index in lockableBitArray.UnlockedIndexes)
            {
	            if (!lockableBitArray[index])
	            {
		            var val = values[index - lockableBitArray.PeriodArea.Minimum];
		            if (val.HasValue)
		            {
			            test.Add(new KeyValuePair<int, double>(index, val.Value));
		            }
	            }
            }

            test.Sort(SortByValueDescending);
	        return test.Select(t => t.Key).ToList();
        }

        private static int SortByValueAscending(KeyValuePair<int, double> x, KeyValuePair<int, double> y)
        {
	        var ret = x.Value.CompareTo(y.Value);

	        return ret == 0 ? -x.Key.CompareTo(y.Key) : ret;
        }

	    private static int SortByValueDescending(KeyValuePair<int, double> x, KeyValuePair<int, double> y)
        {
			var ret = -1 * x.Value.CompareTo(y.Value);

			return ret == 0 ? x.Key.CompareTo(y.Key) : ret;
		}
    }
}