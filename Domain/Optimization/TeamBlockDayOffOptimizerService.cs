using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface ITeamBlockDayOffOptimizerService
	{
		void OptimizeDaysOff(IList<IScheduleMatrixPro> allPersonMatrixList, 
			DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons, 
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IDayOffTemplate dayOffTemplate);

		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
	}

	public class TeamBlockDayOffOptimizerService : ITeamBlockDayOffOptimizerService
	{
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly ISmartDayOffBackToLegalStateService _smartDayOffBackToLegalStateService;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ILockableBitArrayChangesTracker _lockableBitArrayChangesTracker;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly IPeriodValueCalculator _periodValueCalculatorForAllSkills;
		private readonly IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamDayOffModifyer _teamDayOffModifyer;
		private bool _cancelMe;

		public TeamBlockDayOffOptimizerService(
			ITeamInfoFactory teamInfoFactory, 				
			ILockableBitArrayFactory lockableBitArrayFactory,
			IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
			ISmartDayOffBackToLegalStateService smartDayOffBackToLegalStateService,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			ILockableBitArrayChangesTracker lockableBitArrayChangesTracker,
			ITeamBlockScheduler teamBlockScheduler,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			IPeriodValueCalculator periodValueCalculatorForAllSkills,
			IDayOffOptimizationDecisionMakerFactory dayOffOptimizationDecisionMakerFactory,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			ITeamDayOffModifyer teamDayOffModifyer
			)
		{
			_teamInfoFactory = teamInfoFactory;
			_lockableBitArrayFactory = lockableBitArrayFactory;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
			_smartDayOffBackToLegalStateService = smartDayOffBackToLegalStateService;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_lockableBitArrayChangesTracker = lockableBitArrayChangesTracker;
			_teamBlockScheduler = teamBlockScheduler;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_periodValueCalculatorForAllSkills = periodValueCalculatorForAllSkills;
			_dayOffOptimizationDecisionMakerFactory = dayOffOptimizationDecisionMakerFactory;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamDayOffModifyer = teamDayOffModifyer;
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public void OptimizeDaysOff(
			IList<IScheduleMatrixPro> allPersonMatrixList, 
			DateOnlyPeriod selectedPeriod,
		    IList<IPerson> selectedPersons, 
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IDayOffTemplate dayOffTemplate
			)
		{
			ISchedulingOptions schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			schedulingOptions.DayOffTemplate = dayOffTemplate;
			// create a list of all teamInfos
			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				allTeamInfoListOnStartDate.Add(_teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod, allPersonMatrixList));
			}

			// find a random selected TeamInfo/matrix
			var remainingInfoList = new List<ITeamInfo>(allTeamInfoListOnStartDate);

			while (remainingInfoList.Count > 0)
			{
				var previousPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
				var teamInfosToRemove = runOneOptimizationRound(optimizationPreferences, schedulePartModifyAndRollbackService,
				                                                remainingInfoList, schedulingOptions, previousPeriodValue, selectedPeriod, selectedPersons);

				if (_cancelMe)
					break;

				foreach (var teamInfo in teamInfosToRemove)
				{
					remainingInfoList.Remove(teamInfo);
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

		private IEnumerable<ITeamInfo> runOneOptimizationRound(IOptimizationPreferences optimizationPreferences,
		                                     ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                                     List<ITeamInfo> remainingInfoList, ISchedulingOptions schedulingOptions,
											 double previousPeriodValue, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
		{
			var teamInfosToRemove = new List<ITeamInfo>();
			foreach (var teamInfo in remainingInfoList.GetRandom(remainingInfoList.Count, true))
			{
				if (_cancelMe)
					break;

				schedulePartModifyAndRollbackService.ClearModificationCollection();

				foreach (var matrix in teamInfo.MatrixesForGroupMember(0))
				{
					runOneMatrix(optimizationPreferences, schedulePartModifyAndRollbackService, schedulingOptions, matrix, teamInfo,
					             selectedPeriod, selectedPersons);
				}
				// rollback if failed or not good
				var currentPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
				if (currentPeriodValue >= previousPeriodValue)
				{
					teamInfosToRemove.Add(teamInfo);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);

				}
				previousPeriodValue = currentPeriodValue;

				OnReportProgress("Periodvalue: " + currentPeriodValue + " Optimized team " + teamInfo.GroupPerson.Name);
			}

			return teamInfosToRemove;
		}

		private void runOneMatrix(IOptimizationPreferences optimizationPreferences,
		                          ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                          ISchedulingOptions schedulingOptions, IScheduleMatrixPro matrix,
									ITeamInfo teamInfo, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
		{
			ILockableBitArray originalArray =
				_lockableBitArrayFactory.ConvertFromMatrix(optimizationPreferences.DaysOff.ConsiderWeekBefore,
				                                           optimizationPreferences.DaysOff.ConsiderWeekAfter, matrix);
			ILockableBitArray resultingArray = tryFindMoves(matrix, originalArray, optimizationPreferences);

			if (resultingArray.Equals(originalArray))
				return;

			// find out what have changed
			IList<DateOnly> addedDaysOff = _lockableBitArrayChangesTracker.DaysOffAdded(resultingArray, originalArray, matrix,
			                                                                            optimizationPreferences.DaysOff
			                                                                                                   .ConsiderWeekBefore);
			IList<DateOnly> removedDaysOff = _lockableBitArrayChangesTracker.DaysOffRemoved(resultingArray, originalArray,
			                                                                                matrix,
			                                                                                optimizationPreferences.DaysOff
			                                                                                                       .ConsiderWeekBefore);
			// Does the predictor beleve in this?
			foreach (var dateOnly in removedDaysOff)
			{
				_teamDayOffModifyer.RemoveDayOffAndResourceCalculate(schedulePartModifyAndRollbackService, teamInfo, dateOnly, schedulingOptions);
			}

			foreach (var dateOnly in addedDaysOff)
			{
				_teamDayOffModifyer.AddDayOffAndResourceCalculate(schedulePartModifyAndRollbackService, teamInfo, dateOnly, schedulingOptions);
			}

			foreach (var dateOnly in removedDaysOff)
			{
				ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly,
				                                                                         schedulingOptions
					                                                                         .BlockFinderTypeForAdvanceScheduling);
				_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions, selectedPeriod, selectedPersons);
			}


			// ev back to legal state?
			// if possible reschedule block without clearing
			// else
			//	clear involved teamblocks
			//	reschedule involved teamblocks

			// remember not to break anything in shifts or restrictions
		}

		

		//to new class
		private ILockableBitArray tryFindMoves(IScheduleMatrixPro matrix, ILockableBitArray originalArray, IOptimizationPreferences optimizationPreferences)
		{
			//should use agggregated skills
			IScheduleResultDataExtractor scheduleResultDataExtractor =
				_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix, optimizationPreferences.Advanced);

			// find days off to move within the common matrix period
			IEnumerable<IDayOffDecisionMaker> decisionMakers = _dayOffOptimizationDecisionMakerFactory.CreateDecisionMakers(originalArray, optimizationPreferences);
			foreach (var dayOffDecisionMaker in decisionMakers)
			{
				var workingBitArray = (ILockableBitArray)originalArray.Clone();

				if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values()))
				{
					if (!_smartDayOffBackToLegalStateService.Execute(_smartDayOffBackToLegalStateService.BuildSolverList(workingBitArray), 100))
						continue;

					if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values()))
						continue;
				}

				// DayOffBackToLegal if decisionMaker did something wrong
				if (!_smartDayOffBackToLegalStateService.Execute(_smartDayOffBackToLegalStateService.BuildSolverList(workingBitArray), 100))
					continue;

				return workingBitArray;
			}

			return originalArray;
		}
	}
}