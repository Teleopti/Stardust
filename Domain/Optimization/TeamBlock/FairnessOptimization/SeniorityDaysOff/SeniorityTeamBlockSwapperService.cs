using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	public interface ISeniorityTeamBlockSwapperService
	{
		event EventHandler<ResourceOptimizerProgressEventArgs> BlockSwapped;

		void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
		             IList<IPerson> selectedPersons,
		             IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService,
		             IOptimizationPreferences optimizationPreferences, IDictionary<DayOfWeek, int> weekDayPoints,
		             ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator);
	}

	public class SeniorityTeamBlockSwapperService : ISeniorityTeamBlockSwapperService
	{
		private readonly IConstructTeamBlock _constructTeamBlock;
		private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		private readonly IFilterForFullyScheduledBlocks _filterForFullyScheduledBlocks;
		private readonly ISeniorityExtractor _seniorityExtractor;
		private readonly IFilterOnSwapableTeamBlocks _filterOnSwapableTeamBlocks;
		private readonly ITeamBlockLocatorWithHighestPoints _teamBlockLocatorWithHighestPoints;
		private readonly IWeekDayPointCalculatorForTeamBlock _weekDayPointCalculatorForTeamBlock;
		private readonly ISeniorityTeamBlockSwapper _seniorityTeamBlockSwapper;
		private readonly ITeamBlockSeniorityValidator _teamBlockSeniorityValidator;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private bool _cancelMe;

		public SeniorityTeamBlockSwapperService(IConstructTeamBlock constructTeamBlock,
		                                        IFilterForTeamBlockInSelection filterForTeamBlockInSelection,
		                                        IFilterForFullyScheduledBlocks filterForFullyScheduledBlocks,
		                                        ISeniorityExtractor seniorityExtractor,
		                                        IFilterOnSwapableTeamBlocks filterOnSwapableTeamBlocks,
												ITeamBlockLocatorWithHighestPoints teamBlockLocatorWithHighestPoints,
												IWeekDayPointCalculatorForTeamBlock weekDayPointCalculatorForTeamBlock,
												ISeniorityTeamBlockSwapper seniorityTeamBlockSwapper,
												ITeamBlockSeniorityValidator teamBlockSeniorityValidator,
												ISchedulingOptionsCreator schedulingOptionsCreator)
		{
			_constructTeamBlock = constructTeamBlock;
			_filterForTeamBlockInSelection = filterForTeamBlockInSelection;
			_filterForFullyScheduledBlocks = filterForFullyScheduledBlocks;
			_seniorityExtractor = seniorityExtractor;
			_filterOnSwapableTeamBlocks = filterOnSwapableTeamBlocks;
			_teamBlockLocatorWithHighestPoints = teamBlockLocatorWithHighestPoints;
			_weekDayPointCalculatorForTeamBlock = weekDayPointCalculatorForTeamBlock;
			_seniorityTeamBlockSwapper = seniorityTeamBlockSwapper;
			_teamBlockSeniorityValidator = teamBlockSeniorityValidator;
			_schedulingOptionsCreator = schedulingOptionsCreator;
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> BlockSwapped;

		public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
                            IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, 
							IOptimizationPreferences optimizationPreferences, IDictionary<DayOfWeek, int> weekDayPoints,
							ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator)
		{
			_cancelMe = false;

			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			var allTeamBlocks = _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons, true,
			                                                  BlockFinderType.SchedulePeriod,
			                                                  schedulingOptions.GroupOnGroupPageForTeamBlockPer);

			var teamBlocksToWorkWith = _filterForTeamBlockInSelection.Filter(allTeamBlocks, selectedPersons, selectedPeriod);
			teamBlocksToWorkWith = teamBlocksToWorkWith.Where(_teamBlockSeniorityValidator.ValidateSeniority).ToList();

			teamBlocksToWorkWith = _filterForFullyScheduledBlocks.IsFullyScheduled(teamBlocksToWorkWith, scheduleDictionary);
			var teamBlockPoints = _seniorityExtractor.ExtractSeniority(teamBlocksToWorkWith);
			var seniorityInfoDictionary = teamBlockPoints.ToDictionary(teamBlockPoint => teamBlockPoint.TeamBlockInfo, teamBlockPoint => teamBlockPoint.Points);
			var originalBlocksCont = seniorityInfoDictionary.Count;
			while (seniorityInfoDictionary.Count > 0 && !_cancelMe)
			{
				var mostSeniorTeamBlock = findMostSeniorTeamBlock(seniorityInfoDictionary);
				teamBlocksToWorkWith = new List<ITeamBlockInfo>(seniorityInfoDictionary.Keys);
				trySwapForMostSenior(weekDayPoints, teamBlocksToWorkWith, mostSeniorTeamBlock, rollbackService, scheduleDictionary,
				                     optimizationPreferences, originalBlocksCont, seniorityInfoDictionary.Count,
				                     teamBlockRestrictionOverLimitValidator);

				seniorityInfoDictionary.Remove(mostSeniorTeamBlock);
			}
		}

		private void trySwapForMostSenior(IDictionary<DayOfWeek, int> weekDayPoints,
		                                  IList<ITeamBlockInfo> teamBlocksToWorkWith, ITeamBlockInfo mostSeniorTeamBlock,
		                                  ISchedulePartModifyAndRollbackService rollbackService,
		                                  IScheduleDictionary scheduleDictionary,
		                                  IOptimizationPreferences optimizationPreferences, int originalBlocksCount,
		                                  int remainingBlocksCount,
		                                  ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator)
		{
			var swappableTeamBlocks = _filterOnSwapableTeamBlocks.Filter(teamBlocksToWorkWith, mostSeniorTeamBlock);

			var seniorityValueDic = createSeniorityValueDictionary(teamBlocksToWorkWith, weekDayPoints);
			var seniorValue = seniorityValueDic[mostSeniorTeamBlock];

			var swapSuccess = false;
			while (swappableTeamBlocks.Count > 0 && !swapSuccess && !_cancelMe)
			{
				var blockToSwapWith = _teamBlockLocatorWithHighestPoints.FindBestTeamBlockToSwapWith(swappableTeamBlocks,
				                                                                                     seniorityValueDic);
				if (seniorValue < seniorityValueDic[blockToSwapWith])
				{
					swapSuccess = _seniorityTeamBlockSwapper.SwapAndValidate(mostSeniorTeamBlock, blockToSwapWith, rollbackService,
					                                                         scheduleDictionary, optimizationPreferences,
					                                                         teamBlockRestrictionOverLimitValidator);
				}
				else
				{
					swappableTeamBlocks.Clear();
				}

				swappableTeamBlocks.Remove(blockToSwapWith);

				var successMessage = "Swap successful";
				if (!swapSuccess)
					successMessage = "Swap not sucessful";

				double percentDone = 1 - (remainingBlocksCount/(double) originalBlocksCount);

				var message = successMessage + " for " + mostSeniorTeamBlock.TeamInfo.Name + " " +
				              new Percent(percentDone).ToString() + " done ";

				OnBlockSwapped(new ResourceOptimizerProgressEventArgs(1, 1, message));
			}
		}

		protected virtual void OnBlockSwapped(ResourceOptimizerProgressEventArgs eventArgs)
		{
			EventHandler<ResourceOptimizerProgressEventArgs> temp = BlockSwapped;
			if (temp != null)
			{
				temp(this, eventArgs);
			}
			_cancelMe = eventArgs.Cancel;
		}

		private IDictionary<ITeamBlockInfo, double> createSeniorityValueDictionary(IEnumerable<ITeamBlockInfo> teamBlocksToWorkWith, IDictionary<DayOfWeek, int> weekDayPoints)
		{
			var retDic = new Dictionary<ITeamBlockInfo, double>();
			foreach (var teamBlockInfo in teamBlocksToWorkWith)
			{
				var senoirityValue = _weekDayPointCalculatorForTeamBlock.CalculateDaysOffSeniorityValue(teamBlockInfo, weekDayPoints);
				retDic.Add(teamBlockInfo, senoirityValue);
			}

			return retDic;
		}

		private static ITeamBlockInfo findMostSeniorTeamBlock(IDictionary<ITeamBlockInfo, double> seniorityInfoDictionary)
		{
			var maxValue = double.MinValue;
			ITeamBlockInfo mostSeniorBlock = null;
			foreach (var seniorityInfo in seniorityInfoDictionary)
			{
				if (seniorityInfo.Value > maxValue)
				{
					maxValue = seniorityInfo.Value;
					mostSeniorBlock = seniorityInfo.Key;
				}
			}
			return mostSeniorBlock;
		}
	}
}