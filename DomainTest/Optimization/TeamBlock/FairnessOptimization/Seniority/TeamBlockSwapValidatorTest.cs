using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class TeamBlockSwapValidatorTest
	{
		private MockRepository _mock;
		private ITeamMemberCountValidator _teamMemberCountValidator;
		private ITeamBlockContractTimeValidator _teamBlockContractTimeValidator;
		private ITeamBlockLockValidator _teamBlockLockValidator;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private TeamBlockSwapValidator _target;
			
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamMemberCountValidator = _mock.StrictMock<ITeamMemberCountValidator>();
			_teamBlockContractTimeValidator = _mock.StrictMock<ITeamBlockContractTimeValidator>();
			_teamBlockLockValidator = _mock.StrictMock<ITeamBlockLockValidator>();
			_teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
			_target = new TeamBlockSwapValidator(_teamMemberCountValidator, _teamBlockContractTimeValidator, _teamBlockLockValidator);
		}

		[Test]
		public void ShouldReturnTrueWhenPossibleToSwap()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamMemberCountValidator.ValidateMemberCount(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockContractTimeValidator.ValidateContractTime(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockLockValidator.ValidateLocks(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseWhenValidateMemberCountFails()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamMemberCountValidator.ValidateMemberCount(_teamBlockInfo1, _teamBlockInfo2)).Return(false);	
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2);
				Assert.IsFalse(result);
			}	
		}

		[Test]
		public void ShouldReturnFalsWhenValidateContractTimeFails()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamMemberCountValidator.ValidateMemberCount(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockLockValidator.ValidateLocks(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockContractTimeValidator.ValidateContractTime(_teamBlockInfo1, _teamBlockInfo2)).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2);
				Assert.IsFalse(result);
			}	
		}

		[Test]
		public void ShouldReturnFalsWhenValidateLocksFails()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamMemberCountValidator.ValidateMemberCount(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockLockValidator.ValidateLocks(_teamBlockInfo1, _teamBlockInfo2)).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2);
				Assert.IsFalse(result);
			}
		}
	}
}
