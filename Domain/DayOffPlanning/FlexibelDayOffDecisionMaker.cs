using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	public class FlexibelDayOffDecisionMaker : IDayOffDecisionMaker
	{
		private readonly IList<IDayOffLegalStateValidator> _validatorList;

		public FlexibelDayOffDecisionMaker(IList<IDayOffLegalStateValidator> validatorList)
		{
			_validatorList = validatorList;		
		}

		public bool Execute(ILockableBitArray bitArray, IList<double?> values)
		{
			var dayToLengthen = findDayToLenghten(bitArray, values);
			var result = false;
			if (dayToLengthen.HasValue)
			{
				bitArray.Set(dayToLengthen.Value, false);
				result = true;
			}

			var dayToShorten = findDayToShorten(bitArray, values);
			if (dayToShorten.HasValue)
			{
				bitArray.Set(dayToShorten.Value, true);
				result = true;
			}

			if (!dayToLengthen.HasValue && dayToShorten.HasValue)
			{
				dayToLengthen = findDayToLenghten(bitArray, values);
				if (dayToLengthen.HasValue) bitArray.Set(dayToLengthen.Value, false);
			}

			return result;
		}

		private int? findDayToLenghten(ILockableBitArray lockableBitArray, IList<double?> values)
		{
			var tempKeyValuePairs = new List<KeyValuePair<int, double>>();

			foreach (var index in lockableBitArray.UnlockedIndexes)
			{
				if (!lockableBitArray.DaysOffBitArray[index])continue;

				ILockableBitArray clone = (LockableBitArray)lockableBitArray.Clone();
				clone.Set(index, false);

				var valid = validateArray(clone);
				if (valid && values[index - lockableBitArray.PeriodArea.Minimum].HasValue)tempKeyValuePairs.Add(new KeyValuePair<int, double>(index, values[index - lockableBitArray.PeriodArea.Minimum].Value));
			}

			tempKeyValuePairs.Sort(sortByValueAscending);
			if (tempKeyValuePairs.Count == 0)return null;
			if (tempKeyValuePairs[0].Value < 0)return tempKeyValuePairs[0].Key;

			return null;
		}

		private int? findDayToShorten(ILockableBitArray lockableBitArray, IList<double?> values)
		{
			var tempKeyValuePairs = new List<KeyValuePair<int, double>>();

			foreach (var index in lockableBitArray.UnlockedIndexes)
			{
				if (lockableBitArray.DaysOffBitArray[index]) continue;

				ILockableBitArray clone = (LockableBitArray)lockableBitArray.Clone();
				clone.Set(index, true);

				var valid = validateArray(clone);
				if (valid && values[index - lockableBitArray.PeriodArea.Minimum].HasValue) tempKeyValuePairs.Add(new KeyValuePair<int, double>(index, values[index - lockableBitArray.PeriodArea.Minimum].Value));
			}

			tempKeyValuePairs.Sort(sortByValueDescending);
			if (tempKeyValuePairs.Count == 0)return null;
			if (tempKeyValuePairs[0].Value > 0)return tempKeyValuePairs[0].Key;

			return null;
		}

		private bool validateArray(ILockableBitArray array)
		{
			var longBitArray = array.ToLongBitArray();
			var offset = 0;
			if (array.PeriodArea.Minimum < 7)offset = 7;
			for (var i = array.PeriodArea.Minimum; i <= array.PeriodArea.Maximum; i++)
			{
				if (!longBitArray[i + offset]) continue;
				if (!validateIndex(longBitArray, i + offset)) return false;	
			}
			return true;
		}

		private static int sortByValueAscending(KeyValuePair<int, double> x, KeyValuePair<int, double> y)
		{
			return x.Value.CompareTo(y.Value);
		}

		private static int sortByValueDescending(KeyValuePair<int, double> x, KeyValuePair<int, double> y)
		{
			return -1 * x.Value.CompareTo(y.Value);
		}

		private bool validateIndex(BitArray daysOffArray, int index)
		{
			return _validatorList.All(validator => validator.IsValid(daysOffArray, index));
		}
	}
}
