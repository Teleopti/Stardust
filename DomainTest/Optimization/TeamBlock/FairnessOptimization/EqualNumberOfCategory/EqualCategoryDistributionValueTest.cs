using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	[TestFixture]
	public class EqualCategoryDistributionValueTest
	{
		private MockRepository _mocks;
		private IEqualCategoryDistributionValue _target;
		private IDistributionForPersons _distributionForPersons;
		private ITeamBlockInfo _teamBlockInfo;
		private IScheduleDictionary _scheduleDictionary;
		private ITeamInfo _teamInfo;
		private IGroupPerson _groupPerson;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_distributionForPersons = _mocks.StrictMock<IDistributionForPersons>();
			_target = new EqualCategoryDistributionValue(_distributionForPersons);
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_groupPerson = _mocks.StrictMock<IGroupPerson>();

		}

		[Test]
		public void ShouldCalculate()
		{
			var category1 = ShiftCategoryFactory.CreateShiftCategory("Cat1");
			var category2 = ShiftCategoryFactory.CreateShiftCategory("Cat2");
			var categoryDicAll = new Dictionary<IShiftCategory, int>();
			categoryDicAll.Add(category1, 3);
			categoryDicAll.Add(category2, 1);
			IDistributionSummary totalDistribution = new DistributionSummary(categoryDicAll);
			var categoryDicPerson = new Dictionary<IShiftCategory, int>();
			categoryDicPerson.Add(category1, 1);
			IDistributionSummary personDistribution = new DistributionSummary(categoryDicPerson);
			var person = PersonFactory.CreatePerson("Kalle");
			var personList = new[] {person};

			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupPerson).Return(_groupPerson);
				Expect.Call(_groupPerson.GroupMembers).Return(personList);
				Expect.Call(_distributionForPersons.CreateSummary(personList, _scheduleDictionary)).Return(personDistribution);
			}

			using (_mocks.Playback())
			{
				var result = _target.CalculateValue(_teamBlockInfo, totalDistribution, _scheduleDictionary);
				Assert.AreEqual(0.5, result);
			}
      
      
		}

		[Test]
		public void ShouldCalculateAHigherValue()
		{
			var category1 = ShiftCategoryFactory.CreateShiftCategory("Cat1");
			var category2 = ShiftCategoryFactory.CreateShiftCategory("Cat2");
			var categoryDicAll = new Dictionary<IShiftCategory, int>();
			categoryDicAll.Add(category1, 3);
			categoryDicAll.Add(category2, 1);
			IDistributionSummary totalDistribution = new DistributionSummary(categoryDicAll);
			var categoryDicPerson = new Dictionary<IShiftCategory, int>();
			categoryDicPerson.Add(category1, 1);
			categoryDicPerson.Add(category2, 3);
			IDistributionSummary personDistribution = new DistributionSummary(categoryDicPerson);
			var person = PersonFactory.CreatePerson("Kalle");
			var personList = new[] { person };

			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupPerson).Return(_groupPerson);
				Expect.Call(_groupPerson.GroupMembers).Return(personList);
				Expect.Call(_distributionForPersons.CreateSummary(personList, _scheduleDictionary)).Return(personDistribution);
			}

			using (_mocks.Playback())
			{
				var result = _target.CalculateValue(_teamBlockInfo, totalDistribution, _scheduleDictionary);
				Assert.AreEqual(1, result);
			}


		}

	}
}