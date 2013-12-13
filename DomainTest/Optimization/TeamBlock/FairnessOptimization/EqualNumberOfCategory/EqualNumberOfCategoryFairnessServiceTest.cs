using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
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
			_target = new EqualNumberOfCategoryFairnessService(_constructTeamBlock, _distributionForPersons,
			                                                   _filterForEqualNumberOfCategoryFairness,
			                                                   _filterForTeamBlockInSelection, _filterOnSwapableTeamBlocks,
			                                                   _teamBlockSwapper, _equalCategoryDistributionBestTeamBlockDecider,
			                                                   _equalCategoryDistributionWorstTeamBlockDecider,
															   _filterPersonsForTotalDistribution,
															   _filterForFullyScheduledBlocks);
			_matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_sceduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_teamBlockInfo1 = _mocks.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mocks.StrictMock<ITeamBlockInfo>();
		}

		[Test]
		public void ShouldWork()
		{
			var allMatrixes = new List<IScheduleMatrixPro> {_matrix1};
			var person = PersonFactory.CreatePerson();
			var selectedPersons = new List<IPerson>{person};
			var schedulingOptions = new SchedulingOptions();
			var teamBlockInfos = new List<ITeamBlockInfo> {_teamBlockInfo1, _teamBlockInfo2};
			var totalDistributionSummary = new DistributionSummary(new Dictionary<IShiftCategory, int>());
			
			using (_mocks.Record())
			{
				Expect.Call(_filterPersonsForTotalDistribution.Filter(allMatrixes)).Return(selectedPersons);
				Expect.Call(_constructTeamBlock.Construct(allMatrixes, new DateOnlyPeriod(), selectedPersons, schedulingOptions))
				      .Return(teamBlockInfos);
				Expect.Call(_filterForEqualNumberOfCategoryFairness.Filter(teamBlockInfos)).Return(teamBlockInfos);

				Expect.Call(_distributionForPersons.CreateSummary(selectedPersons, _sceduleDictionary)).IgnoreArguments()
				      .Return(totalDistributionSummary);
				Expect.Call(_filterForTeamBlockInSelection.Filter(teamBlockInfos, selectedPersons, new DateOnlyPeriod())).Return(teamBlockInfos);
				Expect.Call(_filterForFullyScheduledBlocks.IsFullyScheduled(teamBlockInfos, _sceduleDictionary)).Return(teamBlockInfos);

				//first loop
				Expect.Call(_equalCategoryDistributionWorstTeamBlockDecider.FindBlockToWorkWith(totalDistributionSummary,
				                                                                                teamBlockInfos, _sceduleDictionary))
				      .Return(_teamBlockInfo1);
				Expect.Call(_filterOnSwapableTeamBlocks.Filter(teamBlockInfos, _teamBlockInfo1)).Return(teamBlockInfos);
				Expect.Call(_equalCategoryDistributionBestTeamBlockDecider.FindBestSwap(_teamBlockInfo1, teamBlockInfos,
				                                                                        totalDistributionSummary, _sceduleDictionary))
				      .Return(_teamBlockInfo2);
				Expect.Call(_teamBlockSwapper.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _sceduleDictionary)).Return(true);

				//second loop
				Expect.Call(_equalCategoryDistributionWorstTeamBlockDecider.FindBlockToWorkWith(totalDistributionSummary,
																								teamBlockInfos, _sceduleDictionary))
					  .Return(_teamBlockInfo2);
				Expect.Call(_filterOnSwapableTeamBlocks.Filter(teamBlockInfos, _teamBlockInfo2)).Return(teamBlockInfos);
			}

			using (_mocks.Playback())
			{
				_target.Execute(allMatrixes, new DateOnlyPeriod(), selectedPersons, schedulingOptions, _sceduleDictionary, _rollbackService);
			}
 
		}

		[Test]
		public void ShouldResponsToCancel()
		{
			var allMatrixes = new List<IScheduleMatrixPro> { _matrix1 };
			var person = PersonFactory.CreatePerson();
			var selectedPersons = new List<IPerson> { person };
			var schedulingOptions = new SchedulingOptions();
			var teamBlockInfos = new List<ITeamBlockInfo> { _teamBlockInfo1, _teamBlockInfo2 };
			var totalDistributionSummary = new DistributionSummary(new Dictionary<IShiftCategory, int>());

			using (_mocks.Record())
			{
				Expect.Call(_filterPersonsForTotalDistribution.Filter(allMatrixes)).Return(selectedPersons);
				Expect.Call(_constructTeamBlock.Construct(allMatrixes, new DateOnlyPeriod(), selectedPersons, schedulingOptions))
					  .Return(teamBlockInfos);
				Expect.Call(_filterForEqualNumberOfCategoryFairness.Filter(teamBlockInfos)).Return(teamBlockInfos);

				Expect.Call(_distributionForPersons.CreateSummary(selectedPersons, _sceduleDictionary)).IgnoreArguments()
					  .Return(totalDistributionSummary);
				Expect.Call(_filterForTeamBlockInSelection.Filter(teamBlockInfos, selectedPersons, new DateOnlyPeriod())).Return(teamBlockInfos);
				Expect.Call(_filterForFullyScheduledBlocks.IsFullyScheduled(teamBlockInfos, _sceduleDictionary)).Return(teamBlockInfos);

				//first loop
				Expect.Call(_equalCategoryDistributionWorstTeamBlockDecider.FindBlockToWorkWith(totalDistributionSummary,
																								teamBlockInfos, _sceduleDictionary))
					  .Return(_teamBlockInfo1);
				Expect.Call(_filterOnSwapableTeamBlocks.Filter(teamBlockInfos, _teamBlockInfo1)).Return(teamBlockInfos);
				Expect.Call(_equalCategoryDistributionBestTeamBlockDecider.FindBestSwap(_teamBlockInfo1, teamBlockInfos,
																						totalDistributionSummary, _sceduleDictionary))
					  .Return(_teamBlockInfo2);
				Expect.Call(_teamBlockSwapper.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _sceduleDictionary)).Return(true);
			}

			using (_mocks.Playback())
			{
				_target.ReportProgress += _targetReportProgress;
				_target.Execute(allMatrixes, new DateOnlyPeriod(), selectedPersons, schedulingOptions, _sceduleDictionary, _rollbackService);
				_target.ReportProgress -= _targetReportProgress;
			}
		}

		void _targetReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.Cancel = true;
		}

	}
}