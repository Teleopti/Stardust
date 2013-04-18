﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockDayOffOptimizerService
	{
		void OptimizeDaysOff(IList<IScheduleMatrixPro> allPersonMatrixList,
		                     DateOnlyPeriod selectedPeriod,
		                     IList<IPerson> selectedPersons,
		                     IOptimizationPreferences optimizationPreferences,
		                     ISchedulePartModifyAndRollbackService rollbackService,
		                     IDayOffTemplate dayOffTemplate);

		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
	}

	public class TeamBlockDayOffOptimizerService : ITeamBlockDayOffOptimizerService
	{
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly ISmartDayOffBackToLegalStateService _daysOffBackToLegal;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ILockableBitArrayChangesTracker _lockableBitArrayChangesTracker;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly IPeriodValueCalculator _periodValueCalculatorForAllSkills;
		private readonly IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamDayOffModifier _teamDayOffModifier;
		private readonly IBlockSteadyStateValidator _teamBlockSteadyStateValidator;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockRestrictionOverLimitValidator _restrictionOverLimitValidator;
		private bool _cancelMe;

		public TeamBlockDayOffOptimizerService(
			ITeamInfoFactory teamInfoFactory,
			ILockableBitArrayFactory lockableBitArrayFactory,
			IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
			ISmartDayOffBackToLegalStateService daysOffBackToLegal,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			ILockableBitArrayChangesTracker lockableBitArrayChangesTracker,
			ITeamBlockScheduler teamBlockScheduler,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			IPeriodValueCalculator periodValueCalculatorForAllSkills,
			IDayOffOptimizationDecisionMakerFactory dayOffOptimizationDecisionMakerFactory,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			ITeamDayOffModifier teamDayOffModifier,
			IBlockSteadyStateValidator teamBlockSteadyStateValidator,
			ITeamBlockClearer teamBlockClearer,
			ITeamBlockRestrictionOverLimitValidator restrictionOverLimitValidator
			)
		{
			_teamInfoFactory = teamInfoFactory;
			_lockableBitArrayFactory = lockableBitArrayFactory;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
			_daysOffBackToLegal = daysOffBackToLegal;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_lockableBitArrayChangesTracker = lockableBitArrayChangesTracker;
			_teamBlockScheduler = teamBlockScheduler;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_periodValueCalculatorForAllSkills = periodValueCalculatorForAllSkills;
			_dayOffOptimizationDecisionMakerFactory = dayOffOptimizationDecisionMakerFactory;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamDayOffModifier = teamDayOffModifier;
			_teamBlockSteadyStateValidator = teamBlockSteadyStateValidator;
			_teamBlockClearer = teamBlockClearer;
			_restrictionOverLimitValidator = restrictionOverLimitValidator;
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public void OptimizeDaysOff(
			IList<IScheduleMatrixPro> allPersonMatrixList,
			DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,
			IDayOffTemplate dayOffTemplate
			)
		{
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
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
				var teamInfosToRemove = runOneOptimizationRound(optimizationPreferences, rollbackService,
				                                                remainingInfoList, schedulingOptions,
				                                                selectedPeriod, selectedPersons);

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
			EventHandler<ResourceOptimizerProgressEventArgs> handler = ReportProgress;
			if (handler != null)
			{
				var args = new ResourceOptimizerProgressEventArgs(null, 0, 0, message);
				handler(this, args);
				if (args.Cancel)
					_cancelMe = true;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Optimization.TeamBlock.TeamBlockDayOffOptimizerService.OnReportProgress(System.String)")]
		private IEnumerable<ITeamInfo> runOneOptimizationRound(IOptimizationPreferences optimizationPreferences,
		                                                       ISchedulePartModifyAndRollbackService rollbackService,
		                                                       List<ITeamInfo> remainingInfoList,
		                                                       ISchedulingOptions schedulingOptions,
		                                                       DateOnlyPeriod selectedPeriod,
		                                                       IList<IPerson> selectedPersons)
		{
			var teamInfosToRemove = new List<ITeamInfo>();
			double previousPeriodValue =
					_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
			foreach (ITeamInfo teamInfo in remainingInfoList.GetRandom(remainingInfoList.Count, true))
			{
				if (_cancelMe)
					break;

				rollbackService.ClearModificationCollection();

				bool success = true;
				foreach (IScheduleMatrixPro matrix in teamInfo.MatrixesForGroupMember(0))
				{
					success = runOneMatrix(optimizationPreferences, rollbackService, schedulingOptions, matrix, teamInfo,
					             selectedPeriod, selectedPersons);
					if (!success)
						break;
				}
				// rollback if failed or not good
				double currentPeriodValue =
					_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
				if (currentPeriodValue >= previousPeriodValue || !success)
				{
					teamInfosToRemove.Add(teamInfo);
					_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
					currentPeriodValue =
					_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
				}
				previousPeriodValue = currentPeriodValue;

				OnReportProgress(Resources.OptimizingDaysOff + Resources.Colon + teamInfo.GroupPerson.Name +"(" + currentPeriodValue +")");
			}

			return teamInfosToRemove;
		}

		private bool runOneMatrix(IOptimizationPreferences optimizationPreferences,
		                          ISchedulePartModifyAndRollbackService rollbackService,
		                          ISchedulingOptions schedulingOptions, IScheduleMatrixPro matrix,
		                          ITeamInfo teamInfo, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
		{
			bool considerWeekBefore = optimizationPreferences.DaysOff.ConsiderWeekBefore;
			bool considerWeekAfter = optimizationPreferences.DaysOff.ConsiderWeekAfter;
			ILockableBitArray originalArray = _lockableBitArrayFactory.ConvertFromMatrix(considerWeekBefore, considerWeekAfter,
			                                                                             matrix);
			ILockableBitArray resultingArray = tryFindMoves(matrix, originalArray, optimizationPreferences);

			if (resultingArray.Equals(originalArray))
				return false;

			// find out what have changed
			IList<DateOnly> addedDaysOff = _lockableBitArrayChangesTracker.DaysOffAdded(resultingArray, originalArray, matrix,
			                                                                            considerWeekBefore);
			IList<DateOnly> removedDaysOff = _lockableBitArrayChangesTracker.DaysOffRemoved(resultingArray, originalArray, matrix,
			                                                                                considerWeekBefore);

			// Does the predictor beleve in this?

			removeAllDecidedDaysOff(rollbackService, schedulingOptions, teamInfo, removedDaysOff);
			addAllDecidedDaysOf(rollbackService, schedulingOptions, teamInfo, addedDaysOff);
			bool success = reScheduleAllMovedDaysOff(schedulingOptions, teamInfo, selectedPeriod, selectedPersons, removedDaysOff, rollbackService);
			if (!success)
				return false;

			if (!_restrictionOverLimitValidator.Validate(teamInfo, optimizationPreferences))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				lockDaysInMatrixes(addedDaysOff, teamInfo);
				lockDaysInMatrixes(removedDaysOff, teamInfo);

				return true;
			}
			// ev back to legal state?
			// if possible reschedule block without clearing
			// else
			//	clear involved teamblocks
			//	reschedule involved teamblocks
			
			return true;
		}

		private static void lockDaysInMatrixes(IList<DateOnly> datesToLock , ITeamInfo teamInfo)
		{
			foreach (var dateOnly in datesToLock)
			{
				foreach (var scheduleMatrixPro in teamInfo.MatrixesForGroupAndDate(dateOnly))
				{
					scheduleMatrixPro.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly));
				}
			}
		}

		private bool reScheduleAllMovedDaysOff(ISchedulingOptions schedulingOptions, ITeamInfo teamInfo,
		                                       DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
		                                       IEnumerable<DateOnly> removedDaysOff, ISchedulePartModifyAndRollbackService rollbackService)
		{
			foreach (DateOnly dateOnly in removedDaysOff)
			{
                bool singleAgentTeam = schedulingOptions.GroupOnGroupPageForTeamBlockPer != null &&
                                           schedulingOptions.GroupOnGroupPageForTeamBlockPer.Key == "SingleAgentTeam";
                ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly,
				                                                                        schedulingOptions
                                                                                            .BlockFinderTypeForAdvanceScheduling, singleAgentTeam);
				if (!_teamBlockSteadyStateValidator.IsBlockInSteadyState(teamBlockInfo, schedulingOptions))
					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);

				bool success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions, selectedPeriod, selectedPersons);
				if (!success)
					return false;
			}

			return true;
		}

		private void addAllDecidedDaysOf(ISchedulePartModifyAndRollbackService rollbackService,
		                                 ISchedulingOptions schedulingOptions, ITeamInfo teamInfo,
		                                 IEnumerable<DateOnly> addedDaysOff)
		{
			foreach (DateOnly dateOnly in addedDaysOff)
			{
				_teamDayOffModifier.AddDayOffAndResourceCalculate(rollbackService, teamInfo, dateOnly, schedulingOptions);
			}
		}

		private void removeAllDecidedDaysOff(ISchedulePartModifyAndRollbackService rollbackService,
		                                     ISchedulingOptions schedulingOptions, ITeamInfo teamInfo,
		                                     IEnumerable<DateOnly> removedDaysOff)
		{
			foreach (DateOnly dateOnly in removedDaysOff)
			{
				_teamDayOffModifier.RemoveDayOff(rollbackService, teamInfo, dateOnly, schedulingOptions);
			}
		}

		//to new class
		private ILockableBitArray tryFindMoves(IScheduleMatrixPro matrix, ILockableBitArray originalArray,
		                                       IOptimizationPreferences optimizationPreferences)
		{
			//should use agggregated skills
			IScheduleResultDataExtractor scheduleResultDataExtractor =
				_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix, optimizationPreferences.Advanced);

			// find days off to move within the common matrix period
			IEnumerable<IDayOffDecisionMaker> decisionMakers =
				_dayOffOptimizationDecisionMakerFactory.CreateDecisionMakers(originalArray, optimizationPreferences);
			foreach (IDayOffDecisionMaker dayOffDecisionMaker in decisionMakers)
			{
				var workingBitArray = (ILockableBitArray) originalArray.Clone();
				if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values()))
				{
					if (!_daysOffBackToLegal.Execute(_daysOffBackToLegal.BuildSolverList(workingBitArray), 100))
						continue;

					if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values()))
						continue;
				}

				// DayOffBackToLegal if decisionMaker did something wrong
				if (!_daysOffBackToLegal.Execute(_daysOffBackToLegal.BuildSolverList(workingBitArray), 100))
					continue;

				return workingBitArray;
			}

			return originalArray;
		}
	}
}