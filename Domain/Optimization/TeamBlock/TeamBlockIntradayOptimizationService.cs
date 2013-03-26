using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
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
		private readonly IPeriodValueCalculator _periodValueCalculatorForAllSkills;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamBlockIntradayDecisionMaker _teamBlockIntradayDecisionMaker;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ICheckerRestriction _restrictionChecker;
		private readonly ITeamBlockGenerator _teamBlockGenerator;
		private readonly ITeamBlockRestrictionOverLimitValidator _restrictionOverLimitValidator;
		private bool _cancelMe;

		public TeamBlockIntradayOptimizationService(ITeamBlockGenerator teamBlockGenerator,
		                                            ITeamBlockScheduler teamBlockScheduler,
		                                            ISchedulingOptionsCreator schedulingOptionsCreator,
		                                            IPeriodValueCalculator periodValueCalculatorForAllSkills,
		                                            ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
		                                            ITeamBlockIntradayDecisionMaker teamBlockIntradayDecisionMaker,
		                                            ITeamBlockRestrictionOverLimitValidator restrictionOverLimitValidator,
		                                            ITeamBlockClearer teamBlockClearer,
		                                            ICheckerRestriction restrictionChecker)
		{
			_teamBlockScheduler = teamBlockScheduler;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_periodValueCalculatorForAllSkills = periodValueCalculatorForAllSkills;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamBlockIntradayDecisionMaker = teamBlockIntradayDecisionMaker;
			_teamBlockClearer = teamBlockClearer;
			_restrictionChecker = restrictionChecker;
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
				var previousPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.IntradayOptimization);
				var teamBlocksToRemove = optimizeOneRound(selectedPeriod, selectedPersons, optimizationPreferences,
														  schedulingOptions, remainingInfoList,
				                                          previousPeriodValue, schedulePartModifyAndRollbackService);
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

		private IEnumerable<ITeamBlockInfo> optimizeOneRound(DateOnlyPeriod selectedPeriod,
							 IList<IPerson> selectedPersons, IOptimizationPreferences optimizationPreferences,
									  ISchedulingOptions schedulingOptions, List<ITeamBlockInfo> allTeamBlocks, double previousPeriodValue,
										ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
			var teamBlockToRemove = new List<ITeamBlockInfo>();
			
			var sortedTeamBlocks = _teamBlockIntradayDecisionMaker.Decide(allTeamBlocks, optimizationPreferences,
			                                                              schedulingOptions);

			foreach (var teamBlock in sortedTeamBlocks)
			{
				if (_cancelMe)
					break;
				schedulePartModifyAndRollbackService.ClearModificationCollection();
			
				_teamBlockClearer.ClearTeamBlock(schedulingOptions, schedulePartModifyAndRollbackService, teamBlock);

				var datePoint = teamBlock.BlockInfo.BlockPeriod.DayCollection().First();
				var success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlock, datePoint, schedulingOptions, selectedPeriod, selectedPersons);
				if (!success)
				{
					teamBlockToRemove.Add(teamBlock);
					continue;
				}

				if (!_restrictionOverLimitValidator.Validate(teamBlock, optimizationPreferences, schedulingOptions,
				                                            schedulePartModifyAndRollbackService, _restrictionChecker))
					continue;
				
				var currentPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.IntradayOptimization);
				var isPeriodWorse = currentPeriodValue >= previousPeriodValue;
				if (isPeriodWorse)
				{
					teamBlockToRemove.Add(teamBlock);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
				}
				else
				{
					previousPeriodValue = currentPeriodValue;
				}

				OnReportProgress("xxPeriodvalue: " + currentPeriodValue + " xxOptimized team " + teamBlock.TeamInfo.GroupPerson.Name);
			}
			return teamBlockToRemove;
		}

		
	}
}
