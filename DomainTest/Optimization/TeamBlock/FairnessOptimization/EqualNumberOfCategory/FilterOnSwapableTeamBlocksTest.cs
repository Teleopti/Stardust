using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	[TestFixture]
	public class FilterOnSwapableTeamBlocksTest
	{
		private MockRepository _mocks;
		private IFilterOnSwapableTeamBlocks _target;
		private ITeamBlockPeriodValidator _teamBlockPeriodValidator;
		private ITeamMemberCountValidator _teamMemberCountValidator;
		private ITeamBlockContractTimeValidator _teamBlockContractTimeValidator;
		private ITeamBlockSameSkillValidator _teamBlockSameSkillValidator;
		private ITeamBlockSameRuleSetBagValidator _teamBlockSameRuleSetBagValidator;
		private ITeamBlockSameTimeZoneValidator _teamBlockSameTimeZoneValidator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamBlockPeriodValidator = _mocks.StrictMock<ITeamBlockPeriodValidator>();
			_teamMemberCountValidator = _mocks.StrictMock<ITeamMemberCountValidator>();
			_teamBlockContractTimeValidator = _mocks.StrictMock<ITeamBlockContractTimeValidator>();
			_teamBlockSameSkillValidator = _mocks.StrictMock<ITeamBlockSameSkillValidator>();
			_teamBlockSameRuleSetBagValidator = _mocks.StrictMock<ITeamBlockSameRuleSetBagValidator>();
			_teamBlockSameTimeZoneValidator = _mocks.StrictMock<ITeamBlockSameTimeZoneValidator>();
			_target = new FilterOnSwapableTeamBlocks(_teamBlockPeriodValidator, _teamMemberCountValidator,
			                                         _teamBlockContractTimeValidator, _teamBlockSameSkillValidator,
			                                         _teamBlockSameRuleSetBagValidator,
													 _teamBlockSameTimeZoneValidator);
		}

		[Test]
		public void ShouldFilter()
		{
			var teamBlock1 = _mocks.StrictMock<ITeamBlockInfo>();
			var teamBlock2 = _mocks.StrictMock<ITeamBlockInfo>();
			var teamBlockToCompare = teamBlock1;
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockPeriodValidator.ValidatePeriod(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamMemberCountValidator.ValidateMemberCount(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockContractTimeValidator.ValidateContractTime(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockSameSkillValidator.ValidateSameSkill(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockSameRuleSetBagValidator.ValidateSameRuleSetBag(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockSameTimeZoneValidator.Validate(teamBlock1, teamBlockToCompare)).Return(true);

				Expect.Call(_teamBlockPeriodValidator.ValidatePeriod(teamBlock2, teamBlockToCompare)).Return(true);
				Expect.Call(_teamMemberCountValidator.ValidateMemberCount(teamBlock2, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockContractTimeValidator.ValidateContractTime(teamBlock2, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockSameSkillValidator.ValidateSameSkill(teamBlock2, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockSameRuleSetBagValidator.ValidateSameRuleSetBag(teamBlock2, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockSameTimeZoneValidator.Validate(teamBlock2, teamBlockToCompare)).Return(true);
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(new List<ITeamBlockInfo> { teamBlock1, teamBlock2 }, teamBlockToCompare);
				Assert.That(result[0].Equals(teamBlock1));
				Assert.That(result[1].Equals(teamBlock2));
			}
		}

		[Test]
		public void ShouldRemoveNotSameTimeZone()
		{
			var teamBlock1 = _mocks.StrictMock<ITeamBlockInfo>();
			var teamBlockToCompare = teamBlock1;
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockPeriodValidator.ValidatePeriod(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamMemberCountValidator.ValidateMemberCount(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockContractTimeValidator.ValidateContractTime(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockSameSkillValidator.ValidateSameSkill(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockSameRuleSetBagValidator.ValidateSameRuleSetBag(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockSameTimeZoneValidator.Validate(teamBlock1, teamBlockToCompare)).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(new List<ITeamBlockInfo> { teamBlock1 }, teamBlockToCompare);
				Assert.That(result.Count == 0);
			}
		}
		

		[Test]
		public void ShouldRemoveIllegalRuleSetBags()
		{
			var teamBlock1 = _mocks.StrictMock<ITeamBlockInfo>();
			var teamBlockToCompare = teamBlock1;
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockPeriodValidator.ValidatePeriod(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamMemberCountValidator.ValidateMemberCount(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockContractTimeValidator.ValidateContractTime(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockSameSkillValidator.ValidateSameSkill(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockSameRuleSetBagValidator.ValidateSameRuleSetBag(teamBlock1, teamBlockToCompare)).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(new List<ITeamBlockInfo> { teamBlock1 }, teamBlockToCompare);
				Assert.That(result.Count == 0);
			}
		}

		[Test]
		public void ShouldRemoveIllegalSkills()
		{
			var teamBlock1 = _mocks.StrictMock<ITeamBlockInfo>();
			var teamBlockToCompare = teamBlock1;
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockPeriodValidator.ValidatePeriod(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamMemberCountValidator.ValidateMemberCount(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockContractTimeValidator.ValidateContractTime(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockSameSkillValidator.ValidateSameSkill(teamBlock1, teamBlockToCompare)).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(new List<ITeamBlockInfo> { teamBlock1 }, teamBlockToCompare);
				Assert.That(result.Count == 0);
			}
		}

		[Test]
		public void ShouldRemoveIllegalContractTime()
		{
			var teamBlock1 = _mocks.StrictMock<ITeamBlockInfo>();
			var teamBlockToCompare = teamBlock1;
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockPeriodValidator.ValidatePeriod(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamMemberCountValidator.ValidateMemberCount(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamBlockContractTimeValidator.ValidateContractTime(teamBlock1, teamBlockToCompare)).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(new List<ITeamBlockInfo> { teamBlock1 }, teamBlockToCompare);
				Assert.That(result.Count == 0);
			}
		}

		[Test]
		public void ShouldRemoveIllegalMemberCount()
		{
			var teamBlock1 = _mocks.StrictMock<ITeamBlockInfo>();
			var teamBlockToCompare = teamBlock1;
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockPeriodValidator.ValidatePeriod(teamBlock1, teamBlockToCompare)).Return(true);
				Expect.Call(_teamMemberCountValidator.ValidateMemberCount(teamBlock1, teamBlockToCompare)).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(new List<ITeamBlockInfo> { teamBlock1 }, teamBlockToCompare);
				Assert.That(result.Count == 0);
			}
		}

		[Test]
		public void ShouldRemoveIllegalPeriod()
		{
			var teamBlock1 = _mocks.StrictMock<ITeamBlockInfo>();
			var teamBlockToCompare = teamBlock1;
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockPeriodValidator.ValidatePeriod(teamBlock1, teamBlockToCompare)).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(new List<ITeamBlockInfo> { teamBlock1 }, teamBlockToCompare);
				Assert.That(result.Count == 0);
			}
		}

		[Test]
		public void ShouldRemoveNullBlock()
		{
			var teamBlock1 = _mocks.StrictMock<ITeamBlockInfo>();
			var result = _target.Filter(new List<ITeamBlockInfo> {teamBlock1}, null);
			Assert.That(result.Count == 0);
			_mocks.ReplayAll();
		}
	}
}