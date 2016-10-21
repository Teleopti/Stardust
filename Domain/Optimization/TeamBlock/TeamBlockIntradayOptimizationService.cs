﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockIntradayOptimizationService
	{
		void Optimize(IList<IScheduleMatrixPro> allPersonMatrixList,
			DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays,
			INewBusinessRuleCollection businessRuleCollection);

		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
	}

	public class TeamBlockIntradayOptimizationService : ITeamBlockIntradayOptimizationService
	{
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamBlockIntradayDecisionMaker _teamBlockIntradayDecisionMaker;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
		private readonly ITeamBlockGenerator _teamBlockGenerator;
		private readonly IDailyTargetValueCalculatorForTeamBlock _dailyTargetValueCalculatorForTeamBlock;
		private readonly ITeamBlockSteadyStateValidator _teamTeamBlockSteadyStateValidator;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private readonly RestrictionOverLimitValidator _teamBlockOptimizationLimits;

		public TeamBlockIntradayOptimizationService(ITeamBlockGenerator teamBlockGenerator,
			ITeamBlockScheduler teamBlockScheduler,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			ITeamBlockIntradayDecisionMaker teamBlockIntradayDecisionMaker,
			RestrictionOverLimitValidator teamBlockOptimizationLimits,
			ITeamBlockClearer teamBlockClearer,
			ITeamBlockMaxSeatChecker teamBlockMaxSeatChecker,
			IDailyTargetValueCalculatorForTeamBlock dailyTargetValueCalculatorForTeamBlock,
			ITeamBlockSteadyStateValidator teamTeamBlockSteadyStateValidator,
			ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator)
		{
			_teamBlockScheduler = teamBlockScheduler;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamBlockIntradayDecisionMaker = teamBlockIntradayDecisionMaker;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockMaxSeatChecker = teamBlockMaxSeatChecker;
			_dailyTargetValueCalculatorForTeamBlock = dailyTargetValueCalculatorForTeamBlock;
			_teamTeamBlockSteadyStateValidator = teamTeamBlockSteadyStateValidator;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
			_teamBlockGenerator = teamBlockGenerator;
			_teamBlockOptimizationLimits = teamBlockOptimizationLimits;
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public void Optimize(IList<IScheduleMatrixPro> allPersonMatrixList,
			DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, 
			INewBusinessRuleCollection businessRuleCollection)
		{
			var cancelMe = false;
			var progressResult = onReportProgress(new ResourceOptimizerProgressEventArgs(0, 0, Resources.OptimizingIntraday + Resources.Colon + Resources.CollectingData,()=>cancelMe=true));
			if (progressResult.ShouldCancel) cancelMe = true;
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			var teamBlocks = _teamBlockGenerator.Generate(allPersonMatrixList, selectedPeriod, selectedPersons, schedulingOptions);
			var remainingInfoList = new List<ITeamBlockInfo>(teamBlocks);

			while (remainingInfoList.Count > 0)
			{
				if (cancelMe)
					break;

				var teamBlocksToRemove = optimizeOneRound(selectedPeriod, optimizationPreferences,
					schedulingOptions, remainingInfoList,
					schedulePartModifyAndRollbackService,
					resourceCalculateDelayer,
					skillDays, businessRuleCollection, ()=> { cancelMe = true; });
				foreach (var teamBlock in teamBlocksToRemove)
				{
					remainingInfoList.Remove(teamBlock);
				}
			}
		}

		private CancelSignal onReportProgress(ResourceOptimizerProgressEventArgs args)
		{
			var handler = ReportProgress;
			if (handler != null)
			{
				handler(this, args);
				if (args.Cancel)
					return new CancelSignal {ShouldCancel = true};
			}
			return new CancelSignal();
		}

		private IEnumerable<ITeamBlockInfo> optimizeOneRound(DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences, ISchedulingOptions schedulingOptions,
			IList<ITeamBlockInfo> allTeamBlockInfos, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, INewBusinessRuleCollection businessRuleCollection, 
			Action cancelAction)
		{
			var teamBlockToRemove = new List<ITeamBlockInfo>();

			var sortedTeamBlockInfos = _teamBlockIntradayDecisionMaker.Decide(allTeamBlockInfos, optimizationPreferences,
				schedulingOptions);

			int totalTeamBlockInfos = sortedTeamBlockInfos.Count;
			int runningTeamBlockCounter = 0;
			foreach (var teamBlockInfo in sortedTeamBlockInfos)
			{
				if (!_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(teamBlockInfo, schedulingOptions))
				{
					teamBlockToRemove.Add(teamBlockInfo);
					continue;
				}

				runningTeamBlockCounter++;
				
				string teamName = teamBlockInfo.TeamInfo.Name.DisplayString(20);
				schedulePartModifyAndRollbackService.ClearModificationCollection();

				var previousTargetValue = _dailyTargetValueCalculatorForTeamBlock
					.TargetValue(teamBlockInfo, optimizationPreferences.Advanced);
				_teamBlockClearer.ClearTeamBlock(schedulingOptions, schedulePartModifyAndRollbackService, teamBlockInfo);
				var firstSelectedDay = selectedPeriod.DayCollection().First();
				var datePoint = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().FirstOrDefault(x => x >= firstSelectedDay);
			
				var success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, datePoint, schedulingOptions,
					schedulePartModifyAndRollbackService,
					resourceCalculateDelayer, skillDays.ToSkillDayEnumerable(), new ShiftNudgeDirective(), businessRuleCollection);
				if (!success)
				{
					var progressResult = onReportProgress(new ResourceOptimizerProgressEventArgs(0, 0, Resources.OptimizingIntraday + Resources.Colon + Resources.RollingBackSchedulesFor + " " +
					                 teamBlockInfo.BlockInfo.BlockPeriod.DateString + " " + teamName,cancelAction));
					teamBlockToRemove.Add(teamBlockInfo);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
					if (progressResult.ShouldCancel)
					{
						cancelAction();
						break;
					}
					continue;
				}

				if (!_teamBlockMaxSeatChecker.CheckMaxSeat(datePoint, schedulingOptions, teamBlockInfo.TeamInfo, skillDays) || !_teamBlockOptimizationLimits.Validate(teamBlockInfo.MatrixesForGroupAndBlock(), optimizationPreferences))
				{
					var progressResult = onReportProgress(new ResourceOptimizerProgressEventArgs(0, 0, Resources.OptimizingIntraday + Resources.Colon + Resources.RollingBackSchedulesFor + " " + teamBlockInfo.BlockInfo.BlockPeriod.DateString + " " + teamName,cancelAction));
					teamBlockToRemove.Add(teamBlockInfo);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
					if (progressResult.ShouldCancel)
					{
						cancelAction();
						break;
					}
					continue;
				}

				if (!_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo, null, optimizationPreferences))
				{
					var progressResult = onReportProgress(new ResourceOptimizerProgressEventArgs(0, 0, Resources.OptimizingIntraday + Resources.Colon + Resources.RollingBackSchedulesFor + " " + teamBlockInfo.BlockInfo.BlockPeriod.DateString + " " + teamName, cancelAction));
					teamBlockToRemove.Add(teamBlockInfo);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
					if (progressResult.ShouldCancel)
					{
						cancelAction();
						break;
					}
					continue;
				}
				

				var newTargetValue = _dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo,
					optimizationPreferences.Advanced);
				var isWorse = newTargetValue >= previousTargetValue;
				string commonProgress = Resources.OptimizingIntraday + Resources.Colon + "(" + totalTeamBlockInfos + ")(" +
				                        runningTeamBlockCounter + ") " + teamBlockInfo.BlockInfo.BlockPeriod.DateString + " " +
				                        teamName + " ";
				if (isWorse)
				{
					teamBlockToRemove.Add(teamBlockInfo);
					var progressResult = onReportProgress(new ResourceOptimizerProgressEventArgs(0, 0, commonProgress + previousTargetValue + "(" + newTargetValue + ")",cancelAction));
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
					if (progressResult.ShouldCancel)
					{
						cancelAction();
						break;
					}
				}
				else
				{
					var progressResult = onReportProgress(new ResourceOptimizerProgressEventArgs(0, 0, commonProgress + newTargetValue + " - " + Resources.Improved,cancelAction));
					if (progressResult.ShouldCancel)
					{
						cancelAction();
						break;
					}
				}
			}
			return teamBlockToRemove;
		}
	}
}
