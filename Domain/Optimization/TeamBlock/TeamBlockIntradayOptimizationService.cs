using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
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
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly IPeriodValueCalculator _periodValueCalculatorForAllSkills;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamBlockIntradayDecisionMaker _teamBlockIntradayDecisionMaker;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockRestrictionOverLimitValidator _restrictionOverLimitValidator;
		private bool _cancelMe;

		public TeamBlockIntradayOptimizationService(ITeamInfoFactory teamInfoFactory,
		                                            ITeamBlockInfoFactory teamBlockInfoFactory,
		                                            ITeamBlockScheduler teamBlockScheduler,
		                                            ISchedulingOptionsCreator schedulingOptionsCreator,
		                                            IPeriodValueCalculator periodValueCalculatorForAllSkills,
		                                            ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
		                                            ITeamBlockIntradayDecisionMaker teamBlockIntradayDecisionMaker,
		                                            ITeamBlockRestrictionOverLimitValidator restrictionOverLimitValidator)
		                                            ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
													ITeamBlockClearer teamBlockClearer)
		{
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockScheduler = teamBlockScheduler;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_periodValueCalculatorForAllSkills = periodValueCalculatorForAllSkills;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamBlockIntradayDecisionMaker = teamBlockIntradayDecisionMaker;
			_teamBlockClearer = teamBlockClearer;
			_restrictionOverLimitValidator = restrictionOverLimitValidator;
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public void Optimize(IList<IScheduleMatrixPro> allPersonMatrixList,
							 DateOnlyPeriod selectedPeriod,
							 IList<IPerson> selectedPersons,
							 IOptimizationPreferences optimizationPreferences,
							 ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
			ISchedulingOptions schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);

			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				allTeamInfoListOnStartDate.Add(_teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod,
																			   allPersonMatrixList));
			}
			var allTeamBlocksInHashSet = new HashSet<ITeamBlockInfo>();
			foreach (var teamInfo in allTeamInfoListOnStartDate)
			{
				foreach (var day in selectedPeriod.DayCollection())
				{
					var teamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, day,
																			  schedulingOptions.BlockFinderTypeForAdvanceScheduling);
					allTeamBlocksInHashSet.Add(teamBlock);
				}
			}
			var allTeamBlocks = new List<ITeamBlockInfo>();
			allTeamBlocks.AddRange(allTeamBlocksInHashSet);
			
			//rounds
			var remainingInfoList = new List<ITeamBlockInfo>(allTeamBlocksInHashSet);
			
			while (remainingInfoList.Count > 0)
			{
				if (_cancelMe)
					break;
				var previousPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.IntradayOptimization);
				var teamBlocksToRemove = optimizeOneRound(optimizationPreferences,
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

		private IEnumerable<ITeamBlockInfo> optimizeOneRound(IOptimizationPreferences optimizationPreferences,
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
			
				//clear block
				_teamBlockClearer.ClearTeamBlock(schedulingOptions, schedulePartModifyAndRollbackService, teamBlock);

				var datePoint = teamBlock.BlockInfo.BlockPeriod.DayCollection().First();
				var success = _teamBlockScheduler.ScheduleTeamBlock(teamBlock, datePoint, schedulingOptions);
				if (!success)
				{
					teamBlockToRemove.Add(teamBlock);
					continue;
				}

				if (!_restrictionOverLimitValidator.Validate(teamBlock, optimizationPreferences, schedulingOptions,
				                                            schedulePartModifyAndRollbackService, new RestrictionChecker()))
					continue;
				
				var currentPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
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

				OnReportProgress("Periodvalue: " + currentPeriodValue + " Optimized team " + teamBlock.TeamInfo.GroupPerson.Name);
			}
			return teamBlockToRemove;
		}

		
	}
}
