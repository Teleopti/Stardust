using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;



namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	[TestFixture]
	public class EqualNumberOfCategoryFairnessServiceTest
	{
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
		private ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;
		private IFilterForNoneLockedTeamBlocks _filterForNoneLockedTeamBlocks;
		private SchedulingOptions _schedulingOptions;
		private List<IScheduleMatrixPro> _allMatrixes;
		private List<IPerson> _selectedPersons;
		private List<ITeamBlockInfo> _teamBlockInfos;
		private DistributionSummary _totalDistributionSummary;
		private ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private IDaysOffPreferences _daysOffPreferences;
		private IDayOffOptimizationPreferenceProvider _dayOffOptimizationPreferenceProvider;

		[SetUp]
		public void Setup()
		{
			_constructTeamBlock = MockRepository.GenerateMock<IConstructTeamBlock>();
			_distributionForPersons = MockRepository.GenerateMock<IDistributionForPersons>();
			_filterForEqualNumberOfCategoryFairness = MockRepository.GenerateMock<IFilterForEqualNumberOfCategoryFairness>();
			_filterForTeamBlockInSelection = MockRepository.GenerateMock<IFilterForTeamBlockInSelection>();
			_filterOnSwapableTeamBlocks = MockRepository.GenerateMock<IFilterOnSwapableTeamBlocks>();
			_teamBlockSwapper = MockRepository.GenerateMock<ITeamBlockSwapper>();
			_equalCategoryDistributionBestTeamBlockDecider = MockRepository.GenerateMock<IEqualCategoryDistributionBestTeamBlockDecider>();
			_equalCategoryDistributionWorstTeamBlockDecider = MockRepository.GenerateMock<IEqualCategoryDistributionWorstTeamBlockDecider>();
			_filterPersonsForTotalDistribution = MockRepository.GenerateMock<IFilterPersonsForTotalDistribution>();
			_filterForFullyScheduledBlocks = MockRepository.GenerateMock<IFilterForFullyScheduledBlocks>();
			_equalCategoryDistributionValue = MockRepository.GenerateMock<IEqualCategoryDistributionValue>();
			_teamBlockOptimizationLimits = MockRepository.GenerateMock<ITeamBlockOptimizationLimits>();
			_optimizationPreferences = new OptimizationPreferences();
			_filterForNoneLockedTeamBlocks = MockRepository.GenerateMock<IFilterForNoneLockedTeamBlocks>();
			_teamBlockShiftCategoryLimitationValidator = MockRepository.GenerateMock<ITeamBlockShiftCategoryLimitationValidator>();
			_target = new EqualNumberOfCategoryFairnessService(_constructTeamBlock, _distributionForPersons,
			                                                   _filterForEqualNumberOfCategoryFairness,
			                                                   _filterForTeamBlockInSelection, _filterOnSwapableTeamBlocks,
			                                                   _teamBlockSwapper, _equalCategoryDistributionBestTeamBlockDecider,
			                                                   _equalCategoryDistributionWorstTeamBlockDecider,
															   _filterPersonsForTotalDistribution,
															   _filterForFullyScheduledBlocks,
															   _equalCategoryDistributionValue,
															   _filterForNoneLockedTeamBlocks,
															   _teamBlockOptimizationLimits,
															   _teamBlockShiftCategoryLimitationValidator);
			_matrix1 = MockRepository.GenerateMock<IScheduleMatrixPro>();
			_sceduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			_rollbackService = MockRepository.GenerateMock<ISchedulePartModifyAndRollbackService>();
			_teamBlockInfo1 = MockRepository.GenerateMock<ITeamBlockInfo>();
			_teamBlockInfo2 = MockRepository.GenerateMock<ITeamBlockInfo>();
			_optimizationPreferences = new OptimizationPreferences();
			_schedulingOptions = new SchedulingOptions();
			_allMatrixes = new List<IScheduleMatrixPro> { _matrix1 };
			var person = PersonFactory.CreatePerson();
			_selectedPersons = new List<IPerson> { person };
			_teamBlockInfos = new List<ITeamBlockInfo> { _teamBlockInfo1, _teamBlockInfo2 };
			_totalDistributionSummary = new DistributionSummary(new Dictionary<IShiftCategory, int>());
			_daysOffPreferences = new DaysOffPreferences();
			_dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(_daysOffPreferences);
		}

		[Test]
		public void ShouldWork()
		{
			commonMocks();

			const int valueAfter = 4;

			_equalCategoryDistributionWorstTeamBlockDecider.Stub(x => x.FindBlockToWorkWith(_totalDistributionSummary, _teamBlockInfos, _sceduleDictionary)).Repeat.Once().Return(_teamBlockInfo1);
			_filterOnSwapableTeamBlocks.Stub(x => x.Filter(_teamBlockInfos, _teamBlockInfo1)).IgnoreArguments().Return(_teamBlockInfos);
			_equalCategoryDistributionBestTeamBlockDecider.Stub(x => x.FindBestSwap(_teamBlockInfo1, _teamBlockInfos, _totalDistributionSummary, _sceduleDictionary)).Return(_teamBlockInfo2);
			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo1, _totalDistributionSummary, _sceduleDictionary)).Repeat.Once().Return(5);
			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo2, _totalDistributionSummary, _sceduleDictionary)).Repeat.Once().Return(5);
			_teamBlockSwapper.Stub(x => x.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _sceduleDictionary)).Return(true);

			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo1, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo2, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);

			_teamBlockShiftCategoryLimitationValidator.Stub(x => x.Validate(_teamBlockInfo1, _teamBlockInfo2, _optimizationPreferences)).Return(true);

			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo1)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo2)).Return(true);

			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo1, _totalDistributionSummary, _sceduleDictionary)).Return(valueAfter);
			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo2, _totalDistributionSummary, _sceduleDictionary)).Return(valueAfter);

			_target.Execute(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions, 
							_sceduleDictionary, _rollbackService, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);

			_rollbackService.AssertWasNotCalled(x => x.Rollback());
		}

		[Test]
		public void ShouldResponsToCancel()
		{
			commonMocks();

			const int valueAfter = 4;

			_equalCategoryDistributionWorstTeamBlockDecider.Stub(x => x.FindBlockToWorkWith(_totalDistributionSummary, _teamBlockInfos, _sceduleDictionary)).Repeat.Once().Return(_teamBlockInfo1);
			_filterOnSwapableTeamBlocks.Stub(x => x.Filter(_teamBlockInfos, _teamBlockInfo1)).IgnoreArguments().Return(_teamBlockInfos);
			_equalCategoryDistributionBestTeamBlockDecider.Stub(x => x.FindBestSwap(_teamBlockInfo1, _teamBlockInfos, _totalDistributionSummary, _sceduleDictionary)).Return(_teamBlockInfo2);
			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo1, _totalDistributionSummary, _sceduleDictionary)).Repeat.Once().Return(5);
			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo2, _totalDistributionSummary, _sceduleDictionary)).Repeat.Once().Return(5);
			_teamBlockSwapper.Stub(x => x.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _sceduleDictionary)).Return(true);

			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo1, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);

			_teamBlockShiftCategoryLimitationValidator.Stub(x => x.Validate(_teamBlockInfo1, _teamBlockInfo2, _optimizationPreferences)).Return(true);

			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo1)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo2)).Return(true);

			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo1, _totalDistributionSummary, _sceduleDictionary)).Return(valueAfter);
			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo2, _totalDistributionSummary, _sceduleDictionary)).Return(valueAfter);

			_target.ReportProgress += _targetReportProgressCancel;
			_target.Execute(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions, _sceduleDictionary, 
								_rollbackService, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
			_target.ReportProgress -= _targetReportProgressCancel;

			_equalCategoryDistributionWorstTeamBlockDecider.AssertWasNotCalled(x => x.FindBlockToWorkWith(_totalDistributionSummary, _teamBlockInfos, _sceduleDictionary), o => o.Repeat.Twice());
		}

		[Test]
		public void ShouldUserCancel()
		{
			commonMocks();

			const int valueAfter = 4;

			_equalCategoryDistributionWorstTeamBlockDecider.Stub(x => x.FindBlockToWorkWith(_totalDistributionSummary, _teamBlockInfos, _sceduleDictionary)).Repeat.Once().Return(_teamBlockInfo1);
			_filterOnSwapableTeamBlocks.Stub(x => x.Filter(_teamBlockInfos, _teamBlockInfo1)).IgnoreArguments().Return(_teamBlockInfos);
			_equalCategoryDistributionBestTeamBlockDecider.Stub(x => x.FindBestSwap(_teamBlockInfo1, _teamBlockInfos, _totalDistributionSummary, _sceduleDictionary)).Return(_teamBlockInfo2);
			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo1, _totalDistributionSummary, _sceduleDictionary)).Repeat.Once().Return(5);
			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo2, _totalDistributionSummary, _sceduleDictionary)).Repeat.Once().Return(5);
			_teamBlockSwapper.Stub(x => x.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _sceduleDictionary)).Return(true);

			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo1, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo2, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);

			_teamBlockShiftCategoryLimitationValidator.Stub(x => x.Validate(_teamBlockInfo1, _teamBlockInfo2, _optimizationPreferences)).Return(true);

			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo1)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo2)).Return(true);

			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo1, _totalDistributionSummary, _sceduleDictionary)).Return(valueAfter);
			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo2, _totalDistributionSummary, _sceduleDictionary)).Return(valueAfter);

			_target.ReportProgress += _targetReportProgressUserCancel;
			_target.Execute(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions, 
							_sceduleDictionary, _rollbackService, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
			_target.ReportProgress -= _targetReportProgressUserCancel;

			_equalCategoryDistributionWorstTeamBlockDecider.AssertWasNotCalled(x => x.FindBlockToWorkWith(_totalDistributionSummary, _teamBlockInfos, _sceduleDictionary), o => o.Repeat.Twice());
		}

		[Test]
		public void ShouldBailOutWhenNothingToWorkWith()
		{
			commonMocks();

			_target.Execute(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions, 
							_sceduleDictionary, _rollbackService, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
		}

		[Test]
		public void ShouldRollBackIfValueIsNotBetter()
		{
			commonMocks();

			const int valueAfterFail = 6;

			_equalCategoryDistributionWorstTeamBlockDecider.Stub(x => x.FindBlockToWorkWith(_totalDistributionSummary, _teamBlockInfos, _sceduleDictionary)).Repeat.Once().Return(_teamBlockInfo1);
			_filterOnSwapableTeamBlocks.Stub(x => x.Filter(_teamBlockInfos, _teamBlockInfo1)).IgnoreArguments().Return(_teamBlockInfos);
			_equalCategoryDistributionBestTeamBlockDecider.Stub(x => x.FindBestSwap(_teamBlockInfo1, _teamBlockInfos, _totalDistributionSummary, _sceduleDictionary)).Return(_teamBlockInfo2);
			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo1, _totalDistributionSummary, _sceduleDictionary)).Repeat.Once().Return(5);
			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo2, _totalDistributionSummary, _sceduleDictionary)).Repeat.Once().Return(5);
			_teamBlockSwapper.Stub(x => x.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _sceduleDictionary)).Return(true);

			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo1, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo2, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);

			_teamBlockShiftCategoryLimitationValidator.Stub(x => x.Validate(_teamBlockInfo1, _teamBlockInfo2, _optimizationPreferences)).Return(true);

			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo1)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo2)).Return(true);

			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo1, _totalDistributionSummary, _sceduleDictionary)).Return(valueAfterFail);
			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo2, _totalDistributionSummary, _sceduleDictionary)).Return(valueAfterFail);

			_target.Execute(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions, 
							_sceduleDictionary, _rollbackService, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
			_rollbackService.AssertWasCalled(x => x.Rollback());
		}

		[Test]
		public void ShouldRollBackIfBreakingRestrictionLimit()
		{
				commonMocks();

				_equalCategoryDistributionWorstTeamBlockDecider.Stub(x => x.FindBlockToWorkWith(_totalDistributionSummary, _teamBlockInfos, _sceduleDictionary)).Repeat.Once().Return(_teamBlockInfo1);
				_filterOnSwapableTeamBlocks.Stub(x => x.Filter(_teamBlockInfos, _teamBlockInfo1)).IgnoreArguments().Return(_teamBlockInfos);
				_equalCategoryDistributionBestTeamBlockDecider.Stub(x => x.FindBestSwap(_teamBlockInfo1, _teamBlockInfos, _totalDistributionSummary, _sceduleDictionary)).Return(_teamBlockInfo2);
				_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo1, _totalDistributionSummary, _sceduleDictionary)).Repeat.Once().Return(5);
				_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo2, _totalDistributionSummary, _sceduleDictionary)).Repeat.Once().Return(5);
				_teamBlockSwapper.Stub(x => x.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _sceduleDictionary)).Return(true);

				_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo1, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
				_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo2, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(false);

			_target.Execute(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions, 
								_sceduleDictionary, _rollbackService, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);

			_rollbackService.AssertWasCalled(x => x.Rollback());
		}

		[Test]
		public void ShouldRollBackIfBreakingCategoryLimitation()
		{
				commonMocks();

				_equalCategoryDistributionWorstTeamBlockDecider.Stub(x => x.FindBlockToWorkWith(_totalDistributionSummary, _teamBlockInfos, _sceduleDictionary)).Repeat.Once().Return(_teamBlockInfo1);
				_filterOnSwapableTeamBlocks.Stub(x => x.Filter(_teamBlockInfos, _teamBlockInfo1)).IgnoreArguments().Return(_teamBlockInfos);
				_equalCategoryDistributionBestTeamBlockDecider.Stub(x => x.FindBestSwap(_teamBlockInfo1, _teamBlockInfos, _totalDistributionSummary, _sceduleDictionary)).Return(_teamBlockInfo2);
				_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo1, _totalDistributionSummary, _sceduleDictionary)).Repeat.Once().Return(5);
				_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo2, _totalDistributionSummary, _sceduleDictionary)).Repeat.Once().Return(5);
				_teamBlockSwapper.Stub(x => x.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _sceduleDictionary)).Return(true);


				_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo1, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
				_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo2, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);

				_teamBlockShiftCategoryLimitationValidator.Stub(x => x.Validate(_teamBlockInfo1, _teamBlockInfo2, _optimizationPreferences)).Return(false);

				_target.Execute(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions, 
								_sceduleDictionary, _rollbackService, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
				_rollbackService.AssertWasCalled(x => x.Rollback());
		}

		[Test]
		public void ShouldRollBackIfBreakingMinWorkTimePerWeek()
		{
			commonMocks();

			_equalCategoryDistributionWorstTeamBlockDecider.Stub(x => x.FindBlockToWorkWith(_totalDistributionSummary, _teamBlockInfos, _sceduleDictionary)).Repeat.Once().Return(_teamBlockInfo1);
			_filterOnSwapableTeamBlocks.Stub(x => x.Filter(_teamBlockInfos, _teamBlockInfo1)).IgnoreArguments().Return(_teamBlockInfos);
			_equalCategoryDistributionBestTeamBlockDecider.Stub(x => x.FindBestSwap(_teamBlockInfo1, _teamBlockInfos, _totalDistributionSummary, _sceduleDictionary)).Return(_teamBlockInfo2);
			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo1, _totalDistributionSummary, _sceduleDictionary)).Repeat.Once().Return(5);
			_equalCategoryDistributionValue.Stub(x => x.CalculateValue(_teamBlockInfo2, _totalDistributionSummary, _sceduleDictionary)).Repeat.Once().Return(5);
			_teamBlockSwapper.Stub(x => x.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _sceduleDictionary)).Return(true);


			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo1, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo2, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);

			_teamBlockShiftCategoryLimitationValidator.Stub(x => x.Validate(_teamBlockInfo1, _teamBlockInfo2, _optimizationPreferences)).Return(true);

			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo1)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo2)).Return(false);

			_target.Execute(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions, 
						_sceduleDictionary, _rollbackService, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);

			_rollbackService.AssertWasCalled(x => x.Rollback());
		}

		void _targetReportProgressCancel(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.Cancel = true;
		}

		void _targetReportProgressUserCancel(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.CancelAction();
		}

		private void commonMocks()
		{
			_filterPersonsForTotalDistribution.Stub(x => x.Filter(_allMatrixes)).Return(_selectedPersons);
			_constructTeamBlock.Stub(x => x.Construct(_allMatrixes, new DateOnlyPeriod(), _selectedPersons, _schedulingOptions.BlockFinder(), _schedulingOptions.GroupOnGroupPageForTeamBlockPer)).IgnoreArguments().Return(_teamBlockInfos);
			_filterForEqualNumberOfCategoryFairness.Stub(x => x.Filter(_teamBlockInfos)).Return(_teamBlockInfos);

			_distributionForPersons.Stub(x => x.CreateSummary(_selectedPersons, _sceduleDictionary)).IgnoreArguments().Return(_totalDistributionSummary);
			_filterForTeamBlockInSelection.Stub(x => x.Filter(_teamBlockInfos, _selectedPersons, new DateOnlyPeriod())).Return(_teamBlockInfos);
			_filterForFullyScheduledBlocks.Stub(x => x.Filter(_teamBlockInfos, _sceduleDictionary)).Return(_teamBlockInfos);
			_filterForNoneLockedTeamBlocks.Stub(x => x.Filter(_teamBlockInfos)).Return(_teamBlockInfos);
		}
	}
}