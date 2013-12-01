using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	[TestFixture]
	public class DistributionForPersonsTest
	{
		private MockRepository _mocks;
		private IDistributionForPersons _target;
		private IPerson _person1;
		private IPerson _person2;
		private List<IPerson> _personList;
		private IScheduleDictionary _dic;
		private IScheduleRange _range1;
		private IScheduleRange _range2;
		private IShiftCategoryFairnessHolder _cachedFairness1;
		private IShiftCategoryFairnessHolder _cachedFairness2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new DistributionForPersons();
			_person1 = PersonFactory.CreatePerson();
			_person2 = PersonFactory.CreatePerson();
			_personList = new List<IPerson> {_person1, _person2};
			_dic = _mocks.StrictMock<IScheduleDictionary>();
			_range1 = _mocks.StrictMock<IScheduleRange>();
			_range2 = _mocks.StrictMock<IScheduleRange>();
			_cachedFairness1 = _mocks.StrictMock<IShiftCategoryFairnessHolder>();
			_cachedFairness2 = _mocks.StrictMock<IShiftCategoryFairnessHolder>();
		}

		[Test]
		public void ShouldSummarize()
		{
			var category1 = ShiftCategoryFactory.CreateShiftCategory("hej");
			var category2 = ShiftCategoryFactory.CreateShiftCategory("hopp");
			var dic1 = new Dictionary<IShiftCategory, int>();
			dic1.Add(category1, 2);
			dic1.Add(category2, 3);
			var dic2 = new Dictionary<IShiftCategory, int>();
			dic2.Add(category1, 2);

			using (_mocks.Record())
			{
				Expect.Call(_dic[_person1]).Return(_range1);
				Expect.Call(_range1.CachedShiftCategoryFairness()).Return(_cachedFairness1);
				Expect.Call(_cachedFairness1.ShiftCategoryFairnessDictionary).Return(dic1);

				Expect.Call(_dic[_person2]).Return(_range2);
				Expect.Call(_range2.CachedShiftCategoryFairness()).Return(_cachedFairness2);
				Expect.Call(_cachedFairness2.ShiftCategoryFairnessDictionary).Return(dic2);
			}

			using (_mocks.Playback())
			{
				var result = _target.CreateSummary(_personList, _dic);
				Assert.That(result[category1] == 4);
				Assert.That(result[category2] == 3);
			}
		}
	}
}