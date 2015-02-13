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
		private IShiftCategory _shiftCategory3;
		private IShiftCategory _shiftCategory4;
		private IList<IShiftCategory> _shiftCategories;
		private ShiftCategoryPoints _target;

		[SetUp]
		public void SetUp()
		{
			_shiftCategory1 = ShiftCategoryFactory.CreateShiftCategory("DD");
			_shiftCategory1.Rank = 1; 
			_shiftCategory2 = ShiftCategoryFactory.CreateShiftCategory("CC");
			_shiftCategory2.Rank = 2;
			_shiftCategory3 = ShiftCategoryFactory.CreateShiftCategory("BB");
			_shiftCategory4 = ShiftCategoryFactory.CreateShiftCategory("AA");
			_shiftCategories = new List<IShiftCategory>{_shiftCategory1, _shiftCategory2, _shiftCategory3, _shiftCategory4};
			_target = new ShiftCategoryPoints();	
		}

		[Test]
		public void ShouldExtractShiftCategoryPoints()
		{
			var result = _target.ExtractShiftCategoryPoints(_shiftCategories);
			Assert.AreEqual(1, result[_shiftCategory3]);
			Assert.AreEqual(2, result[_shiftCategory4]);
			Assert.AreEqual(3, result[_shiftCategory2]);
			Assert.AreEqual(4, result[_shiftCategory1]);
		}
	}
}
