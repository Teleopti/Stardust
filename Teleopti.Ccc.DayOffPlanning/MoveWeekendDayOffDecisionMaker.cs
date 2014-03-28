using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Secret
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
            string decisionMakerName = this.ToString();
            _logWriter.LogInfo("Execute of " + decisionMakerName);

            IList<int> indexesToMoveFrom = CreatePreferredIndexesToMoveFrom(lockableBitArray, values);
            IList<int> indexesToMoveTo = CreatePreferredIndexesToMoveTo(lockableBitArray, values);

            _logWriter.LogInfo("Move from preference index: " + CreateCommaSeparatedString(indexesToMoveFrom));
            _logWriter.LogInfo("Move to preference index: " + CreateCommaSeparatedString(indexesToMoveTo));

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

                        _logWriter.LogInfo(string.Format(CultureInfo.CurrentCulture, "{0} has moved the following day indexes: from{1}-to{2}; from{3}-to{4}", decisionMakerName,
                            moveFrom.Key.ToString(CultureInfo.CurrentCulture), moveTo.Key.ToString(CultureInfo.CurrentCulture),
                            moveFrom.Value.ToString(CultureInfo.CurrentCulture), moveTo.Value.ToString(CultureInfo.CurrentCulture)));

                        return true;
                    }
                }
            }

            _logWriter.LogInfo(string.Format(CultureInfo.CurrentCulture, "{0} has not found any legal day to move", decisionMakerName));
            return false;
        }

        private IList<int> CreatePreferredIndexesToMoveFrom(ILockableBitArray lockableBitArray, IList<double?> values)
        {
            List<KeyValuePair<int, double>> tryList = new List<KeyValuePair<int, double>>();

            IList<int> weekendDayIndexes =
                ExtractWeekendDayIndexes(lockableBitArray.PeriodArea.Minimum, lockableBitArray.PeriodArea.Maximum);

            //should be an unlocked day off
            foreach (int index in lockableBitArray.UnlockedIndexes)
            {
                if (lockableBitArray[index]
                    && values[index - lockableBitArray.PeriodArea.Minimum].HasValue
                    && weekendDayIndexes.Contains(index))
                    tryList.Add(new KeyValuePair<int, double>(index, values[index - lockableBitArray.PeriodArea.Minimum].Value));
            }

            tryList.Sort(SortByValueAscending);

            IList<int> ret = new List<int>();
            foreach (KeyValuePair<int, double> keyValuePair in tryList)
            {
                ret.Add(keyValuePair.Key);
            }

            return ret;
        }

        private IList<int> CreatePreferredIndexesToMoveTo(ILockableBitArray lockableBitArray, IList<double?> values)
        {
            //should be an unlocked no day off
            List<KeyValuePair<int, double>> test = new List<KeyValuePair<int, double>>();

            IList<int> weekendDayIndexes =
                ExtractWeekendDayIndexes(lockableBitArray.PeriodArea.Minimum, lockableBitArray.PeriodArea.Maximum);

            //should be an unlocked day off
            foreach (int index in lockableBitArray.UnlockedIndexes)
            {
                if (!lockableBitArray[index]
                    && values[index - lockableBitArray.PeriodArea.Minimum].HasValue
                    && weekendDayIndexes.Contains(index))
                    test.Add(new KeyValuePair<int, double>(index, values[index - lockableBitArray.PeriodArea.Minimum].Value));
            }

            test.Sort(SortByValueDescending);

            IList<int> ret = new List<int>();
            foreach (KeyValuePair<int, double> keyValuePair in test)
            {
                ret.Add(keyValuePair.Key);
            }

            return ret;
        }

        private IList<int> ExtractWeekendDayIndexes(int minimumIndex, int maximumIndex)
        {
            IList<int> result = new List<int>();
            IList<int> officialWeekendDays = _officialWeekendDays.WeekendDayIndexesRelativeStartDayOfWeek();

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

        private static string CreateCommaSeparatedString(IEnumerable<int> indexesToMoveFrom)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (int i in indexesToMoveFrom)
            {
                stringBuilder.Append(i.ToString(CultureInfo.CurrentCulture) + ",");
            }
            string result = stringBuilder.ToString();
            if (result.Length > 0)
                result = result.Substring(0, result.Length - 1);
            return result;
        }
    }
}