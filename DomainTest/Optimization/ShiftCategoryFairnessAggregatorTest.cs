using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class ShiftCategoryFairnessAggregatorTest
	{
		private MockRepository _mocks;
		private IScheduleDictionary _dic;
		private IScheduleRange _range1;
		private IScheduleRange _range2;
		private IShiftCategoryFairnessAggregator _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dic = _mocks.StrictMock<IScheduleDictionary>();
			_range1 = _mocks.StrictMock<IScheduleRange>();
			_range2 = _mocks.StrictMock<IScheduleRange>();

			_target = new ShiftCategoryFairnessAggregator();
		}

		[Test]
		public void ShouldAggregateCategories()
		{
			var cat = ShiftCategoryFactory.CreateShiftCategory("lille katt");
			var person1 = PersonFactory.CreatePerson("herr persson");
			var person2 = PersonFactory.CreatePerson("herr person");
			var catDic = new Dictionary<IShiftCategory, int> {{cat, 2}};

			var fairness = new ShiftCategoryFairness(catDic,new FairnessValueResult() );

			Expect.Call(_dic[person1]).Return(_range1);
			Expect.Call(_range1.CachedShiftCategoryFairness()).Return(fairness);
			Expect.Call(_dic[person2]).Return(_range2);
			Expect.Call(_range2.CachedShiftCategoryFairness()).Return(fairness);
			_mocks.ReplayAll();
			var result = _target.GetShiftCategoryFairnessForPersons(_dic, new List<IPerson> {person1, person2});
			Assert.That(result.ShiftCategoryFairnessDictionary[cat],Is.EqualTo(4));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldAddCategories()
		{
			var cat = ShiftCategoryFactory.CreateShiftCategory("lille katt");
			var cat2 = ShiftCategoryFactory.CreateShiftCategory("lille katt");
			var person1 = PersonFactory.CreatePerson("herr persson");
			var person2 = PersonFactory.CreatePerson("herr person");
			var catDic = new Dictionary<IShiftCategory, int> { { cat, 2 } };
			var catDic2 = new Dictionary<IShiftCategory, int> { { cat2, 2 } };

			var fairness = new ShiftCategoryFairness(catDic, new FairnessValueResult());
			var fairness2 = new ShiftCategoryFairness(catDic2, new FairnessValueResult());

			Expect.Call(_dic[person1]).Return(_range1);
			Expect.Call(_range1.CachedShiftCategoryFairness()).Return(fairness);
			Expect.Call(_dic[person2]).Return(_range2);
			Expect.Call(_range2.CachedShiftCategoryFairness()).Return(fairness2);
			_mocks.ReplayAll();
			var result = _target.GetShiftCategoryFairnessForPersons(_dic, new List<IPerson> { person1, person2 });
			Assert.That(result.ShiftCategoryFairnessDictionary[cat], Is.EqualTo(2));
			Assert.That(result.ShiftCategoryFairnessDictionary[cat2], Is.EqualTo(2));
			_mocks.VerifyAll();
		}
	}

	
}