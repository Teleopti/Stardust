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
		private IScheduleDictionary _scheduleDictionary;
		private ITeamBlockInfo _teamBlockInfo1;
		private IEqualCategoryDistributionValue _equalCategoryDistributionValue;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_equalCategoryDistributionValue = _mocks.StrictMock<IEqualCategoryDistributionValue>();
			_target = new EqualCategoryDistributionWorstTeamBlockDecider(_equalCategoryDistributionValue);
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_teamBlockInfo1 = _mocks.StrictMock<ITeamBlockInfo>();
		}
		

		[Test]
		public void ShouldReturnTeamBlockWithHighestDistributionError()
		{
			var category1 = ShiftCategoryFactory.CreateShiftCategory("hej");
			var category2 = ShiftCategoryFactory.CreateShiftCategory("hopp");
			var distributionDic1 = new Dictionary<IShiftCategory, int> {{category1, 1}, {category2, 1}};
			var totalDistribution = new DistributionSummary(distributionDic1);

			using (_mocks.Record())
			{
				Expect.Call(_equalCategoryDistributionValue.CalculateValue(_teamBlockInfo1, totalDistribution, _scheduleDictionary)).Return(5);
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