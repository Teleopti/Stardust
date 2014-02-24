using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	[TestFixture]
	public class EqualNumberOfCategoryFairnessServiceTest
	{
		private MockRepository _mocks;
		private IEqualNumberOfCategoryFairnessService _target;
		private IConstructTeamBlock _constructTeamBlock;
		private IDistributionForPersons _distributionForPersons;
		private IFilterForEqualNumberOfCategoryFairness _filterForEqualNumberOfCategoryFairness;
		private IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		private IFilterOnSwapableTeamBlocks _filterOnSwapableTeamBlocks;
		private ITeamBlockSwapper _teamBlockSwapper;
		private IEqualCategoryDistributionBestTeamBlockDecider _equalCategoryDistributionBestTeamBlockDecider;
		private IEqualCategoryDistributionWorstTeamBlockDecider _equalCategoryDistributionWorstTeamBlockDecider;
		private IScheduleMatrixPro _matrix1;
		private IScheduleDictionary _sceduleDictionary;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private IFilterPersonsForTotalDistribution _filterPersonsForTotalDistribution;
		private IFilterForFullyScheduledBlocks _filterForFullyScheduledBlocks;
		private IEqualCategoryDistributionValue _equalCategoryDistributionValue;
		private IOptimizationPreferences _optimizationPreferences;
		private ITeamBlockRestrictionOverLimitValidator _teamBlockRestrictionOverLimitValidator;
		private IFilterForNoneLockedTeamBlocks _filterForNoneLockedTeamBlocks;
		private ISchedulingOptions _schedulingOptions;
		private List<IScheduleMatrixPro> _allMatrixes;
		private List<IPerson> _selectedPersons;
		private List<ITeamBlockInfo> _teamBlockInfos;
		private DistributionSummary _totalDistributionSummary;
		private ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_constructTeamBlock = _mocks.StrictMock<IConstructTeamBlock>();
			_distributionForPersons = _mocks.StrictMock<IDistributionForPersons>();
			_filterForEqualNumberOfCategoryFairness = _mocks.StrictMock<IFilterForEqualNumberOfCategoryFairness>();
			_filterForTeamBlockInSelection = _mocks.StrictMock<IFilterForTeamBlockInSelection>();
			_filterOnSwapableTeamBlocks = _mocks.StrictMock<IFilterOnSwapableTeamBlocks>();
			_teamBlockSwapper = _mocks.StrictMock<ITeamBlockSwapper>();
			_equalCategoryDistributionBestTeamBlockDecider = _mocks.StrictMock<IEqualCategoryDistributionBestTeamBlockDecider>();
			_equalCategoryDistributionWorstTeamBlockDecider =
				_mocks.StrictMock<IEqualCategoryDistributionWorstTeamBlockDecider>();
			_filterPersonsForTotalDistribution = _mocks.StrictMock<IFilterPersonsForTotalDistribution>();
			_filterForFullyScheduledBlocks = _mocks.StrictMock<IFilterForFullyScheduledBlocks>();
			_equalCategoryDistributionValue = _mocks.StrictMock<IEqualCategoryDistributionValue>();
			_teamBlockRestrictionOverLimitValidator = _mocks.StrictMock<ITeamBlockRestrictionOverLimitValidator>();
			_optimizationPreferences = new OptimizationPreferences();
			_filterForNoneLockedTeamBlocks = _mocks.StrictMock<IFilterForNoneLockedTeamBlocks>();
			_teamBlockShiftCategoryLimitationValidator = _mocks.StrictMock<ITeamBlockShiftCategoryLimitationValidator>();
			_target = new EqualNumberOfCategoryFairnessService(_constructTeamBlock, _distributionForPersons,
			                                                   _filterForEqualNumberOfCategoryFairness,
			                                                   _filterForTeamBlockInSelection, _filterOnSwapableTeamBlocks,
			                                                   _teamBlockSwapper, _equalCategoryDistributionBestTeamBlockDecider,
			                                                   _equalCategoryDistributionWorstTeamBlockDecider,
															   _filterPersonsForTotalDistribution,
															   _filterForFullyScheduledBlocks,
															   _equalCategoryDistributionValue,
															   _filterForNoneLockedTeamBlocks,
															   _teamBlockRestrictionOverLimitValidator,
															   _teamBlockShiftCategoryLimitationValidator);
			_matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_sceduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_teamBlockInfo1 = _mocks.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mocks.StrictMock<ITeamBlockInfo>();
			_optimizationPreferences = new OptimizationPreferences();
			_schedulingOptions = new SchedulingOptions();
			_allMatrixes = new List<IScheduleMatrixPro> { _matrix1 };
			var person = PersonFactory.CreatePerson();
			_selectedPersons = new List<IPerson> { person };
			_teamBlockInfos = new List<ITeamBlockInfo> { _teamBlockInfo1, _teamBlockInfo2 };
			//_teamBlockInfosToWorkWith = new List<ITeamBlockInfo> { _teamBlockInfo1, _teamBlockInfo2 };
			_totalDistributionSummary = new DistributionSummary(new Dictionary<IShiftCategory, int>());
		}

		[Test]
		public void ShouldWork()
		{
			using (_mocks.Record())
			{
				commonMocks();
				successfulMove();

				Expect.Call(_equalCategoryDistributionWorstTeamBlockDecider.FindBlockToWorkWith(_totalDistributionSummary,
																							_teamBlockInfos, _sceduleDictionary))
				  .IgnoreArguments().Return(null);
			}

			using (_mocks.Playback())
			{
				_target.Execute(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions, _sceduleDictionary,
				                _rollbackService, _optimizationPreferences);
			}
		}

		[Test]
		public void ShouldResponsToCancel()
		{
			using (_mocks.Record())
			{
				commonMocks();
				firstInnerLoop(false, false, false);
			}

			using (_mocks.Playback())
			{
				_target.ReportProgress += _targetReportProgress;
				_target.Execute(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions, _sceduleDictionary,
								_rollbackService, _optimizationPreferences);
				_target.ReportProgress -= _targetReportProgress;
			}
		}

		[Test]
		public void ShouldBailOutWhenNothingToWorkWith()
		{
			using (_mocks.Record())
			{
				commonMocks();

				Expect.Call(_equalCategoryDistributionWorstTeamBlockDecider.FindBlockToWorkWith(_totalDistributionSummary,
				                                                                                _teamBlockInfos, _sceduleDictionary))
				      .Return(null);
			}

			using (_mocks.Playback())
			{
				_target.Execute(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions, _sceduleDictionary,
								_rollbackService, _optimizationPreferences);
			}
		}

		[Test]
		public void ShouldRollBackIfValueIsNotBetter()
		{
			using (_mocks.Record())
			{
				commonMocks();
				failOnValue();
			}

			using (_mocks.Playback())
			{
				_target.Execute(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions, _sceduleDictionary,
								_rollbackService, _optimizationPreferences);
			}
		}

		[Test]
		public void ShouldRollBackIfBreakingRestrictionLimit()
		{
			using (_mocks.Record())
			{
				commonMocks();
				failOnRestriction();
			}

			using (_mocks.Playback())
			{
				_target.Execute(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions, _sceduleDictionary,
								_rollbackService, _optimizationPreferences);
			}
		}

		[Test]
		public void ShouldRollBackIfBreakingCategoryLimitation()
		{
			using (_mocks.Record())
			{
				commonMocks();
				failOnCategoryLimitation();
			}

			using (_mocks.Playback())
			{
				_target.Execute(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions, _sceduleDictionary,
								_rollbackService, _optimizationPreferences);
			}
		}

		void _targetReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.Cancel = true;
		}

		private void failOnRestriction()
		{
			//first loop
			firstInnerLoop(false, true, false);

			//second loop
			Expect.Call(_equalCategoryDistributionWorstTeamBlockDecider.FindBlockToWorkWith(_totalDistributionSummary,
																							_teamBlockInfos, _sceduleDictionary))
				  .IgnoreArguments().Return(null);
		}

		private void failOnCategoryLimitation()
		{
			//first loop
			firstInnerLoop(false, false, true);

			//second loop
			Expect.Call(_equalCategoryDistributionWorstTeamBlockDecider.FindBlockToWorkWith(_totalDistributionSummary,
																							_teamBlockInfos, _sceduleDictionary))
				  .IgnoreArguments().Return(null);
		}

		private void failOnValue()
		{
			//first loop
			firstInnerLoop(true, false, false);

			//second loop
			Expect.Call(_equalCategoryDistributionWorstTeamBlockDecider.FindBlockToWorkWith(_totalDistributionSummary,
																							_teamBlockInfos, _sceduleDictionary))
				  .IgnoreArguments().Return(null);
		}

		private void successfulMove()
		{
			//first loop
			firstInnerLoop(false, false, false);

			//second loop
			Expect.Call(_equalCategoryDistributionWorstTeamBlockDecider.FindBlockToWorkWith(_totalDistributionSummary,
																							_teamBlockInfos, _sceduleDictionary))
				  .IgnoreArguments().Return(null);
		}

		private void firstInnerLoop(bool failOnValue, bool failOnRestriction, bool failOnCategoryLimitation)
		{
			var valueAfter = 4;
			if (failOnValue)
				valueAfter = 6;

			Expect.Call(_equalCategoryDistributionWorstTeamBlockDecider.FindBlockToWorkWith(_totalDistributionSummary,
																							_teamBlockInfos, _sceduleDictionary))
				  .Return(_teamBlockInfo1);
			Expect.Call(_filterOnSwapableTeamBlocks.Filter(_teamBlockInfos, _teamBlockInfo1))
				  .IgnoreArguments()
				  .Return(_teamBlockInfos);
			Expect.Call(_equalCategoryDistributionBestTeamBlockDecider.FindBestSwap(_teamBlockInfo1, _teamBlockInfos,
																					_totalDistributionSummary, _sceduleDictionary))
				  .Return(_teamBlockInfo2);
			Expect.Call(_equalCategoryDistributionValue.CalculateValue(_teamBlockInfo1, _totalDistributionSummary,
																	   _sceduleDictionary)).Return(5);
			Expect.Call(_equalCategoryDistributionValue.CalculateValue(_teamBlockInfo2, _totalDistributionSummary,
																	   _sceduleDictionary)).Return(5);
			Expect.Call(_teamBlockSwapper.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _sceduleDictionary))
				  .Return(true);
			Expect.Call(_teamBlockRestrictionOverLimitValidator.Validate(_teamBlockInfo1, _optimizationPreferences))
				  .Return(true);
			Expect.Call(_teamBlockRestrictionOverLimitValidator.Validate(_teamBlockInfo2, _optimizationPreferences))
				  .Return(!failOnRestriction);

			if (!failOnRestriction)
			{
				Expect.Call(_teamBlockShiftCategoryLimitationValidator.Validate(_teamBlockInfo1, _teamBlockInfo2,
				                                                                _optimizationPreferences))
				      .Return(!failOnCategoryLimitation);

				if (!failOnCategoryLimitation)
				{
					Expect.Call(_equalCategoryDistributionValue.CalculateValue(_teamBlockInfo1, _totalDistributionSummary,
					                                                           _sceduleDictionary)).Return(valueAfter);
					Expect.Call(_equalCategoryDistributionValue.CalculateValue(_teamBlockInfo2, _totalDistributionSummary,
					                                                           _sceduleDictionary)).Return(valueAfter);
				}
			}

			if (failOnValue || failOnRestriction || failOnCategoryLimitation)
			{
				Expect.Call(() => _rollbackService.Rollback());
			}
		}

		private void commonMocks()
		{

			Expect.Call(_filterPersonsForTotalDistribution.Filter(_allMatrixes)).Return(_selectedPersons);
			Expect.Call(_constructTeamBlock.Construct(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions.UseTeamBlockPerOption,
																 _schedulingOptions.BlockFinderTypeForAdvanceScheduling,
																 _schedulingOptions.GroupOnGroupPageForTeamBlockPer))
				  .Return(_teamBlockInfos);
			Expect.Call(_filterForEqualNumberOfCategoryFairness.Filter(_teamBlockInfos)).Return(_teamBlockInfos);

			Expect.Call(_distributionForPersons.CreateSummary(_selectedPersons, _sceduleDictionary)).IgnoreArguments()
				  .Return(_totalDistributionSummary);
			Expect.Call(_filterForTeamBlockInSelection.Filter(_teamBlockInfos, _selectedPersons, new DateOnlyPeriod()))
				  .Return(_teamBlockInfos);
			Expect.Call(_filterForFullyScheduledBlocks.Filter(_teamBlockInfos, _sceduleDictionary)).Return(_teamBlockInfos);
			Expect.Call(_filterForNoneLockedTeamBlocks.Filter(_teamBlockInfos)).Return(_teamBlockInfos);
		}

	}
}