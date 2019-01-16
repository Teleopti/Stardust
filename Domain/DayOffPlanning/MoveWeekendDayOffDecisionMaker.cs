using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    /// <summary>
    /// Moves the whole week-end to all possible possible places without loosing legal state
    /// according to the provided validators
    /// </summary>
    public class MoveWeekendDayOffDecisionMaker : IDayOffDecisionMaker
    {
        private readonly ILogWriter _logWriter;
        private readonly IList<IDayOffLegalStateValidator> _validatorList;
        private readonly IOfficialWeekendDays _officialWeekendDays;
        bool _lockFoundDays;

        public MoveWeekendDayOffDecisionMaker(
            IList<IDayOffLegalStateValidator> validatorList, 
            IOfficialWeekendDays officialWeekendDays, 
            bool lockFoundDays,
            ILogWriter logWriter)
        {
            _logWriter = logWriter;
            _validatorList = validatorList;
            _officialWeekendDays = officialWeekendDays;
            _lockFoundDays = lockFoundDays;
        }

        public bool Execute(ILockableBitArray lockableBitArray, IList<double?> values)
        {
            _logWriter.LogInfo(()=>$"Execute of {nameof(MoveWeekendDayOffDecisionMaker)}");

            IList<int> indexesToMoveFrom = CreatePreferredIndexesToMoveFrom(lockableBitArray, values);
            IList<int> indexesToMoveTo = CreatePreferredIndexesToMoveTo(lockableBitArray, values);

            _logWriter.LogInfo(()=>$"Move from preference index: {string.Join(", ",indexesToMoveFrom)}");
            _logWriter.LogInfo(()=>$"Move to preference index: {string.Join(", ",indexesToMoveTo)}");

            IEnumerable<KeyValuePair<int, int>> indexPairsToMoveFrom =
                CreateIndexPairsToMoveWeekDay(indexesToMoveFrom);

            IEnumerable<KeyValuePair<int, int>> indexPairsToMoveTo =
                CreateIndexPairsToMoveWeekDay(indexesToMoveTo);

            foreach (KeyValuePair<int, int> moveFrom in indexPairsToMoveFrom)
            {
                foreach (KeyValuePair<int, int> moveTo in indexPairsToMoveTo)
                {
                    ILockableBitArray clone = (LockableBitArray)lockableBitArray.Clone();

                    clone.Set(moveFrom.Key, false);
                    clone.Set(moveFrom.Value, false);

                    clone.Set(moveTo.Key, true);
                    clone.Set(moveTo.Value, true);

                    BitArray longBitArray = clone.ToLongBitArray();
                    bool skip = false;
                    int offset = 0;
                    if (clone.PeriodArea.Minimum < 7)
                        offset = 7;
                    for (int i = clone.PeriodArea.Minimum; i <= clone.PeriodArea.Maximum; i++)
                    {
                        if (longBitArray[i + offset])
                        {
                            if (!ValidateIndex(longBitArray, i + offset))
                            {
                                skip = true;
                                break;
                            }
                        }
                    }
                    if (!skip)
                    {
                        lockableBitArray.Set(moveFrom.Key, false);
                        lockableBitArray.Set(moveFrom.Value, false);
                        lockableBitArray.Set(moveTo.Key, true);
                        lockableBitArray.Set(moveTo.Value, true);
                        if (_lockFoundDays)
                        {
                            lockableBitArray.Lock(moveTo.Key, true);
                            lockableBitArray.Lock(moveTo.Value, true);
                        }

                        _logWriter.LogInfo(()=>$"{nameof(MoveWeekendDayOffDecisionMaker)} has moved the following day indexes: from {moveFrom.Key}-to {moveTo.Key}; from{moveFrom.Value}-to{moveTo.Value}");

                        return true;
                    }
                }
            }

            _logWriter.LogInfo(()=>$"{nameof(MoveWeekendDayOffDecisionMaker)} has not found any legal day to move");
            return false;
        }

        private IList<int> CreatePreferredIndexesToMoveFrom(ILockableBitArray lockableBitArray, IList<double?> values)
        {
            List<KeyValuePair<int, double>> tryList = new List<KeyValuePair<int, double>>();

            var weekendDayIndexes =
                ExtractWeekendDayIndexes(lockableBitArray.PeriodArea.Minimum, lockableBitArray.PeriodArea.Maximum);

            //should be an unlocked day off
            foreach (int index in lockableBitArray.UnlockedIndexes)
            {
	            if (lockableBitArray[index])
	            {
					var val = values[index - lockableBitArray.PeriodArea.Minimum];
		            if (val.HasValue && weekendDayIndexes.Contains(index))
		            {
			            tryList.Add(new KeyValuePair<int, double>(index, val.Value));
		            }
	            }
            }

            tryList.Sort(SortByValueAscending);
	        return tryList.Select(t => t.Key).ToList();
        }

        private IList<int> CreatePreferredIndexesToMoveTo(ILockableBitArray lockableBitArray, IList<double?> values)
        {
            //should be an unlocked no day off
            List<KeyValuePair<int, double>> test = new List<KeyValuePair<int, double>>();

            var weekendDayIndexes =
                ExtractWeekendDayIndexes(lockableBitArray.PeriodArea.Minimum, lockableBitArray.PeriodArea.Maximum);

            //should be an unlocked day off
            foreach (int index in lockableBitArray.UnlockedIndexes)
            {
	            if (!lockableBitArray[index])
	            {
		            var val = values[index - lockableBitArray.PeriodArea.Minimum];
		            if (val.HasValue && weekendDayIndexes.Contains(index))
		            {
			            test.Add(new KeyValuePair<int, double>(index, val.Value));
		            }
	            }
            }

            test.Sort(SortByValueDescending);
	        return test.Select(t => t.Key).ToList();
        }

        private HashSet<int> ExtractWeekendDayIndexes(int minimumIndex, int maximumIndex)
        {
            var result = new HashSet<int>();
            var officialWeekendDays = _officialWeekendDays.WeekendDayIndexesRelativeStartDayOfWeek();

            for (int i = minimumIndex; i <= maximumIndex; i++)
            {
                int weekPosition = i % 7;
                if (officialWeekendDays.Contains(weekPosition))
                    result.Add(i);
            }
            return result;
        }

        private bool ValidateIndex(BitArray daysOffArray, int index)
        {
            foreach (IDayOffLegalStateValidator validator in _validatorList)
            {
                if (!validator.IsValid(daysOffArray, index))
                    return false;
            }
            return true;
        }

        private static int SortByValueAscending(KeyValuePair<int, double> x, KeyValuePair<int, double> y)
        {
            return x.Value.CompareTo(y.Value);
        }

        private static int SortByValueDescending(KeyValuePair<int, double> x, KeyValuePair<int, double> y)
        {
            return -1 * x.Value.CompareTo(y.Value);
        }

        private static IEnumerable<KeyValuePair<int, int>> CreateIndexPairsToMoveWeekDay(IList<int> indexesToMove)
        {
            List<KeyValuePair<int, int>> ret = new List<KeyValuePair<int, int>>();
            if (indexesToMove.Count > 1)
            {
                for (int i = 0; i < indexesToMove.Count - 1; i++)
                {
                    for (int j = i + 1; j < indexesToMove.Count; j++)
                    {
                        KeyValuePair<int, int> valuePair =
                            new KeyValuePair<int, int>(indexesToMove[i], indexesToMove[j]);
                        ret.Add(valuePair);
                    }
                }
            }
            return ret;
        }
    }
}