using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class ShiftCategoryPointsTest
	{
		private ShiftCategoryPoints _target;

		[Test]
		public void ShouldExtractPoints()
		{
			_target = new ShiftCategoryPoints();
			var shiftCategory1 = ShiftCategoryFactory.CreateShiftCategory("shiftCategory1");
			var shiftCategory2 = ShiftCategoryFactory.CreateShiftCategory("shiftCategory2");
			var shiftCategory3 = ShiftCategoryFactory.CreateShiftCategory("shiftCategory3");

			shiftCategory1.Rank = 2;
			shiftCategory2.Rank = 1;

			var shiftCatgories = new List<IShiftCategory> {shiftCategory3, shiftCategory2, shiftCategory1};

			var result = _target.ExtractShiftCategoryPoints(shiftCatgories);

			Assert.AreEqual(3, result.Count);
			Assert.AreEqual(1, result[shiftCategory3]);
			Assert.AreEqual(2, result[shiftCategory1]);
			Assert.AreEqual(3, result[shiftCategory2]);
		}
	}
}
