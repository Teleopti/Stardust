using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    /// <summary>
    /// Moves the whole week-end to all possible possible places without loosing legal state
    /// according to the provided validators
    /// </summary>
    public class MoveWeekendDayOffNoValidationDecisionMaker : IDayOffDecisionMaker
    {
        private readonly IOfficialWeekendDays _officialWeekendDays;
        private readonly ILogWriter _logWriter;

        public MoveWeekendDayOffNoValidationDecisionMaker(
            IOfficialWeekendDays officialWeekendDays,
            ILogWriter logWriter)
        {
            _officialWeekendDays = officialWeekendDays;
            _logWriter = logWriter;
        }

        public bool Execute(ILockableBitArray lockableBitArray, IList<double?> values)
        {
            string decisionMakerName = this.ToString();
            _logWriter.LogInfo("Execute of " + decisionMakerName);

            IList<int> indexesToMoveFrom = CreatePreferredIndexesToMoveFrom(lockableBitArray, values);
            IList<int> indexesToMoveTo = CreatePreferredIndexesToMoveTo(lockableBitArray, values);

            _logWriter.LogInfo("Move from preference index: " + CreateCommaSeparatedString(indexesToMoveFrom));
            _logWriter.LogInfo("Move to preference index: " + CreateCommaSeparatedString(indexesToMoveTo));

            if (indexesToMoveFrom.Count < 2 || indexesToMoveTo.Count < 2)
            {
                _logWriter.LogInfo(string.Format(CultureInfo.CurrentCulture, "{0} has not found any legal day to move", decisionMakerName));
                return false;
            }

            lockableBitArray.Set(indexesToMoveFrom[0], false);
            lockableBitArray.Set(indexesToMoveFrom[1], false);
            lockableBitArray.Set(indexesToMoveTo[0], true);
            lockableBitArray.Set(indexesToMoveTo[1], true);

            _logWriter.LogInfo(string.Format(CultureInfo.CurrentCulture, "{0} has moved the following day indexes: from{1}-to{2}; from{3}-to{4}", decisionMakerName,
                indexesToMoveFrom[0].ToString(CultureInfo.CurrentCulture), indexesToMoveTo[0].ToString(CultureInfo.CurrentCulture),
                indexesToMoveFrom[1].ToString(CultureInfo.CurrentCulture), indexesToMoveTo[1].ToString(CultureInfo.CurrentCulture)));

            return true;
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
            IList<int> officialWeekendDays = _officialWeekendDays.WeekendDayIndexes();

            for (int i = minimumIndex; i <= maximumIndex; i++)
            {
                int weekPosition = i % 7;
                if (officialWeekendDays.Contains(weekPosition))
                    result.Add(i);
            }
            return result;
        }

        private static int SortByValueAscending(KeyValuePair<int, double> x, KeyValuePair<int, double> y)
        {
            return x.Value.CompareTo(y.Value);
        }

        private static int SortByValueDescending(KeyValuePair<int, double> x, KeyValuePair<int, double> y)
        {
            return -1 * x.Value.CompareTo(y.Value);
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