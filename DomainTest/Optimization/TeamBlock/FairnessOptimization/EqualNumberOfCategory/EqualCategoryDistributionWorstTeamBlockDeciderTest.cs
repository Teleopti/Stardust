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
	public class EqualCategoryDistributionWorstTeamBlockDeciderTest
	{
		private MockRepository _mocks;
		private IEqualCategoryDistributionWorstTeamBlockDecider _target;
		private IDistributionForPersons _distributionForPersons;
		private IScheduleDictionary _scheduleDictionary;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamInfo _teamInfo;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_distributionForPersons = _mocks.StrictMock<IDistributionForPersons>();
			_target = new EqualCategoryDistributionWorstTeamBlockDecider(_distributionForPersons);
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_teamBlockInfo1 = _mocks.StrictMock<ITeamBlockInfo>();
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
		}
		

		[Test]
		public void ShouldReturnTeamBlockWithHighestDistributionError()
		{
			var category1 = ShiftCategoryFactory.CreateShiftCategory("hej");
			var category2 = ShiftCategoryFactory.CreateShiftCategory("hopp");
			var distributionDic1 = new Dictionary<IShiftCategory, int> {{category1, 1}, {category2, 1}};
			var totalDistribution = new DistributionSummary(distributionDic1);
			var distributionDic2 = new Dictionary<IShiftCategory, int> {{category1, 1}};
			var teamBlockDistribution = new DistributionSummary(distributionDic2);
			var person1 = PersonFactory.CreatePerson();
			var memberList = new List<IPerson> {person1};

			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(memberList);
				Expect.Call(_distributionForPersons.CreateSummary(memberList, _scheduleDictionary)).Return(teamBlockDistribution);
			}

			using (_mocks.Playback())
			{
				var result = _target.FindBlockToWorkWith(totalDistribution, new List<ITeamBlockInfo> {_teamBlockInfo1},
				                                         _scheduleDictionary);
				Assert.That(result.Equals(_teamBlockInfo1));
			}
		}
	}
}