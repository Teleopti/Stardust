using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;



namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization
{
	[TestFixture]
	public class TeamBlockSameRuleSetBagValidatorTest
	{
		private MockRepository _mocks;
		private ITeamBlockSameRuleSetBagValidator _target;
		private ITeamBlockInfo _teamBlockInfo;
		private ITeamInfo _teamInfo;
		private IPerson _person1;
		private IPerson _person2;
		private BlockInfo _blockInfo;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new TeamBlockSameRuleSetBagValidator();
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly());
			_person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly());
			_blockInfo = new BlockInfo(new DateOnlyPeriod(2013, 12, 4, 2013, 12, 4));
		}

		[Test]
		public void ShouldReturnTrueIfAllRuleSetBagsMatch()
		{
			var ruleSetBag = new RuleSetBag();
			_person1.Period(new DateOnly()).RuleSetBag = ruleSetBag;
			_person2.Period(new DateOnly()).RuleSetBag = ruleSetBag;
			var members1 = new List<IPerson> { _person1 };
			var members2 = new List<IPerson> { _person2 };
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(members1);

				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(members2);

				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
			}

			using (_mocks.Playback())
			{
				var result = _target.ValidateSameRuleSetBag(_teamBlockInfo, _teamBlockInfo);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfAllRuleSetBagsDoesNotMatch()
		{
			_person1.Period(new DateOnly()).RuleSetBag = new RuleSetBag();
			_person2.Period(new DateOnly()).RuleSetBag = new RuleSetBag();
			var members1 = new List<IPerson> { _person1 };
			var members2 = new List<IPerson> { _person2 };
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(members1);

				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(members2);

				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
			}

			using (_mocks.Playback())
			{
				var result = _target.ValidateSameRuleSetBag(_teamBlockInfo, _teamBlockInfo);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldHandleRuleSetBagEqualsNull()
		{
			_person1.Period(new DateOnly()).RuleSetBag = null;
			_person2.Period(new DateOnly()).RuleSetBag = new RuleSetBag();
			var members1 = new List<IPerson> { _person1 };
			var members2 = new List<IPerson> { _person2 };
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(members1);

				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(members2);

				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
			}

			using (_mocks.Playback())
			{
				var result = _target.ValidateSameRuleSetBag(_teamBlockInfo, _teamBlockInfo);
				Assert.IsFalse(result);
			}
		}

	}
}