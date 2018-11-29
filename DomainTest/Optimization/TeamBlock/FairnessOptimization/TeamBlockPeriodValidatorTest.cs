using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization
{
	[TestFixture]
	public class TeamBlockPeriodValidatorTest
	{
		private MockRepository _mock;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private IBlockInfo _blockInfo1;
		private IBlockInfo _blockInfo2;
		private TeamBlockPeriodValidator _target;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
			_blockInfo1 = _mock.StrictMock<IBlockInfo>();
			_blockInfo2 = _mock.StrictMock<IBlockInfo>();
			_target = new TeamBlockPeriodValidator();
		}

		[Test]
		public void ShouldReturnTrueWhenSamePeriod()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 1);

			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo1.BlockInfo).Return(_blockInfo1);
				Expect.Call(_teamBlockInfo2.BlockInfo).Return(_blockInfo2);
				Expect.Call(_blockInfo1.BlockPeriod).Return(dateOnlyPeriod);
				Expect.Call(_blockInfo2.BlockPeriod).Return(dateOnlyPeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidatePeriod(_teamBlockInfo1, _teamBlockInfo2);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseWhenDifferentPeriod()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 1);
			var differentDateOnlyPeriod = new DateOnlyPeriod(2014, 1, 1, 2014, 1, 1);

			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo1.BlockInfo).Return(_blockInfo1);
				Expect.Call(_teamBlockInfo2.BlockInfo).Return(_blockInfo2);
				Expect.Call(_blockInfo1.BlockPeriod).Return(dateOnlyPeriod);
				Expect.Call(_blockInfo2.BlockPeriod).Return(differentDateOnlyPeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidatePeriod(_teamBlockInfo1, _teamBlockInfo2);
				Assert.IsFalse(result);
			}	
		}
	}
}
