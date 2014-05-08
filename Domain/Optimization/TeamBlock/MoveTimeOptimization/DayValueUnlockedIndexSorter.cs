using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public interface IDayValueUnlockedIndexSorter
	{
		IList<int> SortAscending(ILockableBitArray lockableBitArray, IList<double?> values);
		IList<int> SortDescending(ILockableBitArray lockableBitArray, IList<double?> values);
	}

	public class DayValueUnlockedIndexSorter : IDayValueUnlockedIndexSorter
	{
		public IList<int> SortAscending(ILockableBitArray lockableBitArray, IList<double?> values)
		{
			var test = new List<KeyValuePair<int, double>>();

			IList<int> ret = new List<int>();

			foreach (int index in lockableBitArray.UnlockedIndexes)
			{
				if (values[index - lockableBitArray.PeriodArea.Minimum].HasValue)
					test.Add(new KeyValuePair<int, double>(index, values[index - lockableBitArray.PeriodArea.Minimum].Value));
			}

			test.Sort(sortByValueAscending);
			foreach (KeyValuePair<int, double> keyValuePair in test)
			{
				ret.Add(keyValuePair.Key);
			}

			return ret;
		}

		public IList<int> SortDescending(ILockableBitArray lockableBitArray, IList<double?> values)
		{
			//should be an unlocked no day off
			var test = new List<KeyValuePair<int, double>>();

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

		private int sortByValueAscending(KeyValuePair<int, double> x, KeyValuePair<int, double> y)
		{
			return x.Value.CompareTo(y.Value);
		}

		private int sortByValueDescending(KeyValuePair<int, double> x, KeyValuePair<int, double> y)
		{
			return -1 * x.Value.CompareTo(y.Value);
		}
	}
}