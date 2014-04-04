using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
		public bool Execute(ILockableBitArray lockableBitArray, IList<double?> values)
		{
			if (!_is2222)
				return false;

			extractConsecutiveWorkdaysValidator();
			var workingBitArray = (ILockableBitArray)lockableBitArray.Clone();

			string decisionMakerName = ToString();
			_logWriter.LogInfo("Execute of " + decisionMakerName);

			IList<int> indexesToMoveFrom = createPreferredIndexesToMoveFrom(workingBitArray, values);
			if (indexesToMoveFrom.Count() < 2)
				return false;

			int weekIndex = (int)Math.Floor(indexesToMoveFrom.First() / 7d);
			IList<int> indexesToMoveTo = createPreferredIndexesToMoveTo(workingBitArray, values, weekIndex);
			if (indexesToMoveTo.Count() < 2)
				return false;

			while(!moveAndValidate(indexesToMoveFrom, indexesToMoveTo, workingBitArray, _validatorListWithoutMaxConsecutiveWorkdays))
			{
				workingBitArray.Lock(indexesToMoveTo.First(), true);
				indexesToMoveTo = createPreferredIndexesToMoveTo(workingBitArray, values, weekIndex);
				if (indexesToMoveTo.Count() < 2)
					return false;
			}

			while (!validateConsecutiveWorkdays(workingBitArray, _maxConsecutiveWorkdaysValidator))
			{
				if (weekIndex == (int)Math.Floor((workingBitArray.Count - 1) / 7d))
					weekIndex = 0;

				indexesToMoveFrom = findMoveInSpecificWeek(weekIndex + 1, workingBitArray, values);
				if (indexesToMoveFrom.Count() < 2)
					return false;

				weekIndex = (int)Math.Floor(indexesToMoveFrom.First() / 7d);
				indexesToMoveTo = createPreferredIndexesToMoveTo(workingBitArray, values, weekIndex);
				if (indexesToMoveTo.Count() < 2)
					return false;

				while (!moveAndValidate(indexesToMoveFrom, indexesToMoveTo, workingBitArray, _validatorListWithoutMaxConsecutiveWorkdays))
				{
					workingBitArray.Lock(indexesToMoveTo.First(), true);
					indexesToMoveTo = createPreferredIndexesToMoveTo(workingBitArray, values, weekIndex);
					if (indexesToMoveTo.Count() < 2)
						return false;
				}

			}

			_logWriter.LogInfo("Move from preference index: " + createCommaSeparatedString(indexesToMoveFrom));
			_logWriter.LogInfo("Move to preference index: " + createCommaSeparatedString(indexesToMoveTo));


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

		private static IList<int> findMoveInSpecificWeek(int weekIndex, ILockableBitArray lockableBitArray, IList<double?> values)
		{
			IList<int> ret = new List<int>();

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
				if (!values[index - lockableBitArray.PeriodArea.Minimum].HasValue)
					continue;
				if (!values[index + 1 - lockableBitArray.PeriodArea.Minimum].HasValue)
					continue;

				double pairValue = values[index - lockableBitArray.PeriodArea.Minimum].Value +
								   values[index + 1 - lockableBitArray.PeriodArea.Minimum].Value;

				if (pairValue < lowestPairValue)
				{
					lowestPairValue = pairValue;
					firstIndex = index;
				}
			}

			if (firstIndex == -1)
				return ret;

			return new List<int> { firstIndex, firstIndex + 1 };
		}

		private static bool validateConsecutiveWorkdays(ILockableBitArray workingArray, IDayOffLegalStateValidator consecutiveWorkdaysValidator)
		{
			bool valid = validateArray(workingArray, new List<IDayOffLegalStateValidator>{ consecutiveWorkdaysValidator});
			return valid;
		}

		private static bool moveAndValidate(IList<int> indexesToMoveFrom, IList<int> indexesToMoveTo, ILockableBitArray workingArray, IList<IDayOffLegalStateValidator> validatorList)
		{
			ILockableBitArray clone = (LockableBitArray)workingArray.Clone();
			clone.Set(indexesToMoveFrom.First(), false);
			clone.Set(indexesToMoveTo.First(), true);
			clone.Set(indexesToMoveFrom.Last(), false);
			clone.Set(indexesToMoveTo.Last(), true);

			bool valid = validateArray(clone, validatorList);
			if(valid)
			{
				workingArray.Set(indexesToMoveFrom.First(), false);
				workingArray.Set(indexesToMoveTo.First(), true);
				workingArray.Set(indexesToMoveFrom.Last(), false);
				//lock this one to prevent moving back and forward and back and.....
				workingArray.Lock(indexesToMoveFrom.Last(), true);
				workingArray.Set(indexesToMoveTo.Last(), true);

				return true;
			}

			return false;
		}

		private static IList<int> createPreferredIndexesToMoveFrom(ILockableBitArray lockableBitArray, IList<double?> values)
		{
			IList<int> ret = new List<int>();
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
				if(!values[unlockedIndex - lockableBitArray.PeriodArea.Minimum].HasValue)
					continue;
				if (!values[unlockedIndex + 1 - lockableBitArray.PeriodArea.Minimum].HasValue)
					continue;
				//In same week?
				double d1 = Math.Floor(unlockedIndex/7d);
				double d2 = Math.Floor((unlockedIndex + 1) / 7d);
				if(d1 != d2)
					continue;

				double pairValue = values[unlockedIndex - lockableBitArray.PeriodArea.Minimum].Value +
				                   values[unlockedIndex + 1 - lockableBitArray.PeriodArea.Minimum].Value;
				if (pairValue < lowestPairValue)
				{
					lowestPairValue = pairValue;
					firstIndex = unlockedIndex;
				}
			}

			if (firstIndex == -1)
				return ret;

			return new List<int> {firstIndex, firstIndex + 1};
		}

		private static IList<int> createPreferredIndexesToMoveTo(ILockableBitArray lockableBitArray, IList<double?> values, int weekIndex)
		{
			IList<int> ret = new List<int>();

			int firstIndex = -1;
			double highestPairValue = double.MinValue;
			for (int i = 0; i <= 5; i++)
			{
				int index = i + 7*weekIndex;
				if (lockableBitArray[index])
					continue;
				//if (lockableBitArray[index + 1])
				//    continue;
				if (lockableBitArray.IsLocked(index, true))
					continue;
				if (lockableBitArray.IsLocked(index + 1, true))
					continue;
				if (!values[index - lockableBitArray.PeriodArea.Minimum].HasValue)
					continue;
				if (!values[index + 1 - lockableBitArray.PeriodArea.Minimum].HasValue)
					continue;

				double pairValue = values[index - lockableBitArray.PeriodArea.Minimum].Value +
								   values[index + 1 - lockableBitArray.PeriodArea.Minimum].Value;

				if(pairValue > highestPairValue)
				{
					highestPairValue = pairValue;
					firstIndex = index;
				}
			}

			if (firstIndex == -1)
				return ret;

			return new List<int> { firstIndex, firstIndex + 1 };
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

		private static string createCommaSeparatedString(IEnumerable<int> indexesToMoveFrom)
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