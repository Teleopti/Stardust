using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class ShiftCategoryPointExtractorTest
	{
		private IShiftCategory _shiftCategory1;
		private IShiftCategory _shiftCategory2;
		private IList<IShiftCategory> _shiftCategories;
		private ShiftCategoryPoints _target;

		[SetUp]
		public void SetUp()
		{
			_shiftCategory1 = ShiftCategoryFactory.CreateShiftCategory("BB");
			_shiftCategory2 = ShiftCategoryFactory.CreateShiftCategory("AA");
			_shiftCategories = new List<IShiftCategory>{_shiftCategory1, _shiftCategory2};
			_target = new ShiftCategoryPoints();	
		}

		[Test]
		public void ShouldExtractShiftCategoryPoints()
		{
			var result = _target.ExtractShiftCategoryPoints(_shiftCategories);
			Assert.AreEqual(1, result[_shiftCategory1]);
			Assert.AreEqual(2, result[_shiftCategory2]);
		}
	}
}
