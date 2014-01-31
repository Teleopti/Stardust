using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	[TestFixture]
	public class SeniorityTeamBlockSwapperTest
	{
		private MockRepository _mocks;
		private ISeniorityTeamBlockSwapper _target;
		private ITeamBlockSwapper _teamBlockSwapper;
		private ISeniorityTeamBlockSwapValidator _seniorityTeamBlockSwapValidator;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IScheduleDictionary _scheduleDictionary;
		private IOptimizationPreferences _optimizationPreferences;
		private ITeamBlockRestrictionOverLimitValidator _teamBlockRestrictionOverLimitValidator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamBlockSwapper = _mocks.StrictMock<ITeamBlockSwapper>();
			_seniorityTeamBlockSwapValidator = _mocks.StrictMock<ISeniorityTeamBlockSwapValidator>();
			_target = new SeniorityTeamBlockSwapper(_teamBlockSwapper, _seniorityTeamBlockSwapValidator);
			_teamBlockInfo1 = _mocks.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mocks.StrictMock<ITeamBlockInfo>();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_optimizationPreferences = new OptimizationPreferences();
			_teamBlockRestrictionOverLimitValidator = _mocks.StrictMock<ITeamBlockRestrictionOverLimitValidator>();
		}

		[Test]
		public void ShouldReturnFalseIfSwapFails()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSwapper.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary)).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.SwapAndValidate(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary,
				                                     _optimizationPreferences, _teamBlockRestrictionOverLimitValidator);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfFirstValidatorFails()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSwapper.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary)).Return(true);
				Expect.Call(_seniorityTeamBlockSwapValidator.Validate(_teamBlockInfo1, _optimizationPreferences)).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.SwapAndValidate(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary,
													 _optimizationPreferences, _teamBlockRestrictionOverLimitValidator);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfSecondValidatorFails()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSwapper.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary)).Return(true);
				Expect.Call(_seniorityTeamBlockSwapValidator.Validate(_teamBlockInfo1, _optimizationPreferences)).Return(true);
				Expect.Call(_seniorityTeamBlockSwapValidator.Validate(_teamBlockInfo2, _optimizationPreferences)).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.SwapAndValidate(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary,
													 _optimizationPreferences, _teamBlockRestrictionOverLimitValidator);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseFirstRestictionValidatorFails()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSwapper.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary)).Return(true);
				Expect.Call(_seniorityTeamBlockSwapValidator.Validate(_teamBlockInfo1, _optimizationPreferences)).Return(true);
				Expect.Call(_seniorityTeamBlockSwapValidator.Validate(_teamBlockInfo2, _optimizationPreferences)).Return(true);
				Expect.Call(_teamBlockRestrictionOverLimitValidator.Validate(_teamBlockInfo1, _optimizationPreferences))
				      .Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.SwapAndValidate(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary,
													 _optimizationPreferences, _teamBlockRestrictionOverLimitValidator);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseSecondRestictionValidatorFails()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSwapper.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary)).Return(true);
				Expect.Call(_seniorityTeamBlockSwapValidator.Validate(_teamBlockInfo1, _optimizationPreferences)).Return(true);
				Expect.Call(_seniorityTeamBlockSwapValidator.Validate(_teamBlockInfo2, _optimizationPreferences)).Return(true);
				Expect.Call(_teamBlockRestrictionOverLimitValidator.Validate(_teamBlockInfo1, _optimizationPreferences))
					  .Return(true);
				Expect.Call(_teamBlockRestrictionOverLimitValidator.Validate(_teamBlockInfo2, _optimizationPreferences))
					  .Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.SwapAndValidate(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary,
													 _optimizationPreferences, _teamBlockRestrictionOverLimitValidator);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnTrueIfAllSuccess()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSwapper.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary)).Return(true);
				Expect.Call(_seniorityTeamBlockSwapValidator.Validate(_teamBlockInfo1, _optimizationPreferences)).Return(true);
				Expect.Call(_seniorityTeamBlockSwapValidator.Validate(_teamBlockInfo2, _optimizationPreferences)).Return(true);
				Expect.Call(_teamBlockRestrictionOverLimitValidator.Validate(_teamBlockInfo1, _optimizationPreferences))
					  .Return(true);
				Expect.Call(_teamBlockRestrictionOverLimitValidator.Validate(_teamBlockInfo2, _optimizationPreferences))
					  .Return(true);
			}

			using (_mocks.Playback())
			{
				var result = _target.SwapAndValidate(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary,
													 _optimizationPreferences, _teamBlockRestrictionOverLimitValidator);
				Assert.IsTrue(result);
			}
		}

	}
}