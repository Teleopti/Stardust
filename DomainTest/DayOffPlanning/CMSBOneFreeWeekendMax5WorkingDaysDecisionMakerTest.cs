using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
	[TestFixture]
	public class CMSBOneFreeWeekendMax5WorkingDaysDecisionMakerTest
	{
		private IDayOffDecisionMaker _target;
		private IList<double?> _values;
		private ILockableBitArray _workingArray;

		[SetUp]
		public void Setup()
		{
			_target = new CMSBOneFreeWeekendMax5WorkingDaysDecisionMaker(new OfficialWeekendDays(), new falseRandomizerForTest());
			_values = new List<double?> { 0, 1, 3, 40, 25, 3, 2, 0, 1, 3, 30, 30, 3, 2 };
			_workingArray = new LockableBitArray(21, false, false, null);
			_workingArray.PeriodArea = new MinMax<int>(2, 15);
			_workingArray.Lock(0, true);
			_workingArray.Lock(1, true);
			_workingArray.Lock(16, true);
			_workingArray.Lock(17, true);
			_workingArray.Lock(18, true);
			_workingArray.Lock(19, true);
			_workingArray.Lock(20, true);
		}

		[Test]
		public void ShouldReturnFalseIfNotAtLeastTwoUnlockedDaysOffFound()
		{
			_workingArray.Set(5, true);
			_workingArray.Set(6, true);
			_workingArray.Lock(5, true);
			_workingArray.Lock(6, true);
			_workingArray.Set(12, true);
			_workingArray.Set(13, true);
			_workingArray.Lock(13, true);

			Assert.IsFalse(_target.Execute(_workingArray, _values));
		}

		[Test]
		public void ShouldAssignToLeastUnderstaffedWeekend()
		{
			_workingArray.Set(5, true);
			_workingArray.Set(6, true);
			_workingArray.Set(12, true);
			_workingArray.Set(13, true);

			Assert.IsTrue(_target.Execute(_workingArray, _values));
			Assert.IsTrue(_workingArray[5]);
			Assert.IsTrue(_workingArray[6]);
			Assert.IsFalse(_workingArray[12]);
			Assert.IsFalse(_workingArray[13]);
		}

		[Test]
		public void ShouldDropRestOfDaysEveryFiveSpotLeftToRight()
		{
			_workingArray.Set(5, true);
			_workingArray.Set(6, true);
			_workingArray.Set(12, true);
			_workingArray.Set(13, true);

			Assert.IsTrue(_target.Execute(_workingArray, _values));
			Assert.IsTrue(_workingArray[5]);
			Assert.IsTrue(_workingArray[6]);
			Assert.IsTrue(_workingArray[11]); //<-- first free spot counting 5days from left to right
		}

		[Test]
		public void ShouldDropRestOfDaysRightToLeftStartingWithLastUnlockedSpotAndThenEveryFourSpot()
		{
			_workingArray.Set(5, true);
			_workingArray.Set(6, true);
			_workingArray.Set(7, true);
			_workingArray.Set(12, true);
			_workingArray.Set(13, true);

			Assert.IsTrue(_target.Execute(_workingArray, _values));
			Assert.IsTrue(_workingArray[5]);
			Assert.IsTrue(_workingArray[6]);
			Assert.IsTrue(_workingArray[11]); //<-- first free spot counting 5days from left to right
			Assert.IsTrue(_workingArray[15]); //<-- last spot in the area
			Assert.IsTrue(_workingArray[7]); //<-- first free spot counteing 4 from right to left
		}

		[Test]
		public void ShouldReturnFalseIfNotAllDaysOffCouldBeDropped()
		{
			_workingArray.Set(4, true);
			_workingArray.Set(5, true);
			_workingArray.Set(6, true);
			_workingArray.Set(7, true);
			_workingArray.Set(12, true);
			_workingArray.Set(13, true);
			_workingArray.Set(14, true);

			Assert.IsFalse(_target.Execute(_workingArray, _values));
			Assert.IsTrue(_workingArray[5]);
			Assert.IsTrue(_workingArray[6]);
			Assert.IsTrue(_workingArray[11]); //<-- first free spot counting 5days from left to right
			Assert.IsTrue(_workingArray[15]); //<-- last spot in the area
			Assert.IsTrue(_workingArray[7]); //<-- first free spot counting 4 from right to left
			Assert.IsTrue(_workingArray[3]); //<-- next free spot counting 4 from right to left
		}

		[Test]
		public void ShouldDetectFullWeekendsOnly()
		{
			_values = new List<double?> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };
			_workingArray = new LockableBitArray(42, false, false, null);
			_workingArray.PeriodArea = new MinMax<int>(6, 36);
			_workingArray.Set(6, true);
			_workingArray.Set(7, true);

			Assert.IsTrue(_target.Execute(_workingArray, _values));
			Assert.IsTrue(_workingArray[33]);
			Assert.IsTrue(_workingArray[34]);
		}

		private class falseRandomizerForTest : ITrueFalseRandomizer
		{
			public bool Randomize()
			{
				return false;
			}
		}
	}
}