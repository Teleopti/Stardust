using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class ShiftCategoryFairnessComparerTest
	{
		private ShiftCategoryFairnessComparer _target;

		[SetUp]
		public void Setup()
		{
			_target = new ShiftCategoryFairnessComparer();
		}

		[Test]
		public void ShouldContainAllCategories()
		{
			var cat = ShiftCategoryFactory.CreateShiftCategory("Morning");
			var cat2 = ShiftCategoryFactory.CreateShiftCategory("Day");
			var cat3 = ShiftCategoryFactory.CreateShiftCategory("Late");

			var catDic = new Dictionary<IShiftCategory, int> { { cat, 2 }, { cat2, 2 }};
			var catDic2 = new Dictionary<IShiftCategory, int> { { cat2, 2 }, { cat3, 2 } };

			var fairness = new ShiftCategoryFairness(catDic, new FairnessValueResult());
			var fairness2 = new ShiftCategoryFairness(catDic2, new FairnessValueResult());

			var result = _target.Compare(fairness, fairness2, new List<IShiftCategory>{cat,cat2,cat3});
			Assert.That(result.Count,Is.EqualTo(3));
			Assert.That(result[0].Original, Is.EqualTo(.50));	
			Assert.That(result[1].Original, Is.EqualTo(.50));	
			Assert.That(result[2].Original, Is.EqualTo(.0));

			Assert.That(result[0].ComparedTo, Is.EqualTo(.0));
			Assert.That(result[1].ComparedTo, Is.EqualTo(.50));
			Assert.That(result[2].ComparedTo, Is.EqualTo(.50));
		}
	}

	
}