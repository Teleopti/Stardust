using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CMSB")]
	public class CMSBOneFreeWeekendMax5WorkingDaysDecisionMaker : IDayOffDecisionMaker
	{
		private readonly IOfficialWeekendDays _officialWeekendDays;
		private readonly ITrueFalseRandomizer _trueFalseRandomizer;

		public CMSBOneFreeWeekendMax5WorkingDaysDecisionMaker(IOfficialWeekendDays officialWeekendDays, ITrueFalseRandomizer trueFalseRandomizer)
		{
			_officialWeekendDays = officialWeekendDays;
			_trueFalseRandomizer = trueFalseRandomizer;
		}

		public bool Execute(ILockableBitArray lockableBitArray, IList<double?> values)
		{
			var daysOffStack = new Stack<int>();
			pickAllUnlockedDaysOff(daysOffStack, lockableBitArray);
			IEnumerable<int> indexesToMoveTo = createPreferredIndexesToMoveTo(lockableBitArray, values);
			if (!dropDaysOffSelectedWeekend(indexesToMoveTo, lockableBitArray, daysOffStack))
				return false;
			dropTheRestOfTheDaysOffLeftToRight(daysOffStack, lockableBitArray);
			dropTheRestOfTheDaysOffRightToLeft(daysOffStack, lockableBitArray);
			if (daysOffStack.Count > 0)
				return false;

			return true;
		}
		
		private static void dropTheRestOfTheDaysOffRightToLeft(Stack<int> daysOffStack, ILockableBitArray lockableBitArray)
		{
			int rightMostIndex = 0;
			for (int i = lockableBitArray.Count - 1; i >= 0; i--)
			{
				if (!lockableBitArray[i] && !lockableBitArray.IsLocked(i, true))
				{
					rightMostIndex = i;
					break;
				}
					
			}

			if(daysOffStack.Count > 0)
			{
				lockableBitArray.Set(rightMostIndex, true);
				daysOffStack.Pop();
			}

			while (daysOffStack.Count > 0)
			{
				rightMostIndex -= 4;
				if (rightMostIndex < 0)
					return;

				if (!lockableBitArray[rightMostIndex] && !lockableBitArray.IsLocked(rightMostIndex, true))
				{
					lockableBitArray.Set(rightMostIndex, true);
					daysOffStack.Pop();
				}
			}
		}

		private static void dropTheRestOfTheDaysOffLeftToRight(Stack<int> daysOffStack, ILockableBitArray lockableBitArray)
		{
			int leftMostIndex = -1;
			while (daysOffStack.Count > 0)
			{
				leftMostIndex += 6;
				if (leftMostIndex >= lockableBitArray.Count - 1)
					break;

				if(!lockableBitArray[leftMostIndex] && !lockableBitArray.IsLocked(leftMostIndex, true))
				{
					lockableBitArray.Set(leftMostIndex, true);
					daysOffStack.Pop();
				}
			}
		}

		private static bool dropDaysOffSelectedWeekend(IEnumerable<int> indexesToMoveTo, ILockableBitArray lockableBitArray, Stack<int> daysOffStack)
		{
			if (daysOffStack.Count < 2)
				return false;

			foreach (var index in indexesToMoveTo)
			{
				lockableBitArray.Set(index, true);
				daysOffStack.Pop();
			}

			return true;
		}

		private static void pickAllUnlockedDaysOff(Stack<int> daysOffStack, ILockableBitArray lockableBitArray)
		{
			for (int i = 0; i < lockableBitArray.Count; i++)
			{
				if (lockableBitArray[i] && !lockableBitArray.IsLocked(i, true))
				{
					daysOffStack.Push(1);
					lockableBitArray.Set(i, false);
				}
			}
		}

		private IEnumerable<int> createPreferredIndexesToMoveTo(ILockableBitArray lockableBitArray, IList<double?> values)
		{
			IEnumerable<Tuple<int, int>> weekendDayIndexes =
				extractWeekendDayIndexes(lockableBitArray.PeriodArea.Minimum, lockableBitArray.PeriodArea.Maximum);

			////Add the aggregated values for the weekend
			Tuple<int, int> maxTuple = null;
			double maxValue = double.MinValue;
			foreach (var weekendDayIndexTuple in weekendDayIndexes)
			{
				double aggregatedValue = values[weekendDayIndexTuple.Item1 - lockableBitArray.PeriodArea.Minimum].Value + values[weekendDayIndexTuple.Item2 - lockableBitArray.PeriodArea.Minimum].Value;
				if (lockableBitArray.IsLocked(weekendDayIndexTuple.Item1, true))
					continue;
				if (lockableBitArray.IsLocked(weekendDayIndexTuple.Item2, true))
					continue;
				if (aggregatedValue == maxValue)
				{
					if(_trueFalseRandomizer.Randomize())
					{
						maxTuple = weekendDayIndexTuple;
					}
				}
				if(aggregatedValue > maxValue)
				{
					maxValue = aggregatedValue;
					maxTuple = weekendDayIndexTuple;
				}
			}

			IList<int> ret = new List<int>();
			if(maxTuple != null)
			{ 
				ret.Add(maxTuple.Item1);
				ret.Add(maxTuple.Item2);
			}

			return ret;
		}

		private IEnumerable<Tuple<int, int>> extractWeekendDayIndexes(int minimumIndex, int maximumIndex)
		{
			IList<Tuple<int, int>> result = new List<Tuple<int, int>>();
			var officialWeekendDays = _officialWeekendDays.WeekendDayIndexesRelativeStartDayOfWeek();

			for (int i = minimumIndex; i <= maximumIndex; i++)
			{
				if (i+1 > maximumIndex)
					break;
				int weekPosition = i % 7;
				if (officialWeekendDays.Contains(weekPosition) && officialWeekendDays.Contains(weekPosition + 1))
				{
					result.Add(new Tuple<int, int>(i, i + 1));
					i++;
				}	
			}
			return result;
		}

	}
}