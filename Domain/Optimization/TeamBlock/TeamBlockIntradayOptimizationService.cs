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
		private readonly ITeamBlockRestrictionOverLimitValidator _restrictionOverLimitValidator;
	    private readonly IDailyTargetValueCalculatorForTeamBlock _dailyTargetValueCalculatorForTeamBlock;
		private bool _cancelMe;

		public TeamBlockIntradayOptimizationService(ITeamBlockGenerator teamBlockGenerator,
		                                            ITeamBlockScheduler teamBlockScheduler,
		                                            ISchedulingOptionsCreator schedulingOptionsCreator,
		                                            ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
		                                            ITeamBlockIntradayDecisionMaker teamBlockIntradayDecisionMaker,
		                                            ITeamBlockRestrictionOverLimitValidator restrictionOverLimitValidator,
		                                            ITeamBlockClearer teamBlockClearer,
													ITeamBlockMaxSeatChecker teamBlockMaxSeatChecker, IDailyTargetValueCalculatorForTeamBlock dailyTargetValueCalculatorForTeamBlock)
		{
			_teamBlockScheduler = teamBlockScheduler;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamBlockIntradayDecisionMaker = teamBlockIntradayDecisionMaker;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockMaxSeatChecker = teamBlockMaxSeatChecker;
		    _dailyTargetValueCalculatorForTeamBlock = dailyTargetValueCalculatorForTeamBlock;
		    _teamBlockGenerator = teamBlockGenerator;
			_restrictionOverLimitValidator = restrictionOverLimitValidator;
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
			OnReportProgress(Resources.OptimizingIntraday + Resources.Colon + Resources.CollectingData);
			
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			var teamBlocks = _teamBlockGenerator.Generate(allPersonMatrixList, selectedPeriod, selectedPersons, schedulingOptions);
			var remainingInfoList = new List<ITeamBlockInfo>(teamBlocks);
			
			while (remainingInfoList.Count > 0)
			{
				if (_cancelMe)
					break;
				var teamBlocksToRemove = optimizeOneRound(selectedPeriod, optimizationPreferences,
														  schedulingOptions, remainingInfoList, 
				                                          schedulePartModifyAndRollbackService,
														  resourceCalculateDelayer,
														  schedulingResultStateHolder);
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
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Optimization.TeamBlock.TeamBlockIntradayOptimizationService.OnReportProgress(System.String)")]
		private IEnumerable<ITeamBlockInfo> optimizeOneRound(DateOnlyPeriod selectedPeriod,
							 IOptimizationPreferences optimizationPreferences,
									  ISchedulingOptions schedulingOptions, IList<ITeamBlockInfo> allTeamBlockInfos,
										ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
										IResourceCalculateDelayer resourceCalculateDelayer,
										ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var teamBlockToRemove = new List<ITeamBlockInfo>();
			
			var sortedTeamBlockInfos = _teamBlockIntradayDecisionMaker.Decide(allTeamBlockInfos, optimizationPreferences,
			                                                              schedulingOptions);

			int totalTeamBlockInfos = sortedTeamBlockInfos.Count;
			int runningTeamBlockCounter = 0;
			foreach (var teamBlockInfo in sortedTeamBlockInfos)
			{
				runningTeamBlockCounter++;
				if (_cancelMe)
					break;

				string teamName = StringHelper.DisplayString(teamBlockInfo.TeamInfo.Name, 20);
				schedulePartModifyAndRollbackService.ClearModificationCollection();

                var previousTargetValue = _dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo,
			                                                                                          optimizationPreferences
			                                                                                              .Advanced);
				_teamBlockClearer.ClearTeamBlock(schedulingOptions, schedulePartModifyAndRollbackService, teamBlockInfo);
				var firstSelectedDay = selectedPeriod.DayCollection().First();
				var datePoint = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().FirstOrDefault(x => x >= firstSelectedDay);
                                var success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, datePoint, schedulingOptions,
				                                                       schedulePartModifyAndRollbackService,
				                                                       resourceCalculateDelayer, schedulingResultStateHolder, new ShiftNudgeDirective());
				if (!success)
				{
                    OnReportProgress(Resources.OptimizingIntraday + Resources.Colon + Resources.RollingBackSchedulesFor + " " + teamBlockInfo.BlockInfo.BlockPeriod.DateString + " " + teamName);
                    teamBlockToRemove.Add(teamBlockInfo);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
					continue;
				}

				if (!_teamBlockMaxSeatChecker.CheckMaxSeat(datePoint, schedulingOptions) || !_restrictionOverLimitValidator.Validate(teamBlockInfo, optimizationPreferences))
				{
                    OnReportProgress(Resources.OptimizingIntraday + Resources.Colon + Resources.RollingBackSchedulesFor + " " + teamBlockInfo.BlockInfo.BlockPeriod.DateString + " " + teamName);
                    teamBlockToRemove.Add(teamBlockInfo);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
					continue;
				}


				
                var newTargetValue = _dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo, optimizationPreferences.Advanced);
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
