using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.MoveTimeOptimization
{
	[TestFixture]
	public class DayValueUnlockedIndexSorterTest
	{
		private DayValueUnlockedIndexSorter _target;

		[SetUp]
		public void Setup()
		{
			_target = new DayValueUnlockedIndexSorter();
		}

		[Test]
		public void ShouldReturnAscendingList()
		{
			ILockableBitArray lockableBitArray = createBitArray();
			IList<double? > values = new List<double?> { -20, -10, 10, 30,40,-40 };
			var sortedList = _target.SortAscending(lockableBitArray, values).ToArray();
			Assert.AreEqual(6,sortedList[0]);
			Assert.AreEqual(1,sortedList[1]);
			Assert.AreEqual(2,sortedList[2]);
			Assert.AreEqual(3,sortedList[3]);
			Assert.AreEqual(4,sortedList[4]);
		}

		[Test]
		public void ShouldReturnDescendingList()
		{
			ILockableBitArray lockableBitArray = createBitArray();
			IList<double?> values = new List<double?> { -20, -10, 10, 30, 40, -40 };
			var sortedList = _target.SortDescending(lockableBitArray, values).ToArray();
			Assert.AreEqual(4, sortedList[0]);
			Assert.AreEqual(3, sortedList[1]);
			Assert.AreEqual(2, sortedList[2]);
			Assert.AreEqual(1, sortedList[3]);
			Assert.AreEqual(6, sortedList[4]);
		}

		private static ILockableBitArray createBitArray()
		{
			ILockableBitArray bitArray = new LockableBitArray(7, false, false);
			bitArray.PeriodArea = new MinMax<int>(1, 5);
			bitArray.Lock(0, true);
			bitArray.Lock(5, true);
			return bitArray;
		}
	}

	
}
