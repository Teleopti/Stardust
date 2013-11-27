using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization
{
	[TestFixture]
	public class TeamBlockSwapValidatorTest
	{
		private MockRepository _mock;
		private ITeamSelectionValidator _teamSelectionValidator;
		private ITeamMemberCountValidator _teamMemberCountValidator;
		private ITeamBlockPeriodValidator _teamBlockPeriodValidator;
		private ITeamBlockContractTimeValidator _teamBlockContractTimeValidator;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private TeamBlockSwapValidator _target;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamSelectionValidator = _mock.StrictMock<ITeamSelectionValidator>();
			_teamMemberCountValidator = _mock.StrictMock<ITeamMemberCountValidator>();
			_teamBlockPeriodValidator = _mock.StrictMock<ITeamBlockPeriodValidator>();
			_teamBlockContractTimeValidator = _mock.StrictMock<ITeamBlockContractTimeValidator>();
			_teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
			_target = new TeamBlockSwapValidator(_teamSelectionValidator, _teamMemberCountValidator, _teamBlockPeriodValidator, _teamBlockContractTimeValidator);
		}

		[Test]
		public void ShouldReturnTrueWhenPossibleToSwap()
		{
			var selectedPersonList = new List<IPerson>();
			var selectedPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 1);

			using (_mock.Record())
			{
				Expect.Call(_teamSelectionValidator.ValidateSelection(selectedPersonList, selectedPeriod)).Return(true);
				Expect.Call(_teamMemberCountValidator.ValidateMemberCount(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockPeriodValidator.ValidatePeriod(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockContractTimeValidator.ValidateContractTime(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateCanSwap(selectedPersonList, selectedPeriod, _teamBlockInfo1, _teamBlockInfo2);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseWhenValidateSelectionFails()
		{
			var selectedPersonList = new List<IPerson>();
			var selectedPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 1);

			using (_mock.Record())
			{
				Expect.Call(_teamSelectionValidator.ValidateSelection(selectedPersonList, selectedPeriod)).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateCanSwap(selectedPersonList, selectedPeriod, _teamBlockInfo1, _teamBlockInfo2);
				Assert.IsFalse(result);
			}	
		}

		[Test]
		public void ShouldReturnFalseWhenValidateMemberCountFails()
		{
			var selectedPersonList = new List<IPerson>();
			var selectedPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 1);

			using (_mock.Record())
			{
				Expect.Call(_teamSelectionValidator.ValidateSelection(selectedPersonList, selectedPeriod)).Return(true);
				Expect.Call(_teamMemberCountValidator.ValidateMemberCount(_teamBlockInfo1, _teamBlockInfo2)).Return(false);	
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateCanSwap(selectedPersonList, selectedPeriod, _teamBlockInfo1, _teamBlockInfo2);
				Assert.IsFalse(result);
			}	
		}

		[Test]
		public void ShouldReturnFalseWhenValidatePeriodFails()
		{
			var selectedPersonList = new List<IPerson>();
			var selectedPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 1);

			using (_mock.Record())
			{
				Expect.Call(_teamSelectionValidator.ValidateSelection(selectedPersonList, selectedPeriod)).Return(true);
				Expect.Call(_teamMemberCountValidator.ValidateMemberCount(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockPeriodValidator.ValidatePeriod(_teamBlockInfo1, _teamBlockInfo2)).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateCanSwap(selectedPersonList, selectedPeriod, _teamBlockInfo1, _teamBlockInfo2);
				Assert.IsFalse(result);
			}	
		}

		[Test]
		public void ShouldReturnFalsWhenValidateContractTimeFails()
		{
			var selectedPersonList = new List<IPerson>();
			var selectedPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 1);

			using (_mock.Record())
			{
				Expect.Call(_teamSelectionValidator.ValidateSelection(selectedPersonList, selectedPeriod)).Return(true);
				Expect.Call(_teamMemberCountValidator.ValidateMemberCount(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockPeriodValidator.ValidatePeriod(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockContractTimeValidator.ValidateContractTime(_teamBlockInfo1, _teamBlockInfo2)).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateCanSwap(selectedPersonList, selectedPeriod, _teamBlockInfo1, _teamBlockInfo2);
				Assert.IsFalse(result);
			}	
		}
	}
}
