using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
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
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IScheduleDictionary _scheduleDictionary;
		private IOptimizationPreferences _optimizationPreferences;
	    private IPostSwapValidationForTeamBlock _postSwapValidationForTeamBlock;
		private ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamBlockSwapper = _mocks.StrictMock<ITeamBlockSwapper>();
	        _postSwapValidationForTeamBlock = _mocks.StrictMock<IPostSwapValidationForTeamBlock>();
		    _teamBlockShiftCategoryLimitationValidator = _mocks.StrictMock<ITeamBlockShiftCategoryLimitationValidator>();
		    _target = new SeniorityTeamBlockSwapper(_teamBlockSwapper,_postSwapValidationForTeamBlock, _teamBlockShiftCategoryLimitationValidator);
			_teamBlockInfo1 = _mocks.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mocks.StrictMock<ITeamBlockInfo>();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_optimizationPreferences = new OptimizationPreferences();
		}

		[Test]
		public void ShouldReturnFalseIfSwapFails()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSwapper.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary)).Return(false);
				Expect.Call(_rollbackService.Rollback);
			}

			using (_mocks.Playback())
			{
				var result = _target.SwapAndValidate(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary,
				                                     _optimizationPreferences);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfFirstValidatorFails()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSwapper.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary)).Return(true);
				Expect.Call(_postSwapValidationForTeamBlock.Validate(_teamBlockInfo1, _optimizationPreferences)).Return(false);
				Expect.Call(_rollbackService.Rollback);
			}

			using (_mocks.Playback())
			{
				var result = _target.SwapAndValidate(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary,
													 _optimizationPreferences);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfSecondValidatorFails()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSwapper.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary)).Return(true);
                Expect.Call(_postSwapValidationForTeamBlock.Validate(_teamBlockInfo1, _optimizationPreferences)).Return(true);
                Expect.Call(_postSwapValidationForTeamBlock.Validate(_teamBlockInfo2, _optimizationPreferences)).Return(false);
				Expect.Call(_rollbackService.Rollback);
			}

			using (_mocks.Playback())
			{
				var result = _target.SwapAndValidate(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary,
													 _optimizationPreferences);
				Assert.IsFalse(result);
			}
		}
        
		[Test]
		public void ShouldReturnTrueIfAllSuccess()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSwapper.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary)).Return(true);
				Expect.Call(_postSwapValidationForTeamBlock.Validate(_teamBlockInfo1, _optimizationPreferences)).Return(true);
                Expect.Call(_postSwapValidationForTeamBlock.Validate(_teamBlockInfo2, _optimizationPreferences)).Return(true);
				Expect.Call(_teamBlockShiftCategoryLimitationValidator.Validate(_teamBlockInfo1, _teamBlockInfo2,
				                                                                _optimizationPreferences)).Return(true);
			}

			using (_mocks.Playback())
			{
				var result = _target.SwapAndValidate(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary,
													 _optimizationPreferences);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldRollbackIfFailed()
		{
			Expect.Call(_teamBlockSwapper.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary)).Return(false);
			Expect.Call(_rollbackService.Rollback);
		}

	}
}