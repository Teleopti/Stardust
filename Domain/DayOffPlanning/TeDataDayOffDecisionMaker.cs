using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Te")]
	public class TeDataDayOffDecisionMaker : IDayOffDecisionMaker
	{

		//if not 2-2-2-2 return false
		//find the best two connecting days in same week to move from
		//find the best two connecting days in same week as above to move to, do not validate max cons work days
		//execute the move in the bittarray
		//validate max cons work days, if ok return the moves

		//goto week before
		//find the best two connecting days in that week to move from
		//find the best two connecting days in that week as above to move to, do not validate max cons work days
		//execute the move in the bittarray
		//validate max cons work days, if ok return the moves

		//repeate until success or no more weeks

		private IList<IDayOffLegalStateValidator> _validatorListWithoutMaxConsecutiveWorkdays;
		private IDayOffLegalStateValidator _maxConsecutiveWorkdaysValidator;
		private readonly IList<IDayOffLegalStateValidator> _validatorList;
		private readonly bool _is2222;
		private readonly ILogWriter _logWriter;

		public TeDataDayOffDecisionMaker(
            IList<IDayOffLegalStateValidator> validatorList, 
			bool is2222,
            ILogWriter logWriter)
		{
			_validatorList = validatorList;
			_is2222 = is2222;
			_logWriter = logWriter;
		}

		public bool Execute(ILockableBitArray lockableBitArray, IList<double?> values)
		{
			if (!_is2222)
				return false;

			extractConsecutiveWorkdaysValidator();
			var workingBitArray = (ILockableBitArray)lockableBitArray.Clone();
			
			_logWriter.LogInfo(()=>$"Execute of {nameof(TeDataDayOffDecisionMaker)}");

			int[] indexesToMoveFrom = createPreferredIndexesToMoveFrom(workingBitArray, values);
			if (indexesToMoveFrom.Length < 2)
				return false;

			int weekIndex = (int)Math.Floor(indexesToMoveFrom[0] / 7d);
			int[] indexesToMoveTo = createPreferredIndexesToMoveTo(workingBitArray, values, weekIndex);
			if (indexesToMoveTo.Length < 2)
				return false;

			while(!moveAndValidate(indexesToMoveFrom, indexesToMoveTo, workingBitArray, _validatorListWithoutMaxConsecutiveWorkdays))
			{
				workingBitArray.Lock(indexesToMoveTo[0], true);
				indexesToMoveTo = createPreferredIndexesToMoveTo(workingBitArray, values, weekIndex);
				if (indexesToMoveTo.Length < 2)
					return false;
			}

			while (!validateConsecutiveWorkdays(workingBitArray, _maxConsecutiveWorkdaysValidator))
			{
				if (weekIndex == (int)Math.Floor((workingBitArray.Count - 1) / 7d))
					weekIndex = 0;

				indexesToMoveFrom = findMoveInSpecificWeek(weekIndex + 1, workingBitArray, values);
				if (indexesToMoveFrom.Length < 2)
					return false;

				weekIndex = (int)Math.Floor(indexesToMoveFrom[0] / 7d);
				indexesToMoveTo = createPreferredIndexesToMoveTo(workingBitArray, values, weekIndex);
				if (indexesToMoveTo.Length < 2)
					return false;

				while (!moveAndValidate(indexesToMoveFrom, indexesToMoveTo, workingBitArray, _validatorListWithoutMaxConsecutiveWorkdays))
				{
					workingBitArray.Lock(indexesToMoveTo[0], true);
					indexesToMoveTo = createPreferredIndexesToMoveTo(workingBitArray, values, weekIndex);
					if (indexesToMoveTo.Length < 2)
						return false;
				}

			}

			_logWriter.LogInfo(()=>$"Move from preference index: {string.Join(", ",indexesToMoveFrom)}");
			_logWriter.LogInfo(()=>$"Move to preference index: {string.Join(",", indexesToMoveTo)}");
			
			for (int i = 0; i < lockableBitArray.Count; i++)
			{
				if(!lockableBitArray.IsLocked(i,true))
					lockableBitArray.Set(i, workingBitArray[i]);
			}
			return true;
		}

		private void extractConsecutiveWorkdaysValidator()
		{
			IDayOffLegalStateValidator maxConsecutiveWorkdaysValidator = null;
			foreach (var dayOffLegalStateValidator in _validatorList)
			{
				if (dayOffLegalStateValidator.GetType() == typeof(ConsecutiveWorkdayValidator))
					maxConsecutiveWorkdaysValidator = dayOffLegalStateValidator;
			}

			_validatorListWithoutMaxConsecutiveWorkdays = new List<IDayOffLegalStateValidator>(_validatorList);
			_validatorListWithoutMaxConsecutiveWorkdays.Remove(maxConsecutiveWorkdaysValidator);
			_maxConsecutiveWorkdaysValidator = maxConsecutiveWorkdaysValidator;
		}

		private static int[] findMoveInSpecificWeek(int weekIndex, ILockableBitArray lockableBitArray, IList<double?> values)
		{
			int firstIndex = -1;
			double lowestPairValue = double.MaxValue;
			for (int i = 0; i <= 5; i++)
			{
				int index = i + 7*weekIndex;
				if (!lockableBitArray[index])
					continue;
				if (!lockableBitArray[index + 1])
					continue;
				if (lockableBitArray.IsLocked(index, true))
					continue;
				if (lockableBitArray.IsLocked(index + 1, true))
					continue;
				var currentVal = values[index - lockableBitArray.PeriodArea.Minimum];
				if (!currentVal.HasValue)
					continue;
				var nextVal = values[index + 1 - lockableBitArray.PeriodArea.Minimum];
				if (!nextVal.HasValue)
					continue;

				double pairValue = currentVal.Value + nextVal.Value;
				if (pairValue < lowestPairValue)
				{
					lowestPairValue = pairValue;
					firstIndex = index;
				}
			}

			if (firstIndex == -1)
				return new int[0];

			return new[] { firstIndex, firstIndex + 1 };
		}

		private static bool validateConsecutiveWorkdays(ILockableBitArray workingArray, IDayOffLegalStateValidator consecutiveWorkdaysValidator)
		{
			bool valid = validateArray(workingArray, new List<IDayOffLegalStateValidator>{ consecutiveWorkdaysValidator});
			return valid;
		}

		private static bool moveAndValidate(int[] indexesToMoveFrom, int[] indexesToMoveTo, ILockableBitArray workingArray, IList<IDayOffLegalStateValidator> validatorList)
		{
			ILockableBitArray clone = (LockableBitArray)workingArray.Clone();
			clone.Set(indexesToMoveFrom[0], false);
			clone.Set(indexesToMoveTo[0], true);
			clone.Set(indexesToMoveFrom[indexesToMoveFrom.Length-1], false);
			clone.Set(indexesToMoveTo[indexesToMoveTo.Length - 1], true);

			bool valid = validateArray(clone, validatorList);
			if(valid)
			{
				workingArray.Set(indexesToMoveFrom[0], false);
				workingArray.Set(indexesToMoveTo[0], true);
				workingArray.Set(indexesToMoveFrom[indexesToMoveFrom.Length - 1], false);
				//lock this one to prevent moving back and forward and back and.....
				workingArray.Lock(indexesToMoveFrom[indexesToMoveFrom.Length - 1], true);
				workingArray.Set(indexesToMoveTo[indexesToMoveTo.Length - 1], true);

				return true;
			}

			return false;
		}

		private static int[] createPreferredIndexesToMoveFrom(ILockableBitArray lockableBitArray, IList<double?> values)
		{
			int firstIndex = -1;

			double lowestPairValue = double.MaxValue;
			foreach (var unlockedIndex in lockableBitArray.UnlockedIndexes)
			{
				if(unlockedIndex == lockableBitArray.Count -1)
					continue;
				if(!lockableBitArray[unlockedIndex])
					continue;
				if(!lockableBitArray[unlockedIndex+1])
					continue;
				if(lockableBitArray.IsLocked(unlockedIndex+1, true))
					continue;
				var currentVal = values[unlockedIndex - lockableBitArray.PeriodArea.Minimum];
				if(!currentVal.HasValue)
					continue;
				var nextVal = values[unlockedIndex + 1 - lockableBitArray.PeriodArea.Minimum];
				if (!nextVal.HasValue)
					continue;
				//In same week?
				double d1 = Math.Floor(unlockedIndex/7d);
				double d2 = Math.Floor((unlockedIndex + 1) / 7d);
				if(d1 != d2)
					continue;

				double pairValue = currentVal.Value +
				                   nextVal.Value;
				if (pairValue < lowestPairValue)
				{
					lowestPairValue = pairValue;
					firstIndex = unlockedIndex;
				}
			}

			if (firstIndex == -1)
				return new int[0];

			return new [] {firstIndex, firstIndex + 1};
		}

		private static int[] createPreferredIndexesToMoveTo(ILockableBitArray lockableBitArray, IList<double?> values, int weekIndex)
		{
			int firstIndex = -1;
			double highestPairValue = double.MinValue;
			for (int i = 0; i <= 5; i++)
			{
				int index = i + 7*weekIndex;
				if (lockableBitArray[index])
					continue;
				if (lockableBitArray.IsLocked(index, true))
					continue;
				if (lockableBitArray.IsLocked(index + 1, true))
					continue;
				var currentVal = values[index - lockableBitArray.PeriodArea.Minimum];
				if (!currentVal.HasValue)
					continue;
				var nextVal = values[index + 1 - lockableBitArray.PeriodArea.Minimum];
				if (!nextVal.HasValue)
					continue;

				double pairValue = currentVal.Value + nextVal.Value;
				if(pairValue > highestPairValue)
				{
					highestPairValue = pairValue;
					firstIndex = index;
				}
			}

			if (firstIndex == -1)
				return new int[0];

			return new [] { firstIndex, firstIndex + 1 };
		}

		private static bool validateArray(ILockableBitArray array, IList<IDayOffLegalStateValidator> validatorList)
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