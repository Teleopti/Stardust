using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
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
			ISchedulingResultStateHolder schedulingResultStateHolder);

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
		private readonly ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;
		private readonly IDailyTargetValueCalculatorForTeamBlock _dailyTargetValueCalculatorForTeamBlock;
		private readonly ITeamBlockSteadyStateValidator _teamTeamBlockSteadyStateValidator;
		private readonly bool _isMaxSeatToggleEnabled;
		private bool _cancelMe;
		private ResourceOptimizerProgressEventArgs _progressEvent;

		public TeamBlockIntradayOptimizationService(ITeamBlockGenerator teamBlockGenerator,
			ITeamBlockScheduler teamBlockScheduler,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			ITeamBlockIntradayDecisionMaker teamBlockIntradayDecisionMaker,
			ITeamBlockOptimizationLimits teamBlockOptimizationLimits,
			ITeamBlockClearer teamBlockClearer,
			ITeamBlockMaxSeatChecker teamBlockMaxSeatChecker,
			IDailyTargetValueCalculatorForTeamBlock dailyTargetValueCalculatorForTeamBlock,
			ITeamBlockSteadyStateValidator teamTeamBlockSteadyStateValidator,
			//remove this - instead use two different impl of (a smaller interface of) ITeamBlockIntradayOptimizationService
			bool isMaxSeatToggleEnabled)
		{
			_teamBlockScheduler = teamBlockScheduler;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamBlockIntradayDecisionMaker = teamBlockIntradayDecisionMaker;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockMaxSeatChecker = teamBlockMaxSeatChecker;
			_dailyTargetValueCalculatorForTeamBlock = dailyTargetValueCalculatorForTeamBlock;
			_teamTeamBlockSteadyStateValidator = teamTeamBlockSteadyStateValidator;
			_isMaxSeatToggleEnabled = isMaxSeatToggleEnabled;
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
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_progressEvent = null;
			OnReportProgress(Resources.OptimizingIntraday + Resources.Colon + Resources.CollectingData);
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			var teamBlocks = _teamBlockGenerator.Generate(allPersonMatrixList, selectedPeriod, selectedPersons, schedulingOptions);
			var remainingInfoList = new List<ITeamBlockInfo>(teamBlocks);

			while (remainingInfoList.Count > 0)
			{
				if (_cancelMe)
					break;

				if (_progressEvent != null && _progressEvent.UserCancel)
					break;

				var teamBlocksToRemove = optimizeOneRound(selectedPeriod, optimizationPreferences,
					schedulingOptions, remainingInfoList,
					schedulePartModifyAndRollbackService,
					resourceCalculateDelayer,
					schedulingResultStateHolder, _isMaxSeatToggleEnabled);
				foreach (var teamBlock in teamBlocksToRemove)
				{
					remainingInfoList.Remove(teamBlock);
				}
			}
		}

		public void OnReportProgress(string message)
		{
			var handler = ReportProgress;
			if (handler != null)
			{
				var args = new ResourceOptimizerProgressEventArgs(0, 0, message);
				handler(this, args);
				if (args.Cancel)
					_cancelMe = true;

				if (_progressEvent != null && _progressEvent.UserCancel)
					return;

				_progressEvent = args;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId =
				"Teleopti.Ccc.Domain.Optimization.TeamBlock.TeamBlockIntradayOptimizationService.OnReportProgress(System.String)")]
		private IEnumerable<ITeamBlockInfo> optimizeOneRound(DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences, ISchedulingOptions schedulingOptions,
			IList<ITeamBlockInfo> allTeamBlockInfos, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder,
			bool isMaxSeatToggleEnabled)
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
				if (_cancelMe)
					break;

				if (_progressEvent != null && _progressEvent.UserCancel)
					break;

				string teamName = teamBlockInfo.TeamInfo.Name.DisplayString(20);
				schedulePartModifyAndRollbackService.ClearModificationCollection();

				var previousTargetValue = _dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo,
					optimizationPreferences
						.Advanced, isMaxSeatToggleEnabled);
				_teamBlockClearer.ClearTeamBlock(schedulingOptions, schedulePartModifyAndRollbackService, teamBlockInfo);
				var firstSelectedDay = selectedPeriod.DayCollection().First();
				var datePoint = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().FirstOrDefault(x => x >= firstSelectedDay);
				var success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, datePoint, schedulingOptions,
					schedulePartModifyAndRollbackService,
					resourceCalculateDelayer, schedulingResultStateHolder, new ShiftNudgeDirective());
				if (!success)
				{
					OnReportProgress(Resources.OptimizingIntraday + Resources.Colon + Resources.RollingBackSchedulesFor + " " +
					                 teamBlockInfo.BlockInfo.BlockPeriod.DateString + " " + teamName);
					teamBlockToRemove.Add(teamBlockInfo);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
					continue;
				}

				//if (!_teamBlockMaxSeatChecker.CheckMaxSeat(datePoint, schedulingOptions) || !_restrictionOverLimitValidator.Validate(teamBlockInfo, optimizationPreferences))
				//{
				//	OnReportProgress(Resources.OptimizingIntraday + Resources.Colon + Resources.RollingBackSchedulesFor + " " +
				//					 teamBlockInfo.BlockInfo.BlockPeriod.DateString + " " + teamName);
				//	teamBlockToRemove.Add(teamBlockInfo);
				//	_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
				//	continue;
				//}

				if (!_teamBlockMaxSeatChecker.CheckMaxSeat(datePoint, schedulingOptions) || !_teamBlockOptimizationLimits.Validate(teamBlockInfo, optimizationPreferences))
				{
					OnReportProgress(Resources.OptimizingIntraday + Resources.Colon + Resources.RollingBackSchedulesFor + " " + teamBlockInfo.BlockInfo.BlockPeriod.DateString + " " + teamName);
					teamBlockToRemove.Add(teamBlockInfo);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
					continue;
				}



				var newTargetValue = _dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo,
					optimizationPreferences.Advanced, isMaxSeatToggleEnabled);
				var isWorse = newTargetValue >= previousTargetValue;
				string commonProgress = Resources.OptimizingIntraday + Resources.Colon + "(" + totalTeamBlockInfos + ")(" +
				                        runningTeamBlockCounter + ") " + teamBlockInfo.BlockInfo.BlockPeriod.DateString + " " +
				                        teamName + " ";
				if (isWorse)
				{
					teamBlockToRemove.Add(teamBlockInfo);
					OnReportProgress(commonProgress + previousTargetValue + "(" + newTargetValue + ")");
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
				}
				else
				{
					OnReportProgress(commonProgress + newTargetValue + " - " + Resources.Improved);
				}
			}
			return teamBlockToRemove;
		}


	}
}
