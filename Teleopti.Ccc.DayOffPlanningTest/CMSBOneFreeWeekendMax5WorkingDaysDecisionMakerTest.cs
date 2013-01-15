using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CMSB"), TestFixture]
	public class CMSBOneFreeWeekendMax5WorkingDaysDecisionMakerTest
	{
		private IDayOffDecisionMaker _target;
		private IList<double?> _values;
		private ILockableBitArray _workingArray;

		[SetUp]
		public void Setup()
		{
			_target = new CMSBOneFreeWeekendMax5WorkingDaysDecisionMaker(new OfficialWeekendDays(), new FalseRandomizerForTest());
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
	}

	public class FalseRandomizerForTest : ITrueFalseRandomizer
	{
		public bool Randomize(int seed)
		{
			return false;
		}
	}
}