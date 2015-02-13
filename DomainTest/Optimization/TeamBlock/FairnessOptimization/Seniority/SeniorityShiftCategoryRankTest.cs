using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class SeniorityShiftCategoryRankTest
	{
		private SeniorityShiftCategoryRank _target;
		private IShiftCategory _shiftCategory;
	
		[SetUp]
		public void SetUp()
		{
			_shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");
			_target = new SeniorityShiftCategoryRank(_shiftCategory);
		}

		[Test]
		public void ShouldSetDefaultValues()
		{
			Assert.AreEqual(_shiftCategory, _target.ShiftCategory);
			Assert.AreEqual(Int32.MaxValue, _target.Rank);
		}

		[Test]
		public void ShouldSetRank()
		{
			_target.Rank = 1;
			Assert.AreEqual(1, _target.Rank);
		}

		[Test]
		public void ShouldGetText()
		{
			Assert.AreEqual(_shiftCategory.Description.Name, _target.Text);	
		}
	}
}
