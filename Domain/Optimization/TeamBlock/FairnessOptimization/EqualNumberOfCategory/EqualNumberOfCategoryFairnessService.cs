using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IEqualNumberOfCategoryFairnessService
	{
		void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
		             IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions,
		             IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService,
		             IOptimizationPreferences optimizationPreferences);

		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
	}

	public class EqualNumberOfCategoryFairnessService : IEqualNumberOfCategoryFairnessService
	{
		private readonly IConstructTeamBlock _constructTeamBlock;
		private readonly IDistributionForPersons _distributionForPersons;
		private readonly IFilterForEqualNumberOfCategoryFairness _filterForEqualNumberOfCategoryFairness;
		private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		private readonly IFilterOnSwapableTeamBlocks _filterOnSwapableTeamBlocks;
		private readonly ITeamBlockSwapper _teamBlockSwapper;
		private readonly IEqualCategoryDistributionBestTeamBlockDecider _equalCategoryDistributionBestTeamBlockDecider;
		private readonly IEqualCategoryDistributionWorstTeamBlockDecider _equalCategoryDistributionWorstTeamBlockDecider;
		private readonly IFilterPersonsForTotalDistribution _filterPersonsForTotalDistribution;
		private readonly IFilterForFullyScheduledBlocks _filterForFullyScheduledBlocks;
		private readonly IEqualCategoryDistributionValue _equalCategoryDistributionValue;
		private readonly IFilterForNoneLockedTeamBlocks _filterForNoneLockedTeamBlocks;
		private readonly ITeamBlockRestrictionOverLimitValidator _teamBlockRestrictionOverLimitValidator;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private bool _cancelMe;

		public EqualNumberOfCategoryFairnessService(IConstructTeamBlock constructTeamBlock,
		                                            IDistributionForPersons distributionForPersons,
		                                            IFilterForEqualNumberOfCategoryFairness
			                                            filterForEqualNumberOfCategoryFairness,
		                                            IFilterForTeamBlockInSelection filterForTeamBlockInSelection,
		                                            IFilterOnSwapableTeamBlocks filterOnSwapableTeamBlocks,
		                                            ITeamBlockSwapper teamBlockSwapper,
		                                            IEqualCategoryDistributionBestTeamBlockDecider
			                                            equalCategoryDistributionBestTeamBlockDecider,
		                                            IEqualCategoryDistributionWorstTeamBlockDecider
			                                            equalCategoryDistributionWorstTeamBlockDecider,
		                                            IFilterPersonsForTotalDistribution filterPersonsForTotalDistribution,
													IFilterForFullyScheduledBlocks filterForFullyScheduledBlocks,
													IEqualCategoryDistributionValue equalCategoryDistributionValue,
													IFilterForNoneLockedTeamBlocks filterForNoneLockedTeamBlocks,
													ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator,
													ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator)
		{
			_constructTeamBlock = constructTeamBlock;
			_distributionForPersons = distributionForPersons;
			_filterForEqualNumberOfCategoryFairness = filterForEqualNumberOfCategoryFairness;
			_filterForTeamBlockInSelection = filterForTeamBlockInSelection;
			_filterOnSwapableTeamBlocks = filterOnSwapableTeamBlocks;
			_teamBlockSwapper = teamBlockSwapper;
			_equalCategoryDistributionBestTeamBlockDecider = equalCategoryDistributionBestTeamBlockDecider;
			_equalCategoryDistributionWorstTeamBlockDecider = equalCategoryDistributionWorstTeamBlockDecider;
			_filterPersonsForTotalDistribution = filterPersonsForTotalDistribution;
			_filterForFullyScheduledBlocks = filterForFullyScheduledBlocks;
			_equalCategoryDistributionValue = equalCategoryDistributionValue;
			_filterForNoneLockedTeamBlocks = filterForNoneLockedTeamBlocks;
			_teamBlockRestrictionOverLimitValidator = teamBlockRestrictionOverLimitValidator;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
		                    IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, 
							IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService,
			IOptimizationPreferences optimizationPreferences)
		{
			_cancelMe = false;
			var personListForTotalDistribution = _filterPersonsForTotalDistribution.Filter(allPersonMatrixList).ToList();
			if (!personListForTotalDistribution.Any())
				return;

			var blocksToWorkWith = _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons,
			                                                     schedulingOptions.BlockFinderTypeForAdvanceScheduling,
			                                                     schedulingOptions.GroupOnGroupPageForTeamBlockPer);

			blocksToWorkWith = _filterForEqualNumberOfCategoryFairness.Filter(blocksToWorkWith);
			
			var totalDistribution = _distributionForPersons.CreateSummary(personListForTotalDistribution, scheduleDictionary);
			blocksToWorkWith = _filterForTeamBlockInSelection.Filter(blocksToWorkWith,
			                                                              selectedPersons, selectedPeriod);

			blocksToWorkWith = _filterForFullyScheduledBlocks.Filter(blocksToWorkWith, scheduleDictionary);

			blocksToWorkWith = _filterForNoneLockedTeamBlocks.Filter(blocksToWorkWith);

			double totalBlockCount = blocksToWorkWith.Count;
			var originalBlocks = blocksToWorkWith;

			bool moveFound = true;
			while (moveFound)
			{
				var workingBlocks = new List<ITeamBlockInfo>(originalBlocks);
				moveFound = runOneLoop(scheduleDictionary, rollbackService, _teamBlockRestrictionOverLimitValidator,
				                       optimizationPreferences, workingBlocks, totalDistribution, totalBlockCount);
			}
		}

		

		private bool runOneLoop(IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService,
		                        ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator,
		                        IOptimizationPreferences optimizationPreferences, IList<ITeamBlockInfo> blocksToWorkWith,
		                        IDistributionSummary totalDistribution, double totalBlockCount)
		{
			bool moveFound = false;
			int successes = 0;
			while (blocksToWorkWith.Count > 0 && !_cancelMe)
			{
				var teamBlockInfoToWorkWith =
					_equalCategoryDistributionWorstTeamBlockDecider.FindBlockToWorkWith(totalDistribution, blocksToWorkWith,
					                                                                    scheduleDictionary);

				if (teamBlockInfoToWorkWith == null) 
					break;

				var teamBlockToWorkWith = teamBlockInfoToWorkWith;

				blocksToWorkWith.Remove(teamBlockInfoToWorkWith);

				var possibleTeamBlocksToSwapWith = _filterOnSwapableTeamBlocks.Filter(blocksToWorkWith, teamBlockToWorkWith);
				if (possibleTeamBlocksToSwapWith.Count == 0)
					continue;

				ITeamBlockInfo selectedTeamBlock =
					_equalCategoryDistributionBestTeamBlockDecider.FindBestSwap(teamBlockToWorkWith, possibleTeamBlocksToSwapWith,
					                                                            totalDistribution, scheduleDictionary);
				if (selectedTeamBlock == null)
					continue;

				var valueBefore = _equalCategoryDistributionValue.CalculateValue(teamBlockToWorkWith, totalDistribution,
				                                                                 scheduleDictionary);
				valueBefore += _equalCategoryDistributionValue.CalculateValue(selectedTeamBlock, totalDistribution,
																				 scheduleDictionary);
				bool success = _teamBlockSwapper.TrySwap(teamBlockToWorkWith, selectedTeamBlock, rollbackService,
				                                         scheduleDictionary);
				if (success)
				{
					var firstBlockOk = teamBlockRestrictionOverLimitValidator.Validate(teamBlockToWorkWith, optimizationPreferences);
					var secondBlockOk = teamBlockRestrictionOverLimitValidator.Validate(selectedTeamBlock, optimizationPreferences);
					if (!(firstBlockOk && secondBlockOk))
					{
						rollbackService.Rollback();
						success = false;
					}
				}

				if (success)
				{
					if (!_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockToWorkWith, selectedTeamBlock, optimizationPreferences))
					{
						rollbackService.Rollback();
						success = false;
					}
				}

				if (success)
				{
					var valueAfter = _equalCategoryDistributionValue.CalculateValue(teamBlockToWorkWith, totalDistribution,
																				 scheduleDictionary);
					valueAfter += _equalCategoryDistributionValue.CalculateValue(selectedTeamBlock, totalDistribution,
																				 scheduleDictionary);
					if (valueAfter<valueBefore)
					{
						successes++;
						moveFound = true;
					}
					else
					{
						rollbackService.Rollback();
					}
				}

				var message = Resources.FairnessOptimizationOn + " " + Resources.EqualOfEachShiftCategory + ": " +
							  new Percent((totalBlockCount - blocksToWorkWith.Count) / totalBlockCount) + " " + Resources.Success + " = " + successes;

				OnReportProgress(message);
			}

			return moveFound;
		}

		public virtual void OnReportProgress(string message)
		{
			EventHandler<ResourceOptimizerProgressEventArgs> handler = ReportProgress;
			if (handler != null)
			{
				var args = new ResourceOptimizerProgressEventArgs(0, 0, message);
				handler(this, args);
				if (args.Cancel)
					_cancelMe = true;
			}
		}
	}
}