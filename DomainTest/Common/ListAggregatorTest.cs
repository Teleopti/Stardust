using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class ListAggregatorTest
	{
		private ListAggregator<int> _target;
		private int[] _numbers;

		[SetUp]
		public void Setup()
		{
			_target = new ListAggregator<int>();
		}

		[Test]
		public void VerifyAggregateCreateCorrectSubLists()
		{
			_numbers = new int[] { 1, 2, 4, 5, 6, 10 };
			IList<IList<int>> result = _target.Aggregate(_numbers, AreAttached);

			// This test should create three aggregated list
 			// 1-2, 4-5-6, 10 

			Assert.AreEqual(3, result.Count);
			Assert.AreEqual(2, result[0].Count);
			Assert.AreEqual(3, result[1].Count);
			Assert.AreEqual(1, result[2].Count);
		}

		private bool AreAttached(int value1, int value2)
		{
			return value2 - value1 == 1;
		}
	}



}
