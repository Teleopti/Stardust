using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
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
		private readonly ITeamBlockGenerator _teamBlockGenerator;
		private readonly ITeamBlockRestrictionOverLimitValidator _restrictionOverLimitValidator;
		private bool _cancelMe;

		public TeamBlockIntradayOptimizationService(ITeamBlockGenerator teamBlockGenerator,
		                                            ITeamBlockScheduler teamBlockScheduler,
		                                            ISchedulingOptionsCreator schedulingOptionsCreator,
		                                            ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
		                                            ITeamBlockIntradayDecisionMaker teamBlockIntradayDecisionMaker,
		                                            ITeamBlockRestrictionOverLimitValidator restrictionOverLimitValidator,
		                                            ITeamBlockClearer teamBlockClearer)
		{
			_teamBlockScheduler = teamBlockScheduler;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamBlockIntradayDecisionMaker = teamBlockIntradayDecisionMaker;
			_teamBlockClearer = teamBlockClearer;
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
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			var teamBlocks = _teamBlockGenerator.Generate(allPersonMatrixList, selectedPeriod, selectedPersons, schedulingOptions);
			
			var remainingInfoList = new List<ITeamBlockInfo>(teamBlocks);
			
			while (remainingInfoList.Count > 0)
			{
				if (_cancelMe)
					break;
				var teamBlocksToRemove = optimizeOneRound(selectedPeriod, selectedPersons, optimizationPreferences,
														  schedulingOptions, remainingInfoList,
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
									  ISchedulingOptions schedulingOptions, List<ITeamBlockInfo> allTeamBlockInfos,
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

				var previousStandardDevation = teamBlockInfo.BlockInfo.AverageStandardDeviation; 
				_teamBlockClearer.ClearTeamBlock(schedulingOptions, schedulePartModifyAndRollbackService, teamBlockInfo);

				var datePoint = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().First();
				var success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, datePoint, schedulingOptions, selectedPeriod, selectedPersons);
				if (!success)
				{
					teamBlockToRemove.Add(teamBlockInfo);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
					continue;
				}

				if (!_restrictionOverLimitValidator.Validate(teamBlockInfo, optimizationPreferences))
					continue;

				var newStandardDeviation = _teamBlockIntradayDecisionMaker.RecalculateTeamBlock(teamBlockInfo, optimizationPreferences,
				                                                                                schedulingOptions).BlockInfo.AverageStandardDeviation;
				var isWorse = newStandardDeviation >= previousStandardDevation;
				if (isWorse)
				{
					teamBlockToRemove.Add(teamBlockInfo);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
				}

				OnReportProgress(Resources.OptimizingIntraday + Resources.Colon + teamBlockInfo.TeamInfo.GroupPerson.Name + "(" + newStandardDeviation + ")");
			}
			return teamBlockToRemove;
		}

		
	}
}
