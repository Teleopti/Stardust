using System;
using System.Collections.Generic;
using System.Linq;
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
		              ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService);

		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
	}

	public class TeamBlockIntradayOptimizationService : ITeamBlockIntradayOptimizationService
	{
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamBlockIntradayDecisionMaker _teamBlockIntradayDecisionMaker;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly IStandardDeviationSumCalculator _standardDeviationSumCalculator;
		private readonly ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
		private readonly ITeamBlockGenerator _teamBlockGenerator;
		private readonly ITeamBlockRestrictionOverLimitValidator _restrictionOverLimitValidator;
		private bool _cancelMe;

		public TeamBlockIntradayOptimizationService(ITeamBlockGenerator teamBlockGenerator,
		                                            ITeamBlockScheduler teamBlockScheduler,
		                                            ISchedulingOptionsCreator schedulingOptionsCreator,
		                                            ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
		                                            ITeamBlockIntradayDecisionMaker teamBlockIntradayDecisionMaker,
		                                            ITeamBlockRestrictionOverLimitValidator restrictionOverLimitValidator,
		                                            ITeamBlockClearer teamBlockClearer,
		                                            IStandardDeviationSumCalculator standardDeviationSumCalculator,
													ITeamBlockMaxSeatChecker teamBlockMaxSeatChecker)
		{
			_teamBlockScheduler = teamBlockScheduler;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamBlockIntradayDecisionMaker = teamBlockIntradayDecisionMaker;
			_teamBlockClearer = teamBlockClearer;
			_standardDeviationSumCalculator = standardDeviationSumCalculator;
			_teamBlockMaxSeatChecker = teamBlockMaxSeatChecker;
			_teamBlockGenerator = teamBlockGenerator;
			_restrictionOverLimitValidator = restrictionOverLimitValidator;
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public void Optimize(IList<IScheduleMatrixPro> allPersonMatrixList,
							 DateOnlyPeriod selectedPeriod,
							 IList<IPerson> selectedPersons,
							 IOptimizationPreferences optimizationPreferences,
							 ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
			OnReportProgress(Resources.OptimizingIntraday + Resources.Colon + Resources.CollectingData);
			
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			var teamBlocks = _teamBlockGenerator.Generate(allPersonMatrixList, selectedPeriod, selectedPersons, schedulingOptions);
			var allMatrixesOfOnePerson = allPersonMatrixList.Where(x => x.Person.Equals(selectedPersons.First())).ToList();
			var remainingInfoList = new List<ITeamBlockInfo>(teamBlocks);
			
			while (remainingInfoList.Count > 0)
			{
				if (_cancelMe)
					break;
				var teamBlocksToRemove = optimizeOneRound(selectedPeriod, selectedPersons, optimizationPreferences,
														  schedulingOptions, remainingInfoList, allMatrixesOfOnePerson,
				                                          schedulePartModifyAndRollbackService);
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
				var args = new ResourceOptimizerProgressEventArgs(null, 0, 0, message);
				handler(this, args);
				if (args.Cancel)
					_cancelMe = true;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Optimization.TeamBlock.TeamBlockIntradayOptimizationService.OnReportProgress(System.String)")]
		private IEnumerable<ITeamBlockInfo> optimizeOneRound(DateOnlyPeriod selectedPeriod,
							 IList<IPerson> selectedPersons, IOptimizationPreferences optimizationPreferences,
									  ISchedulingOptions schedulingOptions, IList<ITeamBlockInfo> allTeamBlockInfos,
										IList<IScheduleMatrixPro> allMatrixesOfOnePerson,
										ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
			var teamBlockToRemove = new List<ITeamBlockInfo>();
			
			var sortedTeamBlockInfos = _teamBlockIntradayDecisionMaker.Decide(allTeamBlockInfos, optimizationPreferences,
			                                                              schedulingOptions);

			foreach (var teamBlockInfo in sortedTeamBlockInfos)
			{
				if (_cancelMe)
					break;
				schedulePartModifyAndRollbackService.ClearModificationCollection();

				var previousStandardDevationSum  = _standardDeviationSumCalculator.Calculate(selectedPeriod, allMatrixesOfOnePerson,
																							optimizationPreferences, schedulingOptions);
				_teamBlockClearer.ClearTeamBlock(schedulingOptions, schedulePartModifyAndRollbackService, teamBlockInfo);
				var firstSelectedDay = selectedPeriod.DayCollection().First();
				var datePoint = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().FirstOrDefault(x => x >= firstSelectedDay);
				var success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, datePoint, schedulingOptions, selectedPeriod, selectedPersons);
				if (!success)
				{
					teamBlockToRemove.Add(teamBlockInfo);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
					continue;
				}
				
				if (!_teamBlockMaxSeatChecker.CheckMaxSeat(datePoint) || !_restrictionOverLimitValidator.Validate(teamBlockInfo, optimizationPreferences))
				{
					teamBlockToRemove.Add(teamBlockInfo);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
					continue;
				}

				var newStandardDeviationSum = _standardDeviationSumCalculator.Calculate(selectedPeriod, allMatrixesOfOnePerson,
																								optimizationPreferences, schedulingOptions);
				var isWorse = newStandardDeviationSum >= previousStandardDevationSum;
				if (isWorse)
				{
					teamBlockToRemove.Add(teamBlockInfo);
					OnReportProgress(Resources.OptimizingIntraday + Resources.Colon + previousStandardDevationSum + "("+ newStandardDeviationSum+ ")");
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
				}
				else
				{
					OnReportProgress(Resources.OptimizingIntraday + Resources.Colon + newStandardDeviationSum + " - " + Resources.Improved);
				}
			}
			return teamBlockToRemove;
		}

		
	}
}
